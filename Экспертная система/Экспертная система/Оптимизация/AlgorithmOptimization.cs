using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Экспертная_система
{
    internal class AlgorithmOptimization
    {
        public enum TargetFunctionType
        {
            ACCURACY,
            STDDEV
        }
        public string name;
        public Algorithm algorithm;
        public Hyperparameters[] population;
        public Hyperparameters A;
        public MainForm form1;
        public int mutation_rate;
        public int population_value;
        public double elite_ratio;
        private MultiParameterVisualizer variablesVisualizer;
        private List<string> variablesNames;
        private List<int>[] variablesIDs;
        private int iterarions = 0;
        private Random r;
        private AgentManager agentManager;
        private bool showIndividsParameters = true;
        private bool showOnlyBestIndividsParameters = true;
        private int architecture_variation_rate;

        TargetFunctionType target_function_type;
        public int screenshotIterationTimer = 5;
        public bool multiThreadTraining = true;
        public int multiThreadTrainingRATE = 4;
        private int test_count = 6;
        public AlgorithmOptimization(Algorithm algorithm, MainForm form1, int population_value, int mutation_rate, int architecture_variation_rate, double elite_ratio, int iterarions, int test_count, TargetFunctionType target_function_type)
        {
            r = new Random();
            this.iterarions = iterarions;
            this.test_count = test_count;
            this.form1 = form1;
            this.algorithm = algorithm;
            this.elite_ratio = elite_ratio;
            this.mutation_rate = mutation_rate;
            this.population_value = population_value;
            this.architecture_variation_rate = architecture_variation_rate;
            variablesVisualizer = new MultiParameterVisualizer(form1.picBox, form1);
            population = new Hyperparameters[population_value];
            variablesNames = new List<string>();
            variablesIDs = new List<int>[population_value];

            this.target_function_type = target_function_type;
            for (int i = 0; i < population_value; i++)
                variablesIDs[i] = new List<int>();

            this.agentManager = form1.I.agentManager;

            name = algorithm.name;

            try { Directory.Delete(form1.pathPrefix + "Optimization\\" + name, true); } catch { }

            variablesVisualizer.enableGrid = false;
            variablesVisualizer.addParameter("best individ", Color.LightCyan, 400);
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 800);
            //добавление переменных в  MultiParameterVisualizer
            for (int i = 0; i < population_value; i++)
                variablesVisualizer.parameters[1].functions.Add(new Function(" [" + i.ToString() + "]", valueToColor(0, population_value, population_value - i - 1)));
            //  recurciveVariableAdding(algorithm.h, 0, "0");
            //добавление первых точек в  MultiParameterVisualizer
            // variableChangeMonitoring();
            //   variablesVisualizer.addPoint(0, "target_function");
            variablesVisualizer.refresh();


        }

        private Color valueToColor(double min, double max, double val)
        {
            double R = 0;
            double G = 0;
            double B = 0;

            R = (max - val) / (max - min) * 255;
            G = (Math.Abs(max / 2 - val)) / (max / 2) * 255;
            B = (val - min) / (max - min) * 255;

            return Color.FromArgb(255, Convert.ToInt32(R), Convert.ToInt32(G), Convert.ToInt32(B));
        }

        public int opt_inc;
        private int inc = 0;
        private void optimization()
        {
            form1.Invoke(new Action(() =>
            {
                OptimizationView AOV = new OptimizationView(this);
                AOV.Show();
            }));

            while (opt_inc < iterarions)
            {
                var now = new DateTimeOffset(DateTime.Now);
                var start = now.ToUnixTimeSeconds();
                opt_inc++;
                inc++;
                iteration_of_optimization();
                log(opt_inc.ToString() + "_ITERATION COMPLETE " + TimeSpan.FromSeconds((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()) - start).ToString(), Color.Green);
                if (inc == screenshotIterationTimer)
                {
                    inc = 0;
                    Bitmap screenShot = (Bitmap)form1.picBox.Image.Clone();
                    int h = 3000;
                    if (screenShot.Height < 3001)
                        h = screenShot.Height;
                    for (int i = 0; i < screenShot.Width; i++)
                        for (int j = 0; j < h; j++)
                        {
                            Color c = screenShot.GetPixel(i, j);
                            if (c == Color.FromArgb(0, 0, 0, 0))
                            { screenShot.SetPixel(i, j, Color.Black); }
                        }
                    screenShot.Save(opt_inc.ToString() + "_iteration.bmp");
                }

            }
        }
        public void iteration_of_optimization()
        {


            if (opt_inc == 1)
            {
                string new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[0]" + "\\";
                Directory.CreateDirectory(new_save_folder);
                algorithm.h.setValueByName("save_folder", new_save_folder);
                string predictionsFilePath = new_save_folder + "predictions.txt";
                algorithm.h.setValueByName("predictions_file_path", predictionsFilePath);
                File.WriteAllText(new_save_folder + "h.json", algorithm.h.toJSON(0), System.Text.Encoding.Default);
                algorithm.h.setValueByName("show_train_charts", "False");

                algorithm.train();

                switch (target_function_type)
                {
                    case TargetFunctionType.ACCURACY:
                        {
                            algorithm.h.setValueByName("target_function", algorithm.h.getValueByName("accuracy"));
                            break;
                        }
                    case TargetFunctionType.STDDEV:
                        {
                            algorithm.h.setValueByName("target_function", algorithm.h.getValueByName("stdDev"));
                            break;
                        }
                }

                for (int i = 0; i < population_value; i++)
                {
                    population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1);
                    population[i].setValueByName("code", i.ToString());
                    population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");
                    new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\";
                    Algorithm.CopyFiles(population[i], algorithm.h.getValueByName("save_folder"), new_save_folder);

                    population[i].setValueByName("parents", "создан из " + population[i].nodes[0].name() + "[0]");
                }

                /*if (showOnlyBestIndividsParameters)
                {
                    recurciveVariableAdding(population[0],0, 0, "0");
                }
                else
                {*/
                for (int i = 0; i < population_value; i++)
                {
                    recurciveVariableAdding(population[i], i, 0, population[i].getValueByName("code"));
                }
                // }
            }
            else
            {
                rewriteVariableIDs();

                //kill and concieve
                kill_and_conceive();

                rewriteVariableIDs();

                //mutation
                if (form1.mutationRate != 0)
                    mutation_rate = form1.mutationRate;
                for (int i = 0; i < mutation_rate; i++)
                {
                    mutation();
                }

                for (int i = 0; i < architecture_variation_rate; i++)
                    variateArchitecture();

                rewriteVariableIDs();

                if (form1.test_count != 0)
                    test_count = form1.test_count;
                //   ВВЕДЕНО МНОГОКРАТНОЕ ТЕСТИРОВНИЕ ИНДИВИДОВ ВСЕХ КАТЕГОРИЙ ДЛЯ ПОВЫШЕНИЯ ПОВТОРЯЕМОСТИ РЕЗУЛЬТАТОВ

                //   target_functions - матрица результатов тестирования,
                //   где номер строки (первый индекс (i)) - индекс индивида, а номер столбца (второй индекс (j)) - итерация тестирования
                double[,] target_functions = new double[population_value, test_count];


                //  for (int i = Convert.ToInt16(Math.Round(population_value * elite_ratio)); i < population_value; i++)

                for (int tc = 0; tc < test_count; tc++)
                {
                    var now2 = new DateTimeOffset(DateTime.Now);
                    var start2 = now2.ToUnixTimeSeconds();
                    if (multiThreadTraining)
                    {
                        if (form1.multiThreadTrainingRATE != 0)
                            multiThreadTrainingRATE = form1.multiThreadTrainingRATE;
                        log("multiThreadTrainingRATE = " + multiThreadTrainingRATE.ToString(), Color.Yellow);
                        if (multiThreadTrainingRATE > population_value)
                            multiThreadTrainingRATE = population_value;
                        if (agentManager.agents.Count == 0)
                        {
                            for (int begin = 0; begin < population_value; begin += multiThreadTrainingRATE)
                            {
                                var now1 = new DateTimeOffset(DateTime.Now);
                                var start1 = now1.ToUnixTimeSeconds();
                                int end = 0;
                                if (begin + multiThreadTrainingRATE >= population_value)
                                {
                                    end = population_value;
                                }
                                else
                                {
                                    end = begin + multiThreadTrainingRATE;
                                }
                                List<Algorithm> algorithms = new List<Algorithm>();
                                for (int i = begin; i < end; i++)
                                {
                                    Algorithm alg = Algorithm.newInstance(algorithm);
                                    alg.h = population[i].Clone();
                                    algorithms.Add(alg);

                                    population[i].setValueByName("state", "обучение..");
                                }

                                List<Thread> trainThreads = new List<Thread>();


                                foreach (Algorithm alg in algorithms)
                                {
                                    Thread t = new Thread(new ThreadStart(alg.train));
                                    trainThreads.Add(t);
                                    t.Start();
                                    // trainTasks[algorithms.IndexOf(alg)] = algorithms[algorithms.IndexOf(alg)].train();
                                }

                                foreach (var t in trainThreads)
                                    t.Join();

                                for (int i = begin; i < end; i++)
                                {
                                    population[i] = algorithms[i - begin].h.Clone();

                                    switch (target_function_type)
                                    {
                                        case TargetFunctionType.ACCURACY:
                                            {
                                                target_functions[i, tc] = Convert.ToDouble(population[i].getValueByName("accuracy").Replace('.', ','));
                                                break;
                                            }
                                        case TargetFunctionType.STDDEV:
                                            {
                                                target_functions[i, tc] = Convert.ToDouble(population[i].getValueByName("stdDev").Replace('.', ','));
                                                break;
                                            }
                                    }
                                }
                                log((end).ToString() + '/' + population_value.ToString() + " training comlete" + TimeSpan.FromSeconds((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()) - start1).ToString(), Color.LimeGreen);
                            }
                        }
                        else
                        {
                            // ПАРАЛЛЕЛЬНОЕ ВЫЧИСЛЕНИЕ
                            for (int i = 0; i < population_value; i++)
                            {
                                agentManager.tasks.Add(new AgentTask("train", population[i].Clone()));
                            }

                            Task.Factory.StartNew(() => { agentManager.work(); }).Wait();

                            for (int i = 0; i < population_value; i++)
                            {
                                population[i] = agentManager.tasks[i].h.Clone();

                                switch (target_function_type)
                                {
                                    case TargetFunctionType.ACCURACY:
                                        {
                                            target_functions[i, tc] = Convert.ToDouble(population[i].getValueByName("accuracy").Replace('.', ','));
                                            break;
                                        }
                                    case TargetFunctionType.STDDEV:
                                        {
                                            target_functions[i, tc] = Convert.ToDouble(population[i].getValueByName("stdDev").Replace('.', ','));
                                            break;
                                        }
                                }
                            }
                            agentManager.tasks.Clear();
                        }
                    }
                    else
                    {
                        // SINGLE THREAD - устаревший код
                        /*   for (int i = 0; i < population_value; i++)
                           {
                               algorithm.h = population[i].Clone();
                               algorithm.train().Wait();
                               population[i] = algorithm.h.Clone();
                               target_functions[i, tc] = Convert.ToDouble(population[i].getValueByName("accuracy").Replace('.', ','));
                           }*/
                    }
                    for (int i = 0; i < population_value; i++)
                    {
                        population[i].setValueByName("state", (tc + 1).ToString() + '/' + test_count.ToString() + " " + population[i].getValueByName("state"));
                    }
                    log((tc + 1).ToString() + '/' + test_count.ToString() + " test comlete" + TimeSpan.FromSeconds((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()) - start2).ToString(), Color.LimeGreen);

                }

                //вычисление итоговых значений критерия оптимальности
                for (int i = 0; i < population_value; i++)
                {
                    double sum = 0;

                    // AVG
                    for (int j = 0; j < test_count; j++)
                        sum += target_functions[i, j];
                    double AVG = sum / test_count;

                    sum = 0;
                    // StdDev
                    for (int j = 0; j < test_count; j++)
                        sum += (AVG - target_functions[i, j]) * (AVG - target_functions[i, j]);

                    double StdDev = Math.Sqrt(sum / test_count);

                    // если  target_function равна  (AVG - StdDev), то последующее вычисление критерия оптимальности будет давать результаты ВЫШЕ, чем  target_function
                    population[i].setValueByName("target_function", (AVG - StdDev).ToString().Replace(',', '.'));
                    population[i].setValueByName("target_function_AVG", (AVG).ToString().Replace(',', '.'));
                    population[i].setValueByName("target_function_StdDev", (StdDev).ToString().Replace(',', '.'));
                }



                // сортировка по точности
                string temp;
                string tempFolder = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\temp";

                List<int> tempVariableIDs;
                for (int i = 0; i < population_value - 1; i++)
                {
                    for (int j = i + 1; j < population_value; j++)
                    {
                        double i_value = Convert.ToDouble(population[i].getValueByName("target_function").Replace('.', ','));
                        double j_value = Convert.ToDouble(population[j].getValueByName("target_function").Replace('.', ','));
                        if (i_value < j_value || (double.IsNaN(i_value) && (!double.IsNaN(j_value))))
                        {
                            log(" [" + i.ToString() + "] <- [" + j.ToString() + "]: " + i_value + "<" + j_value, Color.Orchid);

                            string path_to_i = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                            string path_to_j = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + j.ToString() + "]";

                            temp = population[i].toJSON(0);
                            population[i] = new Hyperparameters(population[j].toJSON(0), form1);
                            population[j] = new Hyperparameters(temp, form1);

                            tempVariableIDs = variablesIDs[i];
                            variablesIDs[i] = variablesIDs[j];
                            variablesIDs[j] = tempVariableIDs;

                            /* Algorithm.MoveFiles(population[j], path_to_i, tempFolder);
                             Algorithm.MoveFiles(population[i], path_to_j, path_to_i);
                             Algorithm.MoveFiles(population[j], tempFolder, path_to_j);*/
                        }
                    }
                }

                for (int i = 0; i < population_value; i++)
                {
                    string newPath = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    Hyperparameters oldH = new Hyperparameters(newPath + "\\h.json", form1, true);
                    string oldPath = population[i].getValueByName("save_folder");

                    Algorithm.MoveFiles(oldH, newPath, tempFolder);
                    Algorithm.MoveFiles(population[i], oldPath, newPath);
                    Algorithm.MoveFiles(oldH, tempFolder, oldPath);
                }

                /*   for (int i = 0; i < population_value - 1; i++)
                {
                    string newPath = form1.pathPrefix + "Optimization\\temp\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    string oldPath = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    Hyperparameters oldH = new Hyperparameters(newPath + "\\h.json", form1, true);
                    Algorithm.MoveFiles(oldH, newPath, tempFolder);
                }
                for (int i = 0; i < population_value - 1; i++)
                {
                    string oldPath = form1.pathPrefix + "Optimization\\temp\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    Hyperparameters oldH = new Hyperparameters(oldPath + "\\h.json", form1, true);
                    string newPath = population[i].getValueByName("save_folder");
                    //взять код из i-ого индивида в папке temp, а затем искать индекс того же идивида по коду в массиве population
                    string code = oldH.getValueByName("code");
                    for (int i = 0; i < population_value - 1; i++)
                        Algorithm.MoveFiles(oldH, newPath, tempFolder);
                }*/
            }

            for (int i = 0; i < population_value; i++)
            {
                population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");
            }

            log("Обновление отображаемых параметров", Color.Lime);

            refreshAOTree();
            // A.draw(0, form1.picBox, form1, 15, 150);

            variableChangeMonitoring();

            for (int i = 0; i < population_value; i++)
                variablesVisualizer.addPoint(Convert.ToDouble(population[i].getValueByName("target_function").Replace('.', ',')), " [" + population[i].getValueByName("code") + "]");
            variablesVisualizer.addPoint(Convert.ToDouble(population[0].getValueByName("target_function").Replace('.', ',')), "best individ");

            variablesVisualizer.refresh();

            List<string> codes = new List<string>();
            for (int i = 0; i < population_value; i++)
            {
                string code = population[i].getValueByName("code");
                
                for (int j = 0; j < codes.Count; j++)
                {
                    if (code == codes[j])
                    {
                        log("DUPLICATE OF " + code, Color.Red);

                        Thread.CurrentThread.Abort(); 
                    }   
                }
                codes.Add(code);
            }
        }

        private string[] layerTypes = new string[]
    {
            "Dense",
            "LSTM",
            "Conv1D",
            "Dropout",
            "MaxPooling1D"
    };
        internal void variateArchitecture()
        {
            bool isInvalidArchitecure = false;
            //удаление или добавление
            bool deleteTrueInsertFalse;

            int deleteOrInsert = r.Next(0, 2);
            if (deleteOrInsert == 0)
                deleteTrueInsertFalse = true;
            else
                deleteTrueInsertFalse = false;

            int individIndex = r.Next(Convert.ToInt16(Math.Round(population_value * elite_ratio)), population_value);

        tryToVariateArchitectureAgain:

            var NNstructNode = population[individIndex].getNodeByName("NN_struct")[0];
            //список слоёв
            var layerNodes = population[individIndex].getNodesByparentID(NNstructNode.ID);

            if (deleteTrueInsertFalse & layerNodes.Count > 1)
            {
                //выбор слоя, который будет удалён
                int layerNode_X_Index = r.Next(0, layerNodes.Count - 1);
                Node layerNode_X = layerNodes[layerNode_X_Index];

                //удаление слоя
                population[individIndex].deleteBranch(layerNode_X.ID);

                for (int i = 0; i < layerNodes.Count - layerNode_X_Index - 1; i++)
                {
                    //уменьшение номера слоя на единицу
                    population[individIndex].getNodeByName("layer" + (layerNode_X_Index + i + 2).ToString())[0].setAttribute("name", "layer" + (layerNode_X_Index + i + 1).ToString());
                }

                //исключение дублирования слоя дропаута
                //   if (layerNode_X.getAttributeValue("name") == "Dropout")
                //  {
                layerNodes = population[individIndex].getNodesByparentID(NNstructNode.ID);

                string lastLayerType = "";
                for (int i = 0; i < layerNodes.Count; i++)
                {
                    if (layerNodes[i].getAttributeValue("name") == "Dropout" & lastLayerType == "Dropout")
                    {
                        //удаление второго слоя дропаут
                        population[individIndex].deleteBranch(layerNodes[i].ID);
                        for (int k = 0; k < layerNodes.Count - i - 1; k++)
                        {
                            //уменьшение номера слоя на единицу
                            population[individIndex].getNodeByName("layer" + (i + k + 2).ToString())[0].setAttribute("name", "layer" + (i + k + 1).ToString());
                        }
                        break;
                    }

                    lastLayerType = layerNodes[i].getAttributeValue("name");
                }
                //}
            }
            else
            {


                //добавление слоя
                //позиция вставки: 0 - перед первым слоем,layerNodes.Count-1 - перед последним  
                int insertPosition = r.Next(0, layerNodes.Count - 1);

                string newLayerType = layerTypes[r.Next(0, layerTypes.Length)];

                // временно отсекаемые слои, которые идут после позиции вставки, и у которых далее будут заменены индексы
                string[] surgeryLayersInJSON = new string[layerNodes.Count - insertPosition];

                for (int i = 0; i < surgeryLayersInJSON.Length; i++)
                    surgeryLayersInJSON[i] = population[individIndex].toJSON(layerNodes[insertPosition + i].ID);

                for (int i = surgeryLayersInJSON.Length - 1; i >= 0; i--)
                    population[individIndex].deleteBranch(layerNodes[insertPosition + i].ID);

                // id нового слоя
                int newLayerNodeID;

                if (newLayerType == "Dense")
                {
                    if (layerNodes[0].getValue() == "Dense")
                    {
                        // ошибка размерности входов
                        isInvalidArchitecure = true;
                        goto invalidArchitecure;
                    }
                    newLayerNodeID = population[individIndex].addByParentId(NNstructNode.ID, "name:layer" + (insertPosition + 1).ToString() + ",value:Dense");
                    population[individIndex].addVariable(newLayerNodeID, "neurons_count", 2, 10, 1, 9);
                    population[individIndex].addVariable(newLayerNodeID, "activation", "sigmoid", "sigmoid,linear");
                    isInvalidArchitecure = false;
                }
                if (newLayerType == "LSTM")
                {
                    for (int i = 0; i < insertPosition; i++)
                    {
                        if (layerNodes[i].getValue() == "Dense")
                        {
                            // ошибка размерности входов
                            isInvalidArchitecure = true;
                            goto invalidArchitecure;
                        }
                    }
                    newLayerNodeID = population[individIndex].addByParentId(NNstructNode.ID, "name:layer" + (insertPosition + 1).ToString() + ",value:LSTM");
                    population[individIndex].addVariable(newLayerNodeID, "neurons_count", 2, 10, 1, 9);
                    population[individIndex].addVariable(newLayerNodeID, "activation", "sigmoid", "sigmoid,linear");
                    isInvalidArchitecure = false;
                }
                if (newLayerType == "Conv1D")
                {

                    for (int i = 0; i < insertPosition; i++)
                    {
                        if (layerNodes[i].getValue() == "Dense")
                        {
                            // ошибка размерности входов
                            isInvalidArchitecure = true;
                            goto invalidArchitecure;
                        }
                    }

                    newLayerNodeID = population[individIndex].addByParentId(NNstructNode.ID, "name:layer" + (insertPosition + 1).ToString() + ",value:Conv1D");
                    population[individIndex].addVariable(newLayerNodeID, "neurons_count", 1, 128, 1, 16);
                    population[individIndex].addVariable(newLayerNodeID, "kernel_size", 3, 3, 1, 3);

                    isInvalidArchitecure = false;
                }
                if (newLayerType == "Dropout")
                {
                    if (insertPosition == 0 || layerNodes[insertPosition + 1].getValue() == "Dropout")
                    {
                        //дропаут - первый слой или два дропаута подряд
                        isInvalidArchitecure = true;
                        goto invalidArchitecure;
                    }
                    else
                    {
                        if (layerNodes[insertPosition - 1].getValue() == "Dropout")
                        {
                            // два дропаута подряд
                            isInvalidArchitecure = true;
                            goto invalidArchitecure;
                        }
                    }

                    newLayerNodeID = population[individIndex].addByParentId(NNstructNode.ID, "name:layer" + (insertPosition + 1).ToString() + ",value:Dropout");
                    population[individIndex].addVariable(newLayerNodeID, "dropout", 0.01, 0.8, 0.01, 0.1);
                    isInvalidArchitecure = false;
                }
                if (newLayerType == "MaxPooling1D")
                {
                    if (insertPosition == 0 || layerNodes[insertPosition + 1].getValue() == "MaxPooling1D")
                    {
                        //MaxPooling1D - первый слой или два MaxPooling1D подряд
                        isInvalidArchitecure = true;
                        goto invalidArchitecure;
                    }
                    else
                    {
                        if (layerNodes[insertPosition - 1].getValue() == "MaxPooling1D")
                        {
                            // два MaxPooling1D подряд
                            isInvalidArchitecure = true;
                            goto invalidArchitecure;
                        }
                    }
                    bool isItCNN = false;
                    for (int i = 0; i < insertPosition; i++)
                    {
                        if (layerNodes[i].getValue() == "Conv1D")
                        {
                            isItCNN = true;
                        }
                    }
                    if (isItCNN == false)
                    {
                        //это вообще не CNN, а значит слой пуллинга здесь не нужен
                        isInvalidArchitecure = true;
                        goto invalidArchitecure;
                    }

                    newLayerNodeID = population[individIndex].addByParentId(NNstructNode.ID, "name:layer" + (insertPosition + 1).ToString() + ",value:MaxPooling1D");
                    population[individIndex].addVariable(newLayerNodeID, "pool_size", 1, 10, 1, 3);
                    isInvalidArchitecure = false;
                }
            invalidArchitecure:
                if (isInvalidArchitecure)
                    insertPosition--;
                //возврат отсечённых слоёв
                for (int i = 0; i < surgeryLayersInJSON.Length; i++)
                {
                    //увеличение номера слоя на единицу
                    Hyperparameters layer = new Hyperparameters(surgeryLayersInJSON[i], form1);
                    string oldName = layer.nodes[0].getAttributeValue("name");
                    layer.nodes[0].setAttribute("name", "layer" + (insertPosition + i + 2).ToString());

                    population[individIndex].addBranch(layer, "layer" + (insertPosition + i + 2).ToString(), NNstructNode.ID);
                }
                if (isInvalidArchitecure)
                    goto tryToVariateArchitectureAgain;
            }
            // перезапись списка переменных
            variablesNames.Clear();
            for (int i = 0; i < population_value; i++)
                variablesIDs[i].Clear();

            for (int i = 0; i < population_value; i++)
            {
                recurciveVariableAdding(population[i], i, 0, population[i].getValueByName("code"));
            }
            if (!isInvalidArchitecure)
                population[individIndex].setValueByName("state", "изменена архитектура");
        }



        private void mutation()
        {

            int individIndex = r.Next(Convert.ToInt16(Math.Round(population_value * elite_ratio)), population_value);
            int variableIndex = variablesIDs[individIndex][r.Next(0, variablesIDs[individIndex].Count)];
            if (population[individIndex].nodes[variableIndex].getAttributeValue("variable") == "categorical")
            {
                int categoryIndex = r.Next(0, population[individIndex].nodes[variableIndex].getAttributeValue("categories").Split(',').Length);
                string newValue = population[individIndex].nodes[variableIndex].getAttributeValue("categories").Split(',')[categoryIndex];
                log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                population[individIndex].nodes[variableIndex].setAttribute("value", newValue);
            }
            if (population[individIndex].nodes[variableIndex].getAttributeValue("variable") == "numerical")
            { /*
                // новая логика - новое значение параметра получается в результате увеличения или уменьшения старого на величину step 
                int upOrDown = r.Next(0, 2);
                if (population[individIndex].nodes[variableIndex].getValue()[0] != '0')
                {//если первый символ атрибута value неравен '0' - значит это Int и все остальные атрибуты будут приводиться к int
                    int newValue = 0;
                    if (upOrDown == 0)
                    {
                        newValue = Convert.ToInt32(population[individIndex].nodes[variableIndex].getValue()) + Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("step"));

                        // Если newValue меньше max, то прибавить - иначе вычесть. Таким образом каждое выполнение метода mutation()
                        // ведёт к изменению параметров
                        if (newValue <= Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("max")))
                        {
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString());
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                        else
                        {
                            newValue = Convert.ToInt32(population[individIndex].nodes[variableIndex].getValue()) - Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("step"));
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString());
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                    }
                    else
                    {
                        newValue = Convert.ToInt32(population[individIndex].nodes[variableIndex].getValue()) - Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("step"));
                        if (newValue >= Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("min")))
                        {
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString());
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                        else
                        {
                            newValue = Convert.ToInt32(population[individIndex].nodes[variableIndex].getValue()) + Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("step"));
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString());
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                    }
                }
                else
                {
                    double newValue = 0;
                    if (upOrDown == 0)
                    {
                        newValue = Convert.ToDouble(population[individIndex].nodes[variableIndex].getValue().Replace('.', ',')) + Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("step").Replace('.', ','));

                        if (newValue <= Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("max").Replace('.', ',')))
                        {
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString().Replace(',', '.'));
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                        else
                        {
                            newValue = Convert.ToDouble(population[individIndex].nodes[variableIndex].getValue().Replace('.', ',')) - Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("step").Replace('.', ','));
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString().Replace(',', '.'));
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);

                        }
                    }
                    else
                    {
                        newValue = Convert.ToDouble(population[individIndex].nodes[variableIndex].getValue().Replace('.', ',')) - Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("step").Replace('.', ','));
                        if (newValue >= Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("min").Replace('.', ',')))
                        {
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString().Replace(',', '.'));
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                        else
                        {
                            newValue = Convert.ToDouble(population[individIndex].nodes[variableIndex].getValue().Replace('.', ',')) + Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("step").Replace('.', ','));
                            population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString().Replace(',', '.'));
                            log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                        }
                    }
                }*/

                // логика работы метода mutation(), при которой новое значение параметра выбирается случайным образом из интервала min - max

                if (population[individIndex].nodes[variableIndex].getValue()[0] != '0')
                {
                    int newValue = r.Next(Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("min")), Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("max")) + 1);
                    if (newValue == 0)
                        log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);


                    population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString());
                    log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);

                }
                else
                {
                    double newValue = r.NextDouble();

                    while (newValue < Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("min").Replace('.', ',')) | newValue > Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("max").Replace('.', ',')))
                    {
                        newValue = r.NextDouble();
                    }
                    log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                    population[individIndex].nodes[variableIndex].setAttribute("value", newValue.ToString().Replace(',', '.'));
                }
            }

            population[individIndex].setValueByName("state", "изменены параметры");
        }

        private void kill_and_conceive()
        {
            int inc = Convert.ToInt16((Math.Round(population_value * elite_ratio)));
            if (inc > 0)
            {
                while (inc < population_value)
                {
                    for (int j = 0; j < Convert.ToInt16((Math.Round(population_value * elite_ratio))) - 1; j++)
                    {
                        population[inc] = get_child(population[j], population[j + 1], population[inc], inc);
                        //    log("SET Child: population[" + inc.ToString() + "] " + '\n' + population[inc].prediction_Algorithms[0].get_Hyperparameters().ToString(), Color.LightCyan);
                        inc++;
                        if (inc == population_value)
                        { break; }
                    }
                }
            }
            else
            {
                log("необходимо снизить elite_ratio, так как (population_value * elite_ratio) < 1 !", Color.Red);
                throw new Exception();
            }
        }

        private Hyperparameters get_child(Hyperparameters parent1, Hyperparameters parent2, Hyperparameters old, int indexOld)
        {
            //      При переходе на вариативную архитектуру встала проблема несовместимости параметров родителей.
            //  До тех пор, пока не найдено решение по совмещению параметров разных архитектур,
            //  перекомбинация параметров архитектуры отключена!

            Hyperparameters child = old.Clone();

            foreach (int variableID in variablesIDs[indexOld])
            {
                //перекомбинируются только параметры с parentID == 0
                if (child.nodes[variableID].parentID == 0)
                {
                    //родитель гена выбирается случайно
                    int parent_of_gene = r.Next(0, 2);

                    // если имена параметров совпадают - производится перекомбинация
                    // возможны ошибки при одинаковых именах параметров в разных местах одной архитектуры

                    if (parent_of_gene == 0)
                    {
                        string value = parent1.getValueByName(child.nodes[variableID].name());
                        child.nodes[variableID].setAttribute("value", value);
                    }
                    else
                    {
                        string value = parent2.getValueByName(child.nodes[variableID].name());
                        child.nodes[variableID].setAttribute("value", value);
                    }
                }
            }
            int parent_of_architechture = r.Next(0, 2);

            child.deleteBranch(child.getNodeByName("NN_struct")[0].ID);

            if (parent_of_architechture == 0)
            {
                child.addBranch(new Hyperparameters(parent1.toJSON(parent1.getNodeByName("NN_struct")[0].ID), form1), "NN_struct", 0);
            }
            else
            {
                child.addBranch(new Hyperparameters(parent2.toJSON(parent2.getNodeByName("NN_struct")[0].ID), form1), "NN_struct", 0);
            }


            string path = child.getValueByName("json_file_path");


            child.setValueByName("state", "создан");
            child.setValueByName("parents", parent1.getValueByName("model_name") + " code: " + parent1.getValueByName("code") + " и " + parent2.getValueByName("model_name") + " code: " + parent2.getValueByName("code"));
            //      child.Save(path);
            return child;
        }

        // метод перекомбинации параметров индивидов без вариативности архитектуры
        // перед использованием необходимо сформировать списки идентификаторов переменных
        private Hyperparameters get_child_OLD(Hyperparameters parent1, Hyperparameters parent2, Hyperparameters old, int indexOld)
        {
            Hyperparameters child = new Hyperparameters(old.toJSON(0), form1);

            foreach (int variableID in variablesIDs[indexOld])
            {

                //родитель гена выбирается случайно
                int parent_of_gene = r.Next(0, 2);
                if (parent_of_gene == 0)
                    child.nodes[variableID].setAttribute("value", parent1.nodes[variableID].getValue());
                else
                    child.nodes[variableID].setAttribute("value", parent2.nodes[variableID].getValue());
            }
            return child;
        }

        private Thread newthread;
        public void run()
        {
            opt_inc = 0;
            newthread = new Thread(optimization);
            newthread.Start();
            log("OPTIMIZATION STARTED", Color.Cyan);
        }
        private void rewriteVariableIDs()
        {
            // перезапись списка переменных
            variablesNames.Clear();
            for (int i = 0; i < population_value; i++)
                variablesIDs[i].Clear();

            for (int i = 0; i < population_value; i++)
            {
                recurciveVariableAdding(population[i], i, 0, population[i].getValueByName("code"));
            }
        }
        private void recurciveVariableAdding(Hyperparameters h, int index, int ID, string modelName)
        {
            List<Node> branches = h.getNodesByparentID(ID);
            var asdad = branches;
            if (branches.Count == 0)
            {
                if (h.getNodeById(ID).getAttributeValue("variable") != null)
                {
                    // добавить в список переменных
                    if (showIndividsParameters)
                    {
                        if (h.getNodeById(ID).getAttributeValue("variable") == "numerical")
                            //    variablesVisualizer.addParameter(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Cyan, 200);

                            if (h.getNodeById(ID).getAttributeValue("variable") == "categorical")
                            {
                                //  variablesVisualizer.addParameter(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Black, 40);
                                //variablesVisualizer.parameters[variablesVisualizer.parameters.Count - 1].mainFontDepth = 12;
                            }
                    }

                    variablesNames.Add(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString());
                    variablesIDs[index].Add(ID);
                }
            }
            else
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    recurciveVariableAdding(h, index, branches[i].ID, modelName);
                }
            }
        }

        private void variableChangeMonitoring()
        {
            if (showIndividsParameters)
            {
                if (showOnlyBestIndividsParameters)
                {
                    foreach (int variableID in variablesIDs[0])
                    {
                        string variableName = "0 " + population[0].getNodeById(variableID).name() + " id=" + variableID.ToString();
                        string value = population[0].nodes[variableID].getValue().Replace('.', ',');
                        variablesVisualizer.addPoint(value, variableName);
                    }
                }
                else
                {
                    for (int i = 0; i < population_value; i++)
                        foreach (int variableID in variablesIDs[i])
                        {
                            string variableName = population[i].getValueByName("code") + " " + population[i].getNodeById(variableID).name() + " id=" + variableID.ToString();
                            string value = population[i].nodes[variableID].getValue().Replace('.', ',');
                            variablesVisualizer.addPoint(value, variableName);
                        }
                }
            }
        }

        private void refreshAOTree()
        {
            A = new Hyperparameters(form1, "Algorithm_Population");
            for (int i = 0; i < population_value; i++) { A.addBranch(population[i], population[i].getValueByName("model_name"), 0); }
        }

        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
        }

    }

}
