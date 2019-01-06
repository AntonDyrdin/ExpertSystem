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
            logPath = h.getValueByName("logPath") + DateTime.Now.ToString().Replace(':', '-') + '-' + DateTime.Now.Millisecond.ToString() + ".txt";
            var logs = System.IO.Directory.GetFiles(h.getValueByName("logPath"));

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
        public Infrastructure(Form1 form1)
        {
            h = new Hyperparameters(form1);

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

    }
}
