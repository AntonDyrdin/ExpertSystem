using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
namespace Экспертная_система
{
    [Serializable]
    public class Expert
    {
        public string expertName;
        public double[,] dataset;
        public double[,] dataset1;
        public double[,] dataset2;
        public double[,] dataset3;
        private string period = "day";
        //критерий оптимальности
        public double target_function;
        public Hyperparameters H;
        //список алгоритмов(комитет)
        public List<Algorithm> algorithms;
        public int committeeNodeID;
        public string path_prefix;

        // БАЗОВАЯ  валюта
        //(валюта, которая покупается и продаётся)
        //стоит на ПЕРВОМ месте в валютной паре
        //по команде BUY значение этого депозита должно расти  на количество покупаемых единиц
        //по команде SELL значение этого депозита должно уменьшаться на количество покупаемых единиц
        public double deposit1 = 0;

        //КОТИРУЕМАЯ валюта 
        //валюта входа и выхода (основная)
        //стоит на ВТОРОМ месте в валютной паре
        //по команде BUY значение этого депозита должно уменьшаться  пропорционально цене базовой вылюты
        //по команде SELL значение этого депозита должно расти   пропорционально цене базовой вылюты
        public double deposit2 = 5000;

        public List<double> deposit1History;
        public List<double> deposit2History;
        public List<double> closeValueHistory;
        public List<string> actionHistory;
        public double[] committeeResponse;
        public string presentLine;
        public List<string> report;
        public DecisionMakingSystem DMS;
        public void load(string expertName, Form1 form1)
        {
            this.form1 = form1;
            path_prefix = form1.pathPrefix;
            H = new Hyperparameters(form1, expertName);
            algorithms = new List<Algorithm>();
            Directory.CreateDirectory(form1.pathPrefix + expertName);
            committeeNodeID = H.add("name:committee");
            this.expertName = expertName;
            report = new List<string>();

            DMS = new DecisionMakingSystem(form1);


        }
        public Expert(string expertName, Form1 form1)
        {
            load(expertName, form1);

            foreach (string expertFolder in Directory.GetDirectories(path_prefix))
            {
                if (Path.GetFileName(expertFolder) == expertName)
                {  //В ДАННОМ КОНСТРУКТОРЕ, ПРИ СОЗДАНИИ НОВОГО ЭКЗЕМПЛЯРА КЛАССА Expert, ПАПКА С ТАКИМ ЖЕ ИМЕНЕМ БУДЕТ УДАЛЕНА
                    Directory.Delete(expertFolder, true);
                }
            }
        }
        public Expert(string expertName, Form1 form1, bool DoNotDeleteExpertFolder)
        {
            load(expertName, form1);
        }

        public string getStateStr()
        {
            string stateInString = "A[0]:" + committeeResponse[0].ToString();
            for (int i = 1; i < committeeResponse.Length; i++)
            {
                stateInString += ",A[" + i.ToString() + "]:" + committeeResponse[i].ToString();
            }
            return stateInString;
        }

        //возвращает  действие, о котором было принято решение
        public string getDecision(double[] committeeResponse)
        {


            return DMS.getAction(getStateStr()).type;
            /* 
             double decision = 0;
             double sum = 0;
             for (int i = 0; i < algorithms.Count; i++)
             {
                 sum += committeeResponse[i];
             }
             decision = sum / algorithms.Count;
             if (decision > 0.5)
                 return "buy";
             if (decision < 0.5)
                 return "sell";
             return "nothing";  */
        }
        public void trainAllAlgorithms(bool deleteLowAccModels)
        {
            foreach (Algorithm algorithm in algorithms)
            {
                algorithm.train().Wait();
            }

            if (deleteLowAccModels)
            {
                //УДАЛЕНИЕ АЛГОРИТМОВ С НИЗКИМИ ПОКАЗАТЕЛЯМИ ТОЧНОСТИ
                deleteAlgorithmsWithLowAccuracy(50);
            }
        }

