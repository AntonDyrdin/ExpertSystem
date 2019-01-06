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

        public Algorithm(Form1 form1, string name, int windowSize)
        {
            h = new Hyperparameters(form1);
            this.form1 = form1;
            h.add("name", name);
            h.add("windowSize", windowSize);
        }

        //█==========================================█
        //█              get_prediction              █
        //█==========================================█
        //возвращает прогноз для одного окна
        //inputVector - матрица входных данных, в которой нулевой столбец [i,0] - прогнозируемая величина, а остальные столбцы - предикторы.
        //каждая строка - значения предикторов в j-ый временной интервал
        public string getPrediction(double[,] inputVector)
        {
            // lastPrediction=f(inputVector)

            return "ошибка: метод не реализован";
        }
        //█===========================================█
        //█                  train                    █
        //█===========================================█
        public string train(string inputFile)
        {
            if (h.getValueByName("inputFile") == null)
            {
                h.add("inputFile:" + inputFile);
            }
            File.WriteAllText(form1.pathPrefix + "\\json.txt", h.toJSON(0), System.Text.Encoding.Default);
            string result = runPythonScript(h.getValueByName("trainScriptPath"), "--jsonFile "+ '"' + form1.pathPrefix + "json.txt" + '"');
            return "обучение алгоритма " + h.getValueByName("name") + " - "+ result;
        }


        private string runPythonScript(string scriptFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName =form1.I.h.getValueByName("pythonPath");
            start.Arguments = '"' + scriptFile + '"' + " " +args ;
            start.ErrorDialog = true;
            start.RedirectStandardError = true;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
           // log("runPythonScript:" + start.FileName + " "+start.Arguments);
            Process process = Process.Start(start);
            process.ProcessorAffinity = new IntPtr(0x000F);
            StreamReader reader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;
            string result = reader.ReadToEnd();
            result = result + '\n' + errorReader.ReadToEnd();
            return result;
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
            form1.logBox.Invoke(form1.logDelegate, form1.logBox,  s, System.Drawing.Color.White);
        }
    }
}
