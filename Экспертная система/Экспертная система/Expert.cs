using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace Экспертная_система
{
    [Serializable]
    public class Expert
    {
        public int ENV;
        public int DEV = 0;
        public int TEST = 0;
        public int REAL = 1;

        public string expertName;
        public double[,] dataset;
        public double[,] dataset1;
        public double[,] dataset2;
        public double[,] dataset3;
        private string period = "minute";
        //критерий оптимальности
        public double target_function;
        public Hyperparameters H;
        //список алгоритмов(комитет)
        public List<Algorithm> algorithms;
        public int committeeNodeID;
        public string path_prefix;
        public string reportPath;

        // БАЗОВАЯ  валюта
        //(валюта, которая покупается и продаётся)
        //стоит на ПЕРВОМ месте в валютной паре
        //по команде BUY значение этого депозита должно расти  на количество покупаемых единиц
        //по команде SELL значение этого депозита должно уменьшаться на количество покупаемых единиц
        public const double deposit1StartValue = 0;
        public double deposit1 = deposit1StartValue;

        //КОТИРУЕМАЯ валюта 
        //валюта входа и выхода (основная)
        //стоит на ВТОРОМ месте в валютной паре
        //по команде BUY значение этого депозита должно уменьшаться  пропорционально цене базовой вылюты
        //по команде SELL значение этого депозита должно расти   пропорционально цене базовой вылюты
        public const double deposit2StartValue = 100;
        public double deposit2 = deposit2StartValue;

        public double lot;

        int w1;
        int w2;
        double take_pofit;
        double drawdown;

        public double purchase_limit_amount;
        public double purchase_limit_amount_left;
        public int purchase_limit_interval;
        bool purchase_limit_timer_enabled = false;
        DateTime purchase_limit_timer_start;

        public List<double> deposit1History;
        public List<double> deposit2History;
        public List<double> closeValueHistory;
        public List<string> actionHistory;
        public List<double[]> committeeResponseHistory;
        public List<double> MAVG_bid_history;
        public double[] committeeResponse;
        public string presentLine;
        public List<string> report;
        public DecisionMakingSystem DMS;

        public double lastKnownValue = 0;
        public string lastKnownValueStr = "";
        public List<double> lastKnownValueHistory;

        public double min_bid = 0;
        public double max_bid = 0;

        private bool multiThreadPrediction = false;

        double Purchase = 0;
        public string testExmo(DateTime date1, DateTime date2, string rawDatasetFilePath)
        {
            ENV = TEST;
            ////////////////////////////////////////////////
            /////// ПАРАМЕТРЫ СОСТОЯНИЯ СПР ////////////////
            /*  for (int i = 0; i < algorithms.Count; i++)
                  DMS.addParameter("A[" + i.ToString() + "]", "0,1");
              // состояние депозитов 1 - баланс положительный, 0 - баланс нулевой
              DMS.addParameter("DEP1", "0,1");
              DMS.addParameter("DEP2", "0,1");
              //превышение цены покупки
              DMS.addParameter("HigherThenPurchase", "0,1");

              DMS.defaultActions.Add(new DMSAction("buy"));
              DMS.defaultActions.Add(new DMSAction("sell"));
              DMS.defaultActions.Add(new DMSAction("nothing"));
              DMS.generateStates();*/

            w1 = int.Parse(H.getValueByName("w1"));
            w2 = int.Parse(H.getValueByName("w2"));
            take_pofit = double.Parse(H.getValueByName("take_pofit"));
            drawdown = double.Parse(H.getValueByName("drawdown"));

            deposit1 = deposit1StartValue;
            deposit2 = deposit2StartValue;

            purchase_limit_amount = int.Parse(H.getValueByName("purchase_limit_amount"));
            purchase_limit_amount_left = purchase_limit_amount;
            purchase_limit_interval = int.Parse(H.getValueByName("purchase_limit_interval"));
            purchase_limit_timer_enabled = false;
            purchase_limit_timer_start = date1;

            lot = double.Parse(H.getValueByName("lot"));
            int windowSize = int.Parse(H.getValueByName("w1")) + int.Parse(H.getValueByName("w2"));
            List<double> positions = new List<double>();

            Array a = new double[3];

            MAVG_bid_history = new List<double>();
            committeeResponseHistory = new List<double[]>();
            closeValueHistory = new List<double>();
            deposit1History = new List<double>();
            deposit2History = new List<double>();
            actionHistory = new List<string>();
            report = new List<string>();
            presentLine = "";
            double closeValue = 0;
            string action = "";
            string reportHead = "<presentDate>;<deposit1>;<deposit2>;<action>;<reward>;<ask>;<bid>;";
            double reward = 0;

            report.Add(reportHead);

            var allLines = File.ReadAllLines(rawDatasetFilePath);

            int last_line_index = 1;

            List<string> input = new List<string>();

            //копирование заголовка
            input.Add(allLines[0]);

            if (date1 < date2)
            {
                while (date1 < date2)
                {
                    string reportLine = "";

                    bool dateExist = false;
                    string dateStr;
                    dateStr = date1.ToString("dd.MM.yyyy H:mm");

                    for (int i = last_line_index; i < allLines.Length; i++)
                    {
                        string date;
                        if (date1.Hour < 10)
                            date = allLines[i].Substring(0, 15);
                        else
                            date = allLines[i].Substring(0, 16);

                        if (date == dateStr)
                        {
                            dateExist = true;
                            last_line_index = i;

                            if (input.Count == 1)
                            {
                                for (int k = 0; k < w1 + w2; k++)
                                    input.Add(allLines[i - w1 + w2 + k]);
                            }
                            else
                            {
                                input.Add(allLines[i]);
                                input.RemoveAt(0);
                            }
                            break;
                        }
                    }

                    if (dateExist)
                    {
                        double ask_top = double.Parse(input[input.Count - 1].Split(';')[4]);
                        double bid_top = double.Parse(input[input.Count - 1].Split(';')[1]);
                        closeValue = bid_top;
                        action = getDecision(input.ToArray());
                        if (action == "error")
                        {
                            log("action = error", Color.Red);
                        }
                        if (action == "buy")
                        {
                            if (deposit2 > ask_top * lot)
                            {
                                buy(ask_top, date1);

                                positions.Add(ask_top);
                            }
                            else
                            {
                                action += " (fail USD balance is too low)";
                            }
                        }

                        for (int i = 0; i < positions.Count; i++)
                        {
                            if (bid_top - positions[i] > take_pofit)
                            {
                                action = "sell";

                                if (deposit1 >= 0)
                                {
                                    sell(bid_top);
                                }
                                else
                                {
                                    action += " (fail BTC balance is too low)";
                                }

                                positions.RemoveAt(i);
                                i--;

                                if (positions.Count == 0)
                                {
                                    break;
                                }
                            }
                        }

                        committeeResponseHistory.Add(committeeResponse);
                        closeValueHistory.Add(closeValue);
                        deposit1History.Add(deposit1);
                        deposit2History.Add(deposit2);
                    }
                    else
                    {
                        action = "date doesn't exist";
                        //   log("дата " + dateStr + " не найдена в файле " + rawDatasetFilePath);
                    }

                    actionHistory.Add(action);

                    //    log(comRespStr);
                    //  log("date: " + date1.ToString());
                    //    log("deposit1: " + deposit1.ToString());
                    //    log("deposit2: " + deposit2.ToString());
                    //  log("action: " + action);
                    //    log("reward: " + reward.ToString());
                    //  log("closeValue: " + closeValue.ToString());
                    //   log("presentLine: " + presentLine);

                    reportLine += date1.ToString() + ';' + deposit1.ToString() + ';' + deposit2.ToString() + ';' + action + ';' + reward.ToString() + ';' + closeValue.ToString() + ';' + presentLine;
                    report.Add(reportLine);

                    if (period == "day")
                        date1 = date1.AddDays(1);
                    if (period == "hour")
                        date1 = date1.AddHours(1);
                    if (period == "minute")
                        date1 = date1.AddMinutes(1);
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

            // запись отчёта
            reportPath = H.getValueByName("report_path");
            if (reportPath == "" || reportPath == null)
            {
                reportPath = path_prefix + expertName;
            }
            File.WriteAllLines(reportPath + "\\report.csv", report);


            H.setValueByName("expert_target_function", deposit2.ToString().Replace(',', '.'));
            return "expert has been tested";
        }

        public void buildNew(string expertName, MainForm form1)
        {
            this.form1 = form1;
            path_prefix = form1.pathPrefix;
            H = new Hyperparameters(form1, expertName);
            algorithms = new List<Algorithm>();
            Directory.CreateDirectory(form1.pathPrefix + expertName);
            committeeNodeID = H.add("name:committee,value: ");
            this.expertName = expertName;
            report = new List<string>();
            H.setValueByName("report_path", path_prefix + expertName + '\\');
            DMS = new DecisionMakingSystem(form1);
            /* H.addVariable(0, "epsilon", 0.001, 0.99, 0.01, 0.05);
             H.addVariable(0, "alpha", 0.001, 0.99, 0.01, 0.9);
             H.addVariable(0, "gamma", 0.001, 0.99, 0.01, 0.5);*/
        }
        public Expert(string expertName, MainForm form1)
        {
            foreach (string expertFolder in Directory.GetDirectories(form1.pathPrefix))
            {
                if (Path.GetFileName(expertFolder) == expertName)
                {  //В ДАННОМ КОНСТРУКТОРЕ, ПРИ СОЗДАНИИ НОВОГО ЭКЗЕМПЛЯРА КЛАССА Expert, ПАПКА С ТАКИМ ЖЕ ИМЕНЕМ БУДЕТ УДАЛЕНА
                    Directory.Delete(expertFolder, true);
                }
            }

            buildNew(expertName, form1);
        }
        public Expert(string expertName, MainForm form1, bool DoNotDeleteExpertFolder)
        {
            buildNew(expertName, form1);
        }



        //возвращает  действие, о котором было принято решение
        public double MAVG_bid = 0;
        public string getDecision(string[] input)
        {
            if (w1 == 0)
            {
                w1 = int.Parse(H.getValueByName("w1"));
                w2 = int.Parse(H.getValueByName("w2"));
                take_pofit = double.Parse(H.getValueByName("take_pofit"));
                drawdown = double.Parse(H.getValueByName("drawdown"));
            }
            if (input.Length != w1 + w2 + 1)
            {
                throw new Exception("input.Length != w1+w2+1");
            }

            MAVG_bid = 0;
            for (int i = 0; i < w1; i++)
            {
                MAVG_bid += double.Parse(input[1 + i].Split(';')[1]);
            }
            MAVG_bid = MAVG_bid / w1;
            MAVG_bid_history.Add(MAVG_bid);
            double current_ask = double.Parse(input[input.Length - 1].Split(';')[4]);
            // log("drawdown: " + (MAVG_bid - current_ask).ToString());
            //  log("MAVG_bid: " + MAVG_bid.ToString());

            if (MAVG_bid - current_ask > drawdown)
            {
                return "buy";
            }
            return "";
        }

        void buy_test(double ask_top)
        {
            if (deposit2 > 0)
            {
                deposit1 = deposit1 + lot - (lot * 0.002);
                deposit2 = deposit2 - (ask_top * lot);
                purchase_limit_amount_left -= ask_top * lot;
            }
        }
        void sell_test(double bid_top)
        {
            if (deposit1 > 0)
            {
                deposit1 = deposit1 - lot;
                deposit2 = deposit2 + (bid_top * lot) - ((bid_top * lot) * 0.002);
            }
        }

        void buy(double ask_top, DateTime current_time)
        {
            if (purchase_limit_amount_left - (ask_top * lot) > 0)
            {
                if (ENV == REAL)
                { }
                else
                    buy_test(ask_top);
            }
            else
            {
                if (purchase_limit_timer_enabled == false)
                {
                    purchase_limit_timer_start = current_time;
                    purchase_limit_timer_enabled = true;
                }

                if (purchase_limit_timer_start.AddMinutes(purchase_limit_interval) < current_time)
                {
                    purchase_limit_amount_left = purchase_limit_amount;
                    purchase_limit_timer_enabled = false;
                }
            }
        }
        void sell(double bid_top)
        {
            if (ENV == REAL)
            { }
            else
                sell_test(bid_top);
        }

        //метод делающий из временного ряда *.csv датасет, пригодный для передачи в train_script.py 
        //возвращает путь к файлу датасета
        public string savePreparedDataset(string inputFile, string dropColumn, bool normalize)
        {
            DateTime start = DateTime.Now;
            File.WriteAllLines(inputFile.Replace(".txt", "-dataset.txt"), prepareDataset(inputFile, dropColumn, normalize));
            TimeSpan offset = DateTime.Now - start;
            log("Обработка и сохранение файла " + inputFile.Replace(".txt", "-dataset.txt") + " - " + offset.Minutes.ToString() + ':' + offset.Seconds.ToString() + ':' + offset.Milliseconds.ToString());
            return inputFile.Replace(".txt", "-dataset.txt");
        }
        public string[] prepareDataset(string inputFile, string dropColumn, bool normalize)
        {
            var allLines = skipEmptyLines(File.ReadAllLines(inputFile));
            return prepareDataset(allLines, dropColumn, normalize);
        }

        public string[] prepareDataset(string[] allLines, string dropColumn, bool normalize)
        {
            List<int> colDropInd = new List<int>();
            List<int> badSplittersInd = new List<int>();

            // int windowSie = Convert.ToInt16(algorithms[a].getValueByName("windowSize"));
            string[] featuresNames = allLines[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (dropColumn != null)
            {     //drop column
                var dropColumnNames = dropColumn.Split(';');

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
            }
            var firstDataRow = allLines[1].Split(';');

            for (int c = 0; c < featuresNames.Length; c++)
            {
                bool dropIt = false;
                for (int d = 0; d < colDropInd.Count; d++)
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
                    //в случае ошибки - попробовать заменить разделитель
                    catch
                    {
                        try
                        {
                            if (firstDataRow[c].Contains("."))
                                firstDataRow[c] = firstDataRow[c];

                            var someDouble = Convert.ToDouble(firstDataRow[c]);
                            badSplittersInd.Add(c);
                        }
                        //в случае ошибки - весь столбец дропается
                        catch
                        {
                            log("столбец " + featuresNames[c] + " удалён");
                            colDropInd.Add(c);
                        }
                    }
                }
            }

            //Замена разделителей дробной и целой части
            if (badSplittersInd.Count > 0)
                for (int i = 0; i < allLines.Length; i++)
                {
                    string[] features = allLines[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int c = 0; c < badSplittersInd.Count; c++)
                    {
                        features[badSplittersInd[c]] = features[badSplittersInd[c]];
                    }
                    string s = "";
                    for (int k = 0; k < features.Length; k++)
                    {
                        s += features[k];
                        if (k != features.Length - 1)
                            s += ';';
                    }
                    allLines[i] = s;
                }
            ///////////////////////////////////////////////////////////////////////////////////////////////

            dataset = new double[allLines.Length - 1, featuresNames.Length - colDropInd.Count];

            int coreCount = Environment.ProcessorCount;
            int[,] intervals = new int[coreCount, 3];

            // распределение интервалов входной таблицы между ядрами
            for (int i = 0; i < coreCount; i++)
            {// [i, 0] - начало интервала i-ого ядра
                intervals[i, 0] = allLines.Length / coreCount * i + 1;
                // [i, 1] - конец интервала i-ого ядра
                intervals[i, 1] = allLines.Length / coreCount * (i + 1) + 1;
                // [i, 2] - как только одно из ядер приступает к обработке i-ого интервала это элемент матрицы становится равным единице 
                intervals[i, 2] = 0;
            }
            //поправка второго края последнего интервала
            intervals[coreCount - 1, 1] = allLines.Length;

            Task[] prepDatasetTasks = new Task[coreCount];
            //формирование матрицы dataset
            for (int core = 0; core < coreCount; core++)
            {
                prepDatasetTasks[core] = new Task(() =>
                  {
                      for (int core1 = 0; core1 < coreCount; core1++)
                      {
                          if (intervals[core1, 2] == 0)
                          {
                              intervals[core1, 2] = 1;
                              for (int i = intervals[core1, 0]; i < intervals[core1, 1]; i++)
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
                                              dataset[i - 1, k - shift] = double.Parse(features[k]);
                                          }
                                          catch
                                          {
                                              try
                                              {
                                                  if (features[k].Contains("."))
                                                      features[k] = features[k];

                                                  dataset[i - 1, k - shift] = double.Parse(features[k]);

                                              }
                                              catch (Exception e)
                                              {
                                                  log("Ошибка формирования датасета.", Color.Red);
                                                  log(e.Message, Color.Red);
                                              }
                                          }
                                  }
                              }
                          }
                      }
                  });
            }
            for (int core = 0; core < coreCount; core++)
            {
                prepDatasetTasks[core].Start();
            }
            for (int core = 0; core < coreCount; core++)
            {
                prepDatasetTasks[core].Wait();
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
                // dataset2 = levelOff2(dataset1);
                dataset2 = dataset1;
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
                    toWrite[i + 1] += String.Format("{0:0.########}", dataset2[i, k]).Replace(',', '.') + ';';
                }
                toWrite[i + 1] = toWrite[i + 1].Remove(toWrite[i + 1].Length - 1, 1);
            }

            return toWrite;
        }

        public void copyHyperparametersFromAlgorithmsToExpert()
        {
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

        public void copyExpertParametersToAlgorithms()
        {
            //передача параметров эксперта в базы алгоритмов
            for (int i = 0; i < H.nodes.Count; i++)
            {
                if (H.nodes[i].parentID == 0 && H.nodes[i].name() != "committee" & H.nodes[i].getAttributeValue("variable") == null)
                {
                    for (int j = 0; j < algorithms.Count; j++)
                    {
                        if (algorithms[j].h.getNodeByName(H.nodes[i].name()).Count != 0)
                            algorithms[j].h.deleteBranch(algorithms[j].h.getNodeByName(H.nodes[i].name())[0].ID);
                        algorithms[j].h.addNode(H.nodes[i], 0);
                    }
                }
            }
        }
        public static Expert Open(string expertName, MainForm form1)
        {
            return Open(form1.pathPrefix + expertName, expertName, form1);
        }
        public static Expert Open(string path, string expertName, MainForm form1)
        {
            Expert expert = new Expert(expertName, form1, true);

            expert.H = new Hyperparameters(path + "\\h.json", form1, true);
            expert.committeeNodeID = expert.H.getNodeByName("committee")[0].ID;
            var algorithmBranches = expert.H.getNodesByparentID(expert.committeeNodeID);
            foreach (Node algorithmBranch in algorithmBranches)
            {
                Type algorithmType = typeof(Algorithm);
                IEnumerable<Type> list = System.Reflection.Assembly.GetAssembly(algorithmType).GetTypes().Where(type => type.IsSubclassOf(algorithmType));  // using System.Linq


                foreach (Type type in list)
                {
                    if (type.Name == algorithmBranch.name())
                    {
                        algorithmType = type;
                        break;
                    }
                }

                var constr = algorithmType.GetConstructor(new Type[] { form1.GetType(), ("asd").GetType() });
                var algInst = (Algorithm)constr.Invoke(new object[] { form1, algorithmType.ToString() });

                expert.algorithms[expert.algorithms.Count - 1].h = new Hyperparameters(expert.H.toJSON(algorithmBranch.ID), form1);
                expert.algorithms[expert.algorithms.Count - 1].modelName = expert.algorithms[expert.algorithms.Count - 1].h.getValueByName("model_name");
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
            path += "\\h.json";
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

                      double predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 1]);
                      double realValue = Convert.ToDouble(predictionsCSV[i + 1].Split(';')[Convert.ToInt16(h.getValueByName("predicted_column_index"))]);

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


        [NonSerializedAttribute]
        private MainForm form1;

        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
        }

        private double[,] normalize1(double[,] inputDataset)
        {
            // АКТУАЛЬНО ЕСЛИ ВАЖНО СПРОГНОЗИРОВАТЬ НАПРАВЛЕНИЕ ИЗМЕНЕНИЯ ВРЕМЕННОГО РЯДА (-/+)

            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i - (i-1) ///////////
            ////////////////////////////////////////////////
            //первая строка датасета удаляется из-за нормализации типа i-(i-1)
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
                    normalizedDataset2[i, k] = inputDataset[i + 1, k] - previousLine[k];
                }
                for (int j = 0; j < inputDataset.GetLength(1); j++)
                    previousLine[j] = inputDataset[i + 1, j];
            }
            return normalizedDataset2;
        }

        // движущееся окно добавит жизни в нормализованный по абсолютному значению график
        // при большом объёме датасета отличие его максимальных значений предикторов от их минимальных значений
        // слишком велико по сравнию с изменениями на конкретном шаге. Из-за этого после нормализации график становится приплюснутым - 
        // имеет пару больших короткиъ выбросов, а основная его часть крутится вокруг одного значения
        public int normalization_moving_window = 150;

        public double[,] normalize2(double[,] inputDataset)
        {
            // АКТУАЛЬНО ЕСЛИ ВАЖНО СПРОГНОЗИРОВАТЬ АБСОЛЮТНОЕ ЗНАЧЕНИЕ ВРЕМЕННОГО РЯДА

            ////////////////////////////////////////////////
            //////   НОРМАЛИЗАЦИЯ МАСШТАБИРОВАНИЕМ   ///////
            ////////////////////////////////////////////////

            double[] maxPredictorValue = new double[inputDataset.GetLength(1)];
            double[] minPredictorValue = new double[inputDataset.GetLength(1)];

            double[,] normalizedDataset2 = new double[inputDataset.GetLength(0) - normalization_moving_window, inputDataset.GetLength(1)];

            for (int i = normalization_moving_window; i < inputDataset.GetLength(0); i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    maxPredictorValue[k] = Double.MinValue;
                    minPredictorValue[k] = Double.MaxValue;
                }

                for (int m = -normalization_moving_window; m < 0; m++)
                {
                    for (int k = 0; k < inputDataset.GetLength(1); k++)
                    {
                        if (inputDataset[m + i, k] > maxPredictorValue[k])
                            maxPredictorValue[k] = inputDataset[m + i, k];

                        if (inputDataset[m + i, k] < minPredictorValue[k])
                            minPredictorValue[k] = inputDataset[m + i, k];
                    }
                }
                min_bid = minPredictorValue[0];
                max_bid = maxPredictorValue[0];
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    maxPredictorValue[k] -= minPredictorValue[k];
                }

                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    if (maxPredictorValue[k] != 0)
                        normalizedDataset2[i - normalization_moving_window, k] = (inputDataset[i - 1, k] - minPredictorValue[k]) / maxPredictorValue[k];
                    else
                        normalizedDataset2[i - normalization_moving_window, k] = 0;
                }
            }

            return normalizedDataset2;
        }

        public double[,] levelOff2(double[,] inputDataset)
        {
            double[,] levelOffDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            for (int i = 0; i < inputDataset.GetLength(0); i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    levelOffDataset[i, k] = inputDataset[i, k];

                    //масштабирование СИГМОИДА (-0.5;0.5)  y=x/((1+|x|)*2)
                    // levelOffDataset[i, k] = inputDataset[i, k] / ((1+ Math.Abs(inputDataset[i, k]))*2);

                    //масштабирование СИГМОИДА (-0.5;0.5)  y=tanh(x)/2
                    // levelOffDataset[i, k] =  (( Math.Tanh(inputDataset[i, k])) / 2);

                    //масштабирование СИГМОИДА (-0.5;0.5)  y=arctan(x)/pi
                    levelOffDataset[i, k] = ((Math.Atan(inputDataset[i, k])) / Math.PI);

                    //(-0;1) - 0.5 среднее
                    levelOffDataset[i, k] = levelOffDataset[i, k] + 0.5;
                }
            }
            return levelOffDataset;
        }

        public double[,] scale(double[,] inputDataset)
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
        public static string[] skipGarbadge(string[] allLines)
        {
            bool sccflparse;
            //пропуск строк без даты
            List<string> goodLines = new List<string>();
            foreach (string line in allLines)
            {
                DateTime dt;
                sccflparse = DateTime.TryParse(line.Split(';')[0], out dt);
                if (sccflparse)
                {
                    goodLines.Add(line);
                }
            }
            var res = new string[goodLines.Count];
            for (int i = 0; i < goodLines.Count; i++)
            {
                res[i] = goodLines[i];
            }

            return res;
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

        /// <summary>
        /// Добавляет к датасету столбец со спредом <spread>
        /// </summary>
        public static void addSpread(string datasetFile, string rawFile)
        {
            var allLinesDatasetFile = File.ReadAllLines(datasetFile);

            var allLinesRawFile = File.ReadAllLines(rawFile);

            string[] featuresNames = allLinesRawFile[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            int indexOf_bid_top = 0;
            int indexOf_ask_top = 0;

            for (int i = 0; i < featuresNames.Length; i++)
            {
                if (featuresNames[i] == "<bid_top>")
                    indexOf_bid_top = i;
                if (featuresNames[i] == "<ask_top>")
                    indexOf_ask_top = i;
            }


            allLinesDatasetFile[0] += ";<spread>";

            double bid;
            double ask;
            for (int i = 1; i < allLinesDatasetFile.Length; i++)
            {
                string[] features = allLinesRawFile[i + 1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                bid = double.Parse(features[indexOf_bid_top]);
                ask = double.Parse(features[indexOf_ask_top]);

                allLinesDatasetFile[i] += ';' + String.Format("{0:0.#####}", (ask - bid)).Replace(',', '.');
            }

            File.WriteAllLines(datasetFile, allLinesDatasetFile);
        }

        public string getStateStr()
        {
            string stateInString = "";
            for (int i = 0; i < committeeResponse.Length; i++)
            {
                if (committeeResponse[i] == -1)
                {
                    stateInString = "error";
                    break;
                }
                stateInString += "A[" + i.ToString() + "]:" + committeeResponse[i].ToString() + ',';
            }
            stateInString = stateInString.Remove(stateInString.Length - 1, 1);

            if (deposit1 == 0)
                stateInString += ",DEP1:0";
            else
                stateInString += ",DEP1:1";

            if (closeValueHistory.Count == 0)
            {
                stateInString += ",DEP2:1";
            }
            else
            {
                if (deposit2 < closeValueHistory[closeValueHistory.Count - 1])
                    stateInString += ",DEP2:0";
                else
                    stateInString += ",DEP2:1";
            }

            if (closeValueHistory.Count == 0)
            {
                stateInString += ",HigherThenPurchase:0";
            }
            else
            {
                if (closeValueHistory[closeValueHistory.Count - 1] > Purchase)
                    stateInString += ",HigherThenPurchase:1";
                else
                    stateInString += ",HigherThenPurchase:0";
            }

            return stateInString;
        }
    }
}

