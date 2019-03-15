using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        public string sourceDataFile;
        public System.Threading.Tasks.Task mainTask;
        public System.Threading.Thread mainThread;


        public string workFolder;
        public void Form1_Load(object sender, EventArgs e)
        {
            I = new Infrastructure(this);
            vis = new MultiParameterVisualizer(picBox, this);
            pathPrefix = I.h.getValueByName("path_prefix");
            log("");
            log("");

            mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { startSocket(); });


            workFolder = pathPrefix + "work_folder\\";
        }

        private const int port = 8888;
        private const string address = "127.0.0.1";

        public void startSocket()
        {
            TcpClient client = null;
            /* try
             {   */
            client = new TcpClient(address, port);
            NetworkStream stream = client.GetStream();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                var messageFromServer = recieve(stream);
                if (messageFromServer == "send_files")
                {
                    send(stream, "wait for files");
                    var json_file = recieveBytes(stream);
                    File.WriteAllBytes(workFolder + "json.txt", json_file);
                    send(stream, "file recieved");
                    var train_script_file = recieveBytes(stream);
                    File.WriteAllBytes(workFolder + "train_script.py", train_script_file);
                    send(stream, "file recieved");
                    var input_file = recieveBytes(stream);
                    File.WriteAllBytes(workFolder + "input_file.txt", input_file);
                    send(stream, "file recieved");
                }

                if (recieve(stream) == "train")
                {
                    Algorithm algorithm = new DefaultAlgorithmImpl(this, "Default");
                    algorithm.Open(new Hyperparameters(File.ReadAllText(workFolder + "json.txt"), this));
                    algorithm.mainFolder = workFolder;
                    algorithm.h.setValueByName("json_file_path", workFolder + "json.txt");
                    algorithm.h.setValueByName("predictions_file_path", workFolder + "predictions.txt");
                    algorithm.h.setValueByName("save_folder", workFolder);
                    algorithm.h.setValueByName("train_script_path", workFolder + "train_script.py");
                    algorithm.h.setValueByName("input_file", workFolder + "input_file.txt");
                    algorithm.h.setValueByName("path_prefix", pathPrefix);
                    log("START TRAINING");
                    algorithm.train().Wait();
                    if (algorithm.trainingReport.LastIndexOf("СКРИПТ ОБУЧЕНИЯ ") != -1)
                    {
                        send(stream, "success");
                        recieve(stream);
                        send(stream, algorithm.trainingReport);
                        recieve(stream);
                        client.Client.SendFile(algorithm.h.getValueByName("json_file_path")); 
                        recieve(stream);
                        System.Threading.Thread.Sleep(1000);
                         client.Client.SendFile(algorithm.h.getValueByName("predictions_file_path"));
                        recieve(stream);
                        System.Threading.Thread.Sleep(1000);
                        client.Client.SendFile(algorithm.h.getValueByName("save_folder") + "weights.h5");
                        recieve(stream);
                    }
                    else
                        send(stream, "Произошла ошибка при запуске скрипта обучения, подробности в консоли агента.");



                }
            }
            /*  }
              catch (Exception ex)
              {
                  Console.WriteLine(ex.Message);
              }
              finally
              {
                  client.Close();
              }  */
        }
        public void send(NetworkStream stream, string message)
        {
            var data = Encoding.Default.GetBytes(message);
            stream.Write(data, 0, data.Length);
            log("SEND: " + message);
        }
        public byte[] recieveBytes(NetworkStream stream)
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytesCount = 0;
            do
            {
                bytesCount = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Default.GetString(data, 0, bytesCount));
            }
            while (stream.DataAvailable);

            string message = builder.ToString();
            byte[] bytes = Encoding.Default.GetBytes(message);
            return bytes;
        }
        public string recieve(NetworkStream stream)
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Default.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            string message = builder.ToString();
            log("RECIEVE: " + message);
            return message;
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
