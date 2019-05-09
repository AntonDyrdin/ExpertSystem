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
        public string name;
        public Algorithm algorithm;
        public Hyperparameters[] population;
        public Hyperparameters A;
        private Form1 form1;
        public int mutation_rate;
        public int population_value;
        public double elite_ratio;
        private MultiParameterVisualizer variablesVisualizer;
        private List<string> variablesNames;
        private List<int> variablesIDs;
        private int Iterarions = 0;
        private Random r;
        private AgentManager agentManager;
        private bool showIndividsParameters = false;
        private bool showOnlyBestIndividsParameters = true;

        public int screenshotIterationTimer = 5;
        public bool multiThreadTraining = false;
        public AlgorithmOptimization(Algorithm algorithm, Form1 form1, int population_value, int mutation_rate, double elite_ratio, int Iterarions)
        {
            r = new Random();
            this.Iterarions = Iterarions;
            this.form1 = form1;
            this.algorithm = algorithm;
            this.elite_ratio = elite_ratio;
            this.mutation_rate = mutation_rate;
            this.population_value = population_value;
            variablesVisualizer = new MultiParameterVisualizer(form1.picBox, form1);
            population = new Hyperparameters[population_value];
            variablesNames = new List<string>();
            variablesIDs = new List<int>();

            this.agentManager = form1.I.agentManager;

            name = algorithm.name;

            try { Directory.Delete(form1.pathPrefix + "Optimization\\" + name, true); } catch { }

            variablesVisualizer.enableGrid = false;
            variablesVisualizer.addParameter("best individ", Color.LightCyan, 400);
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 800);
            //добавление переменных в  MultiParameterVisualizer
            for (int i = 0; i < population_value; i++)
                variablesVisualizer.parameters[1].functions.Add(new Function(" [" + i.ToString() + "]", valueToColor(0, population_value, population_value - i - 1)));
            recurciveVariableAdding(algorithm.h, 0, "0");
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
            G = (Math.Abs(((max + min) / 2) - val)) / ((max + min) / 2) * 255;
            B = (val - min) / (max - min) * 255;

            return Color.FromArgb(255, Convert.ToInt32(R), Convert.ToInt32(G), Convert.ToInt32(B));
        }

        public int opt_inc;
        private int inc = 0;
        private void optimization()
        {
            while (opt_inc < Iterarions)
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
                    Image screenShot = (Image)form1.picBox.Image.Clone();
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
                File.WriteAllText(new_save_folder + "json.txt", algorithm.h.toJSON(0), System.Text.Encoding.Default);

                algorithm.train().Wait();
                algorithm.h.setValueByName("target_function", algorithm.h.getValueByName("accuracy"));
                for (int i = 0; i < population_value; i++)
                {
                    population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1);
                    population[i].setValueByName("code", i.ToString());
                    population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");
                    new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\";
                    Algorithm.CopyFiles(population[i], algorithm.h.getValueByName("save_folder"), new_save_folder);
                }
                if (showOnlyBestIndividsParameters)
                {
                    // variablesIDs.Clear();
                    //recurciveVariableAdding(population[0], 0, "0");
                }
                else
                {
                    for (int i = 1; i < population_value; i++)
                    {
                        variablesIDs.Clear();
                        recurciveVariableAdding(population[i], 0, population[i].getValueByName("code"));
                    }
                }
            }
            else
            {
                //kill and concieve
                kill_and_conceive();

                //mutation
                for (int i = 0; i < mutation_rate; i++)
                {
                    mutation();
                }
                //   ВВЕДЕНО МНОГОКРАТНОЕ ТЕСТИРОВНИЕ ИНДИВИДОВ ВСЕХ КАТЕГОРИЙ ДЛЯ ПОВЫШЕНИЯ ПОВТОРЯЕМОСТИ РЕЗУЛЬТАТОВ
                int test_count = 1;
                //   target_functions - матрица результатов тестирования,
                //   где номер строки (первый индекс (i)) - индекс индивида, а номер столбца (второй индекс (j)) - итерация тестирования
                double[,] target_functions = new double[population_value, test_count];

                if (agentManager.agents.Count == 0)
                {
                    //  for (int i = Convert.ToInt16(Math.Round(population_value * elite_ratio)); i < population_value; i++)




                    for (int j = 0; j < test_count; j++)
                    {
                        if (multiThreadTraining)
                        {
                            List<Algorithm> algorithms = new List<Algorithm>();
                            for (int i = 0; i < population_value; i++)
                            {
                                Algorithm alg = Algorithm.newInstance(algorithm);
                                alg.h = population[i].Clone();
                                algorithms.Add(alg);
                            }

                            Task[] trainTasks = new Task[population_value];
                            foreach (Algorithm alg in algorithms)
                            {
                                trainTasks[algorithms.IndexOf(alg)] = new Task(() => algorithms[algorithms.IndexOf(alg)].train().Wait());
                                trainTasks[algorithms.IndexOf(alg)].Start();
                            }

                            bool done = false;
                            while (done == false)
                            {
                                done = true;
                                foreach (var task in trainTasks)
                                    if (task.Status == TaskStatus.Running)
                                    {
                                        done = false;
                                    }
                            }

                            for (int i = 0; i < population_value; i++)
                            {
                                population[i] = algorithms[i].h.Clone();
                                target_functions[i, j] = Convert.ToDouble(population[i].getValueByName("accuracy").Replace('.', ','));
                            }
                        }
                        else
                        {// SINGLE THREAD
                            for (int i = 0; i < population_value; i++)
                            {
                                algorithm.h = new Hyperparameters(population[i].toJSON(0), form1);
                                algorithm.train().Wait();
                                population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1);
                                target_functions[i, j] = Convert.ToDouble(population[i].getValueByName("target_function").Replace('.', ','));
                            }
                        }
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
                        // File.WriteAllText(population[i].getValueByName("json_file_path"), population[i].toJSON(0), System.Text.Encoding.Default);
                    }
                }
                else
                {
                    for (int i = 0; i < population_value; i++)
                    {
                        agentManager.tasks.Add(new AgentTask("train", population[i]));
                    }

                    Task.Factory.StartNew(() => { agentManager.work(); }).Wait();

                    for (int i = 0; i < population_value; i++)
                    {
                        // population[i] = agentManager.tasks[i - Convert.ToInt16(Math.Round(population_value * elite_ratio))].h;
                        population[i] = agentManager.tasks[i].h;
                        File.WriteAllText(population[i].getValueByName("json_file_path"), population[i].toJSON(0), System.Text.Encoding.Default);
                    }
                    agentManager.tasks.Clear();
                }

                // сортировка по точности
                string temp;
                string tempFolder = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\temp";

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

                            /* Algorithm.MoveFiles(population[j], path_to_i, tempFolder);
                             Algorithm.MoveFiles(population[i], path_to_j, path_to_i);
                             Algorithm.MoveFiles(population[j], tempFolder, path_to_j);*/
                        }
                    }
                }
                for (int i = 0; i < population_value; i++)
                {
                    string newPath = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    Hyperparameters oldH = new Hyperparameters(newPath + "\\json.txt", form1, true);
                    string oldPath = population[i].getValueByName("save_folder");

                    Algorithm.MoveFiles(oldH, newPath, tempFolder);
                    Algorithm.MoveFiles(population[i], oldPath, newPath);
                    Algorithm.MoveFiles(oldH, tempFolder, oldPath);
                }
                /*   for (int i = 0; i < population_value - 1; i++)
                {
                    string newPath = form1.pathPrefix + "Optimization\\temp\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    string oldPath = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    Hyperparameters oldH = new Hyperparameters(newPath + "\\json.txt", form1, true);
                    Algorithm.MoveFiles(oldH, newPath, tempFolder);
                }
                for (int i = 0; i < population_value - 1; i++)
                {
                    string oldPath = form1.pathPrefix + "Optimization\\temp\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                    Hyperparameters oldH = new Hyperparameters(oldPath + "\\json.txt", form1, true);
                    string newPath = population[i].getValueByName("save_folder");
                    //взять код из i-ого индивида в папке temp, а затем искать индекс того же идивида по коду в массиве population
                    string code = oldH.getValueByName("code");
                    for (int i = 0; i < population_value - 1; i++)
                        Algorithm.MoveFiles(oldH, newPath, tempFolder);
                }*/
            }

            for (int i = 0; i < population_value; i++)
                population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");

            log("Обновление отображаемых параметров", Color.Lime);

            refreshAOTree();
            // A.draw(0, form1.picBox, form1, 15, 150);

            variableChangeMonitoring();

            for (int i = 0; i < population_value; i++)
                variablesVisualizer.addPoint(Convert.ToDouble(population[i].getValueByName("target_function").Replace('.', ',')), " [" + population[i].getValueByName("code") + "]");
            variablesVisualizer.addPoint(Convert.ToDouble(population[0].getValueByName("target_function").Replace('.', ',')), "best individ");

            variablesVisualizer.refresh();
        }

        private void mutation()
        {

            int variableIndex = variablesIDs[r.Next(0, variablesIDs.Count)];
            int individIndex = r.Next(Convert.ToInt16(Math.Round(population_value * elite_ratio)), population_value);
            if (population[individIndex].nodes[variableIndex].getAttributeValue("variable") == "categorical")
            {
                int categoryIndex = r.Next(0, population[individIndex].nodes[variableIndex].getAttributeValue("categories").Split(',').Length);
                string newValue = population[individIndex].nodes[variableIndex].getAttributeValue("categories").Split(',')[categoryIndex];
                log("individIndex = " + individIndex.ToString() + "; variableIndex = " + variableIndex.ToString() + " (" + population[individIndex].nodes[variableIndex].name() + ")" + "; newValue = " + newValue.ToString(), Color.White);
                population[individIndex].nodes[variableIndex].setAttribute("value", newValue);
            }
            if (population[individIndex].nodes[variableIndex].getAttributeValue("variable") == "numerical")
            {
                if (population[individIndex].nodes[variableIndex].getValue()[0] != '0')
                {
                    int newValue = r.Next(Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("min")), Convert.ToInt32(population[individIndex].nodes[variableIndex].getAttributeValue("max")) + 1);
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
                        population[inc] = get_child(population[j], population[j + 1], population[inc]);
                        //    log("SET Child: population[" + inc.ToString() + "] " + '\n' + population[inc].prediction_Algorithms[0].get_Hyperparameters().ToString(), Color.LightCyan);
                        inc++;
                        if (inc == population_value)
                        { break; }
                    }
                }
            }
            else
            {
                log("необходимо снизить elite_ratio, так как (population_value * elite_ratio) < 1 !", Color.Orange);
            }
        }

        private Hyperparameters get_child(Hyperparameters parent1, Hyperparameters parent2, Hyperparameters old)
        {
            Hyperparameters child = new Hyperparameters(old.toJSON(0), form1);

            foreach (int variableID in variablesIDs)
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

        private void recurciveVariableAdding(Hyperparameters h, int ID, string modelName)
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
                            variablesVisualizer.addParameter(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Cyan, 200);

                        if (h.getNodeById(ID).getAttributeValue("variable") == "categorical")
                        {
                            variablesVisualizer.addParameter(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Black, 40);
                            variablesVisualizer.parameters[variablesVisualizer.parameters.Count - 1].mainFontDepth = 12;
                        }
                    }

                    variablesNames.Add(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString());
                    variablesIDs.Add(ID);
                }
            }
            else
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    recurciveVariableAdding(h, branches[i].ID, modelName);
                }
            }
        }

        private void variableChangeMonitoring()
        {
            if (showIndividsParameters)
            {
                if (showOnlyBestIndividsParameters)
                {
                    foreach (int variableID in variablesIDs)
                    {
                        string variableName = population[0].getValueByName("code") + " " + population[0].getNodeById(variableID).name() + " id=" + variableID.ToString();
                        string value = population[0].nodes[variableID].getValue().Replace('.', ',');
                        variablesVisualizer.addPoint(value, variableName);
                    }
                }
                else
                {
                    foreach (Hyperparameters individ in population)
                        foreach (int variableID in variablesIDs)
                        {
                            string variableName = individ.getValueByName("code") + " " + individ.getNodeById(variableID).name() + " id=" + variableID.ToString();
                            string value = individ.nodes[variableID].getValue().Replace('.', ',');
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
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }

    }
}
