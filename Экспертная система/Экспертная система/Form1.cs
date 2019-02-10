using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace Экспертная_система
{
    public partial class Form1 : Form
    {
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
        public void Form1_Load(object sender, EventArgs e)
        {


            I = new Infrastructure(this);
            vis = new MultiParameterVisualizer(picBox, this);
            pathPrefix = I.h.getValueByName("path_prefix");
            log("");
            log("");

            expert = new Expert("Expert 1", this);
               expert.Open();


        /*   expert.algorithms.Add(new LSTM_1(this, "LSTM_1"));
                    expert.algorithms.Add(new ANN_1(this, "ANN_1"));
                    sourceDataFile = pathPrefix + @"Временные ряды\EURRUB.txt";
                    expert.H.add("input_file", expert.savePreparedDataset(sourceDataFile, "<TIME>;<TICKER>;<PER>;<DATE>;<VOL>"));
                    expert.H.add("path_prefix", pathPrefix);     
                   // expert.h().add("inputFile", pathPrefix + @"Временные ряды\EURRUB-dataset.txt");

                   //  expert.algorithms[0].getAccAndStdDev(File.ReadAllLines(expert.algorithms[0].predictionsFilePath));

                   // expert.test(new DateTime(2010, 2, 10), new DateTime(2010, 3, 10), sourceDataFile);

             /*             var task = System.Threading.Tasks.Task.Factory.StartNew(() =>      // внешняя задача
                       {
                           expert.trainAllAlgorithms();
                           expert.Save();
                       });     */

            // expert.trainAllAlgorithms();  
      expert.synchronizeHyperparameters();
            expert.synchronizeHyperparameters();
            expert.Save();

            expert.H.draw(0, picBox, this, 15, 150);

        }

        private void Hyperparameters_Click(object sender, EventArgs e)
        {
            expert.h().draw(0, picBox, this, 20, 200);
        }
        private void Charts_Click(object sender, EventArgs e)
        {
            double hidedPart = 0;

            vis.enableGrid = true;

            vis.clear();
            /*  vis.addParameter(expert.dataset1, 2, "dataset1", Color.White, 300);
              vis.addParameter(expert.dataset2, 2, "dataset2", Color.White, 300);
              vis.addParameter(expert.dataset3, 2, "dataset3", Color.White, 300); */
            double split_point = Convert.ToDouble(expert.h().getValueByName("split_point").Replace('.', ','));

            vis.addCSV(sourceDataFile, "Real close value", "<CLOSE>", 500, split_point + (1 - split_point) * hidedPart, -2);
            // vis.addCSV(sourceDataFile, "Real close value", expert.h().getValueByName("predicted_column_index"), 500, split_point + (1 - split_point) * hidedPart, -2);
            vis.addCSV(expert.algorithms[0].predictionsFilePath, "realVSpredictions", expert.h().getValueByName("predicted_column_index"), "real", 500, hidedPart, -1);
            vis.addCSV(expert.algorithms[0].predictionsFilePath, "realVSpredictions", "LAST_COLUMN", "predictions", 500, hidedPart, 0);

            vis.refresh();
        }


        private void ImgDataset_Click(object sender, EventArgs e)
        {
            visPredictions = new ImgDataset(expert.algorithms[0].predictionsFilePath, this);
            visPredictions.drawImgWhithPredictions(expert.algorithms[0].predictionsFilePath, "LAST_COLUMN", expert.h().getValueByName("split_point"), expert.h().getValueByName("predicted_column_index"));
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
            richTextBox.SelectionColor = col;
            richTextBox.AppendText(s + '\n');
            richTextBox.SelectionColor = Color.White;
            richTextBox.SelectionStart = richTextBox.Text.Length;
            var strings = new string[1];
            strings[0] = s;
            File.AppendAllLines(I.logPath, strings);
        }
        public void picBoxRefresh() { picBox.Refresh(); }
        public delegate void LogDelegate(RichTextBox richTextBox, string is_completed, Color col);
        public LogDelegate logDelegate;
        public delegate void StringDelegate(string is_completed);
        public delegate void VoidDelegate();
        public StringDelegate stringDelegate;
        public VoidDelegate voidDelegate;
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
