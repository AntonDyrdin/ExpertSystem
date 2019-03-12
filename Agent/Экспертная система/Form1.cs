using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Fleck;
using System.Collections.Generic;
using System.Linq;

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


            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            IWebSocketConnection webSocket=new WebSocketConnection(
          var socket = allSockets[0];
           var server = new WebSocketServer("ws://0.0.0.0:80");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    log("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    log("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    log(message);
                    allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                };
            });


            
       /*     while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }                                      */

        }
        public void algorithmOptimization()
        {
        }
        public void TEST()
        {
        }
        public void buildAndTrain()
        {
        }
        private void Hyperparameters_Click(object sender, EventArgs e)
        {
            expert.algorithms[0].h.draw(0, picBox, this, 15, 150);
        }
        private void Charts_Click(object sender, EventArgs e)
        {
            
        }


        private void ImgDataset_Click(object sender, EventArgs e)
        {
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
        {
            try
            {
                mainThread.Abort();
            }
            catch { }
        }
    }
}
