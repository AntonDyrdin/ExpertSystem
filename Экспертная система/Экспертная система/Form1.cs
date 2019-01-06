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

        public void Form1_Load(object sender, EventArgs e)
        {

            I = new Infrastructure(this);
            pathPrefix = I.h.getValueByName("pathPrefix");

            expert = new Expert(this);
            expert.algorithms.Add(new LSTM_1(this, "LSTM 1", 4));
            expert.prepareDataset(pathPrefix + @"Временные ряды\timeSeries4.txt", "<0>");
            //expert.trainAllAlgorithms(pathPrefix + @"Временные ряды\timeSeries4Short.txt", 0);
            // expert.Algorithms[0].h.draw(0, picBox, this, 20, 200);
            MultyParameterVisualizer vis = new MultyParameterVisualizer(picBox, this);

            vis.addParameter("dataset", Color.White, 100);
            vis.addParameter("normalized dataset", Color.White, 200);
            vis.parameters[0].functionDepth = 1;
            vis.parameters[1].functionDepth = 1;
            for (int i = 0; i < expert.dataset.GetLength(0); i++)
            {
                vis.addPoint(expert.dataset[i, 20], "dataset");
            }
            for (int i = 0; i < expert.normalizedDataset.GetLength(0); i++)
            {
                vis.addPoint(expert.normalizedDataset[i, 20], "normalized dataset");
            }

            vis.enableGrid = false;
            vis.refresh();
            expert.trainAllAlgorithms(pathPrefix + @"Временные ряды\timeSeries4.txt", 20);
            //  expert.Algorithms[0].h.draw(1, picBox, this, 20, 200);
            log(expert.algorithms[0].h.toJSON(1), Color.White);
        }


        public void log(String s, System.Drawing.Color col)
        {
            this.logDelegate = new Form1.LogDelegate(this.delegatelog);
            this.logBox.Invoke(this.logDelegate, this.logBox, s, col);
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

        public void picBox_Click(object sender, EventArgs e) { }


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
