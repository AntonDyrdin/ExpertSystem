using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace Экспертная_система
{
    public class Infrastructure
    {
        public string logPath;
        public Hyperparameters h;
        public AgentManager agentManager;
        public int maxlogFilesCount = 10;
        MainForm form1;

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


        }
        public void showModeSelector()
        {
            modeSelector = new ModeSelector();
            modeSelector.Show();
            modeSelector.button1.Click += new EventHandler(ModeSelectorButtonClick);
        }
        void ModeSelectorButtonClick(object sender, EventArgs e)
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

        void newLog()
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
