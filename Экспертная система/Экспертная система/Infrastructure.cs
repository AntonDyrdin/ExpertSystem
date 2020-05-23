﻿using System;
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

        public int maxlogFilesCount = 20;
        private MainForm form1;

        public ModeSelector modeSelector;


        public Infrastructure(MainForm form1)
        {
            this.form1 = form1;
            if (form1.ENV == form1.REAL)
            {
                if (MessageBox.Show("Торговля на реальном счёте!", "Внимание!", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            h = new Hyperparameters(form1, "Infrastructure");

            form1.logBox.Text += (Environment.MachineName);
            if (Environment.MachineName == "DESKTOP-B3G20T0")
            {
                form1.logBox.Font = new System.Drawing.Font(form1.logBox.Font.FontFamily, 10);
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

            if (h.getValueByName("ENV") == "DEV")
            {
                form1.ENV = form1.DEV;
            }
            else if (h.getValueByName("ENV") == "REAL")
            {
                form1.ENV = form1.REAL;
            }
            else if (h.getValueByName("ENV") == "TEST")
            {
                form1.ENV = form1.TEST;
            }

            if (is_newPC | h.getValueByName("ENV") == null)
            {
                showModeSelector();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "notepad";
                psi.Arguments = "config.txt";
                Process.Start(psi);
            }
            else
            {
                newLog();

                // ИНИЦИАЛИЗАЦИЯ ГЛАВНОЙ ФОРМЫ

                DpiFix();
                form1.WindowState = FormWindowState.Minimized;

                if (logPath != null)
                {
                    form1.vis = new MultiParameterVisualizer(form1.picBox, form1);

                    mode = h.getValueByName("mode");

                    form1.pathPrefix = h.getValueByName("path_prefix");

                    form1.log("");
                    form1.WindowState = FormWindowState.Maximized;
                }
                else
                { }

                // form1.logBox.Font = new Font(form1.logBox.Font.FontFamily, 10);
                //form1.logBox.Text = File.ReadAllText("pic.txt");
                //form1.logBox.Text = form1.logBox.Text.Replace("\n\n", "\n");
            }
        }
        private string mode;

        public void runSelectedMode()
        {
            if (form1.ENV != -1)
            {

                form1.mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { form1.main_thread(); });
                //form1.mainTask = System.Threading.Tasks.Task.Factory.StartNew(() => { form1.optimization_thread(); });

                //executionProgressForm = new ExecutionProgress();
                //executionProgressForm.Show();

                //form1.showInpOutp = new TextBoxes();
                //form1.showInpOutp.Show();
            }
            else
            { form1.WindowState = FormWindowState.Minimized; }
        }
        public void showModeSelector()
        {
            modeSelector = new ModeSelector();
            modeSelector.Show();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        internal ExecutionProgress executionProgressForm;
        private List<Process> executingProcesses = new List<Process>();

        internal void newProcessToShow(Process process)
        {

            int width = 0;
            int height = 0;
            executionProgressForm.panel1.Invoke(new Action(() =>
            {

                width = executionProgressForm.panel1.Width;
                height = executionProgressForm.panel1.Height - 50;
            }));


            if (width > 100 & height > 100)
            {
                executingProcesses.Add(process);
                //process.WaitForExit(1);
                IntPtr fMWH = (IntPtr)0x00000000;
                executionProgressForm.panel1.Invoke(new Action(() =>
                {
                    int waitFor = 100;
                    int inc = 0;
                    while (fMWH == (IntPtr)0x00000000 & inc < waitFor)
                    {
                        try
                        {
                            fMWH = SetParent(process.MainWindowHandle, executionProgressForm.panel1.Handle);
                        }
                        catch
                        { break; }
                        System.Threading.Thread.Sleep(10);
                        inc++;
                    }
                }));
            }

            for (int i = 0; i < executingProcesses.Count; i++)
            {
                if (executingProcesses[i] == null)
                {
                    try
                    {
                        executingProcesses.Remove(executingProcesses[i]);
                        i--;
                    }
                    catch { }
                }
                else
                    if (executingProcesses[i].HasExited)
                {
                    executingProcesses.Remove(executingProcesses[i]);
                    i--;
                }
            }
            if (width > 100 & height > 100)
            {
                if (executingProcesses.Count == 1)
                    MoveWindow(executingProcesses[0].MainWindowHandle, 0, 0, width, executionProgressForm.panel1.Height - 50, true);
                if (executingProcesses.Count > 1)
                    for (int i = 0; i < executingProcesses.Count; i++)
                    {
                        try
                        {
                            if (i % 2 == 0)
                            {
                                MoveWindow(executingProcesses[i].MainWindowHandle, 0, height / executingProcesses.Count * 2 * (i / 2), width / 2, height / executingProcesses.Count * 2, true);
                            }
                            else
                            {
                                MoveWindow(executingProcesses[i].MainWindowHandle, width / 2, height / executingProcesses.Count * 2 * (i / 2), width / 2, height / executingProcesses.Count * 2, true);
                            }
                        }
                        catch { }
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

        public string executePythonScript(string scriptFile, string args)
        {

            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = form1.I.h.getValueByName("python_path");
            start.Arguments = '"' + scriptFile + '"' + " " + args;
            start.ErrorDialog = true;


            //start.RedirectStandardError = true;

            //start.RedirectStandardOutput = true;
            Process process = Process.Start(start);
            if (process == null)
            {
                MessageBox.Show(start.FileName + " не является интерпретатором Python");
                throw new Exception("wrong path to python.exe");
            }

            form1.I.newProcessToShow(process);

            // newLogBox(process.Id);
            while (!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
            }


            string logPath = args.Replace("--json_file_path ", "");

            logPath = logPath.Replace("\"", "");

            string response = "Результат выполнения скрипта не известен";
            string error = "";

            try
            {
                response = File.ReadAllText(Path.GetDirectoryName(logPath) + "\\log.txt", System.Text.Encoding.Default);
                error = File.ReadAllText(Path.GetDirectoryName(logPath) + "\\log_error.txt", System.Text.Encoding.Default);
            }
            catch (System.IO.IOException e)
            {
                form1.log(e.Message, Color.Red);
            }

            ///////////////////////////////////////////////////////////////
            ///////// ВЫВОД В КОНСОЛЬ РЕЗУЛЬТАТА ВЫПОЛНЕНИЯ СКРИПТА ///////
            form1.log(error, Color.Red);                                 ///    
           // var error = "";                                            ///
            //error = errorReader.ReadToEnd();                           ///
            //form1.log(error);                                          ///
            ///////////////////////////////////////////////////////////////
            if (error.Contains("No module named"))
            {
                MessageBox.Show(error);
                throw new Exception(error);
            }

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
            form1.log("Run process " + scriptFile + " " + args);
            return process;
        }
        private void newLog()
        {
            logPath = h.getValueByName("log_path") + DateTime.Now.ToString().Replace(':', '-') + '-' + DateTime.Now.Millisecond.ToString() + ".txt";
            string[] logs;
            try
            {
                logs = System.IO.Directory.GetFiles(h.getValueByName("log_path"));
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                System.IO.Directory.CreateDirectory(h.getValueByName("log_path"));
                logs = new string[0];
            }

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
