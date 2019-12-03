using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
namespace Экспертная_система
{
    public abstract class Algorithm
    {
        public MainForm form1;
        public string lastPrediction;
        public Hyperparameters h;
        public string mainFolder;

        public string args;
        public double stdDev;
        public double accuracy;
        public string name;
        public string modelName;
        public int modelLoadingDelay = 5 * 1000;
        public int pred_script_timeout = 5000;
        public int NNscructNodeId;
        public int layersCount = 0;
        public Algorithm(MainForm form1, string modelName)
        {
            this.modelName = modelName;
            h = new Hyperparameters(form1, modelName);
            h.add("model_name", modelName);
            h.add("state", "created");
            h.add("parents", "создан в " + this.GetType().ToString());
            NNscructNodeId = h.add("name:NN_struct");

            this.form1 = form1;
        }

        public void addLayer(string layerType, parameter[] parameters)
        {
            layersCount++;

            int layerParentID = h.addByParentId(NNscructNodeId, "name:layer" + layersCount.ToString() + ",value:" + layerType);
            foreach (parameter param in parameters)
                if (param.type == parameterType.Const)
                {
                    h.addByParentId(layerParentID, param.name + ':' + param.caonstant);
                }
                else
                    if (param.type == parameterType.Numerical)
                {
                    h.addVariable(layerParentID, param.name, param.min, param.max, param.step, param.value);
                }
                else
                    if (param.type == parameterType.Categorical)
                {
                    h.addVariable(layerParentID, param.name, param.category, param.categories);
                }
        }

