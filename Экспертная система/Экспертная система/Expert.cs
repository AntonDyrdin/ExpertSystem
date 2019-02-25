using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
        public double deposit2 = 500;

        public List<double> deposit1History;
        public List<double> deposit2History;
        public List<double> closeValueHistory;
        public List<string> actionHistory;
        public double[] committeeResponse;
        public string presentLine;
        public List<string> report;
        public Expert(string expertName, Form1 form1)
        {
            this.form1 = form1;
            path_prefix = form1.pathPrefix;
            H = new Hyperparameters(form1, expertName);
            algorithms = new List<Algorithm>();
            Directory.CreateDirectory(form1.pathPrefix + expertName);
            committeeNodeID = H.add("name:committee");
            this.expertName = expertName;
            report = new List<string>();

        }

        public void trainAllAlgorithms(bool deleteLowAccModels)
        {
            var trainAllAlgorithmsTask = Task.Factory.StartNew(() =>
            {

                foreach (Algorithm algorithm in algorithms)
                {
                    var trainTask = algorithm.train();
                }
            });

            trainAllAlgorithmsTask.Wait();
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
            var getPredictionParentTask = Task.Factory.StartNew(() =>
            {
                committeeResponse = new double[algorithms.Count];
                for (int i = 0; i < algorithms.Count; i++)
                {
                    var getPredictionTask = Task.Factory.StartNew(() =>
                    {
                        committeeResponse[i] = algorithms[i].getPrediction(input);

                        if (committeeResponse[i] > 0.5)
                            committeeResponse[i] = 1;
                        if (committeeResponse[i] < 0.5)
                            committeeResponse[i] = 0;
                    }, TaskCreationOptions.AttachedToParent);
                }
            });
            getPredictionParentTask.Wait();
            return committeeResponse;
        }
        //возвращает  действие, о котором было принято решение
        public string getDecision(double[] committeeResponse)
        {
            //Среднее арифметическое прогнозов алгоритмов
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
            return "nothing";
        }
        public string test(DateTime date1, DateTime date2, string rawDatasetFilePath)
        {
            closeValueHistory = new List<double>();
            deposit1History = new List<double>();
            deposit2History = new List<double>();
            closeValueHistory = new List<double>();
            actionHistory = new List<string>();
            report = new List<string>();
            presentLine = "";
            double closeValue = 0;
            string action = "";
            string reportHead = "<presentDate>;<deposit1>;<deposit2>;<action>;<closeValue>;";
            foreach (Algorithm algorithm in algorithms)
                reportHead += "<" + algorithm.modelName + ">;";
            report.Add(reportHead);
            if (date1 < date2)
            {
                foreach (Algorithm algorithm in algorithms)
                    algorithm.runGetPredictionScript();
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

                    if (dateExist)
                    {
                        input = prepareDataset(input, algorithms[0].getValueByName("drop_columns"));
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
                                    log("deposit1 = " + deposit1.ToString());
                                    log("deposit2 = " + deposit2.ToString());
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
                                    log("deposit1 = " + deposit1.ToString());
                                    log("deposit2 = " + deposit2.ToString());
                                }
                            }
                            else
                            {
                                log("closeValue почему-то равно нулю!");
                            }
                        }

                    }
                    else
                    {
                        action = "dateDoesn'tExist";
                        log("дата " + dateStr + " не найдена в файле " + rawDatasetFilePath);
                    }
                    actionHistory.Add(action);
                    //print committee response
                    string comRespStr = "committee response: ";
                    for (int i = 0; i < committeeResponse.Length; i++)
                        comRespStr += " [" + committeeResponse[i] + "]; ";
                    string committeeResponseReportLine = "";
                    for (int i = 0; i < committeeResponse.Length; i++)
                        committeeResponseReportLine += committeeResponse[i] + ";";
                    log(comRespStr);
                    log("date: " + date1.ToString());
                    log("deposit1: " + deposit1.ToString());
                    log("deposit2: " + deposit2.ToString());
                    log("action: " + action);
                    log("closeValue: " + closeValue.ToString());
                    log("presentLine: " + presentLine);

                    reportLine += date1.ToString() + ';' + deposit1.ToString() + ';' + deposit2.ToString() + ';' + action + ';' + closeValue.ToString() + ';' + committeeResponseReportLine + presentLine;
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
        public string savePreparedDataset(string inputFile, string dropColumn)
        {
            File.WriteAllLines(inputFile.Replace(".txt", "-dataset.txt"), prepareDataset(inputFile, dropColumn));
            return inputFile.Replace(".txt", "-dataset.txt");
        }
        public string[] prepareDataset(string inputFile, string dropColumn)
        {
            var allLines = skipEmptyLines(File.ReadAllLines(inputFile));
            return prepareDataset(allLines, dropColumn);
        }

        public string[] prepareDataset(string[] allLines, string dropColumn)
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
                        log("столбец " + featuresNames[c] + " удалён");
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


            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            dataset1 = normalize2(dataset);



            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            dataset2 = levelOff2(dataset1);


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
            var toReWrite = H.getNodesByparentID(committeeNodeID);
            //удаление старых записей
            for (int i = 0; i < toReWrite.Count; i++)
                H.deleteBranch(toReWrite[i].ID);

            //приращение новых к узлу  "committee"
            for (int i = 0; i < algorithms.Count; i++)
            {
                H.addBranch(algorithms[i].h, algorithms[i].name, committeeNodeID);
            }
        }
        public void Open()
        {
            foreach (string expertFolder in Directory.GetDirectories(path_prefix))
            {
                if (Path.GetFileName(expertFolder) == expertName)
                {
                    H = new Hyperparameters(File.ReadAllText(expertFolder + "\\json.txt", System.Text.Encoding.Default), form1);
                    committeeNodeID = H.getNodeByName("committee")[0].ID;
                    var algorithmBranches = H.getNodesByparentID(committeeNodeID);
                    foreach (Node algorithmBranch in algorithmBranches)
                    {
                        if (algorithmBranch.name() == "LSTM_1")
                            algorithms.Add(new LSTM_1(form1, "LSTM_1"));
                        if (algorithmBranch.name() == "ANN_1")
                            algorithms.Add(new ANN_1(form1, "ANN_1"));
                        algorithms[algorithms.Count - 1].Open(new Hyperparameters(H.toJSON(algorithmBranch.ID), form1));
                    }
                }
            }
        }
        public string Save()
        {
            string path = path_prefix + expertName + "\\json.txt";
            File.WriteAllText(path, H.toJSON(0), System.Text.Encoding.Default);
            foreach (Algorithm algorithm in algorithms)
                algorithm.Save();
            return path;
        }
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
/* double[,] csvToDoubleArray(string[] csv)
         { }
         string[] doubleArrayToCSV(double[,] doubleArray)
         { }*/

