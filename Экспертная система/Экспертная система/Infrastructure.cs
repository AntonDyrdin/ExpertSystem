using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Экспертная_система
{
    public class Infrastructure
    {
        public string logPath;
        public Hyperparameters h;
        public int maxlogFilesCount = 10;
        public Infrastructure(Form1 form1)
        {
            h = new Hyperparameters(form1);

            form1.logBox.Text += (Environment.MachineName);
            if (Environment.MachineName == "DESKTOP-B3G20T0")
            {
                form1.trackBar1.SetBounds(1000, 1000, 10, 10);
                form1.panel1.SetBounds(500, 0, 700, 650);
                form1.logBox.SetBounds(0, 0, 500, 650);
                form1.picBox.SetBounds(500, 0, 700, 1000);
            }
            /////////чтене файла конфигурации///////////////////////
            var configLines = File.ReadAllLines("config.txt");

            for (int i = 0; i < configLines.Length; i++)
            {
                //параметры конфигурации начинаются со строки содержащей имя компа
                if (configLines[i].Contains(Environment.MachineName))
                {
                    for (int j = i + 1; j < configLines.Length; j++)
                    {
                        //параметры конфигурации заканчиваются, когда встречается пустая строка
                        if (configLines[j] != "")
                        {
                            if (h.getValueByName(configLines[j].Split(':')[0]) == null)
                                h.add(configLines[j]);
                        }
                        else
                            break;
                    }
                    break;
                }
            }
            ////////////////////////////////////////////////////////


            newLog();
        }
        public Infrastructure(Hyperparameters h, Form1 form1)
        {
            form1.logBox.Text += (Environment.MachineName);

            /////////чтене файла конфигурации///////////////////////
            var configLines = File.ReadAllLines("config.txt");

            for (int i = 0; i < configLines.Length; i++)
            {
                //параметры конфигурации начинаются со строки содержащей имя компа
                if (configLines[i].Contains(Environment.MachineName))
                {
                    for (int j = i + 1; j < configLines.Length; j++)
                    {
                        //параметры конфигурации заканчиваются, когда встречается пустая строка
                        if (configLines[j] != "")
                            h.add(configLines[j]);
                        else
                            break;
                    }
                    break;
                }
            }
            ////////////////////////////////////////////////////////
            newLog();
            
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
}
