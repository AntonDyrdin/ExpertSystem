using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Экспертная_система
{
    public partial class MainForm : Form
    {
        // C:\Users\Антон\AppData\Local\Theano\compiledir_Windows-10-10.0.17134-SP0-Intel64_Family_6_Model_158_Stepping_9_GenuineIntel-3.6.1-64\lock_dir
        public int ENV;
        public int DEV = 0;
        public int TEST = 0;
        public int REAL = 1;
        public MainForm() { InitializeComponent(); }
        public List<logItem> collectLogWhileItFreezed;
        public string pathPrefix;
        public Infrastructure I;
        public MultiParameterVisualizer vis;

        public Expert expert;
        public string sourceDataFile;
        public System.Threading.Tasks.Task mainTask;
        public System.Threading.Thread mainThread;
        private Algorithm algorithm;
        private ExpertOptimization optimization;
        public DecisionMakingSystem DMS;
        Report report;
        double deposit1 = 0;
        double deposit2 = 100;


        public double purchase_limit_amount = 100;
        public double purchase_limit_amount_left = 100;
        public int purchase_limit_interval = 1;
        bool purchase_limit_timer_enabled = false;
        DateTime purchase_limit_timer_start;

        double lot = 0.001;

        ////////////////////////////////
        //  price * quantity = amount //
        ////////////////////////////////

        string pair = "BTC_USD";

        bool maintenance_in_progress = false;
        bool connection_lost = false;

        bool stop_buying = false;

        double bid_top = -1;
        double ask_top = -1;
        public void Form1_Load(object sender, EventArgs e)
        {
            ENV = DEV;
            I = new Infrastructure(this);
            mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { main_thread(); });
        }
        public void main_thread()
        {
            purchase_limit_timer_start = DateTime.Now;
            mainThread = System.Threading.Thread.CurrentThread;

            Hyperparameters order_book;
            var api = new ExmoApi("k", "s");

            string responce = "";

            vis.addParameter(pair, Color.White, 1000);

            vis.parameters[0].showLastNValues = true;
            vis.addParameter("Просадка", Color.Pink, 300);
            vis.parameters[1].showLastNValues = true;
            vis.enableGrid = false;
            vis.addParameter("EXIT", Color.Purple, 500);
            vis.parameters[2].showLastNValues = true;
            vis.addParameter("BTC", Color.LightPink, 400);
            vis.parameters[3].showLastNValues = true;
            vis.addParameter("USD", Color.Green, 400);
            vis.parameters[4].showLastNValues = true;

            //наименьшая цена, по которой можно покупать
            vis.parameters[0].functions.Add(new Function("bid_top", Color.Cyan));
            //наивысшая цена, по которой можно продать
            vis.parameters[0].functions.Add(new Function("ask_top", Color.Red));

            vis.parameters[0].functions.Add(new Function("MAVG_bid", Color.Green));

            int w1 = 78;
            int w2 = 2;
            int take_pofit = 50; // > 0.002 * 2 * bid_top
            int drawdown = 81;
            expert = new Expert("Test Expert MAVG", this);
            expert.H.setValueByName("w1", w1);
            expert.H.setValueByName("w2", w2);
            expert.H.setValueByName("take_pofit", take_pofit);
            expert.H.setValueByName("drawdown", drawdown);

            log(expert.H.toJSON(0), Color.Pink);

            int window = int.Parse(expert.H.getValueByName("w1")) + int.Parse(expert.H.getValueByName("w2"));

            ///////////////////////////////
            // Шаг обновления информации //
            int step = 1000;
            ///////////////////////////////

            List<string> rawInput = new List<string>();

            report = new Report("<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>", pathPrefix);

            rawInput.Add("<DATE_TIME>;<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>");

            if (ENV != REAL)
            {
                string[] history = File.ReadAllLines(pair + "_exmo.txt");
                for (int i = 0; i < window; i++)
                    rawInput.Add(history[history.Length - window + i]);
            }
            List<double> positions = new List<double>();

            string action = "";
            int last_minute = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            while (true)
            {
                vis.enableGrid = false;
                string predictors_line;
                try
                {
                    responce = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", pair }, { "limit", "1" } });
                    dynamic responce_object = JObject.Parse(responce);
                    var ask = (Newtonsoft.Json.Linq.JValue)responce_object[pair].bid_quantity;
                    if (responce.Length > 200)
                    {
                        responce = responce.Substring(0, 200);
                    }
                    if (!responce.Contains("aintenance"))
                    {
                        if (connection_lost)
                        {
                            log(DateTime.Now.ToString() + "| Соединение восстановлено");
                            connection_lost = false;
                        }
                        maintenance_in_progress = false;
                        order_book = new Hyperparameters(responce, this);
                        //<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>
                        predictors_line =
                            DateTime.Now.ToString() + ';'
                            + (Newtonsoft.Json.Linq.JValue)responce_object[pair].bid_top + ';'
                            + (Newtonsoft.Json.Linq.JValue)responce_object[pair].bid_quantity + ';'
                            + (Newtonsoft.Json.Linq.JValue)responce_object[pair].bid_amount + ';'
                            + (Newtonsoft.Json.Linq.JValue)responce_object[pair].ask_top + ';'
                            + (Newtonsoft.Json.Linq.JValue)responce_object[pair].ask_quantity + ';'
                            + (Newtonsoft.Json.Linq.JValue)responce_object[pair].ask_amount;

                        bid_top = Convert.ToDouble((Newtonsoft.Json.Linq.JValue)responce_object[pair].bid_top);
                        ask_top = Convert.ToDouble((Newtonsoft.Json.Linq.JValue)responce_object[pair].ask_top);

                        vis.addPoint(bid_top, "bid_top");
                        vis.addPoint(ask_top, "ask_top");

                        if (DateTime.Now.Minute != last_minute)
                        {
                            last_minute = DateTime.Now.Minute;
                            rawInput.Add(predictors_line);
                            rawInput.RemoveAt(0);
                            //запись входных данных
                            File.AppendAllText(pair + "_exmo.txt", predictors_line + "\n");

                            double closeValue = bid_top;
                            action = expert.getDecision(rawInput.ToArray());

                            if (action == "error")
                            {
                                log("action = error", Color.Red);
                            }
                            // запрет на покупку
                            if (stop_buying && action == "buy")
                            {
                                action = "stop buying";
                            }
                            if (action == "buy")
                            {
                                if (deposit2 > ask_top * lot)
                                {
                                    buy(ask_top);

                                    positions.Add(ask_top);
                                    log(DateTime.Now.ToString() + " BUY", Color.Blue);
                                    log(ask_top.ToString());
                                    log("   USD:" + deposit2.ToString());
                                    log("   BTC:" + deposit1.ToString());

                                }
                                else
                                {
                                    action += " (fail USD balance is too low)";
                                }
                            }
                            /////////////////////////////////////
                            // ОБНОВЛЕНИЕ ОТОБРАЖАЕМОЙ ИНФРМАЦИИ
                            vis.refresh();
                        }

                        vis.parameters[0].addPoint(expert.MAVG_bid, "MAVG_bid");
                        vis.addPoint(expert.MAVG_bid - ask_top, "Просадка");

                        for (int i = 0; i < positions.Count; i++)
                        {
                            if (bid_top - positions[i] > take_pofit)
                            {
                                action = "sell";

                                if (deposit1 >= 0)
                                {
                                    log("Профит сделки: " + (bid_top - positions[i]).ToString());

                                    sell(bid_top);

                                    log(DateTime.Now.ToString() + " SELL", Color.Red);
                                    log(bid_top.ToString());
                                    log("   USD:" + deposit2.ToString());
                                    log("   BTC:" + deposit1.ToString());
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

                        /////////////////////////////////////
                        // ВЕДЕНИЕ ЖУРНАЛА
                        report.log(new ReportLine(predictors_line, expert.MAVG_bid, action, deposit1, deposit2, deposit1 * bid_top + deposit2));

                        vis.addPoint("EXIT", deposit1 * bid_top + deposit2);
                        vis.addPoint("USD", deposit2);
                        vis.addPoint("BTC", deposit1);

                        refresh_deposits();
                    }
                    else
                    {
                        if (!maintenance_in_progress)
                        {
                            log("Технические работы", Color.Red);
                            maintenance_in_progress = true;
                        }
                        Thread.Sleep(10000);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "Невозможно соединиться с удаленным сервером" || e.Message == "Невозможно разрешить удаленное имя: 'api.exmo.com'")
                    {
                        log(DateTime.Now.ToString() + "| Нет соединения", Color.Red);
                        connection_lost = true;
                    }
                    else if (e.Message == "Запрос был прерван: Время ожидания операции истекло.")
                    {
                        log(DateTime.Now.ToString() + "| Соединение потеряно", Color.Red);
                        connection_lost = true;
                    }
                    else
                    {
                        log(e.Message, Color.Red);
                        log(e.StackTrace, Color.Red);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(step);
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        public void optimization_thread()
        {
            vis.enableGrid = false;

            expert = new Expert("Test Expert MAVG", this);

            expert.H.addVariable("w1", 30, 300, 78);
            expert.H.addVariable("w2", 2, 25, 2);
            expert.H.setValueByName("take_pofit", 50);
            expert.H.addVariable("drawdown", 1, 100, 81);
            expert.H.setValueByName("lot", "0.001");

            expert.H.setValueByName("purchase_limit_amount", 100);
            expert.H.addVariable("purchase_limit_interval", 1,12*60,6*60);

            /* expert.testExmo(date1: new DateTime(2020, 04, 27, 14, 58,0),
                 date2: new DateTime(2020, 5, 2, 1, 50,0),
                 rawDatasetFilePath: "Журнал торговли27.04.2020 14-58-51.txt");


             vis.addParameter("bid", Color.White, 460);
             vis.parameters[0].functions.Add(new Function("bid_top", Color.Cyan));
             vis.parameters[0].functions.Add(new Function("MAVG_bid", Color.Green));
             vis.addParameter("BTC", Color.LightPink, 150);
             vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "test", "<bid_top>", "<bid_top>", 460, 0, 0, 50);
             vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "test", "<MAVG_bid>", "<MAVG_bid>", 460, 0, 0, 50);
             vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "<BTC_balance>", "<BTC_balance>", 150, 0, 0, 50);
             vis.addParameter("EXIT", Color.Purple, 200);

             for (int i = 0; i < expert.deposit1History.Count; i++)
             {
                 vis.addPoint(expert.deposit1History[i], "BTC");
                 vis.addPoint(expert.MAVG_bid_history[i], "MAVG_bid");
                 vis.addPoint(expert.closeValueHistory[i], "bid_top");
                 //if (expert.actionHistory[i] != "" && expert.actionHistory[i] != "date doesn't exist")
                 //  vis.markLast("‾"+ expert.actionHistory[i], "bid_top");
                 if (expert.actionHistory[i] == "buy")
                   vis.markLast("‾", "bid_top");
                 vis.addPoint(expert.deposit1History[i] * expert.closeValueHistory[i] + expert.deposit2History[i], "EXIT");
             }

             vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "<EXIT>", "<EXIT>", 200, 0, 0, 40);
             log((expert.deposit2).ToString());

             vis.refresh();
             */
            optimization = new ExpertOptimization(expert, this,
                population_value: 8,
                test_count: 1,
                mutation_rate: 8,
                elite_ratio: 0.25,
                Iterarions: 300,
                date1: new DateTime(2020, 04, 29, 14, 58, 0),
                date2: new DateTime(2020, 05, 5, 1, 0, 0),
                rawDatasetFilePath: pair + "_exmo.txt");

            optimization.run();
        }
        void refresh_deposits()
        {
            Invoke(new Action(() =>
            {
                deposit_1_value.Text = String.Format("{0:0.#####}", deposit1);
                deposit_2_value.Text = String.Format("{0:0.#####}", deposit2);
                sum_of_deposits.Text = String.Format("{0:0.#####}", (deposit2 + (deposit1 * bid_top)));
            }));
        }
        void buy(double ask_top)
        {
            if (purchase_limit_amount_left - (ask_top * lot) > 0)
            {
                if (ENV == REAL)
                    buy_real();
                else
                    buy_test(ask_top);

                vis.markLast("‾BUY", "ask_top");
            }
            else
            {
                if (purchase_limit_timer_enabled == false)
                {
                    purchase_limit_timer_start = DateTime.Now;
                    purchase_limit_timer_enabled = true;
                }

                if (purchase_limit_timer_start.AddMinutes(purchase_limit_interval) < DateTime.Now)
                {
                    purchase_limit_amount_left = purchase_limit_amount;
                    purchase_limit_timer_enabled = false;
                }
                else
                {
                    log("До сброса лимита: " + (purchase_limit_timer_start.AddMinutes(purchase_limit_interval) - DateTime.Now).ToString());
                }
            }
            refresh_deposits();
        }
        void sell(double bid_top)
        {
            if (ENV == REAL)
                sell_real();
            else
                sell_test(bid_top);

            vis.markLast("‾SELL", "bid_top");

            refresh_deposits();
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
        void buy_real() { }
        void sell_real() { }
        private void bye_button_Click(object sender, EventArgs e)
        {
            if (ask_top > 0 && ENV == DEV)
                buy(ask_top);
        }
        private void sell_button_Click(object sender, EventArgs e)
        {
            if (bid_top > 0 && ENV == DEV)
                sell(bid_top);
        }
        private void stop_buying_click(object sender, EventArgs e)
        {
            stop_buying = !stop_buying;
            if (stop_buying)
                b1.BackColor = Color.DarkRed;
            else
                b1.BackColor = Color.Black;
        }
        private void Change_mode_Click(object sender, EventArgs e)
        {
            mainThread.Abort();
            I.showModeSelector();
        }
        public void picBox_Click(object sender, EventArgs e)
        {
            vis.refresh();
        }
        private void picBox_DoubleClick(object sender, EventArgs e) { }

        public void log(String s, System.Drawing.Color col)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    if (!freezeLogBox)
                    {
                        logBox.SelectionColor = col;

                        if (col != Color.White && col != Color.Red)
                            logBox.AppendText(DateTime.Now.ToShortTimeString() + "|  " + s + '\n');
                        else
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
                        catch
                        {
                            goto AppendAllLinesAgain;
                        }
                    }
            }
            catch { }
        }
        public void log(string s)
        {
            log(s, Color.White);
        }

        public void picBoxRefresh() { picBox.Refresh(); }
        public delegate void StringDelegate(string is_completed);
        public delegate void VoidDelegate();
        public StringDelegate stringDelegate;
        public VoidDelegate voidDelegate;
        private void RedClick(object sender, EventArgs e) { mainThread.Abort(); }
        public void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (report != null)
            {
                File.AppendAllLines(report.report_file_name, report.buffer.ToArray());
                report.buffer.Clear();
            }
            // try { mainThread.Abort(); }
            // catch { }
            // Process.GetCurrentProcess().Close();
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) { /* Process.GetCurrentProcess().Kill();*/}
        private void LogBox_MouseEnter(object sender, EventArgs e) { freezeLogBox = true; }
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
        private void wipeLog_Click(object sender, EventArgs e) { logBox.Text = ""; }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.picBox.Width = this.splitContainer1.SplitterDistance;
            this.logBox.Width = this.Width - this.splitContainer1.SplitterDistance;
        }
        private void displayedWindow_Scroll(object sender, EventArgs e)
        {
            vis.setWindow(displayedWindow.Value * displayedWindow.Value);
            vis.refresh();
        }

        public class Report
        {
            public string report_file_name = "";
            public bool first_line = true;
            public string header;
            public string predictors_header;
            public string last_line;
            public List<string> buffer;
            int last_write_minute;
            string path_prefix;
            public Report(string predictors_header, string path_prefix)
            {
                this.path_prefix = path_prefix;
                this.predictors_header = predictors_header;
                buffer = new List<string>();
            }

            public void log(ReportLine line)
            {
                if (first_line)
                {
                    report_file_name = path_prefix + "//trading_log//Журнал торговли" + DateTime.Now.ToString().Replace(':', '-') + ".txt";
                    header = "<DATE_TIME>;" + predictors_header + ";" +
                        "<MAVG_bid>;" +
                        "<action>;" +
                        "<BTC_balance>;" +
                        "<USD_balance>;" +
                        "<EXIT>";
                    File.AppendAllText(report_file_name, header + "\n");
                    first_line = false;
                }

                last_line = line.predictors_line + ';' +
                            line.MAVG_bid + ';' +
                            line.action + ';' +
                            line.BTC_balance + ';' +
                            line.USD_balance + ';' +
                            line.exit;

                buffer.Add(last_line);

                if (DateTime.Now.Minute != last_write_minute)
                {
                    File.AppendAllLines(report_file_name, buffer.ToArray());
                    buffer.Clear();
                    last_write_minute = DateTime.Now.Minute;
                }

            }
        }
        public class ReportLine
        {
            public string predictors_line;
            public string MAVG_bid;
            public string action;
            public string BTC_balance;
            public string USD_balance;
            public string exit;

            public ReportLine(string predictors_line, double MAVG_bid, string action, double BTC_balance, double USD_balance, double exit)
            {
                this.predictors_line = predictors_line;
                this.MAVG_bid = MAVG_bid.ToString();
                this.action = action;
                this.BTC_balance = BTC_balance.ToString();
                this.USD_balance = USD_balance.ToString();
                this.exit = exit.ToString();
            }
        }
    }
}