/*
         inputVector = new double[allLines.Length - windowSie, windowSie, allLines[1].Split(',').Length];
         //формирование входного вектора начинается с позиции последней строки первого окна
         //c учётом того, что первая строка - шапка таблицы!!!!!!!!!!!!!!!!!!!!!

         //запись в БД
         // int inputVectorID = Algorithms[a].h.add("name:inputVector,count:" + (allLines.Length - windowSie).ToString());

         for (int i = windowSie; i < allLines.Length; i++)
         {   //запись в БД
             //  int windowID = Algorithms[a].h.addByParentId(inputVectorID, "name:" + (i - windowSie + 1).ToString() + "stWindow,count:"+ windowSie);

             //затем от этой позиции заполняется первое окно
             for (int j = 0; j < windowSie; j++)
             {     //запись в БД
                   // int lineID = Algorithms[a].h.addByParentId(windowID, "name:" + (j + 1).ToString() + "stLine,count:" + allLines[1].Split(',').Length );
                 string[] features = allLines[i - windowSie + 1 + j].Split(';');
                 for (int k = 0; k < features.Length; k++)
                 {   //запись в БД
                     // Algorithms[a].h.addLeafByParentId(lineID, featuresNames[k] + ":" + features[k]);
                     try
                     {
                         inputVector[i - windowSie, j, k] = Convert.ToDouble(features[k]);
                     }
                     catch
                     {
                         try
                         {
                             inputVector[i - windowSie, j, k] = Convert.ToDouble(features[k].Replace('.', ','));
                         }
                         catch
                         {

                             inputVector[i - windowSie, j, k] = 0;
                         }
                     }
                 }
             }
         }   */
