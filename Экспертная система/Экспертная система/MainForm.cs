﻿using System;
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
        public int ENV = -1;
        public int OPT = 2;
        public int DEV = 0;
        public int TEST = 0;
        public int REAL = 1;

        Trader trader;
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

        string pair = "BTC_USDT";

        public bool maintenance_in_progress = false;
        public bool connection_lost = false;

        public bool stop_buying = false;

        double bid_top = -1;
        double ask_top = -1;

        internal Positions positions;
        public void Form1_Load(object sender, EventArgs e)
        {
            I = new Infrastructure(this);
            mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { main_thread(); });
            positions = new Positions();
            positions.Show();
        }

        public void main_thread()
        {   //////////////////////////////////////////////////////////////////////////////////////////
            // НАЧАЛО ////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////

            if (ENV == -1) { return; }

            expert = new Expert("Test Expert MAVG", this);
            expert.H.setValueByName("w1", 78 * 60);//78
            expert.H.setValueByName("w2", 2 * 60);//2
            expert.H.setValueByName("take_profit", 5);// 90 // > dues * 2 * bid_top
            expert.H.setValueByName("drawdown", 10); // 81
            expert.H.setValueByName("lot", "0.001");
            expert.H.setValueByName("purchase_limit_amount", 30);// 100
            expert.H.addVariable("purchase_limit_interval", 1, 12 * 60,2);// 6 * 60

            trader = new Trader(this, expert.H);

            mainThread = System.Threading.Thread.CurrentThread;

            Hyperparameters order_book;
            var api = new ExmoApi("k", "s");

            string response = "";

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

            //наименьшая цена, по которой можно покупать /\
            vis.parameters[0].functions.Add(new Function("ask_top", Color.Red));
            //наивысшая цена, по которой можно продать   \/
            vis.parameters[0].functions.Add(new Function("bid_top", Color.Cyan));


            vis.parameters[0].functions.Add(new Function("MAVG_bid", Color.Green));

            log(expert.H.toJSON(0), Color.Pink);

            int window = int.Parse(expert.H.getValueByName("w1")) + int.Parse(expert.H.getValueByName("w2"));

            ///////////////////////////////
            // Шаг обновления информации //
            int step = 1000;
            ///////////////////////////////

            List<string> rawInput = new List<string>();

            report = new Report("<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>", pathPrefix);

            rawInput.Add("<DATE_TIME>;<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>");

            if (!File.Exists(pair + "_exmo.txt"))
            {
                File.WriteAllLines(pair + "_exmo.txt", new string[]{
                    "<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>"});
            }
            else
            {
                if (ENV != REAL)
                {
                    string[] history = File.ReadAllLines(pair + "_exmo.txt");
                    if (history.Length > window)
                    {
                        for (int i = 0; i < window; i++)
                            rawInput.Add(history[history.Length - window + i]);
                    }
                    else
                    {
                        for (int i = 0; i < history.Length; i++)
                            rawInput.Add(history[i]);
                    }
                }
            }


            string action = "";
            int last_minute = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            while (true)
            {
                vis.enableGrid = false;
                string predictors_line;
                try
                {
                    response = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", pair }, { "limit", "1" } });
                    dynamic response_object = JObject.Parse(response);
                    var ask = (Newtonsoft.Json.Linq.JValue)response_object[pair].bid_quantity;
                    if (response.Length > 200)
                    {
                        response = response.Substring(0, 200);
                    }
                    if (!response.Contains("aintenance"))
                    {
                        if (connection_lost)
                        {
                            log(DateTime.Now.ToString() + "| Соединение восстановлено");
                            connection_lost = false;
                        }
                        maintenance_in_progress = false;
                        order_book = new Hyperparameters(response, this);
                        //<bid_top>;<bid_quantity>;<bid_amount>;<ask_top>;<ask_quantity>;<ask_amount>
                        predictors_line =
                            DateTime.Now.ToString() + ';'
                            + (Newtonsoft.Json.Linq.JValue)response_object[pair].bid_top + ';'
                            + (Newtonsoft.Json.Linq.JValue)response_object[pair].bid_quantity + ';'
                            + (Newtonsoft.Json.Linq.JValue)response_object[pair].bid_amount + ';'
                            + (Newtonsoft.Json.Linq.JValue)response_object[pair].ask_top + ';'
                            + (Newtonsoft.Json.Linq.JValue)response_object[pair].ask_quantity + ';'
                            + (Newtonsoft.Json.Linq.JValue)response_object[pair].ask_amount;

                        bid_top = Convert.ToDouble((Newtonsoft.Json.Linq.JValue)response_object[pair].bid_top);
                        ask_top = Convert.ToDouble((Newtonsoft.Json.Linq.JValue)response_object[pair].ask_top);

                        vis.addPoint(bid_top, "bid_top");
                        vis.addPoint(ask_top, "ask_top");

                        trader.checkOrders(bid_top, ask_top);

                        //запись входных данных
                        File.AppendAllText(pair + "_exmo.txt", predictors_line + "\n");

                        rawInput.Add(predictors_line);
                        if (rawInput.Count > window)
                        {
                            rawInput.RemoveAt(0);

                            double closeValue = bid_top;
                            // что скажет эксперт?
                            action = expert.getDecision(rawInput.ToArray());

                            if (action == "buy")
                            {
                                if (trader.deposit2 > ask_top * trader.lot)
                                {
                                    trader.createBuyOrder(ask_top);
                                }
                            }
                        }
                        if (DateTime.Now.Minute != last_minute)
                        {
                            vis.refresh();
                            last_minute = DateTime.Now.Minute;
                        }

                        vis.parameters[0].addPoint(expert.MAVG_bid, "MAVG_bid");
                        vis.addPoint(expert.MAVG_bid - ask_top, "Просадка");

                        /////////////////////////////////////
                        // ВЕДЕНИЕ ЖУРНАЛА
                        report.log(new ReportLine(predictors_line, expert.MAVG_bid, action, trader.deposit1, trader.deposit2, trader.deposit1 * bid_top + trader.deposit2));

                        vis.addPoint("EXIT", trader.deposit1 * bid_top + trader.deposit2);
                        vis.addPoint("USD", trader.deposit2);
                        vis.addPoint("BTC", trader.deposit1);

                        refresh_output();
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
            /* ENV = OPT;

              expert = new Expert("Test Expert MAVG", this);
              expert.H.addVariable("w1", 30, 300, 78);//78
              expert.H.addVariable("w2", 2,20,18);//2
              expert.H.setValueByName("take_profit", 80);// 90 // > dues * 2 * bid_top
              expert.H.addVariable("drawdown", 50, 80, 90); // 81
              expert.H.setValueByName("lot", "0.001");
              expert.H.setValueByName("purchase_limit_amount", 30);// 100
              expert.H.addVariable("purchase_limit_interval", 1, 12 * 60, 6 * 60);// 6 * 60

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
             */
            expert = Expert.Open(pathPrefix + "Optimization\\Test Expert MAVG\\Test Expert MAVG[0]", "Test Expert MAVG", this);
            expert.testExmo(date1: new DateTime(2020, 05, 23, 4, 0, 0),
                 date2: new DateTime(2020, 5, 24, 0, 22, 0),
                 rawDatasetFilePath: "BTC_USD_exmo.txt");
            //rawDatasetFilePath: "Журнал торговли27.04.2020 14-58-51.txt");
            vis.addParameter("bid", Color.White, 460);
            vis.parameters[0].functions.Add(new Function("bid_top", Color.Cyan));
            vis.parameters[0].functions.Add(new Function("MAVG_bid", Color.Green));
            vis.addParameter("BTC", Color.LightPink, 150);
            //  vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "test", "<bid_top>", "<bid_top>", 460, 0, 0, 50);
            //  vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "test", "<MAVG_bid>", "<MAVG_bid>", 460, 0, 0, 50);
            //  vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "<BTC_balance>", "<BTC_balance>", 150, 0, 0, 50);
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
            //  vis.addCSV("Журнал торговли27.04.2020 14-58-51.txt", "<EXIT>", "<EXIT>", 200, 0, 0, 40);
            log((expert.t.deposit2).ToString());

            vis.refresh();
        }
        public void refresh_output()
        {
            if (trader != null)
            {
                Invoke(new Action(() =>
                 {
                     deposit_1_value.Text = String.Format("{0:0.#####}", trader.deposit1);
                     deposit_2_value.Text = String.Format("{0:0.#####}", trader.deposit2);
                     sum_of_deposits.Text = String.Format("{0:0.#####}", (trader.deposit2 + (trader.deposit1 * bid_top)));
                     current_ask.Text = ask_top.ToString();
                     current_bid.Text = bid_top.ToString();

                     positions.refresh(trader.positions, bid_top, trader.dues);
                 }));
            }
        }
        private void stop_buying_click(object sender, EventArgs e)
        {
            stop_buying = !stop_buying;

            trader.buyOrders.Clear();

            if (stop_buying)
                b1.BackColor = Color.DarkRed;
            else
                b1.BackColor = Color.Black;
        }
        private void bye_button_Click(object sender, EventArgs e)
        {
            if (ask_top > 0 && ENV == DEV)
                trader.buy(ask_top);
        }
        private void sell_button_Click(object sender, EventArgs e)
        {
            if (bid_top > 0 && ENV == DEV)
                trader.sell(bid_top);
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
                            logBox.AppendText(DateTime.Now.Hour.ToString() + ':' + DateTime.Now.ToShortTimeString() + "|  " + s + '\n');
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
            mainThread.Abort();
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