        public void deleteAlgorithmsWithLowAccuracy(double acceptableLevel)
        {
            log("Удаление моделей с низкими показателями точности прогноза.");
            log("Допустимый уровень точности: " + acceptableLevel + " %");
            for (int i = 0; i < algorithms.Count; i++)
                if (algorithms[i].accuracy < acceptableLevel)
                {
                    committeeNodeID = H.getNodeByName("committee")[0].ID;
                    var algorithmBranches = H.getNodesByparentID(committeeNodeID);
                    H.deleteBranch(algorithmBranches[i].ID);
                    log("     Удалена модель " + algorithms[i].modelName + "accuracy = " + algorithms[i].accuracy.ToString());
                    algorithms.RemoveAt(i);

                    i--;
                }
                else
                {
                    algorithms[i].h.setValueByName("accuracy", algorithms[i].accuracy.ToString().Replace(',', '.'));
                }
            log("Состав комитета после удаления:");
            for (int i = 0; i < algorithms.Count; i++)
                log("       " + algorithms[i].modelName + "accuracy = " + algorithms[i].accuracy.ToString());
        }

        public double[] getPrediction(string[] input)
        {
            committeeResponse = new double[algorithms.Count];
            Task<double>[] getPredTasks = new Task<double>[algorithms.Count];
            for (int i = 0; i < algorithms.Count; i++)
            {
                var algorithm = algorithms[i];
                getPredTasks[i] = Task.Run(() => algorithm.getPrediction(input));
            }

            foreach (var task in getPredTasks)
                task.Wait();

            for (int i = 0; i < algorithms.Count; i++)
            {
                committeeResponse[i] = getPredTasks[i].Result;

                if (committeeResponse[i] > 0.5)
                    committeeResponse[i] = 1;
                if (committeeResponse[i] < 0.5)
                    committeeResponse[i] = 0;
            }
            return committeeResponse;
        }

