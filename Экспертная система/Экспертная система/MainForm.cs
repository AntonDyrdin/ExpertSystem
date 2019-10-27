using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Экспертная_система
{
    public partial class MainForm : Form
    {
        // C:\Users\Антон\AppData\Local\Theano\compiledir_Windows-10-10.0.17134-SP0-Intel64_Family_6_Model_158_Stepping_9_GenuineIntel-3.6.1-64\lock_dir
        public MainForm()
        {
            InitializeComponent();
        }
        public List<logItem> collectLogWhileItFreezed;
        public string pathPrefix;
        public Infrastructure I;
        public Expert expert;
        public MultiParameterVisualizer vis;
        public AgentManager agentManager;
        public string sourceDataFile;
        public System.Threading.Tasks.Task mainTask;
        public System.Threading.Thread mainThread;
        internal AlgorithmOptimization AO;
        private Algorithm algorithm;
        private ExpertOptimization optimization;
        private int Nl = 0;
        private int Nh = 0;
        private double Z = 0;
        private bool tester = false;

        public TextBoxes showInpOutp;
        public void Form1_Load(object sender, EventArgs e)
        {
            I = new Infrastructure(this);

            //   I.showModeSelector();
            I.runSelectedMode();
        }
        public void TEST()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            double balance_BTC = 0;
            double balance_USD = 100;

            double lot = 0.001;



            Hyperparameters order_book;
            var api = new ExmoApi("k", "s");

            string responce = "";
            string s = "";

            vis.addParameter("BTC_USD", Color.White, 1000);

            vis.parameters[0].showLastNValues = true;
            vis.enableGrid = true;
            vis.parameters[0].window = 360;

            //наименьшая цена, по которой можно покупать
            vis.parameters[0].functions.Add(new Function("bid_top", Color.Blue));
            //наивысшая цена, по которой можно продать
            vis.parameters[0].functions.Add(new Function("ask_top", Color.Red));


            expert = new Expert("Expert_Flex", this);

            algorithm = new BidAsk(this, "BidAsk");

            algorithm.Open(@"E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\BidAsk\h.json");

            expert.AddAlgorithm(algorithm);

            int window = int.Parse(algorithm.h.getValueByName("window_size"));

            int step = 60000;

            string[] input;
            double[] predictions = null;
            List<string> rawInput = new List<string>();
            // List<string> spreads = new List<string>();
            rawInput.Add("<DATE_TIME>;<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>");
            //ЗАПУСК СКРИПТОВ ПОТОЧНОГО ПРОГНОЗИРОВНИЯ
            Task[] RunTasks = new Task[expert.algorithms.Count];
            foreach (Algorithm algorithm in expert.algorithms)
                RunTasks[expert.algorithms.IndexOf(algorithm)] = Task.Run(() => algorithm.runGetPredictionScript());

            //ОЖИДАНИЕ ЗАВЕРШЕНИЯ ЗАПУСКА
            foreach (var task in RunTasks)
                task.Wait();


            List<double> positions = new List<double>();

            while (true)
            {
                vis.parameters[0].window = 20;
                try
                {
                    responce = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "BTC_USD" } });
                    if (responce.Length > 400)
                    {
                        responce = responce.Substring(0, 400);
                    }
                    if (!responce.Contains("maintenance"))
                    {
                        order_book = new Hyperparameters(responce, this);
                        //<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>
                        s = DateTime.Now.ToString() + ';'
                            + order_book.nodes[0].getAttributeValue("bid_top") + ';' + order_book.nodes[0].getAttributeValue("bid_quantity") + ';' + order_book.nodes[0].getAttributeValue("bid_amount") + ';'
                            + order_book.nodes[0].getAttributeValue("ask_top") + ';' + order_book.nodes[0].getAttributeValue("ask_quantity") + ';' + order_book.nodes[0].getAttributeValue("ask_amount");

                        s = s.Replace('.', ',');

                        double bid_top = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_top").Replace('.', ','));
                        double ask_top = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_top").Replace('.', ','));

                        /*
                          double bid_quantity = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_quantity").Replace('.', ','));
                          double ask_quantity = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_quantity").Replace('.', ','));

                          double bid_amount = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_amount").Replace('.', ','));
                          double ask_amount = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_amount").Replace('.', ','));*/

                        vis.addPoint(bid_top, "bid_top");
                        vis.addPoint(ask_top, "ask_top");

                        vis.refresh();
                        // log(s);
                        //запись входных данных
                        File.AppendAllText("BTC_USD_exmo.txt", s);

                        rawInput.Add(s);
                        // spreads.Add(String.Format("{0:0.#####}", bid_top - ask_top));
                        showInpOutp.fillTextBox1(rawInput);

                        //+1 для заголовка, +1 для нормализации i/(i-1)
                        if (rawInput.Count < window + 2)
                        {
                            File.AppendAllText("BTC_USD_exmo.txt", "\n");
                        }
                        else
                        {

                            input = expert.prepareDataset(rawInput.ToArray(), "<DATE_TIME>", true);

                            //шапка должна оставаться на месте, поэтому 1, а не 0
                            rawInput.RemoveAt(1);

                            showInpOutp.fillTextBox2(input);

                            if (input.Length == window + 1)
                                predictions = expert.getPrediction(input);
                            else
                                log("input.Length =/= window+1; input.Length = " + input.Length.ToString());

                            string action = expert.getDecision(predictions);

                            if (action == "error")
                            {
                                log("action = error", Color.Red);
                            }
                            if (action == "buy")
                            {
                                if (balance_USD > ask_top * lot)
                                {
                                    balance_BTC = balance_BTC + lot;
                                    balance_USD = balance_USD - (ask_top * lot);
                                    positions.Add(ask_top);
                                    log("BUY", Color.Blue);
                                    log(ask_top.ToString());
                                    log("   USD:" + balance_USD.ToString());
                                    log("   BTC:" + balance_BTC.ToString());

                                    vis.parameters[0].functions[2].points[vis.parameters[0].functions[1].points.Count - 1].mark = "BUY‾‾‾‾‾‾‾‾";
                                }
                                else
                                {
                                    //      log("Баланс USD: " + balance_USD.ToString() + " Невозможно купить " + (bid_top * lot).ToString() + " BTC");
                                    action += " (fail)";
                                }

                            }

                            for (int i = 0; i < positions.Count; i++)
                            {
                                if (positions[i] < bid_top)
                                {
                                    action = "sell";
                                    positions.RemoveAt(i);
                                    break;
                                }
                            }

                            if (action == "sell")
                            {
                                if (bid_top != 0)
                                {
                                    if (balance_BTC >= lot)
                                    {
                                        balance_BTC = balance_BTC - lot;
                                        balance_USD = balance_USD + (bid_top * lot);
                                        log("SELL", Color.Red);
                                        log(bid_top.ToString());
                                        log("   USD:" + balance_USD.ToString());
                                        log("   BTC:" + balance_BTC.ToString());

                                        vis.parameters[0].functions[1].points[vis.parameters[0].functions[1].points.Count - 1].mark = "SELL‾‾‾‾‾‾‾‾";
                                    }
                                    else
                                    {
                                        //    log("Баланс BTC: " + balance_BTC.ToString() + " Невозможно продать " + (lot).ToString() + " BTC");
                                        action += " (fail)";
                                    }
                                }
                                else
                                {
                                    log("bid_top почему-то равно нулю!", Color.Red);
                                    action += " (fail)";
                                }
                            }
                            log(DateTime.Now.ToString() + " " + action);
                            //запись состояния балансов и последнего действия
                            File.AppendAllText("BTC_USD_exmo.txt", balance_BTC.ToString() + ';' + balance_USD.ToString() + ';' + action + "\n");


                        }
                    }
                    else
                    {
                        log("Технические работы", Color.Red);
                    }
                }
                catch (Exception e)
                {
                    log(e.Message, Color.Red);
                    // log(result);
                }
                Thread.Sleep(step);
            }
        }
        public void algorithmOptimization()
        {
            expert = new Expert("Эксперт 1", this);
            mainThread = System.Threading.Thread.CurrentThread;

            algorithm = new Easy(this, "Easy");
             algorithm.Open(@"E:\Anton\Desktop\MAIN\Optimization\Easy\Easy[0]\h.json");
            //   sourceDataFile = pathPrefix + @"Временные ряды\LD2011_2014-cut.txt";

           //sourceDataFile = pathPrefix + @"Временные ряды\85123A_day_of_week.txt";
           // expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<symbol_time>;<server_time>;<TIME>;<DATE>;<local_time>;<TICKER>;<PER>;<DATEandTIME>;<DATE_TIME>;\"\"", false));

            // vis.enableGrid = false;
            // vis.addCSV(pathPrefix + @"Временные ряды\LD2011_2014-cut MAVG-dataset.txt", 0, 1000, 0);

         /*   algorithm.h.addVariable(0, "learning_rate", 0.0001, 0.05, 0.05, 0.013);
            algorithm.h.addVariable(0, "window_size", 2, 30, 2, 19);
            algorithm.h.addVariable(0, "number_of_epochs", 1, 50, 1, 10);
            algorithm.h.setValueByName("split_point", "0.9");
            algorithm.h.setValueByName("steps_forward", "1");
            algorithm.h.setValueByName("start_point", "0");
            algorithm.h.setValueByName("normalize", "true");
            algorithm.h.add("input_file", pathPrefix + @"Временные ряды\85123A-dataset.txt");
            algorithm.h.add("path_prefix", pathPrefix);
            algorithm.h.add("predicted_column_index:0");
            algorithm.h.setValueByName("show_train_charts", "True");*/
           // algorithm.train().Wait();
           
       //  I.executePythonScript(pathPrefix + @"\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\cyclic_prediction.py", "--json_file_path \"" + pathPrefix + @"\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\h.json" + '\"');

            // algorithm.getAccAndStdDev(File.ReadAllLines(@"E:\Anton\Desktop\MAIN\Optimization\Easy\Easy[0]\predictions.txt"));
      //   algorithm.getAccAndStdDev(File.ReadAllLines(pathPrefix + @"\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\cyclic_prediction.txt"));


            vis.enableGrid = true;
            //  vis.addCSV(@"E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\cyclic_prediction.txt", "\"MT_250\"", "\"MT_250\"", "r", 1000, 0, 0);
            //  vis.addCSV(@"E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\cyclic_prediction.txt", "\"MT_250\"", "(predicted -> )\"MT_250\"", "pred", 1000, 0, 0);

            //        vis.addCSV(@"E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\predictions.txt", "realVSpredictions", "MT_250", "real", 1000, 0.95, 0);
            //    vis.addCSV(@"E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\Easy\predictions.txt", "realVSpredictions", "LAST_COLUMN", "predictions", 1000, 0.95, 0);


             AO = new AlgorithmOptimization(algorithm, this,
                 population_value: 32,
                 mutation_rate: 10,
                 architecture_variation_rate: 4,
                 elite_ratio: 0.5,
                 Iterarions: 200,
                 AlgorithmOptimization.TargetFunctionType.STDDEV);

              AO.run();
            //    algorithm.h.draw(0, picBox, 25, 300);
            // algorithm.Save();
            /*   algorithm = new BidAsk(this, "BidAsk");
              algorithm.getAccAndStdDev(File.ReadAllLines(@"E:\Anton\Desktop\MAIN\Экспертная система\Экспертная система\Алгоритмы прогнозирования\BidAsk\predictions.txt"));
             //  algorithm.Open(@"E:\Anton\Desktop\MAIN\Optimization\BidAsk\BidAsk[0]\h.json");

               //<DATE_TIME>;<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>

             /*  sourceDataFile = pathPrefix + @"Временные ряды\BTC_USD_exmo.txt";
                expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<symbol_time>;<server_time>;<TIME>;<DATE>;<local_time>;<TICKER>;<PER>;<DATEandTIME>;<DATE_TIME>", true));

                Expert.addSpread(pathPrefix + @"Временные ряды\BTC_USD_exmo-dataset.txt", sourceDataFile);*/

            /*  algorithm.h.setValueByName("bid_column_index", "0");
                  algorithm.h.setValueByName("ask_column_index", "3");

                  algorithm.h.setValueByName("start_point", "0");
                  algorithm.h.setValueByName("normalize", "true");
                  algorithm.h.add("input_file", pathPrefix + @"Временные ряды\BTC_USD_exmo-dataset.txt");
                  algorithm.h.add("path_prefix", pathPrefix);
                  algorithm.h.add("predicted_column_index:3");
                  algorithm.h.setValueByName("show_train_charts", "True");

               algorithm.train().Wait();
               */

            vis.refresh();
        }
        public void expertOptimization()
        {
            /*     mainThread = System.Threading.Thread.CurrentThread;

                 expert = new Expert("Эксперт 1", this);
                 algorithm = new LSTM_1(this, "LSTM_1[0]");
                 algorithm.Open(@"E:\Anton\Desktop\MAIN\Optimization\" + algorithm.name + '\\' + algorithm.name + @"[0]\h.json");
                 expert.Add(algorithm);

                 //expert.Add(new LSTM_2(this, "LSTM_2[0]"));
                 //expert.Add(new ANN_1(this, "ANN_1[0]"));
                 // expert.Add(new CNN_1(this, "CNN_1[0]"));

                 expert.H.add("normalize:true");

                 expert.H.add("path_prefix", pathPrefix);
                 expert.H.add("drop_columns:none");
                 expert.H.add("name:show_train_charts,value:False");


                 // EURRUB
                 //  expert.H.add("input_file", pathPrefix + @"Временные ряды\EURRUB-dataset.txt");
                 //  expert.H.add("predicted_column_index:3");

                 // SIN
                 sourceDataFile = pathPrefix + @"Временные ряды\SIN+date.txt";
                 // expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<DATE>;<X>", Convert.ToBoolean(expert.H.getValueByName("normalize"))));
                 expert.H.add("input_file", pathPrefix + @"Временные ряды\SIN-dataset.txt");
                 expert.H.add("predicted_column_index:1");

                 expert.Save();



                 optimization = new ExpertOptimization(expert, this, 8, 5, 10, 0.5, 100, new DateTime(2016, 9, 1), new DateTime(2018, 3, 1), sourceDataFile);
                 optimization.run();*/
        }

        public void agent()
        {
            string workFolder = pathPrefix + "work_folder\\";
            AgentLink agent = new AgentLink(this, "192.168.1.5", workFolder);
            mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { agent.startSocket(); });
        }
        internal void exmoAsIndicator()
        {
            log("вход в exmoAsIndicator()");
            vis.addParameter("USD_RUB", Color.Lime, 900);
            vis.parameters[0].functions.Add(new Function("AVG1", Color.Green));
            vis.parameters[0].functions.Add(new Function("AVG2", Color.Pink));
            vis.addParameter("Z", Color.Cyan, 200);
            vis.addParameter("OUTPUT", Color.Lime, 200);

            vis.parameters[0].showLastNValues = true;
            vis.enableGrid = false;
            vis.parameters[0].window = 2000;


            mainThread = System.Threading.Thread.CurrentThread;
            Hyperparameters order_book;
            log("Подключение к ExmoApi...");
            var api = new ExmoApi("k", "s");
            string result = "";
            string s = "";

            double AVG1 = 0;
            double AVG2 = 0;
            double sum = 0;
            List<double> history = new List<double>();

            string[] dataBase = null;

            DateTime lastTime = new DateTime(1, 1, 1, 0, 0, 0);
            if (tester)
            {
                dataBase = File.ReadAllLines("USD_RUB_exmo.txt");
                DateTime.TryParse(dataBase[1].Split(';')[0], out lastTime);
            }


            Invoke(new Action(() =>
            {
                TrackBar5_Scroll(null, null);
                TrackBar6_Scroll(null, null);
                TrackBar7_Scroll(null, null);
            }));



            log("Попытка запустить сервер...");
            MetaTraderLink mtLink = new MetaTraderLink(this);
            log("Сервер запущен.");
            bool parse;
            if (tester) while (mtLink.actualTime == new DateTime(1, 1, 1, 0, 0, 0))
                {
                    log("ожидание подключения metatrader...");
                    Thread.Sleep(1000);
                }


            int screenshotIterationTimer = 3600;
            int inc1 = 0;
            while (true)
            {
                try
                {
                    double bid = 0;
                    if (tester)
                    {

                        for (int i = 1; i < dataBase.Length; i++)
                        {
                            DateTime dt;
                            parse = DateTime.TryParse(dataBase[i].Split(';')[0], out dt);
                            if (parse)
                            {
                                if (mtLink.actualTime == dt)
                                {
                                    bid = Convert.ToDouble(dataBase[i].Split(';')[1].Replace('.', ','));

                                    if (mtLink.actualTime != lastTime.AddSeconds(1))
                                    {
                                        log("Актуальное время не равно предыдущему плюс 1 сек.", Color.Red);
                                        log("> mtLink.actualTime: " + mtLink.actualTime.ToString(), Color.Red);
                                        log("> lastTime: " + lastTime.ToString(), Color.Red);
                                    }
                                    lastTime = mtLink.actualTime;
                                }
                            }
                        }
                        s = mtLink.actualTime.ToString() + ";  " + bid.ToString();
                        log(s);
                    }
                    else
                    {
                        result = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "USD_RUB" } });
                        order_book = new Hyperparameters(result, this);

                        s = DateTime.Now.ToString() + ';' + order_book.nodes[0].getAttributeValue("bid_top") + ';' + order_book.nodes[0].getAttributeValue("ask_top");

                        bid = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_top").Replace('.', ','));
                        //   double ask = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_top").Replace('.', ','));

                        File.AppendAllText("USD_RUB_exmo.txt", s + '\n');
                        log(s);
                    }



                    if (history.Count >= (Nl + Nh))
                    {
                        sum = 0;
                        for (int i = 0; i < Nl; i++)
                        {
                            sum += history[i];
                        }
                        AVG1 = sum / Nl;
                        sum = 0;
                        for (int i = 0; i < Nh; i++)
                        {
                            sum += history[Nl + i];
                        }
                        AVG2 = sum / Nh;

                        double Zi = AVG2 - AVG1;

                        vis.addPoint(AVG1, "AVG1");
                        vis.addPoint(AVG2, "AVG2");
                        if (Zi >= Z)
                        {
                            vis.parameters[0].functions[0].points[vis.parameters[0].functions[0].points.Count - 1].mark = "buy";
                            vis.addPoint(1, "OUTPUT");
                            // ОТПРАВИТЬ КОМАНДУ НА ПОКУПКУ
                            if (mtLink.handler != null)
                            {
                                mtLink.ACTION = "buy";
                            }
                        }
                        else
                        {
                            vis.addPoint(0, "OUTPUT");
                        }
                        vis.addPoint(Zi, "Z");
                        history.RemoveAt(0);
                    }
                    else
                    {
                        vis.addPoint(0, "OUTPUT");
                        vis.addPoint(0, "Z");
                        vis.addPoint(bid, "AVG1");
                        vis.addPoint(bid, "AVG2");
                    }

                    history.Add(bid);
                    vis.addPoint(bid, "USD_RUB");
                    vis.refresh();


                    if (inc1 == screenshotIterationTimer)
                    {
                        inc1 = 0;
                        Bitmap screenShot = (Bitmap)picBox.Image.Clone();
                        int h = screenShot.Height;
                        if (screenShot.Height > 3000)
                            h = 3000;
                        for (int i = 0; i < screenShot.Width; i++)
                            for (int j = 0; j < h; j++)
                            {
                                Color c = screenShot.GetPixel(i, j);
                                if (c == Color.FromArgb(0, 0, 0, 0))
                                { screenShot.SetPixel(i, j, Color.Black); }
                            }
                        screenShot.Save(DateTime.Now.ToString().Replace(':', '-') + ".bmp");
                    }
                    inc1++;
                }
                catch (Exception e)
                {
                    log(e.Message, Color.Red);
                }
                Thread.Sleep(1000);
            }



        }

        internal void script()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            Hyperparameters order_book;

            var api = new ExmoApi("k", "s");

            //  File.AppendAllText("USD_RUB_exmo.txt", "<local_time>;<bid>;<ask>");

            string result = "";

            //  result = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "BTC_USD" } });
            //   order_book = new Hyperparameters(result, this);
            //order_book.draw(0, picBox, this, 25, 250);
            string s = "";

            vis.addParameter("BTC_USD", Color.White, 300);

            vis.parameters[0].showLastNValues = true;
            vis.enableGrid = false;
            vis.parameters[0].window = 200;

            vis.parameters[0].functions.Add(new Function("bid_top", Color.Red));
            vis.parameters[0].functions.Add(new Function("ask_top", Color.Blue));

            //  vis.addParameter("BTC_USD количество", Color.White, 300);
            vis.addParameter("bid_quantity", Color.LightBlue, 300);
            vis.addParameter("ask_quantity", Color.Pink, 300);

            //  vis.addParameter("BTC_USD сумма", Color.White, 300);
            vis.addParameter("bid_amount", Color.Aqua, 300);
            vis.addParameter("ask_amount", Color.Coral, 300);

            int step = 60000;
            int inc = 0;
            List<string> buffer = new List<string>();
            while (true)
            {
                //       vis.parameters[0].window = 500;

                try
                {
                    result = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "BTC_USD" } });
                    if (result.Length > 400)
                    {
                        result = result.Substring(0, 400);
                    }
                    if (!result.Contains("maintenance"))
                    {


                        order_book = new Hyperparameters(result, this);
                        //<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>
                        s = DateTime.Now.ToString() + ';'
                            + order_book.nodes[0].getAttributeValue("bid_top") + ';' + order_book.nodes[0].getAttributeValue("bid_quantity") + ';' + order_book.nodes[0].getAttributeValue("bid_amount") + ';'
                            + order_book.nodes[0].getAttributeValue("ask_top") + ';' + order_book.nodes[0].getAttributeValue("ask_quantity") + ';' + order_book.nodes[0].getAttributeValue("ask_amount");

                        double bid_top = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_top").Replace('.', ','));
                        double ask_top = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_top").Replace('.', ','));

                        double bid_quantity = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_quantity").Replace('.', ','));
                        double ask_quantity = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_quantity").Replace('.', ','));

                        double bid_amount = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_amount").Replace('.', ','));
                        double ask_amount = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_amount").Replace('.', ','));


                        buffer.Add(s);


                        vis.addPoint(bid_top, "bid_top");
                        vis.addPoint(ask_top, "ask_top");

                        vis.addPoint(bid_quantity, "bid_quantity");
                        vis.addPoint(ask_quantity, "ask_quantity");

                        vis.addPoint(bid_amount, "bid_amount");
                        vis.addPoint(ask_amount, "ask_amount");
                        vis.refresh();
                        // log(s);
                        inc++;

                        if (inc > 1800 / (step / 1000) & buffer.Count > 0)
                        {
                            File.AppendAllLines("BTC_USD_exmo.txt", buffer.ToArray());
                            buffer.Clear();
                            log(DateTime.Now.TimeOfDay.ToString() + "  Write!");
                            inc = 0;
                        }
                    }
                    else
                    {
                        log("Технические работы");
                    }
                }
                catch (Exception e)
                {
                    log(e.Message);
                    // log(result);
                }
                Thread.Sleep(step);
            }
        }
        public DecisionMakingSystem DMS;
        public void SARSA()
        {
            MetaTraderLink mtLink = new MetaTraderLink(this);

            // ИНИЦИАЛИЗАЦИЯ СИСТЕМЫ ПРИНЯТИЯ РЕШЕНИЙ
            DMS.epsilon = mtLink.getResponseDouble("epsilon");
            DMS.alpha = mtLink.getResponseDouble("alpha");
            DMS.gamma = mtLink.getResponseDouble("gamma");
            string symbols = mtLink.getResponse("get_symbols");

            string[] splittedSym = symbols.Split(',');
            /////////////////////////////////////////////////////////////////////
            /////// ПАРАМЕТРЫ СОСТОЯНИЯ СИСТЕМЫ ПРИНЯТИЯ РЕШЕНИЙ ////////////////
            for (int i = 0; i < splittedSym.Length; i++)
                DMS.addParameter(splittedSym[i], "0,1");
            // состояние депозитов 1 - баланс положительный, 0 - баланс нулевой
            DMS.addParameter("DEP1", "0,1");

            DMS.defaultActions.Add(new DMSAction("buy"));
            DMS.defaultActions.Add(new DMSAction("sell"));
            DMS.defaultActions.Add(new DMSAction("nothing"));
            DMS.generateStates();

            while (true)
            {
                //ожидание новых данных
                string prices = mtLink.getResponse("get_prices");

                string deposit1 = mtLink.getResponse("get_deposit1");

                //возврат действия
                string action = DMS.getAction(prices + ",DEP1:" + deposit1).type;

                mtLink.send(action);

                //ожидание вознаграждения
                double r = mtLink.getResponseDouble("r");

                //обновление состояния
                prices = mtLink.getResponse("get_prices");
                deposit1 = mtLink.getResponse("get_deposit1");
                DMS.setActualState(prices + ",DEP1:" + deposit1);

                //установка R
                DMS.setR(r);
            }
        }

        public void buildAndTest()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = Expert.Open("Эксперт 1", this);

            //  expert.Add(new LSTM_1(this, "LSTM_1[0]"));
            // expert.Add(new LSTM_2(this, "LSTM_2[0]"));
            //expert.Add(new ANN_1(this, "ANN_1[0]"));
            // expert.Add(new CNN_1(this, "CNN_1[0]"));
            // expert.algorithms[0].Open(@"E:\Anton\Desktop\MAIN\Эксперт 1\CNN_1[0]\h.json");
            //    expert.algorithms[0].Open(pathPrefix + @"Optimization\LSTM_1\LSTM_1[0]\h.json");
            //    expert.algorithms[1].Open(pathPrefix + @"Optimization\LSTM_2\LSTM_2[0]\h.json");
            //   expert.algorithms[2].Open(pathPrefix + @"Optimization\ANN_1\ANN_1[0]\h.json");

            /*    expert.H.setValueByName("start_point", "0.5");
                 expert.H.setValueByName("normalize", "true");
                 expert.H.add("input_file", pathPrefix + @"Временные ряды\USD_RUB_exmo-dataset.txt");
                 expert.H.add("path_prefix", pathPrefix);
                 expert.H.add("drop_columns:<local_time>");
                 expert.H.add("predicted_column_index:1");
                 expert.H.add("name:show_train_charts,value:True");*/

            /* expert.H.add("normalize:true");
             expert.H.add("input_file", pathPrefix + @"Временные ряды\EURRUB-dataset.txt");
             expert.H.add("path_prefix", pathPrefix);
             expert.H.add("drop_columns:none");
             expert.H.add("predicted_column_index:3");
             expert.H.add("name:show_train_charts,value:True");*/
            /* expert.copyExpertParametersToAlgorithms();
             expert.trainAllAlgorithms(false);

             expert.copyHyperparametersFromAlgorithmsToExpert();
             expert.H.draw(0, picBox, this, 20, 150);
             expert.Save();
             */
            sourceDataFile = pathPrefix + @"Временные ряды\USD_RUB_exmo.txt";
            expert.test(new DateTime(2019, 6, 27, 0, 0, 0), new DateTime(2019, 6, 27, 23, 59, 59), sourceDataFile);


        }


        public void buildAndTrain()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = new Expert("Эксперт 1", this);

            //    expert.Add(new ANN_1(this, "ANN_1[1]"));

            expert.algorithms[0].setAttributeByName("number_of_epochs", 50);
            /*  expert.algorithms[0].setAttributeByName("window_size", 30); 
              expert.algorithms[0].setAttributeByName("batch_size", 200);
              expert.algorithms[0].setAttributeByName("split_point", "0.99");    */
            /* for(int i=0;i<20;i++)
             expert.Add(new ANN_1(this, "ANN_1_["+i.ToString()+"]"));

             for (int i = 0; i < 20; i++)
                 expert.Add(new LSTM_1(this, "LSTM_1_[" + i.ToString() + "]"));*/



            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.H.add("normalize:true");
            expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "none", Convert.ToBoolean(expert.H.getValueByName("normalize"))));
            expert.H.add("path_prefix", pathPrefix);
            expert.H.add("drop_columns:none");
            expert.H.add("predicted_column_index:3");
            expert.H.add("name:show_train_charts,value:False");

            expert.copyExpertParametersToAlgorithms();
            expert.copyHyperparametersFromAlgorithmsToExpert();
            expert.trainAllAlgorithms(false);
            expert.copyExpertParametersToAlgorithms();
            expert.copyHyperparametersFromAlgorithmsToExpert();
            expert.H.draw(0, picBox, 15, 150);
            expert.Save();
        }
        private void Hyperparameters_Click(object sender, EventArgs e)
        {
            I.agentManagerView = new AgentManagerView(I.agentManager);
            I.agentManagerView.Show();
        }
        private void Charts_Click(object sender, EventArgs e)
        {
            //      I.executionProgressForm = new ExecutionProgress();
            //       I.executionProgressForm.Show();
            //  vis.enableGrid = false;
            vis.clear();
            vis.addCSV(@"E:\Anton\Desktop\MAIN\Optimization\Easy\Easy[0]\predictions.txt", "realVSpredictions", "MT_254", "real", 1000, 0.95, -1);

            vis.addCSV(@"E:\Anton\Desktop\MAIN\Optimization\Easy\Easy[0]\predictions.txt", "realVSpredictions", "LAST_COLUMN", "predictions", 1000, 0.95, 0);


            vis.refresh();
            /* double hidedPart = 0.99;
           
             vis.clear();*/
            // vis.addCSV(pathPrefix + @"Временные ряды\LD2011_2014-cut.txt", "MT_251", "MT_251", "MT_251", 1000, 0,0);
            //  vis.addCSV(@"E:\\Anton\Desktop\MAIN\Временные ряды\отладка 3.txt", "dbid&dask", "<bid_top>", "dbid", 1000, 0.9, 0);
            //  vis.addCSV(@"E:\\Anton\Desktop\MAIN\Временные ряды\отладка 3.txt", "dbid&dask", "<ask_top>", "dask", 1000, 0.9, 0);

            //  vis.addCSV(@"E:\\Anton\Desktop\MAIN\Временные ряды\отладка 3 — копия.txt", "bid&ask", "<bid_top>", "bid", 1000, 0.9, 0);
            //  vis.addCSV(@"E:\\Anton\Desktop\MAIN\Временные ряды\отладка 3 — копия.txt", "bid&ask", "<ask_top>", "ask", 1000, 0.9, 0);

            //Algorithm a = new LSTM_1(this, "asdasd");
            //a.h.add("predicted_column_index:4");
            //a.getAccAndStdDev(File.ReadAllLines(@"D:\Anton\Desktop\MAIN\Optimization\CNN_1\CNN_1[0]\predictions.txt"));
            //  vis.addCSV(@"E:\Anton\Desktop\MAIN\Optimization\" + algorithm.name + '\\' + algorithm.name + @"[0]\predictions.txt", "realVSpredictions", "<CLOSE>", 1000, hidedPart, -1);
            // vis.addCSV(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_1\LSTM_1[0]\predictions.txt", "realVSpredictions", "<DATEandTIME>", "0.5", 1000, hidedPart, -1);
            // vis.addCSV(sourceDataFile, "Real close value", expert.h().getValueByName("predicted_column_index"), 500, split_point + (1 - split_point) * hidedPart, -2);
            //   vis.addCSV(@"E:\Anton\Desktop\MAIN\Optimization\" + algorithm.name + '\\' + algorithm.name + @"[0]\predictions.txt", "realVSpredictions", "LAST_COLUMN", "predictions", 1000, hidedPart, 0);

            //  vis.refresh();

            /*vis.addParameter(expert.dataset1, 2, "dataset1", Color.White, 300);
              vis.addParameter(expert.dataset2, 2, "dataset2", Color.White, 300);
              vis.addParameter(expert.dataset3, 2, "dataset3", Color.White, 300); */
            /*double split_point = Convert.ToDouble(expert.h().getValueByName("split_point").Replace('.', ','));
             vis.addCSV(sourceDataFile, "Real close value", "<CLOSE>", 500, split_point + (1 - split_point) * hidedPart, -2);
             // vis.addCSV(sourceDataFile, "Real close value", expert.h().getValueByName("predicted_column_index"), 500, split_point + (1 - split_point) * hidedPart, -2);
             vis.addCSV(expert.algorithms[0].getValueByName("predictions_file_path"), "realVSpredictions", expert.h().getValueByName("predicted_column_index"), "real", 500, hidedPart, -1);
             vis.addCSV(expert.algorithms[0].getValueByName("predictions_file_path"), "realVSpredictions", "LAST_COLUMN", "predictions", 500, hidedPart, 0);
             vis.refresh(); */
        }


        private void Change_mode_Click(object sender, EventArgs e)
        {
            I.showModeSelector();
        }



        public void picBox_Click(object sender, EventArgs e) { }
        private void picBox_DoubleClick(object sender, EventArgs e) { }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                expert.algorithms[0].h.lightsOn = true;
                picBox.BackColor = Color.White;
                logBox.BackColor = Color.White;
                logBox.ForeColor = Color.Black;
                vis.lightsOn = true;
            }
            else
            {
                expert.algorithms[0].h.lightsOn = false;
                picBox.BackColor = Color.Black;
                logBox.BackColor = Color.Black;
                logBox.ForeColor = Color.White;
                vis.lightsOn = false;
            }
        }

        public void log(String s, System.Drawing.Color col)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    if (!freezeLogBox)
                    {
                        logBox.SelectionColor = col;
                        logBox.AppendText(s + '\n');
                        logBox.SelectionColor = Color.White;


                        logBox.SelectionStart = logBox.Text.Length;
                        logBox.ScrollToCaret();
                    }
                    else
                    {
                        collectLogWhileItFreezed.Add(new logItem(s, col));
                    }
                }));
                var strings = new string[1];
                strings[0] = s;
                if (I != null)
                    if (I.logPath != null)
                    {
                    AppendAllLinesAgain:
                        try
                        {
                            File.AppendAllLines(I.logPath, strings);
                        }
                        catch (IOException e)
                        {
                            goto AppendAllLinesAgain;
                        }
                    }
            }
            catch { }
        }
        public void log(string s)
        {
            // if (checkBox1.Checked)

            log(s, Color.White);
            /*else
                log(s, Color.White);*/
        }

        public void picBoxRefresh() { picBox.Refresh(); }
        public delegate void StringDelegate(string is_completed);
        public delegate void VoidDelegate();
        public StringDelegate stringDelegate;
        public VoidDelegate voidDelegate;

        private void RedClick(object sender, EventArgs e)
        {
            mainThread.Abort();
        }

        public void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //  try
            //{
            if (I.agentManager != null)
                if (I.agentManager.TCPListener != null)
                    I.agentManager.TCPListener.Stop();
            // }
            //catch { }
            try
            {
                mainThread.Abort();
            }
            catch { }
            Process.GetCurrentProcess().Close();

        }
        public int multiThreadTrainingRATE = 0;

        internal void TrackBar2_Scroll(object sender, EventArgs e)
        {
            multiThreadTrainingRATE = trackBar2.Value;
        }
        public int mutationRate = 0;
        internal void TrackBar3_Scroll(object sender, EventArgs e)
        {
            mutationRate = trackBar3.Value;
        }
        public int test_count = 0;
        internal void TrackBar4_Scroll(object sender, EventArgs e)
        {
            test_count = trackBar4.Value;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Process.GetCurrentProcess().Kill();
        }

        private void LogBox_MouseEnter(object sender, EventArgs e)
        {
            freezeLogBox = true;
        }

        private bool freezeLogBox = false;
        private void LogBox_MouseLeave(object sender, EventArgs e)
        {
            freezeLogBox = false;

            for (int i = 0; i < collectLogWhileItFreezed.Count; i++)
            {
                log(collectLogWhileItFreezed[i].text, collectLogWhileItFreezed[i].color);
                collectLogWhileItFreezed.RemoveAt(i);
                i--;
            }
        }

        private void Draw_window_Scroll(object sender, EventArgs e)
        {
            for (int i = 0; i < vis.parameters.Count; i++)
                vis.parameters[i].window = draw_window.Value * 10;
            //  vis.refresh();
        }

        internal void TrackBar5_Scroll(object sender, EventArgs e)
        {
            Nl = trackBar5.Value * 10;
            textBox1.Text = Nl.ToString();
        }

        internal void TrackBar6_Scroll(object sender, EventArgs e)
        {
            Nh = trackBar6.Value * 10;
            textBox2.Text = Nh.ToString();
        }

        internal void TrackBar7_Scroll(object sender, EventArgs e)
        {
            Z = trackBar7.Value / 20000.0;
            textBox3.Text = Z.ToString();
        }
        //System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackTrace().ToString());
    }
}
