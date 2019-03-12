using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
namespace Экспертная_система
{
    public abstract class Algorithm
    {
        public Form1 form1;
        public string lastPrediction;
        public Hyperparameters h;
        public string mainFolder;

        public string args;
        public double stdDev;
        public double accuracy;
        public string name;
        public string modelName;
        public int modelLoadingDelay = 10 * 1000;
        public int pred_script_timeout = 5000;
        public Algorithm(Form1 form1, string modelName)
        {
            this.modelName = modelName;
            h = new Hyperparameters(form1, modelName);
            h.add("model_name", modelName);

            this.form1 = form1;
        }
        public void fillFilePaths()
        {
            mainFolder = form1.pathPrefix + "Экспертная система\\Экспертная система\\Алгоритмы прогнозирования\\" + name + "\\";
            Directory.CreateDirectory(mainFolder);
            string getPredictionFilePath = mainFolder + "get_prediction.py";
            h.add("get_prediction_script_path:" + getPredictionFilePath);
            string trainScriptPath = mainFolder + "train_script.py";
            h.add("train_script_path:" + trainScriptPath);
            string jsonFilePath = mainFolder + "json.txt";
            string predictionsFilePath = mainFolder + "predictions.txt";
            h.add("predictions_file_path", predictionsFilePath);
            h.add("json_file_path:" + jsonFilePath);
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
            args = "--json_file_path " + '"' + getValueByName("json_file_path") + '"';
            var get_prediction_script_path = getValueByName("get_prediction_script_path");
            predict_process = runProcess(get_prediction_script_path, args);
            predict_process_error_stream = predict_process.StandardError;
            predict_process_write_stream = predict_process.StandardInput;
            predict_process_read_stream = predict_process.StandardOutput;
        }

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
                        //  log("Этап загрузки модели в скрипте поточного прогнозирования пройден успешно");
                        Continue = true;
                    }
                }
                System.Threading.Thread.Sleep(100);
                inc++;
                if (inc > modelLoadingDelay / 100)
                {
                    string error = predict_process.StandardError.ReadToEnd();
                    log("Завис поток прогнозирования на этапе запуска скрипта и загрузки модели", Color.Red);
                    log(script_conclusion);
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
                    script_conclusion = script_conclusion + '\n' + buffer;
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
                            log("Процесс поточного прогнозирования  остановился на этапе записи входного вектора в поток: ", Color.Red);
                            log(script_conclusion);
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


            while (Continue == false && inc * 100 < pred_script_timeout)
            {
                var buffer = predict_process_read_stream.ReadLine();
                script_conclusion = script_conclusion + buffer + '\n';
                if (script_conclusion != null)
                {
                    if (script_conclusion.IndexOf("prediction:") != -1)
                    {
                        Continue = true;

                        //ПОЛНЫЙ ЛОГ ВЫПОЛНЕНИЯ СКРИПТА
                        log(script_conclusion);
                        script_conclusion = script_conclusion.Substring(script_conclusion.IndexOf("prediction:") + 11);

                        //ТОЛЬКО ПРОГНОЗ
                        // log(script_conclusion);
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
                }
                System.Threading.Thread.Sleep(100);
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
            Continue = true;
            return Y;

        }

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


        public async Task train()
        {
            if (h.getValueByName("input_file") == null)
            {
                log("файл датасета не задан");
            }
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
            args = "--json_file_path " + '"' + h.getValueByName("json_file_path") + '"';

            string response = await Task.Run(() => runPythonScript(getValueByName("train_script_path"), args));

            try
            {
                response = response.Substring(response.IndexOf('{'));
                Hyperparameters responseH = new Hyperparameters(response, form1);
                var avg = responseH.getValueByName("AVG");
                h.setValueByName("AVG", avg);
            }
            catch
            {
                log("Не удалось спарсить RESPONSE");
            }



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

            // return "обучение алгоритма " + name + "заверешно.";
        }


        public void getAccAndStdDev(string[] predictionsCSV)
        {
            predictionsCSV = Expert.skipEmptyLines(predictionsCSV);
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
            accuracy = Convert.ToDouble(rightCount) / Convert.ToDouble(rightCount + leftCount) * 100;
            stdDev = Math.Sqrt(sqrtSum / inc);

            if (double.IsNaN(accuracy))
                accuracy = 0;

            log("accuracy = " + accuracy.ToString() + " %");
          //  log("stdDev = " + stdDev.ToString());
           // log("accuracy/stdDev = " + (accuracy / stdDev).ToString());
            log("_____________________________________________");
            h.setValueByName("accuracy", accuracy.ToString().Replace(',', '.'));
            h.setValueByName("stdDev", stdDev.ToString().Replace(',', '.'));
            h.setValueByName("target_function", (accuracy).ToString().Replace(',', '.'));
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
            Process process = Process.Start(start);

            StreamReader standardOutputReader = process.StandardOutput;
            /*  int blockSize = 1;
             * char[] buffer = new char[blockSize];
             * int size = 0;
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
              }  */
            StreamReader errorReader = process.StandardError;
            string response = standardOutputReader.ReadToEnd();
            log(response);
            var error = "";
            error = errorReader.ReadToEnd();
            log(error);
            return response;
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
        public abstract void Save();
        public abstract void Open(Hyperparameters h);

        public static void CopyFiles(Hyperparameters h, string source, string destination)
        {
            if (destination[destination.Length - 1] != '\\')
                destination += destination + '\\';
            Directory.CreateDirectory(destination);
            if (source != destination)
            {
                foreach (string file in Directory.GetFiles(source))
                {
                    repeat1:
                    try
                    {
                        if (Path.GetFileName(file) != "json.txt")
                            File.Copy(file, destination + Path.GetFileName(file));
                    }
                    catch
                    { goto repeat1; }
                }
            }
            else
            {
            }
            //указание пути сохранения в параметрах
            h.setValueByName("save_folder", destination);
            string jsonFilePath = destination + "json.txt";
            h.setValueByName("json_file_path", jsonFilePath);
            string predictionsFilePath = destination + "predictions.txt";
            h.setValueByName("predictions_file_path", predictionsFilePath);
            File.WriteAllText(jsonFilePath, h.toJSON(0), System.Text.Encoding.Default);
        }

        public static void MoveFiles(Hyperparameters h, string source, string destination)
        {
            if (destination[destination.Length - 1] != '\\')
                destination += '\\';
            Directory.CreateDirectory(destination);
            if (source != destination)
            {
                foreach (string file in Directory.GetFiles(destination))
                {
                    File.Delete(file);
                }
                foreach (string file in Directory.GetFiles(source))
                {
                    repeat1:
                    try
                    {
                        if (Path.GetFileName(file) != "json.txt")
                            File.Copy(file, destination + Path.GetFileName(file));
                        else
                            File.Delete(file);
                    }
                    catch
                    { goto repeat1; }
                }
            }
            else
            {
            }
            //указание пути сохранения в параметрах
            h.setValueByName("save_folder", destination);
            string jsonFilePath = destination + "json.txt";
            h.setValueByName("json_file_path", jsonFilePath);
            string predictionsFilePath = destination + "predictions.txt";
            h.setValueByName("predictions_file_path", predictionsFilePath);
            File.WriteAllText(jsonFilePath, h.toJSON(0), System.Text.Encoding.Default);
        }

        public string getValueByName(string name)
        { return h.getValueByName(name); }
        public void setAttributeByName(string name, int value)
        { h.setValueByName(name, value); }
        public void setAttributeByName(string name, string value)
        { h.setValueByName(name, value); }

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