        public string test(DateTime date1, DateTime date2, string rawDatasetFilePath)
        {
            DMS.defaultActions.Clear();
            DMS.parameters.Clear();
            DMS.S.Clear();

            for (int i = 0; i < algorithms.Count; i++)
                DMS.addParameter("A[" + i.ToString() + "]", "0,1");

            DMS.defaultActions.Add(new DMSAction("buy"));
            DMS.defaultActions.Add(new DMSAction("sell"));
            DMS.defaultActions.Add(new DMSAction("nothing"));
            DMS.generateStates();



            closeValueHistory = new List<double>();
            deposit1History = new List<double>();
            deposit2History = new List<double>();
            closeValueHistory = new List<double>();
            actionHistory = new List<string>();
            report = new List<string>();
            presentLine = "";
            double closeValue = 0;
            string action = "";
            string reportHead = "<presentDate>;<deposit1>;<deposit2>;<action>;<reward>;<closeValue>;";
            foreach (Algorithm algorithm in algorithms)
                reportHead += "<" + algorithm.modelName + ">;";
            report.Add(reportHead);
            if (date1 < date2)
            {
                //ЗАПУСК СКРИПТОВ ПОТОЧНОГО ПРОГНОЗИРОВНИЯ
                Task[] RunTasks = new Task[algorithms.Count];
                foreach (Algorithm algorithm in algorithms)
                    RunTasks[algorithms.IndexOf(algorithm)] = Task.Run(() => algorithm.runGetPredictionScript());

                //ОЖИДАНИЕ ЗАВЕРШЕНИЯ ЗАПУСКА
                foreach (var task in RunTasks)
                    task.Wait();

                while (date1 < date2)
                {
                    string reportLine = "";
                    string closeValueStr;

                    bool dateExist = false;
                    string dateStr = "";
                    if (date1.Day < 10) dateStr += "0" + date1.Day.ToString(); else dateStr += date1.Day.ToString();
                    dateStr += '/';
                    if (date1.Month < 10) dateStr += "0" + date1.Month.ToString(); else dateStr += date1.Month.ToString();
                    dateStr += '/';
                    dateStr += date1.Year.ToString().Substring(2, 2);
                    int[] windowSizes = new int[algorithms.Count];
                    for (int i = 0; i < algorithms.Count; i++)
                        windowSizes[i] = Convert.ToInt32(algorithms[i].getValueByName("window_size"));
                    int windowSize = 0;
                    for (int i = 0; i < windowSizes.Length; i++)
                    {
                        if (windowSizes[i] > windowSize)
                            windowSize = windowSizes[i];
                    }
                    //+1 для заголовка;+1 для нормализации i/(i-1)
                    string[] input = new string[windowSize + 1 + 1];
                    var allLines = skipEmptyLines(File.ReadAllLines(rawDatasetFilePath));
                    //копирование заголовка
                    input[0] = allLines[0];


                    for (int i = 1; i < allLines.Length; i++)
                    {
                        if (allLines[i].Contains(dateStr))
                        {
                            dateExist = true;
                            for (int j = 0; j < windowSize + 1; j++)
                            {
                                //j+1, так как первая строка - заголовок
                                input[j + 1] = allLines[i - windowSize + j];
                            }

                        }
                    }
                    string rawInputLine = input[input.Length - 1];


                    string stateInString = "";
                    if (dateExist)
                    {
                        // ГЕНЕРАЦИЯ МАТРИЦЫ INPUT
                        input = prepareDataset(input, algorithms[0].getValueByName("drop_columns"), Convert.ToBoolean(H.getValueByName("normalize")));
                        // ВЫЗОВ getPrediction(input)
                        var committeeResponse = getPrediction(input);


                        int closeIndexInNormalizedDataset = Convert.ToInt32(H.getValueByName("predicted_column_index"));
                        string featureName = input[0].Split(';')[closeIndexInNormalizedDataset];
                        int closeIndexInRawDataset = -1;
                        for (int i = 0; i < allLines[0].Split(';').Length; i++)
                            if (allLines[0].Split(';')[i] == featureName)
                                closeIndexInRawDataset = i;
                        closeValueStr = rawInputLine.Split(';')[closeIndexInRawDataset];
                        closeValue = Convert.ToDouble(closeValueStr.Replace('.', ','));
                        closeValueHistory.Add(closeValue);
                        if (!report[0].Contains(input[0]))
                            report[0] += input[0];
                        presentLine = input[input.Length - 1];

                        //обновление состояния
                        DMS.setActualState(getStateStr());
                        //Отправка запроса к системе принятия решений
                        action = getDecision(committeeResponse);

                        if (action == "buy")
                        {
                            if (closeValue != 0)
                            {
                                if (deposit2 > closeValue)
                                {
                                    deposit1 = deposit1 + 1;
                                    deposit2 = deposit2 - closeValue;
                                }
                                else
                                {
                                    log("Основной депозит исчерпан! Не возможно купить базовую валюту.");
                                }
                            }
                            else
                            {
                                log("closeValue почему-то равно нулю!");
                            }
                        }
                        if (action == "sell")
                        {
                            if (closeValue != 0)
                            {
                                if (deposit1 >= 1)
                                {
                                    deposit1 = deposit1 - 1;
                                    deposit2 = deposit2 + closeValue;
                                }
                                else
                                {
                                    log("Депозит базовой валюты исчерпан (состояние выхода с рынка). Не возможно продать базовую валюту.");
                                }
                            }
                            else
                            {
                                log("closeValue почему-то равно нулю!", Color.Red);
                            }
                        }

                    }
                    else
                    {
                        action = "dateDoesn'tExist";
                        //   log("дата " + dateStr + " не найдена в файле " + rawDatasetFilePath);
                    }
                    deposit1History.Add(deposit1);
                    deposit2History.Add(deposit2);

                    //обновление состояния
                    DMS.setActualState(getStateStr());
                    //вознаграждение системы принятия решений
                    double reward = 0;
                    if (deposit1History.Count > 1)
                    {
                        reward = (closeValue * (deposit1History[deposit1History.Count - 1] - deposit1History[deposit1History.Count - 2])) + (deposit2History[deposit2History.Count - 1] - deposit2History[deposit2History.Count - 2]);
                        // reward =  (deposit2History[deposit2History.Count - 1] - deposit2History[deposit2History.Count - 2]);

                    }
                    DMS.setR(reward);
                    ////////////////////////////////////////

                    actionHistory.Add(action);
                    //print committee response
                    string comRespStr = "committee response: ";
                    for (int i = 0; i < committeeResponse.Length; i++)
                        comRespStr += " [" + committeeResponse[i] + "]; ";
                    string committeeResponseReportLine = "";
                    for (int i = 0; i < committeeResponse.Length; i++)
                        committeeResponseReportLine += committeeResponse[i] + ";";
                    log(comRespStr);
                    //  log("date: " + date1.ToString());
                    log("deposit1: " + deposit1.ToString());
                    log("deposit2: " + deposit2.ToString());
                    //  log("action: " + action);
                    log("reward: " + reward.ToString());
                    //  log("closeValue: " + closeValue.ToString());
                    //   log("presentLine: " + presentLine);

                    reportLine += date1.ToString() + ';' + deposit1.ToString() + ';' + deposit2.ToString() + ';' + action + ';' + reward.ToString() + ';' + closeValue.ToString() + ';' + committeeResponseReportLine + presentLine;
                    report.Add(reportLine);

                    if (period == "day")
                        date1 = date1.AddDays(1);
                    if (period == "hour")
                        date1 = date1.AddHours(1);
                }
            }
            else
                log("date1>date2 !");
            //выход с рынка
            deposit2 = deposit2 + (closeValue * deposit1);
            deposit1 = 0;
            action = "exit";
            string reportLineExit = date1.ToString() + ';' + deposit1.ToString() + ';' + deposit2.ToString() + ';' + action + ';' + closeValue.ToString() + ';';
            report.Add(reportLineExit);

            //завершение всех операций и запись отчёта
            File.WriteAllLines(path_prefix + expertName + "\\report.csv", report);

            H.setValueByName("expert_target_function", deposit2.ToString().Replace(',', '.'));
            return "expert has been tested";
        }
        public void Add(Algorithm algorithm)
        {
            Directory.CreateDirectory(path_prefix + expertName + "\\" + algorithm.modelName + "\\");
            algorithm.h.setValueByName("save_folder", path_prefix + expertName + "\\" + algorithm.modelName + "\\");
            algorithm.h.setValueByName("json_file_path", path_prefix + expertName + "\\" + algorithm.modelName + "\\json.txt");
            algorithm.h.setValueByName("predictions_file_path", path_prefix + expertName + "\\" + algorithm.modelName + "\\predictions.txt");

            algorithms.Add(algorithm);
        }

