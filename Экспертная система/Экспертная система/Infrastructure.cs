using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Экспертная_система
{
    public class Infrastructure
    {
        public string logPath;
        public Hyperparameters h;
        public AgentManager agentManager;
        public int maxlogFilesCount = 10;
        private MainForm form1;

        public ModeSelector modeSelector;
        public Infrastructure(MainForm form1)
        {
            this.form1 = form1;
            h = new Hyperparameters(form1, "Infrastructure");

            form1.logBox.Text += (Environment.MachineName);
            if (Environment.MachineName == "DESKTOP-B3G20T0")
            {
                form1.logBox.Font = new System.Drawing.Font(form1.logBox.Font.FontFamily, 8);
            }
            form1.collectLogWhileItFreezed = new List<logItem>();

            /////////    чтене файла конфигурации    ///////////////////////
            var configLines = File.ReadAllLines("CONFIG.txt");

            bool is_newPC = true;
            for (int i = 0; i < configLines.Length; i++)
            {
                //параметры конфигурации начинаются со строки содержащей имя компа
                if (configLines[i].Contains(Environment.MachineName))
                {
                    is_newPC = false;
                    for (int j = i + 1; j < configLines.Length; j++)
                    {
                        //параметры конфигурации заканчиваются, когда встречается пустая строка
                        if (configLines[j] != "")
                        {
                            try
                            {
                                h.add(configLines[j]);
                            }
                            catch { }
                        }
                        else
                            break;
                    }
                    break;
                }
            }
            ////////////////////////////////////////////////////////

            if (is_newPC | h.getValueByName("mode") == null)
            {
                showModeSelector();
            }
            else
            {
                newLog();
            }

            executionProgressForm = new ExecutionProgress();
            executionProgressForm.Show();
        }
        public void showModeSelector()
        {
            modeSelector = new ModeSelector();
            modeSelector.Show();
            modeSelector.button1.Click += new EventHandler(ModeSelectorButtonClick);
        }

        private void ModeSelectorButtonClick(object sender, EventArgs e)
        {
            string mode = "";
            foreach (var control in modeSelector.groupBox1.Controls)
                if (control.GetType() == modeSelector.radioButton1.GetType())
                {
                    RadioButton rb = (RadioButton)control;
                    if (rb.Checked)
                    {
                        mode = rb.Text;
                    }
                }

            h.setValueByName("mode", mode);

            var configLines = File.ReadAllLines("CONFIG.txt").ToList();

            for (int i = 0; i < configLines.Count; i++)
            {
                //параметры конфигурации начинаются со строки содержащей имя компа
                if (configLines[i].Contains(Environment.MachineName))
                {
                    for (int j = i + 1; j < configLines.Count; j++)
                    {
                        if (configLines[j].Contains("mode"))
                        {
                            configLines[j] = "mode:" + mode;
                            break;
                        }
                        //параметры конфигурации заканчиваются, когда встречается пустая строка
                        if (configLines[j] != "")
                        { }
                        else
                        {
                            configLines.Insert(j, "mode:" + mode);
                            break;
                        }
                    }
                    break;
                }
            }
            File.WriteAllLines("CONFIG.txt", configLines.ToArray());
            Application.Exit();
        }
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        internal ExecutionProgress executionProgressForm;
        private List<Process> executingProcesses = new List<Process>();
        internal void newProcessToShow(Process process)
        {

            executingProcesses.Add(process);
            process.WaitForExit(1000);
            executionProgressForm.panel1.Invoke(new Action(() =>
            {
                SetParent(process.MainWindowHandle, executionProgressForm.panel1.Handle);

                int count = executingProcesses.Count;
                var width = executionProgressForm.panel1.Width;

                for (int i = 0; i < count; i++)
                    if (executingProcesses[i].HasExited)
                        executingProcesses.Remove(executingProcesses[i]);

                for (int i = 0; i < count; i++)
                    MoveWindow(executingProcesses[i].MainWindowHandle, width / count * i, 0, width / count, executionProgressForm.panel1.Height, true);
            }));
        }

        internal void deleteLogBox(int processID)
        {
            for (int i = 0; i < form1.panel1.Controls.Count; i++)
            {
                if (form1.panel1.Controls[i].Name == processID.ToString())
                {
                    form1.panel1.Controls[i].Dispose();
                    form1.panel1.Controls.Remove(form1.panel1.Controls[i]);
                    break;
                }
            }
        }

        internal void newLogBox(int processID)
        {
            var LogBox = new RichTextBox();
            LogBox.Name = processID.ToString();
            LogBox.BackColor = Color.Black;
            LogBox.ForeColor = Color.White;

            executionProgressForm.Invoke(new Action(() => { executionProgressForm.panel1.Controls.Add(LogBox); }));

            int count = executionProgressForm.panel1.Controls.Count;
            var height = executionProgressForm.panel1.Height;
            var width = executionProgressForm.panel1.Width;
            for (int i = 0; i < executionProgressForm.panel1.Controls.Count; i++)
            {
                if (executionProgressForm.panel1.Controls[i].InvokeRequired)
                    executionProgressForm.Invoke(new Action(() =>
                    {

                        executionProgressForm.panel1.Controls[i].Location = new Point(width / count * i, 0);
                        executionProgressForm.panel1.Controls[i].Size = new Size(width / count, height);

                    }));
                else
                {
                    executionProgressForm.panel1.Controls[i].Location = new Point(width / count * i, 0);
                    executionProgressForm.panel1.Controls[i].Size = new Size(width / count, height);
                }
            }
        }
        internal void processLog(string processID, StreamReader streamReader)
        {

            int blockSize = 1;
            char[] buffer = new char[blockSize];
            int size = 0;
            string line = "";
            Task task = new Task(() =>
            {
                size = streamReader.Read(buffer, 0, blockSize);
                line += new string(buffer);
                while (size > 0)
                {
                    size = streamReader.Read(buffer, 0, blockSize);
                    line += new string(buffer);
                    if (line.Contains("\n"))
                    {
                        processLog(processID, line);
                        line = "";
                    }
                }
            });
            task.Start();
        }

        internal void processLog(string processID, string s)
        {
            bool isNew = true;
            for (int i = 0; i < executionProgressForm.panel1.Controls.Count; i++)
            {
                if (executionProgressForm.panel1.Controls[i].Name == processID.ToString())
                {
                    var logBox = (RichTextBox)executionProgressForm.panel1.Controls[i];

                    executionProgressForm.Invoke(new Action(() =>
                    {

                        logBox.AppendText(s);
                        logBox.SelectionStart = logBox.Text.Length;
                        logBox.ScrollToCaret();

                    }));

                    isNew = false;
                    break;
                }
            }
            if (isNew)
            {
                newLogBox(int.Parse(processID));
                processLog(processID, s);
            }
        }

        /// <summary>
        /// Исправление блюра при включенном масштабировании в ОС windows 8 и выше
        /// </summary>
        public static void DpiFix()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
        }

        /// <summary>
        /// WinAPI SetProcessDPIAware 
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public void startAgentManager()
        {
            agentManager = new AgentManager(this.form1);
        }
        public string executePythonScript(string scriptFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = form1.I.h.getValueByName("python_path");
            start.Arguments = '"' + scriptFile + '"' + " " + args;
            start.ErrorDialog = true;
            //start.RedirectStandardError = true;
             //start.UseShellExecute = false;
            // start.CreateNoWindow = true;
            //start.RedirectStandardOutput = true;
            Process process = Process.Start(start);
            string pid = process.Id.ToString();
            form1.I.newProcessToShow(process);
            // newLogBox(process.Id);

            while (!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
            }
            string response = File.ReadAllText(Path.GetDirectoryName(scriptFile) + "\\log.txt", System.Text.Encoding.Default);

            /*   StreamReader standardOutputReader = process.StandardOutput;

               string response = "";

               int blockSize = 1;
               char[] buffer = new char[blockSize];
               int size = 0;
               string line = "";
               size = standardOutputReader.Read(buffer, 0, blockSize);
               line += new string(buffer);
               while (size > 0)
               {
                   size = standardOutputReader.Read(buffer, 0, blockSize);
                   line += new string(buffer);
                   if (line.Contains("\n"))
                   {
                       response += line;
                       processLog(pid, line);
                       line = "";
                   }
               }
               */
          //  StreamReader errorReader = process.StandardError;
            ///////////////////////////////////////////////////////////////
            ///////// ВЫВОД В КОНСОЛЬ РЕЗУЛЬТАТА ВЫПОЛНЕНИЯ СКРИПТА ///////
            // log(response);                                            ///    
           // var error = "";                                             ///
            //error = errorReader.ReadToEnd();                            ///
            //form1.log(error);                                           ///
            ///////////////////////////////////////////////////////////////


            return response;
        }

        public Process runProcess(string scriptFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = form1.I.h.getValueByName("python_path");
            start.Arguments = '"' + scriptFile + '"' + " " + args;
            start.ErrorDialog = true;
            start.CreateNoWindow = true;
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            Process process = Process.Start(start);
            return process;
        }
        private void newLog()
        {
            logPath = h.getValueByName("log_path") + DateTime.Now.ToString().Replace(':', '-') + '-' + DateTime.Now.Millisecond.ToString() + ".txt";
            var logs = System.IO.Directory.GetFiles(h.getValueByName("log_path"));

            if (logs.Length > maxlogFilesCount)
            {
                System.IO.FileInfo[] logFilesInfo = new FileInfo[logs.Length];
                for (int i = 0; i < logs.Length; i++)
                {
                    logFilesInfo[i] = new FileInfo(logs[i]);
                }

                for (int i = 0; i < logs.Length; i++)
                {
                    var youngest = logFilesInfo[i];
                    int youngestInd = i;
                    for (int j = i; j < logs.Length; j++)
                    {
                        if (youngest.CreationTime < logFilesInfo[j].CreationTime)
                        {
                            youngest = logFilesInfo[j];
                            youngestInd = j;
                        }
                    }
                    var temp = logFilesInfo[i];
                    logFilesInfo[i] = logFilesInfo[youngestInd];
                    logFilesInfo[youngestInd] = temp;
                }
                for (int i = maxlogFilesCount; i < logs.Length; i++)
                {
                    logFilesInfo[i].Delete();
                }
            }
        }
    }
    public class logItem
    {
        public string text;
        public System.Drawing.Color color;

        public logItem(string text, Color color)
        {
            this.text = text;
            this.color = color;
        }
    }
}
