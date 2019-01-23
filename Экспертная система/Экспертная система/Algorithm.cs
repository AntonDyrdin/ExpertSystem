using System;
using System.Diagnostics;
using System.IO;
namespace Экспертная_система
{
    public abstract class Algorithm
    {
        public Form1 form1;
        public string lastPrediction;
        public Hyperparameters h;

        public Algorithm(Form1 form1, string name)
        {
            h = new Hyperparameters(form1);
            this.form1 = form1;
            h.add("name", name);
        }

        //█=====================█
        //█              get_prediction              █
        //█=====================█
        //возвращает прогноз для одного окна
        //inputVector - матрица входных данных, в которой нулевой столбец [i,0] - прогнозируемая величина, а остальные столбцы - предикторы.
        //каждая строка - значения предикторов в j-ый временной интервал
        public string getPrediction(double[,] inputVector)
        {
            // lastPrediction=f(inputVector)

            return "ошибка: метод не реализован";
        }

        //█===================█
        //█                  train                    █
        //█===================█
        public System.Threading.Thread trainingThread;
        public string train()
        {
            if (h.getValueByName("inputFile") == null)
            {
                log("файл датасета не задан");
            }
            scriptFile = h.getValueByName("trainScriptPath");
            string jsonFilePath = System.IO.Path.GetDirectoryName(scriptFile) + "\\json.txt";
            string predictionsFilePath = System.IO.Path.GetDirectoryName(scriptFile) + "\\predictions.txt";
            h.add("predictionsFilePath", predictionsFilePath);
            File.WriteAllText(jsonFilePath, h.toJSON(0), System.Text.Encoding.Default);
            args = "--jsonFile " + '"' + jsonFilePath + '"';
            trainingThread = new System.Threading.Thread(trainingThreadMethod);
            trainingThread.Start();
            return "обучение алгоритма " + h.getValueByName("name") + "...";
        }

        public string scriptFile;
        public string args;
        public double stdDev;
        public double accuracy;
        public void trainingThreadMethod()
        {
            runPythonScript(scriptFile, args);

            string[] predictionsCSV = null;
            //попытка прочитать данные из файла, полученного из скрипта 
            try
            {
                predictionsCSV = File.ReadAllLines(h.getValueByName("predictionsFilePath"));
            }
            catch { }
            //если данные имеются, то определить показатели точности прогнозирования
            if (predictionsCSV != null)
            {
                getAccAndStdDev(predictionsCSV);
            }
        }
        public void getAccAndStdDev(string [] predictionsCSV)
        {
            double sqrtSum = 0;
            int rightCount = 0;
            int leftCount = 0;
            int inc = 0;
            double predictedValue = Convert.ToDouble(predictionsCSV[1].Split(';')[Convert.ToInt16(h.getValueByName("predicted_column_index"))].Replace('.', ','));
            for (int i = 1; i < predictionsCSV.Length-1; i++)
            {
                var features = predictionsCSV[i].Split(';');
              
                double realValue = Convert.ToDouble(features[features.Length - 1].Replace('.', ','));
                if (realValue > 0.5 && predictedValue > 0.5)
                { rightCount++; }
                else
                      if (realValue < 0.5 && predictedValue < 0.5)
                { rightCount++; }
                else
                    if (realValue > 0.5 && predictedValue < 0.5)
                { leftCount++; }
                else
                if (realValue < 0.5 && predictedValue > 0.5)
                { leftCount++; }
                sqrtSum += (realValue - predictedValue) * (realValue - predictedValue);
                inc++;
                predictedValue  = Convert.ToDouble(features[Convert.ToInt16(h.getValueByName("predicted_column_index"))].Replace('.', ','));
            }
            accuracy = Convert.ToDouble(rightCount ) / Convert.ToDouble(leftCount);
            stdDev = sqrtSum / inc;
        }
        public string runPythonScript(string scriptFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = form1.I.h.getValueByName("pythonPath");
            start.Arguments = '"' + scriptFile + '"' + " " + args;
            start.ErrorDialog = true;
            start.RedirectStandardError = true;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            // log("runPythonScript:" + start.FileName + " "+start.Arguments);
            Process process = Process.Start(start);
            process.ProcessorAffinity = new IntPtr(0x000F);

            int blockSize = 5;
            //Буфер для считываемых данных
            char[] buffer = new char[blockSize];
            StreamReader standardOutputReader = process.StandardOutput;
            int size = 0;
            string line = "";
            size = standardOutputReader.Read(buffer, 0, blockSize);
            while (size > 0)
            {
                size = standardOutputReader.Read(buffer, 0, blockSize);
                line += new string(buffer);
                if (line.Contains("\n"))
                {
                    log(line);
                    line = "";
                }
            }
            StreamReader errorReader = process.StandardError;
            //string result = reader.ReadToEnd();
            log(errorReader.ReadToEnd());
            //   log(standardOutputReader.ReadToEnd());
            return "";
        }

        public string getValueByName(string name)
        { return h.getValueByName(name); }
        public void setAttributeByName(string name, int value)
        { h.setAttributeByName(name, value); }
        public void setAttributeByName(string name, string value)
        { h.setAttributeByName(name, value); }
        public void variate(string name)
        { h.variate(name); }
        public void log(String s, System.Drawing.Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }
        public void log(String s)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, System.Drawing.Color.White);
        }
    }
}