        //метод делающий из временного ряда (*.csv) датасет, пригодный для передачи в train.py скрипт
        //возвращает путь к файлу датасета
        public string savePreparedDataset(string inputFile, string dropColumn, bool normalize)
        {
            File.WriteAllLines(inputFile.Replace(".txt", "-dataset.txt"), prepareDataset(inputFile, dropColumn, normalize));
            return inputFile.Replace(".txt", "-dataset.txt");
        }
        public string[] prepareDataset(string inputFile, string dropColumn, bool normalize)
        {
            var allLines = skipEmptyLines(File.ReadAllLines(inputFile));
            return prepareDataset(allLines, dropColumn, normalize);
        }

        public string[] prepareDataset(string[] allLines, string dropColumn, bool normalize)
        {
            List<int> colDropInd;

            // int windowSie = Convert.ToInt16(algorithms[a].getValueByName("windowSize"));
            string[] featuresNames = allLines[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //drop column
            var dropColumnNames = dropColumn.Split(';');
            colDropInd = new List<int>();
            for (int c = 0; c < featuresNames.Length; c++)
            {
                for (int cind = 0; cind < dropColumnNames.Length; cind++)
                {
                    if (featuresNames[c] == dropColumnNames[cind])
                    {
                        colDropInd.Add(c);
                    }
                }
            }
            var firstDataRow = allLines[1].Split(';');
            for (int c = 0; c < featuresNames.Length; c++)
            {
                bool dropIt = false;
                for (int d = 0; c < colDropInd.Count; c++)
                {
                    if (colDropInd[d] == c)
                    {
                        dropIt = true;
                    }
                }

                if (!dropIt)
                {
                    //попытка преобразовать предиктор в тип double
                    try
                    {
                        var someDouble = Convert.ToDouble(firstDataRow[c]);
                    }
                    //в случае ошибки - весь столбец дропается
                    catch
                    {
                        //  log("столбец " + featuresNames[c] + " удалён");
                        colDropInd.Add(c);
                    }
                }
            }


            dataset = new double[allLines.Length - 1, featuresNames.Length - colDropInd.Count];
            //формирование матрицы dataset
            for (int i = 1; i < allLines.Length; i++)
            {
                string[] features = allLines[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                int shift = 0;
                for (int k = 0; k < features.Length; k++)
                {
                    bool dropIt = false;
                    for (int c = 0; c < colDropInd.Count; c++)
                    {
                        if (colDropInd[c] == k)
                        {
                            dropIt = true;
                            shift++;
                        }
                    }

                    if (!dropIt)
                        try
                        {
                            dataset[i - 1, k - shift] = Convert.ToDouble(features[k]);
                        }
                        catch
                        {
                            try
                            {
                                dataset[i - 1, k - shift] = Convert.ToDouble(features[k]);
                            }
                            catch
                            {

                                dataset[i - 1, k - shift] = 0;
                            }
                        }
                }
            }
            if (normalize)
            {

                ////////////////////////////////////////////////
                ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
                ////////////////////////////////////////////////
                dataset1 = normalize2(dataset);



                /////////////////////////////////
                //////     СГЛАЖИВАНИЕ    ///////
                /////////////////////////////////
                dataset2 = levelOff2(dataset1);

            }
            else
            {
                dataset2 = dataset;
            }
            string[] toWrite = new string[dataset2.GetLength(0) + 1];
            for (int k = 0; k < featuresNames.Length; k++)
            {
                bool dropIt = false;
                for (int c = 0; c < colDropInd.Count; c++)
                {
                    if (colDropInd[c] == k)
                    {
                        dropIt = true;
                    }
                }

                if (!dropIt)
                    toWrite[0] += featuresNames[k] + ';';
            }
            toWrite[0] = toWrite[0].Remove(toWrite[0].Length - 1, 1);


            for (int i = 0; i < dataset2.GetLength(0); i++)
            {
                for (int k = 0; k < dataset2.GetLength(1); k++)
                {
                    if ((dataset2[i, k]).ToString().Replace(',', '.').Length > 8)
                        toWrite[i + 1] += (dataset2[i, k]).ToString().Replace(',', '.').Substring(0, 8) + ';';
                    else
                        toWrite[i + 1] += (dataset2[i, k]).ToString().Replace(',', '.') + ';';
                }
                toWrite[i + 1] = toWrite[i + 1].Remove(toWrite[i + 1].Length - 1, 1);
            }

            return toWrite;
        }



        public void synchronizeHyperparameters()
        {
            //передача параметров эксперта в базы алгоритмов
            for (int i = 0; i < H.nodes.Count; i++)
            {
                if (H.nodes[i].parentID == 0 && H.nodes[i].name() != "committee")
                {

                    for (int j = 0; j < algorithms.Count; j++)
                    {
                        if (algorithms[j].h.getNodeByName(H.nodes[i].name()).Count != 0)
                            algorithms[j].h.deleteBranch(algorithms[j].h.getNodeByName(H.nodes[i].name())[0].ID);
                        algorithms[j].h.addNode(H.nodes[i], 0);
                    }
                }
            }

            //приращение баз алгоритмов к общей базе эксперта

            //  СПИСОК ВЕТВЕЙ АЛГОРИТМОВ
            List<Node> toReWrite = H.getNodesByparentID(committeeNodeID);
            //удаление старых записей
            for (int i = 0; i < toReWrite.Count; i++)
                H.deleteBranch(toReWrite[i].ID);

            //приращение новых записей к узлу  "committee"
            for (int i = 0; i < algorithms.Count; i++)
            {
                H.addBranch(algorithms[i].h, algorithms[i].name, committeeNodeID);
            }
        }
        public static Expert Open(string expertName, Form1 form1)
        {
            Expert expert = new Expert(expertName, form1, true);

            foreach (string expertFolder in Directory.GetDirectories(expert.path_prefix))
            {
                if (Path.GetFileName(expertFolder) == expertName)
                {
                    expert.H = new Hyperparameters(File.ReadAllText(expertFolder + "\\json.txt", System.Text.Encoding.Default), form1);
                    expert.committeeNodeID = expert.H.getNodeByName("committee")[0].ID;
                    var algorithmBranches = expert.H.getNodesByparentID(expert.committeeNodeID);
                    foreach (Node algorithmBranch in algorithmBranches)
                    {
                        // Type t = Type.GetType("Namespace." + algorithmBranch.name());
                        //  object cc = Activator.CreateInstance(t);

                        if (algorithmBranch.name() == "LSTM_1")
                            expert.algorithms.Add(new LSTM_1(form1, "LSTM_1"));
                        if (algorithmBranch.name() == "LSTM_2")
                            expert.algorithms.Add(new LSTM_2(form1, "LSTM_2"));
                        if (algorithmBranch.name() == "ANN_1")
                            expert.algorithms.Add(new ANN_1(form1, "ANN_1"));

                        expert.algorithms[expert.algorithms.Count - 1].h = new Hyperparameters(expert.H.toJSON(algorithmBranch.ID), form1);
                        expert.algorithms[expert.algorithms.Count - 1].modelName = expert.algorithms[expert.algorithms.Count - 1].h.getValueByName("model_name");
                    }
                }
            }
            return expert;
        }
        public string Save()
        {
            string path = path_prefix + expertName;
            return Save(path);
        }
        /// <summary>
        ///  path = path_prefix + expertName
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string Save(string path)
        {
            path += "\\json.txt";
            File.WriteAllText(path, H.toJSON(0), System.Text.Encoding.Default);
            foreach (Algorithm algorithm in algorithms)
                algorithm.Save();
            return path;
        }
        /*  public double stdDev;
          public double accuracy;

          public void getAccAndStdDev()
          {
              var algorithmBranches = H.getNodesByparentID(committeeNodeID);
              foreach (Node algorithmBranch in algorithmBranches)
              {
                  predictionsCSV = Expert.skipEmptyLines(predictionsCSV);
                  double sqrtSum = 0;
                  int rightCount = 0;
                  int leftCount = 0;
                  int inc = 0;
                  for (int i = 1; i < predictionsCSV.Length - 1; i++)
                  {
                      var features = predictionsCSV[i].Split(';');

                      double predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 1].Replace('.', ','));
                      double realValue = Convert.ToDouble(predictionsCSV[i + 1].Split(';')[Convert.ToInt16(h.getValueByName("predicted_column_index"))].Replace('.', ','));

                      if (realValue > 0.5 && predictedValue > 0.5)
                      { rightCount++; }
                      else
                            if (realValue < 0.5 && predictedValue < 0.5)
                      { rightCount++; }
                      else
                          if (realValue > 0.5 && predictedValue < 0.5)
                      { leftCount++; }
                      else
                      if (realValue < 0.5 && predictedValue > 0.5)
                      { leftCount++; }
                      sqrtSum += (realValue - predictedValue) * (realValue - predictedValue);
                      inc++;

                  }
                  accuracy = Convert.ToDouble(rightCount) / Convert.ToDouble(rightCount + leftCount) * 100;
                  stdDev = sqrtSum / inc;
                  log("accuracy = " + accuracy.ToString() + " %");
                  log("stdDev = " + Math.Sqrt(stdDev).ToString());
                  h.setValueByName("accuracy", accuracy.ToString());
              }
          }  */

        public Hyperparameters h()
        { return algorithms[0].h; }

        [NonSerializedAttribute]
        private Form1 form1;

        public void log(String s, System.Drawing.Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }
        public void log(String s)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, System.Drawing.Color.White);
        }