        public static Algorithm newInstance(Algorithm algorithm)
        {
            var constr = algorithm.GetType().GetConstructor(new Type[] { algorithm.form1.GetType(), (" asd").GetType() });
            var algInst = (Algorithm)constr.Invoke(new object[] { algorithm.form1, algorithm.GetType().ToString() });
            return algInst;
            /*   if (algorithm.GetType().Name == "LSTM_1")
                return new LSTM_1(algorithm.form1, "LSTM_1");
            if (algorithm.GetType().Name == "LSTM_2")
                return new LSTM_2(algorithm.form1, "LSTM_2");
            if (algorithm.GetType().Name == "ANN_1")
                return new ANN_1(algorithm.form1, "ANN_1");
            if (algorithm.GetType().Name == "CNN_1")
                return new CNN_1(algorithm.form1, "CNN_1");
            if (algorithm.GetType().Name == "CNN_1")
                return new CNN_1(algorithm.form1, "CNN_1");
            if (algorithm.GetType().Name == "FlexNN")
                return new FlexNN(algorithm.form1, "FlexNN");

            return null;*/
        }
        public void fillFilePaths()
        {
            mainFolder = form1.pathPrefix + "Экспертная система\\Экспертная система\\Алгоритмы прогнозирования\\" + name + "\\";
            Directory.CreateDirectory(mainFolder);
            string getPredictionFilePath = mainFolder + "get_prediction.py";
            h.add("get_prediction_script_path:" + getPredictionFilePath);
            string trainScriptPath = mainFolder + "train_script.py";
            h.add("train_script_path:" + trainScriptPath);
            string jsonFilePath = mainFolder + "h.json";
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
            log("Запуск скрипта прогнозирования..");
            args = "--json_file_path " + '"' + getValueByName("json_file_path") + '"';
            var get_prediction_script_path = getValueByName("get_prediction_script_path");

            // ЗАПУСК ПРОЦЕССА
            predict_process = form1.I.runProcess(get_prediction_script_path, args);
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
            string error = "";
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
                System.Threading.Thread.Sleep(1);
                inc++;
                if (inc > modelLoadingDelay / 1)
                {
                    error = predict_process.StandardError.ReadToEnd();
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
                    if (buffer != "" & buffer != null)
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
                            log(error);
                            return -1000;
                        }

                    }
                }
                Continue = false;
            }
            predict_process_write_stream.WriteLine("over");
            Continue = false;
            inc = 0;


            while (Continue == false && inc * 1 < pred_script_timeout)
            {
                var buffer = predict_process_read_stream.ReadLine();
                if (buffer != "" & buffer != null)
                    script_conclusion = script_conclusion + buffer + '\n';
                if (script_conclusion != null)
                {
                    if (script_conclusion.IndexOf("prediction:") != -1)
                    {
                        Continue = true;

                        //ПОЛНЫЙ ЛОГ ВЫПОЛНЕНИЯ СКРИПТА
                        //log(script_conclusion);
                        script_conclusion = script_conclusion.Substring(script_conclusion.IndexOf("prediction:") + 11);

                        //ТОЛЬКО ПРОГНОЗ
                        //   log(script_conclusion);
                    }

                }
                else
                {
                    if (predict_process.HasExited)
                    {
                        log("Процесс поточного прогнозирования  остановился на этапе чтения Y из потока: ", Color.Red);
                        log(script_conclusion);
                        log(predict_process_error_stream.ReadToEnd());
                        log(error);
                        return -1000;
                    }
                }
                System.Threading.Thread.Sleep(1);
                inc++;
            }
            if (inc * 1 >= pred_script_timeout)
            {
                log("Завис поток прогнозирования на этапе получения Y ", Color.Red);
                log(script_conclusion);
                log(predict_process_error_stream.ReadToEnd());
                log(error);
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

        public string trainingResponse;

        public void train()
        {
            trainingResponse = "";

            h.setValueByName("state", "обучение..");

            if (h.getValueByName("input_file") == null)
            {
                log("файл датасета не задан");
            }
            File.WriteAllText(h.getValueByName("json_file_path"), h.toJSON(0), System.Text.Encoding.Default);
            args = "--json_file_path " + '"' + h.getValueByName("json_file_path") + '"';

            trainingResponse = Task.Run(() => form1.I.executePythonScript(getValueByName("train_script_path"), args)).Result;

            try
            {
                trainingResponse = trainingResponse.Substring(trainingResponse.IndexOf('{'));
                Hyperparameters responseH = new Hyperparameters(trainingResponse, form1);
                //   var avg = responseH.getValueByName("AVG");
                //   if (avg != null)
                //       h.setValueByName("AVG", avg);

                h.setValueByName("state", "обучение завершено");
                log(responseH.getValueByName("response"));

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
                    try
                    {
                        getAccAndStdDev(predictionsCSV);
                    }
                    catch
                    {
                        log("Не удалось прочитать файл с тестовым прогнозом", Color.Red);
                        h.setValueByName("state", "Не удалось прочитать файл с тестовым прогнозом");
                    }
                }
                else
                {
                    log("Не удалось прочитать файл с тестовым прогнозом", Color.Red);
                    h.setValueByName("state", "Не удалось прочитать файл с тестовым прогнозом");
                }
            }
            catch
            {
                log("Не удалось спарсить RESPONSE");

                h.setValueByName("AVG", "-1");
                h.setValueByName("accuracy", "0");
                h.setValueByName("stdDev", "0");

                h.setValueByName("state", "ошибка при обучении");
            }
            // return "обучение алгоритма " + name + "заверешно.";
        }


        /* public virtual void getAccAndStdDev(string[] predictionsCSV)
         {
             predictionsCSV = Expert.skipEmptyLines(predictionsCSV);
           //  double sqrtSum = 0;
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
               //  sqrtSum += (realValue - predictedValue) * (realValue - predictedValue);
                 inc++;

             }
             accuracy = Convert.ToDouble(rightCount) / Convert.ToDouble(rightCount + leftCount) * 100;
            // stdDev = Math.Sqrt(sqrtSum / inc);

             if (double.IsNaN(accuracy))
                 accuracy = 0;

             log("accuracy = " + String.Format("{0:0.#####}", accuracy) + " %");
             //  log("stdDev = " + stdDev.ToString());
             // log("accuracy/stdDev = " + (accuracy / stdDev).ToString());
             log("_____________________________________________");
             h.setValueByName("accuracy", String.Format("{0:0.#####}", accuracy).Replace(',', '.'));
            // h.setValueByName("stdDev", stdDev.ToString().Replace(',', '.'));
             //    h.setValueByName("target_function", (accuracy).ToString().Replace(',', '.'));

             h.setValueByName("processed_by", System.Net.Dns.GetHostName());
         }*/
        public void getAccAndStdDev(string[] predictionsCSV)
        {

            bool showCharts = bool.Parse(h.getValueByName("show_train_charts"));

            /*     form1.vis.addParameter("bid_ask", Color.Red, 1000);
                 form1.vis.enableGrid = true;
                 form1.vis.parameters[0].showLastNValues = true;
                 form1.vis.parameters[0].window = 60;
                 form1.vis.parameters[0].functions.Add(new Function("ask", Color.Red));
                 form1.vis.parameters[0].functions.Add(new Function("bid", Color.Blue));*/



            predictionsCSV = Expert.skipEmptyLines(predictionsCSV);

            int rightCount = 0;
            int leftCount = 0;
            int inc = 0;

            if (h.getValueByName("wait_for_rise") != null)
            {
                int wait_for_rise = int.Parse(h.getValueByName("wait_for_rise"));

                int real_count_001 = 0;
                int real_count_010 = 0;
                int real_count_100 = 0;

                int catched_count_001 = 0;
                int catched_count_010 = 0;
                int catched_count_100 = 0;

                double integr_ask = 0;
                double integr_bid = 0;

                for (int i = 2; i < predictionsCSV.Length - 1; i++)
                {
                    var features = predictionsCSV[i].Split(';');
                    double which_class = 0.5;
                    double delta_bid = 0;
                    double delta_ask = 0;
                    double spread = Convert.ToDouble(predictionsCSV[i - 1].Split(';')[2].Replace('.', ','));

                    /*   integr_ask += Convert.ToDouble(predictionsCSV[i].Split(';')[1].Replace('.', ','));
                       integr_bid += Convert.ToDouble(predictionsCSV[i].Split(';')[0].Replace('.', ','));
                       form1.vis.addPoint(integr_ask, "ask");
                       form1.vis.addPoint(integr_bid, "bid");*/


                    for (int k = 0; k < wait_for_rise; k++)
                    {
                        if ((i + k) < (predictionsCSV.Length - 1))
                        {
                            delta_bid += Convert.ToDouble(predictionsCSV[i + k].Split(';')[0].Replace('.', ','));
                            if (delta_bid > spread * 1.2)
                            {

                                which_class = 1;
                                break;
                            }
                            delta_ask += Convert.ToDouble(predictionsCSV[i + k].Split(';')[1].Replace('.', ','));
                            if (delta_ask < -spread)
                            {

                                which_class = 0;
                                break;
                            }
                        }
                    }

                    double predictedValue = Convert.ToDouble(predictionsCSV[i - 1].Split(';')[3].Replace('.', ','));

                    if (predictedValue == 0.5)
                    {

                        if (which_class == 0.5)
                        {
                            rightCount++;
                            catched_count_100++;
                        }
                        else
                        { leftCount++; }
                    }

                    if (predictedValue == 1)
                    {
                        //   form1.vis.markLast("↗‾‾‾‾‾‾‾‾", "ask");
                        if (which_class == 1)
                        {
                            rightCount++;
                            catched_count_001++;
                        }
                        else
                        { leftCount++; }
                    }

                    if (predictedValue == 0)
                    {
                        // form1.vis.markLast("↘‾‾‾‾‾‾‾‾", "bid");

                        if (which_class == 0)
                        {
                            rightCount++;
                            catched_count_010++;
                        }
                        else
                        { leftCount++; }
                    }

                    if (which_class == 0.5)
                    {
                        real_count_100++;
                    }

                    if (which_class == 1)
                    {


                        real_count_001++;
                    }

                    if (which_class == 0)
                    {


                        real_count_010++;
                    }

                    inc++;

                    //  form1.vis.refresh();
                }
                log('\n' + "меток класса \"1\" : " + real_count_001.ToString() + "; найдено " + String.Format("{0:0.##}", (double)(catched_count_001) / real_count_001 * 100) + " %" +
           '\n' + "меток класса \"0\":" + real_count_010.ToString() + "; найдено " + String.Format("{0:0.##}", (double)(catched_count_010) / real_count_010 * 100) + " %" +
            '\n' + "меток класса \"0.5\":" + real_count_100.ToString() + "; найдено " + String.Format("{0:0.##}", (double)(catched_count_100) / real_count_100 * 100) + " %" +
           '\n' + "accuracy = " + String.Format("{0:0.#####}", accuracy) + " %");
            }
            else
            {
                double integr_real = 0;
                double integr_prediction = 0;

                double sqrtSum = 0;

                if (showCharts)
                {
                    form1.vis.clear();
                    form1.vis.addParameter("real/predictions integ", Color.Red, 100);
                    form1.vis.enableGrid = false;
                    form1.vis.parameters[0].showLastNValues = true;
                    form1.vis.parameters[0].window = 100;

                    form1.vis.parameters[0].functions.Add(new Function("real integr", Color.Red));
                    form1.vis.parameters[0].functions.Add(new Function("prediction integr", Color.Cyan));

                    form1.vis.addParameter("real/predictions", Color.Red, 900);

                    form1.vis.parameters[1].functions.Add(new Function("real", Color.Red));
                    form1.vis.parameters[1].functions.Add(new Function("prediction", Color.Cyan));

                    form1.vis.parameters[1].functions.Add(new Function("Графики должны совпадать", Color.DarkGray));
                    for (int k = 0; k < int.Parse(h.getValueByName("steps_forward")) - 1; k++)
                    {
                        form1.vis.addPoint(0, "prediction integr");
                        form1.vis.addPoint(0, "prediction");
                    }
                }
                int predicted_column_index = Convert.ToInt16(h.getValueByName("predicted_column_index"));

                bool is_cyclic_prediction = false;
                var features = predictionsCSV[1].Split(';');
                if (predictionsCSV[1].Split(';')[features.Length - 1].Contains("real"))
                { is_cyclic_prediction = true; }

                for (int i = 1; i < predictionsCSV.Length - 1; i++)
                {
                    double predictedValue;
                    double realValue;
                    if (is_cyclic_prediction)
                    {
                         predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 2].Replace('.', ','));
                         realValue = Convert.ToDouble(predictionsCSV[i + 1].Split(';')[predicted_column_index].Replace('.', ','));
                    }
                    else
                    {
                         predictedValue = Convert.ToDouble(predictionsCSV[i].Split(';')[features.Length - 1].Replace('.', ','));
                         realValue = Convert.ToDouble(predictionsCSV[i + 1].Split(';')[predicted_column_index].Replace('.', ','));
                    }
                    if (showCharts)
                    {
                        integr_real += Math.Tan((realValue - 0.5) * Math.PI);
                        form1.vis.addPoint(integr_real, "real integr");
                        form1.vis.addPoint(realValue, "real");

                        integr_prediction += Math.Tan((predictedValue - 0.5) * Math.PI);
                        form1.vis.addPoint(integr_prediction, "prediction integr");
                        form1.vis.addPoint(predictedValue, "prediction");


                        if (i == int.Parse(h.getValueByName("window_size") + int.Parse(h.getValueByName("steps_forward"))))
                            form1.vis.markLast("|", "real");
                    }

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

                stdDev = 1 - Math.Sqrt(sqrtSum / inc);
                h.setValueByName("stdDev", stdDev.ToString().Replace(',', '.'));

                /*   form1.vis.addCSV(h.getValueByName("predictions_file_path"),0, 500,  0);
                   form1.vis.addCSV(h.getValueByName("predictions_file_path"), 1, 500, 1);

                    form1.vis.parameters[1].showLastNValues = true;
                     form1.vis.parameters[1].window = 100;
                     form1.vis.parameters[2].showLastNValues = true;
                     form1.vis.parameters[2].window = 100;*/
                if (showCharts)
                    form1.vis.refresh();

                accuracy = Convert.ToDouble(rightCount) / Convert.ToDouble(rightCount + leftCount) * 100;

                //log(String.Format("{0:0.#####}", accuracy) + " %");
                log(String.Format("{0:0.#####}", stdDev));
            }
            accuracy = Convert.ToDouble(rightCount) / Convert.ToDouble(rightCount + leftCount) * 100;


            if (double.IsNaN(accuracy))
                accuracy = 0;



            //  log("stdDev = " + stdDev.ToString());
            // log("accuracy/stdDev = " + (accuracy / stdDev).ToString());
            log("_____________________________________________");
            h.setValueByName("accuracy", String.Format("{0:0.#####}", accuracy).Replace(',', '.'));

            //    h.setValueByName("target_function", (accuracy).ToString().Replace(',', '.'));

            h.setValueByName("processed_by", System.Net.Dns.GetHostName());
        }
        public abstract void Save();
        public abstract void Open(string jsonPath);

        public static void CopyFiles(Hyperparameters h, string source, string destination)
        {
            if (destination[destination.Length - 1] != '\\')
                destination += destination + '\\';
            Directory.CreateDirectory(destination);
            if (source.Replace("\\\\", "\\") != destination.Replace("\\\\", "\\"))
            {
                foreach (string file in Directory.GetFiles(source))
                {

                    if (Path.GetFileName(file) != "h.json")
                        File.Copy(file, destination + Path.GetFileName(file), true);
                }
            }

            //указание пути сохранения в параметрах
            h.setValueByName("save_folder", destination);
            string jsonFilePath = destination + "h.json";
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
                        if (Path.GetFileName(file) != "h.json")
                            File.Copy(file, destination + Path.GetFileName(file));
                        else
                            File.Delete(file);
                    }
                    catch
                    {
                        throw;
                        goto repeat1;
                    }
                }
            }
            else
            {
            }
            //указание пути сохранения в параметрах
            h.setValueByName("save_folder", destination);
            string jsonFilePath = destination + "h.json";
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

        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
        }
        public enum parameterType
        {
            Const,
            Numerical,
            Categorical
        }
        public class parameter
        {
            public parameter(string name, string caonstant)
            {
                this.name = name;
                this.caonstant = caonstant;
                type = parameterType.Const;
            }
            public parameter(string name, double min, double max, double step, double value)
            {
                this.name = name;
                this.value = value;
                this.min = min;
                this.max = max;
                this.step = step;
                type = parameterType.Numerical;
            }
            public parameter(string name, string category, string categories)
            {
                this.name = name;
                this.category = category;
                this.categories = categories;
                type = parameterType.Categorical;
            }
            public string name;
            public parameterType type;
            public double min;
            public double max;
            public double step;
            public double value;
            public string caonstant;
            public string category;
            public string categories;
        }
    }
}
