using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
namespace Экспертная_система
{
    public abstract class Algorithm
    {
        public Form1 form1;
        public string lastPrediction;
        public Hyperparameters h;
        public string mainFolder;
        public string jsonFilePath;
        public string predictionsFilePath;
        public string getPredictionFilePath;
        public string trainScriptPath;
        public string args;
        public double stdDev;
        public double accuracy;

        public Algorithm(Form1 form1, string name)
        {
            h = new Hyperparameters(form1);
            this.form1 = form1;
            h.add("name", name);
            mainFolder = form1.pathPrefix + "Экспертная система\\Экспертная система\\Алгоритмы прогнозирования\\" + name + "\\";
            getPredictionFilePath = mainFolder + "get_prediction.py";
            h.add("get_prediction:" + getPredictionFilePath);
            trainScriptPath = mainFolder + "train_script.py";
            h.add("train_script_path:" + trainScriptPath);
            jsonFilePath = mainFolder + "json.txt";
            h.add("json_file_path:" + jsonFilePath);
            predictionsFilePath = mainFolder + "predictions.txt";
            h.add("predictions_file_path", predictionsFilePath);
            
            h.add("weights_file_path", mainFolder+ "weights.h5");


            h.add("save_folder:" + mainFolder);
        }

        private Process predict_process;
        [NonSerializedAttribute]
        private StreamReader predict_process_error_stream;
        [NonSerializedAttribute]
        private StreamWriter predict_process_write_stream;
        [NonSerializedAttribute]
        private StreamReader predict_process_read_stream;
        private string script_conclusion = "";
        private bool Continue = false;
        public void runGetPredictionScript()
        {
            args = "--json_file_path " + '"' + jsonFilePath + '"';
            predict_process = runProcess(getPredictionFilePath, args);
            predict_process_error_stream = predict_process.StandardError;
            predict_process_write_stream = predict_process.StandardInput;
            predict_process_read_stream = predict_process.StandardOutput;


        }
        //█=====================█
        //█////get_prediction///█
        //█=====================█
        //возвращает прогноз для одного окна
        //inputVector - матрица входных данных, в которой нулевой столбец [i,0] - прогнозируемая величина, а остальные столбцы - предикторы.
        //каждая строка - значения предикторов в j-ый временной интервал
        public double getPrediction(string[] input)
        {
            int inc = 0;
            //загрузка скрипта поточного прогнозирования, если он не запущен
            while (Continue == false)
            {
                var buffer = predict_process_read_stream.ReadLine();
                script_conclusion = script_conclusion + buffer;

                if (script_conclusion != null)
                {
                    if (script_conclusion.IndexOf("model loaded") != -1)
                    {
                        log("Этап загрузки модели в скрипте поточного прогнозирования пройден успешно");
                        Continue = true;
                    }
                }
                System.Threading.Thread.Sleep(100);
                inc++;
                if (inc > 10)
                {
                    string error = predict_process.StandardError.ReadToEnd();
                    log("Завис поток прогнозирования на этапе запуска скрипта и загрузки модели" , Color.Red);
                    log( script_conclusion );
                    log(error);
                    return -1000;
                }
            }
            Continue = false;
            for (int i = 1; i < input.Length; i++)
            {
                predict_process_write_stream.WriteLine(input[i]);
                while (Continue == false)
                {
                    string buffer = predict_process_read_stream.ReadLine();
                    if (script_conclusion.Contains("EXEPTION"))
                        log(script_conclusion);
                    script_conclusion = script_conclusion +'\n'+ buffer;
                    if (buffer != null)
                    {
                        if (buffer.IndexOf("next") != -1)
                        {
                            Continue = true;
                        }
                    }
                    else
                    {
                        if (predict_process.HasExited)
                        {
                            log("Процесс поточного прогнозирования  остановился на этапе записи входного вектора в поток: " ,Color.Red);
                            log( script_conclusion );
                            log(predict_process_error_stream.ReadToEnd());
                            return -1000;
                        }

                    }
                }
                Continue = false;
            }
            predict_process_write_stream.WriteLine("over");
            Continue = false;
            inc = 0;

            double pred_script_timeout = 5000;
            while (Continue == false && inc * 100 < pred_script_timeout)
            {
                var buffer = predict_process_read_stream.ReadLine();
                script_conclusion = script_conclusion + buffer + '\n';
                if (script_conclusion != null)
                {
                    if (script_conclusion.IndexOf("prediction:") != -1)
                    {
                        Continue = true;
                        log(script_conclusion);
                        script_conclusion = script_conclusion.Substring(script_conclusion.IndexOf("prediction:") + 11);
                    }
                    
                }
                else
                {
                    if (predict_process.HasExited)
                    {
                        log("Процесс поточного прогнозирования  остановился на этапе чтения Y из потока: ", Color.Red);
                        log(script_conclusion);
                        log(predict_process_error_stream.ReadToEnd());
                        return -1000;
                    }
                    System.Threading.Thread.Sleep(10);
                }
                inc++;
            }
            if (inc * 100 >= pred_script_timeout)
            {
                log("Завис поток прогнозирования на этапе получения Y ", Color.Red);
                log(script_conclusion);
                log(predict_process_error_stream.ReadToEnd());
                return -1000;
            }


            string[] output = script_conclusion.Split('\r', '[', ']', ' ', '\n');
            double Y = -1;
            for (int j = 0; j < output.Length; j++)
            {
                try
                {
                    output[j] = output[j].Replace('.', ',');

                    Y = Convert.ToDouble(output[j]);
                    break;
                }
                catch
                { }
            }
            return Y;
        }
        //█======================================================█
        //█            stop_get_prediction_script                █
        //█======================================================█
        public void stop_get_prediction_script()
        {
            Continue = false;
            if (!predict_process.HasExited)
            {
                predict_process.Kill();
            }
            else
                log(script_conclusion + predict_process_error_stream.ReadToEnd());
        }
        //█===================█
        //█//////train////////█
        //█===================█
        public System.Threading.Thread trainingThread;
        public string train()
        {
            if (h.getValueByName("input_file") == null)
            {
                log("файл датасета не задан");
            }

            File.WriteAllText(jsonFilePath, h.toJSON(0), System.Text.Encoding.Default);
            args = "--json_file_path " + '"' + jsonFilePath + '"';
            trainingThread = new System.Threading.Thread(trainingThreadMethod);
            trainingThread.Start();
            return "обучение алгоритма " + h.getValueByName("name") + "...";
        }


