﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace Экспертная_система
{
    public partial class Form1 : Form
    {
       // C:\Users\Антон\AppData\Local\Theano\compiledir_Windows-10-10.0.17134-SP0-Intel64_Family_6_Model_158_Stepping_9_GenuineIntel-3.6.1-64\lock_dir
        public Infrastructure I;
        public Form1()
        {
            InitializeComponent();

        }
        public string pathPrefix;
        public Expert expert;
        private MultiParameterVisualizer vis;
        private ImgDataset visPredictions;
        public string sourceDataFile;
        public System.Threading.Tasks.Task mainTask;
        public System.Threading.Thread mainThread;
        public void Form1_Load(object sender, EventArgs e)
        {
            I = new Infrastructure(this);
            vis = new MultiParameterVisualizer(picBox, this);
            pathPrefix = I.h.getValueByName("path_prefix");
            log("");
            log("");

           // expert = new Expert("Эксперт 1", this);

          //    mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { buildAndTrain(); });
             mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { TEST(); });


        }
        public void TEST()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            expert =  Expert.Open("Эксперт 1", this);
            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.H.replaceStringInAllValues(expert.H.getValueByName("path_prefix"), pathPrefix);
            expert.synchronizeHyperparameters();
            expert.test(new DateTime(2010, 3, 1), new DateTime(2010, 3, 30), sourceDataFile);
            expert.H.draw(0, picBox, this, 15, 150);
            expert.Save();
        }
        public void buildAndTrain()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            expert = new Expert("Эксперт 1", this);

            expert.Add(new ANN_1(this, "ANN_1[1]"));
            expert.Add(new ANN_1(this, "ANN_1[2]"));
            expert.Add(new ANN_1(this, "ANN_1[3]"));
            expert.Add(new LSTM_1(this, "LSTM_1[1]"));
            expert.Add(new LSTM_1(this, "LSTM_1[2]"));
            expert.algorithms[0].setAttributeByName("number_of_epochs", 8);
            /*  expert.algorithms[0].setAttributeByName("window_size", 30); 
              expert.algorithms[0].setAttributeByName("batch_size", 200);
              expert.algorithms[0].setAttributeByName("split_point", "0.99");    */
            /* for(int i=0;i<20;i++)
             expert.Add(new ANN_1(this, "ANN_1_["+i.ToString()+"]"));

             for (int i = 0; i < 20; i++)
                 expert.Add(new LSTM_1(this, "LSTM_1_[" + i.ToString() + "]"));*/


            sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
            expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<TIME>;<TICKER>;<PER>;<DATE>;<VOL>"));
            expert.H.add("path_prefix", pathPrefix);
            expert.H.add("drop_columns:<TIME>;<TICKER>;<PER>;<DATE>;<VOL>");
            expert.H.add("predicted_column_index:3");
            expert.H.add("name:show_train_charts,value:False");

            expert.synchronizeHyperparameters();
            expert.trainAllAlgorithms(false);
            expert.synchronizeHyperparameters();
            expert.H.draw(0, picBox, this, 15, 150);
            expert.Save();
         /*   try
            {
                Charts_Click(null, null);
            }
            catch { }   */
        }
        private void Hyperparameters_Click(object sender, EventArgs e)
        {
            expert.H.draw(0, picBox, this, 15, 150);
        }
        private void Charts_Click(object sender, EventArgs e)
        {
            double hidedPart = 0.99;

            vis.enableGrid = true;

            vis.clear();
            /*  vis.addParameter(expert.dataset1, 2, "dataset1", Color.White, 300);
              vis.addParameter(expert.dataset2, 2, "dataset2", Color.White, 300);
              vis.addParameter(expert.dataset3, 2, "dataset3", Color.White, 300); */
            double split_point = Convert.ToDouble(expert.h().getValueByName("split_point").Replace('.', ','));

            vis.addCSV(sourceDataFile, "Real close value", "<CLOSE>", 500, split_point + (1 - split_point) * hidedPart, -2);
            // vis.addCSV(sourceDataFile, "Real close value", expert.h().getValueByName("predicted_column_index"), 500, split_point + (1 - split_point) * hidedPart, -2);
            vis.addCSV(expert.algorithms[0].getValueByName("predictions_file_path"), "realVSpredictions", expert.h().getValueByName("predicted_column_index"), "real", 500, hidedPart, -1);
            vis.addCSV(expert.algorithms[0].getValueByName("predictions_file_path"), "realVSpredictions", "LAST_COLUMN", "predictions", 500, hidedPart, 0);

            vis.refresh();
        }


        private void ImgDataset_Click(object sender, EventArgs e)
        {
            visPredictions = new ImgDataset(expert.algorithms[0].getValueByName("predictions_file_path"), this);
            visPredictions.drawImgWhithPredictions(expert.algorithms[0].getValueByName("predictions_file_path"), "LAST_COLUMN", expert.h().getValueByName("split_point"), expert.h().getValueByName("predicted_column_index"));
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
        public void log(String s)
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
        {   try
            {
                mainThread.Abort();
            } catch { }
        }
        /*  ТЕСТ РАБОТЫ КЛАССА Hyperparameters

Hyperparameters h = new Hyperparameters(this);
h.addByParentId(0, "name:person,firstName:Антон,age:21");
var friendsId = h.addByParentId(1, "name:friends");
h.addByParentId(friendsId, "name:person,firstName:Сергей,age:26");
var friendsId1 = h.addByParentId(h.addByParentId(friendsId, "name:person,firstName:Ксения,age:18"), "name:friends");
h.addByParentId(friendsId1, "name:person,firstName:Павел,age:24");
h.addByParentId(friendsId1, "name:person,firstName:Карина,age:21");
h.addByParentId(friendsId, "name:person,firstName:Анна,age:24");
h.addByParentId(friendsId, "name:person,firstName:Александр,age:21");
var friendsList = h.getNodesByparentID(friendsId);
foreach (Node friend in friendsList)
{ log(friend.getAttributeValue("firstName"), Color.White); }
var dateId = h.addByParentId(1, "name:date,day:24,month:november,year:2018");
h.addByParentId(1, "day:24");

h.draw(1, picBox, this);

log(h.toJSON(1), Color.White);*/

        //System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackTrace().ToString());

    }
}
