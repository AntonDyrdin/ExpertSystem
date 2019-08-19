using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
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
        public void Form1_Load(object sender, EventArgs e)
        {
            I = new Infrastructure(this);

            //  I.showModeSelector();
        }

        public void algorithmOptimization()
        {
            expert = new Expert("Эксперт 1", this);
            mainThread = System.Threading.Thread.CurrentThread;
            algorithm = new FlexNN(this, "FlexNN");

          //  sourceDataFile = pathPrefix + @"Временные ряды\USD_RUB_exmo.txt";
            //expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<TIME>;<TICKER>;<PER>;<DATE>;<local_time>", true));
            algorithm.h.setValueByName("start_point", "0");
            algorithm.h.setValueByName("normalize", "true");
            algorithm.h.add("input_file", pathPrefix + @"Временные ряды\SIN+date-dataset.txt");
            algorithm.h.add("path_prefix", pathPrefix);
            algorithm.h.add("drop_columns:<local_time>");
            algorithm.h.add("predicted_column_index:0");
            algorithm.h.add("name:show_train_charts,value:False");

        //    algorithm.train().Wait();

        /*    vis.addParameter("X", Color.White, 500);
            vis.addCSV(algorithm.h.getValueByName("predictions_file_path"), "_X", "<X>", "X", 500, 0, -1);
            vis.addCSV(algorithm.h.getValueByName("predictions_file_path"), "_X", "LAST_COLUMN", "X", 500, 0, 0);
            vis.enableGrid = false;
            vis.refresh();*/

            /*vis.addParameter("ask", Color.White, 500);
            vis.addCSV(algorithm.h.getValueByName("predictions_file_path"), "_ask", "<ask>", "ask", 500, 0, -1);
            vis.addCSV(algorithm.h.getValueByName("predictions_file_path"), "_ask", "LAST_COLUMN", "ask", 500, 0, 0);
            vis.enableGrid = false;
            vis.refresh();*/
            //algorithm.Open(@"E:\Anton\Desktop\MAIN\Optimization\LSTM_1\LSTM_1[0]\h.json");

            algorithm.Save();

           AO = new AlgorithmOptimization(algorithm, this, 8, 10, 5, 0.5, 100);
            AO.run();
        //    algorithm.h.draw(0, picBox, 25, 300);
            // algorithm.Save();
        }
        public void expertOptimization()
        {
            mainThread = System.Threading.Thread.CurrentThread;

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
            optimization.run();
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

            result = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "USD_RUB" } });
            order_book = new Hyperparameters(result, this);
            //order_book.draw(0, picBox, this, 25, 250);
            string s = "";

            vis.addParameter("USD_RUB", Color.Lime, 900);
            vis.parameters[0].showLastNValues = true;
            vis.enableGrid = false;
            vis.parameters[0].window = 200;
            double last_ask = 0;
            double last_bid = 0;

            while (true)
            {
                //       vis.parameters[0].window = 500;
                try
                {
                    result = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "USD_RUB" } });
                    order_book = new Hyperparameters(result, this);

                    s = DateTime.Now.ToString() + ';' + order_book.nodes[0].getAttributeValue("bid_top") + ';' + order_book.nodes[0].getAttributeValue("ask_top");

                    double bid = Convert.ToDouble(order_book.nodes[0].getAttributeValue("bid_top").Replace('.', ','));
                    double ask = Convert.ToDouble(order_book.nodes[0].getAttributeValue("ask_top").Replace('.', ','));

                    if (last_ask != ask | last_bid != bid)
                    {
                        File.AppendAllText("USD_RUB_exmo.txt", s + '\n');
                        last_ask = ask;
                        last_bid = bid;
                    }
                    vis.addPoint(bid, "USD_RUB");
                    vis.refresh();
                    log(s);
                }
                catch (Exception e)
                {
                    log(e.Message);
                    // log(result);
                }
                Thread.Sleep(1000);
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

        public void TEST()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            expert = Expert.Open("Эксперт 1", this);
            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.H.replaceStringInAllValues(expert.H.getValueByName("path_prefix"), pathPrefix);
            expert.copyExpertParametersToAlgorithms();
            expert.copyHyperparametersFromAlgorithmsToExpert();
            expert.test(new DateTime(2017, 8, 1), new DateTime(2017, 9, 30), sourceDataFile);
            expert.H.draw(0, picBox, 15, 150);
            expert.Save();
        }
        public void buildAndTrain()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = new Expert("Эксперт 1", this);

            expert.Add(new ANN_1(this, "ANN_1[1]"));

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
            /*if (optimization != null)
                optimization.E.draw(0, picBox, this, 25, 150);
            if (expert != null)
                expert.H.draw(0, picBox, this, 25, 150);
            */
            if (AO != null)
                AO.A.draw(0, picBox, 25, 150);
            /* if (algorithm != null)
                 algorithm.h.draw(0, picBox, this, 25, 150);*/
        }
        private void Charts_Click(object sender, EventArgs e)
        {

            double hidedPart = 0;
            vis.enableGrid = true;
            vis.clear();
            //Algorithm a = new LSTM_1(this, "asdasd");
            //a.h.add("predicted_column_index:4");
            //a.getAccAndStdDev(File.ReadAllLines(@"D:\Anton\Desktop\MAIN\Optimization\CNN_1\CNN_1[0]\predictions.txt"));
            vis.addCSV(@"E:\Anton\Desktop\MAIN\Optimization\" + algorithm.name + '\\' + algorithm.name + @"[0]\predictions.txt", "realVSpredictions", "<CLOSE>", 1000, hidedPart, -1);
            // vis.addCSV(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_1\LSTM_1[0]\predictions.txt", "realVSpredictions", "<DATEandTIME>", "0.5", 1000, hidedPart, -1);
            // vis.addCSV(sourceDataFile, "Real close value", expert.h().getValueByName("predicted_column_index"), 500, split_point + (1 - split_point) * hidedPart, -2);
            vis.addCSV(@"E:\Anton\Desktop\MAIN\Optimization\" + algorithm.name + '\\' + algorithm.name + @"[0]\predictions.txt", "realVSpredictions", "LAST_COLUMN", "predictions", 1000, hidedPart, 0);

            vis.refresh();

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