        public void trainingThreadMethod()
        {
            runPythonScript(trainScriptPath, args);

            string[] predictionsCSV = null;
            //попытка прочитать данные из файла, полученного из скрипта 
            try
            {
                predictionsCSV = File.ReadAllLines(h.getValueByName("predictions_file_path"));
            }
            catch { }
            //если данные имеются, то определить показатели точности прогнозирования
            if (predictionsCSV != null)
            {
                getAccAndStdDev(predictionsCSV);
            }
        }
        public void getAccAndStdDev(string[] predictionsCSV)
        {
            double sqrtSum = 0;
            int rightCount = 0;
            int leftCount = 0;
            int inc = 0;
            for (int i = 1; i < predictionsCSV.Length - 1; i++)
            {
                var features = predictionsCSV[i].Split(';');

                double predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 1].Replace('.', ','));
                double realValue = Convert.ToDouble(predictionsCSV[i + 1].Split(';')[Convert.ToInt16(h.getValueByName("predicted_column_index"))].Replace('.', ','));

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

            }
            accuracy = Convert.ToDouble(rightCount) / Convert.ToDouble(leftCount);
            stdDev = sqrtSum / inc;
            log("accuracy = " + accuracy.ToString());
            log("stdDev = " + stdDev.ToString());
        }
        public string runPythonScript(string scriptFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = form1.I.h.getValueByName("python_path");
            start.Arguments = '"' + scriptFile + '"' + " " + args;
            start.ErrorDialog = true;
            start.RedirectStandardError = true;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            // log("runPythonScript:" + start.FileName + " "+start.Arguments);
            Process process = Process.Start(start);
            process.ProcessorAffinity = new IntPtr(0x000F);

            int blockSize = 1;
            //Буфер для считываемых данных
            char[] buffer = new char[blockSize];
            StreamReader standardOutputReader = process.StandardOutput;
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

        private Process runProcess(string scriptFile, string args)
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
