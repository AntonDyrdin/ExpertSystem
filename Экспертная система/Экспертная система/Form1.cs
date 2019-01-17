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
        private MultyParameterVisualizer vis;
        public void Form1_Load(object sender, EventArgs e)
        {


            I = new Infrastructure(this);
            vis = new MultyParameterVisualizer(picBox, this);
            expert = new Expert(this);
            log("");
            log("");

            pathPrefix = I.h.getValueByName("pathPrefix");
            expert.algorithms.Add(new LSTM_1(this, "LSTM 1"));
          //  expert.algorithms[0].h.add("inputFile", expert.prepareDataset(pathPrefix + @"Временные ряды\test.txt", ""));
            expert.algorithms[0].h.add("inputFile", pathPrefix + @"Временные ряды\test-dataset.txt");
            expert.algorithms[0].h.add("pathPrefix", pathPrefix);
            expert.trainAllAlgorithms();
  
         
          //  expert.Algorithms[0].h.draw(1, picBox, this, 20, 200);
            // log(expert.algorithms[0].h.toJSON(0), Color.White);
        }


        public void log(String s, System.Drawing.Color col)
        {
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
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

        public void trackBar1_Scroll(object sender, EventArgs e)
        {
        }

        public void picBox_Click(object sender, EventArgs e)
        {
            expert.algorithms[0].h.draw(0, picBox, this, 15, 150);
        }

        private void picBox_DoubleClick(object sender, EventArgs e)
        {     //  vis.addParameter(expert.dataset, 2, "dataset", Color.White, 300);
            //  vis.addParameter(expert.normalizedDataset2, 2, "normalized[2]", Color.White, 300);
            //  vis.addParameter(expert.normalizedDataset2, 0, "normalized[0]", Color.White, 300);
            //  vis.addParameter(expert.normalizedDataset2, 1, "normalized[1]", Color.White, 300);
            // vis.addParameter(expert.normalizedDataset2, 3, "normalized[3]", Color.White, 300); 
            vis.addCSV(@"C:\Users\anton\Рабочий стол\MAIN\predictions.txt",Convert.ToInt16(expert.algorithms[0].h.getValueByName("predicted_column_index")), 300);
            vis.addCSV(@"C:\Users\anton\Рабочий стол\MAIN\predictions.txt","LAST_COLUMN", 300);
            vis.enableGrid = false;
            vis.refresh();
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
