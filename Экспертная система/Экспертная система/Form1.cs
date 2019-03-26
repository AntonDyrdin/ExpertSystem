using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Экспертная_система
{
    public partial class Form1 : Form
    {
        // C:\Users\Антон\AppData\Local\Theano\compiledir_Windows-10-10.0.17134-SP0-Intel64_Family_6_Model_158_Stepping_9_GenuineIntel-3.6.1-64\lock_dir


        public Form1()
        {
            InitializeComponent();

        }
        public string pathPrefix;
        public Infrastructure I;
        public Expert expert;
        public MultiParameterVisualizer vis;
        public AgentManager agentManager;
        private ImgDataset visPredictions;
        public string sourceDataFile;
        public System.Threading.Tasks.Task mainTask;
        public System.Threading.Thread mainThread;
        private AlgorithmOptimization AO;
        private Algorithm algorithm;
        ExpertOptimization optimization;
        public void Form1_Load(object sender, EventArgs e)
        {
            I = new Infrastructure(this);
            vis = new MultiParameterVisualizer(picBox, this);
            I.startAgentManager();
            pathPrefix = I.h.getValueByName("path_prefix");
            log("");
            log("");

            //mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { buildAndTest(); });
            //     mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { algorithmOptimization(); });
            mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { expertOptimization(); });
        }
        public void expertOptimization()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = new Expert("Эксперт 1", this);

            expert.Add(new LSTM_1(this, "LSTM_1[0]"));
            expert.Add(new ANN_1(this, "ANN_1[0]"));

            expert.H.add("normalize:true");
            expert.H.add("input_file", pathPrefix + @"Временные ряды\EURRUB-dataset.txt");
            expert.H.add("path_prefix", pathPrefix);
            expert.H.add("drop_columns:none");
            expert.H.add("predicted_column_index:3");
            expert.H.add("name:show_train_charts,value:False");

            expert.Save();

            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";

            optimization = new ExpertOptimization(expert, this, 4, 10, 0.5, 10, new DateTime(2017, 8, 1), new DateTime(2017, 8, 10), sourceDataFile);
            optimization.run();
        }
        public void buildAndTest()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = new Expert("Эксперт 1", this);

            expert.Add(new LSTM_1(this, "LSTM_1[0]"));
            expert.Add(new LSTM_2(this, "LSTM_2[0]"));
            expert.Add(new ANN_1(this, "ANN_1[0]"));


            expert.algorithms[0].Open(pathPrefix + @"Optimization\LSTM_1\LSTM_1[0]\json.txt");
            expert.algorithms[1].Open(pathPrefix + @"Optimization\LSTM_2\LSTM_2[0]\json.txt");
            expert.algorithms[2].Open(pathPrefix + @"Optimization\ANN_1\ANN_1[0]\json.txt");

            expert.H.add("normalize:true");
            expert.H.add("input_file", pathPrefix + @"Временные ряды\EURRUB-dataset.txt");
            expert.H.add("path_prefix", pathPrefix);
            expert.H.add("drop_columns:none");
            expert.H.add("predicted_column_index:3");
            expert.H.add("name:show_train_charts,value:False");

            //  expert.synchronizeHyperparameters();
            //  expert.trainAllAlgorithms(true);
            expert.synchronizeHyperparameters();
            //  expert.H.draw(0, picBox, this, 15, 150);
            expert.Save();

            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.test(new DateTime(2017, 6, 1), new DateTime(2017, 9, 30), sourceDataFile);


        }
        public void algorithmOptimization()
        {
            expert = new Expert("Эксперт 1", this);
            mainThread = System.Threading.Thread.CurrentThread;
            algorithm = new LSTM_2(this, "LSTM_2");

            //  sourceDataFile = pathPrefix + @"Временные ряды\EURRUB_long_min.txt";
            //  expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<TIME>;<TICKER>;<PER>;<DATE>;<VOL>", true));

            algorithm.h.add("normalize:true");
            algorithm.h.add("input_file", pathPrefix + @"Временные ряды\EURRUB-dataset.txt");
            algorithm.h.add("path_prefix", pathPrefix);
            algorithm.h.add("drop_columns:none");
            algorithm.h.add("predicted_column_index:3");
            algorithm.h.add("name:show_train_charts,value:False");
            AO = new AlgorithmOptimization(algorithm, this, 8, 20, 0.25, 500);
            AO.run();
            // algorithm.h.draw(0, picBox, this, 15, 150);
            // algorithm.Save();
        }
        public void TEST()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            expert = Expert.Open("Эксперт 1", this);
            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.H.replaceStringInAllValues(expert.H.getValueByName("path_prefix"), pathPrefix);
            expert.synchronizeHyperparameters();
            expert.test(new DateTime(2017, 8, 1), new DateTime(2017, 9, 30), sourceDataFile);
            expert.H.draw(0, picBox, this, 15, 150);
            expert.Save();
        }
        public void buildAndTrain()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = new Expert("Эксперт 1", this);

            /*  expert.Add(new ANN_1(this, "ANN_1[1]"));
              expert.Add(new ANN_1(this, "ANN_1[2]"));
              expert.Add(new ANN_1(this, "ANN_1[3]")); */
            expert.Add(new LSTM_1(this, "LSTM_1[1]"));
            /* expert.Add(new LSTM_1(this, "LSTM_1[2]"));
             expert.Add(new LSTM_2(this, "LSTM_2[1]"));  */

            expert.algorithms[0].setAttributeByName("number_of_epochs", 50);
            /*  expert.algorithms[0].setAttributeByName("window_size", 30); 
              expert.algorithms[0].setAttributeByName("batch_size", 200);
              expert.algorithms[0].setAttributeByName("split_point", "0.99");    */
            /* for(int i=0;i<20;i++)
             expert.Add(new ANN_1(this, "ANN_1_["+i.ToString()+"]"));

             for (int i = 0; i < 20; i++)
                 expert.Add(new LSTM_1(this, "LSTM_1_[" + i.ToString() + "]"));*/

            //EURRUB
            /* sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
             expert.H.add("normalize:true");
             expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<TIME>;<TICKER>;<PER>;<DATE>;<VOL>"));
             expert.H.add("path_prefix", pathPrefix);
             expert.H.add("drop_columns:<TIME>;<TICKER>;<PER>;<DATE>;<VOL>");  
             expert.H.add("predicted_column_index:3");
             expert.H.add("name:show_train_charts,value:False");      */

            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.H.add("normalize:true");
            expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "none", Convert.ToBoolean(expert.H.getValueByName("normalize"))));
            expert.H.add("path_prefix", pathPrefix);
            expert.H.add("drop_columns:none");
            expert.H.add("predicted_column_index:3");
            expert.H.add("name:show_train_charts,value:False");

            expert.synchronizeHyperparameters();
            expert.trainAllAlgorithms(false);
            expert.synchronizeHyperparameters();
            expert.H.draw(0, picBox, this, 15, 150);
            expert.Save();
        }
        private void Hyperparameters_Click(object sender, EventArgs e)
        {
            if (expert != null)
                expert.H.draw(0, picBox, this, 15, 150);
            if (AO != null)
                AO.A.draw(0, picBox, this, 15, 150);
            if (algorithm != null)
                algorithm.h.draw(0, picBox, this, 15, 150);
        }
        private void Charts_Click(object sender, EventArgs e)
        {

            double hidedPart = 0.9;
            vis.enableGrid = true;
            vis.clear();
            Algorithm a = new LSTM_1(this, "asdasd");
            a.h.add("predicted_column_index:4");
            a.getAccAndStdDev(File.ReadAllLines(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_2\LSTM_2[0]\predictions.txt"));
            vis.addCSV(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_2\LSTM_2[0]\predictions.txt", "realVSpredictions", "<CLOSE>", 1000, hidedPart, -1);
            // vis.addCSV(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_1\LSTM_1[0]\predictions.txt", "realVSpredictions", "<DATEandTIME>", "0.5", 1000, hidedPart, -1);
            // vis.addCSV(sourceDataFile, "Real close value", expert.h().getValueByName("predicted_column_index"), 500, split_point + (1 - split_point) * hidedPart, -2);
            vis.addCSV(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_2\LSTM_2[0]\predictions.txt", "realVSpredictions", "LAST_COLUMN", "predictions", 1000, hidedPart, 0);

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


        private void ImgDataset_Click(object sender, EventArgs e)
        {
            visPredictions = new ImgDataset(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_1\LSTM_1[0]\predictions.txt", this);
            visPredictions.drawImgWhithPredictions(@"D:\Anton\Desktop\MAIN\Optimization\LSTM_1\LSTM_1[0]\predictions.txt", "LAST_COLUMN", "0", "1");
            /* visPredictions = new ImgDataset(expert.algorithms[0].getValueByName("predictions_file_path"), this);
             visPredictions.drawImgWhithPredictions(expert.algorithms[0].getValueByName("predictions_file_path"), "LAST_COLUMN", expert.h().getValueByName("split_point"), expert.h().getValueByName("predicted_column_index"));
             */
        }


        public void trackBar1_Scroll(object sender, EventArgs e) { }
        public void picBox_Click(object sender, EventArgs e) { }
        private void picBox_DoubleClick(object sender, EventArgs e) { }
        private void logBox_TextChanged(object sender, EventArgs e) { }
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
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
            if (checkBox1.Checked)
                this.logBox.Invoke(this.logDelegate, this.logBox, s, Color.Black);
            else
                this.logBox.Invoke(this.logDelegate, this.logBox, s, col);
            var strings = new string[1];
            strings[0] = s;
            File.AppendAllLines(I.logPath, strings);
        }
        public void log(string s)
        {
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
            this.logBox.Invoke(this.logDelegate, this.logBox, s, Color.White);
            var strings = new string[1];
            strings[0] = s;
            File.AppendAllLines(I.logPath, strings);
        }
        public void delegatelog(RichTextBox richTextBox, String s, Color col)
        {
            try
            {
                richTextBox.SelectionColor = col;
                richTextBox.AppendText(s + '\n');
                richTextBox.SelectionColor = Color.White;
                richTextBox.SelectionStart = richTextBox.Text.Length;
                var strings = new string[1];
                strings[0] = s;
                File.AppendAllLines(I.logPath, strings);
            }
            catch { }
        }
        public void picBoxRefresh() { picBox.Refresh(); }
        public delegate void LogDelegate(RichTextBox richTextBox, string is_completed, Color col);
        public LogDelegate logDelegate;
        public delegate void StringDelegate(string is_completed);
        public delegate void VoidDelegate();
        public StringDelegate stringDelegate;
        public VoidDelegate voidDelegate;

        private void RedClick(object sender, EventArgs e)
        {
            mainThread.Abort();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
               I.agentManager.TCPListener.Stop();
            }
            catch { }
            try
            {
                mainThread.Abort();
            }
            catch { }
        }

        //System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackTrace().ToString());

    }
}