        private double[,] normalize1(double[,] inputDataset)
        {

            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            //первая строка датасета удаляется из-за нормализации типа i/(i-1)
            double[,] normalizedDataset2 = new double[inputDataset.GetLength(0) - 1, inputDataset.GetLength(1)];

            //заполнение строки previousLine для первой итерации алгоритма   ____i/(i-1)__
            double[] previousLine = new double[inputDataset.GetLength(1)];
            for (int j = 0; j < inputDataset.GetLength(1); j++)
                previousLine[j] = inputDataset[0, j];


            //___________i/(i-1)__________________
            for (int i = 0; i < inputDataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    if (previousLine[k] != 0)
                        normalizedDataset2[i, k] = Convert.ToDouble(inputDataset[i + 1, k]) / Convert.ToDouble(previousLine[k]);
                    else
                        normalizedDataset2[i, k] = 0;
                }
                for (int j = 0; j < inputDataset.GetLength(1); j++)
                    previousLine[j] = inputDataset[i + 1, j];
            }
            return normalizedDataset2;
        }

        private double[,] normalize2(double[,] inputDataset)
        {

            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            //первая строка датасета удаляется из-за нормализации типа i/(i-1)
            double[,] normalizedDataset2 = new double[inputDataset.GetLength(0) - 1, inputDataset.GetLength(1)];

            //заполнение строки previousLine для первой итерации алгоритма   ____i/(i-1)__
            double[] previousLine = new double[inputDataset.GetLength(1)];
            for (int j = 0; j < inputDataset.GetLength(1); j++)
                previousLine[j] = inputDataset[0, j];


            //___________i-(i-1)__________________
            for (int i = 0; i < inputDataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    if (previousLine[k] != 0)
                        normalizedDataset2[i, k] = Convert.ToDouble(inputDataset[i + 1, k]) - Convert.ToDouble(previousLine[k]);
                    else
                        normalizedDataset2[i, k] = 0;
                }
                for (int j = 0; j < inputDataset.GetLength(1); j++)
                    previousLine[j] = inputDataset[i + 1, j];
            }
            return normalizedDataset2;
        }

        private double[,] levelOff1(double[,] inputDataset)
        {
            double[,] levelOffDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            for (int i = 0; i < inputDataset.GetLength(0); i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    //приведение его к 0.5 - среднему делением на 2
                    levelOffDataset[i, k] = levelOffDataset[i, k] / 2;
                    //для увеличение стандартного отклонения сначала вычислим имеющееся i-ое отклонение, приведя к 0 - среднему
                    levelOffDataset[i, k] = levelOffDataset[i, k] - 0.5;
                    //а затем отмасштабируем
                    //levelOffDataset[i, k] = levelOffDataset[i, k] * (1 / (Math.Abs(levelOffDataset[i, k] + 0.5)));
                    //вернём к 0.5 - среднему
                    levelOffDataset[i, k] = levelOffDataset[i, k] + 0.5;
                    //и подровняем выбросы
                    if (levelOffDataset[i, k] > 1)
                    {
                        levelOffDataset[i, k] = 1;
                    }
                    else if (levelOffDataset[i, k] < 0)
                    {
                        levelOffDataset[i, k] = 0;
                    }
                }
            }
            return levelOffDataset;
        }

        private double[,] levelOff2(double[,] inputDataset)
        {
            double[,] levelOffDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            for (int i = 0; i < inputDataset.GetLength(0); i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    //масштабирование
                    //  levelOffDataset[i, k] = levelOffDataset[i, k] * (1 / (Math.Abs(levelOffDataset[i, k] + 0.5)));
                    //0.5 - среднее
                    levelOffDataset[i, k] = inputDataset[i, k] + 0.5;
                    //выбросы
                    if (levelOffDataset[i, k] > 1)
                    {
                        levelOffDataset[i, k] = 1;
                    }
                    else if (levelOffDataset[i, k] < 0)
                    {
                        levelOffDataset[i, k] = 0;
                    }
                }
            }
            return levelOffDataset;
        }

        private double[,] scale(double[,] inputDataset)
        {
            //масштабирование 

            double[,] scaleedDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            var subt = new double[dataset.GetLength(1)];
            for (int j = 0; j < dataset.GetLength(1); j++)
                subt[j] = dataset[0, j];
            for (int i = 0; i < dataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    if (subt[k] > dataset[i + 1, k])
                    { subt[k] = dataset[i + 1, k]; }
                }
            }
            var div = new double[dataset.GetLength(1)];
            for (int j = 0; j < dataset.GetLength(1); j++)
                div[j] = dataset[0, j];
            for (int i = 0; i < dataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    if (div[k] < (dataset[i + 1, k] - subt[k]))
                    { div[k] = (dataset[i + 1, k] - subt[k]); }
                }
            }

            for (int i = 0; i < dataset.GetLength(0); i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    scaleedDataset[i, k] = (dataset[i, k] - subt[k]) / div[k];
                }
            }
            return scaleedDataset;
        }

        public static string[] skipEmptyLines(string[] allLines)
        {

            //пропуск пустых строк
            List<string> filledLines = new List<string>();
            foreach (string line in allLines)
            {
                if (line != "")
                {
                    filledLines.Add(line);
                }
            }
            var res = new string[filledLines.Count];
            for (int i = 0; i < filledLines.Count; i++)
            {
                res[i] = filledLines[i];
            }

            return res;
        }
    }
}

