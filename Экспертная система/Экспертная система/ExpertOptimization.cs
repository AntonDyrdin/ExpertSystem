using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Экспертная_система
{
    internal class ExpertOptimization
    {
        public string name;
        public Expert expert;
        public Hyperparameters[] population;
        public Hyperparameters E;
        private MainForm form1;
        public int mutation_rate;
        public int population_value;
        public double elite_ratio;
        private DateTime date1;
        private DateTime date2;
        private MultiParameterVisualizer variablesVisualizer;
        private List<string> variablesNames;
        private List<int> variablesIDs;
        private int Iterarions = 0;
        private Random r;
        private AgentManager agentManager;
        private string rawDatasetFilePath;
        private int test_count = 1;
        public int screenshotIterationTimer = 5;
        private bool multiThreadTraining = true;

        public int multiThreadTrainingRATE = 4;
        public ExpertOptimization(Expert expert, MainForm form1, int population_value, int test_count, int mutation_rate, double elite_ratio, int Iterarions, DateTime date1, DateTime date2, string rawDatasetFilePath)
        {
            r = new Random();
            this.Iterarions = Iterarions;
            this.rawDatasetFilePath = rawDatasetFilePath;
            this.form1 = form1;
            this.expert = expert;
            this.elite_ratio = elite_ratio;
            this.mutation_rate = mutation_rate;
            this.population_value = population_value;
            this.test_count = test_count;
            this.date1 = date1;
            this.date2 = date2;
            variablesVisualizer = new MultiParameterVisualizer(form1.picBox, form1);
            population = new Hyperparameters[population_value];
            variablesNames = new List<string>();
            variablesIDs = new List<int>();

            this.agentManager = form1.I.agentManager;

            name = expert.expertName;

            try { Directory.Delete(form1.pathPrefix + "Optimization\\" + name, true); } catch { }

            //  refreshAOTree();
            variablesVisualizer.enableGrid = false;
            variablesVisualizer.addParameter("best_individ", Color.Cyan, 300);
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 600);
            variablesVisualizer.parameters[1].functions.Add(new Function("averege best", Color.Lime));
            //добавление переменных в  MultiParameterVisualizer
            for (int i = 0; i < population_value; i++)
                variablesVisualizer.parameters[1].functions.Add(new Function(" [" + i.ToString() + "]", valueToColor(0, population_value, population_value - i - 1)));

            for (int i = 0; i < population_value; i++)
            {
                // variablesVisualizer.addParameter("actions [" + i.ToString() + "]", Color.Pink, 50);
                variablesVisualizer.addParameter("committee response [" + i.ToString() + "]", Color.Pink, 300);
                variablesVisualizer.addParameter("close [" + i.ToString() + "]", Color.Pink, 300);
                variablesVisualizer.addParameter("deposit1 [" + i.ToString() + "]", Color.Green, 300);
                variablesVisualizer.addParameter("exit [" + i.ToString() + "]", Color.LightSeaGreen, 300);
                /* variablesVisualizer.addParameter("deposit history [" + i.ToString() + "]", Color.LightCyan, 300);
                 variablesVisualizer.parameters[1 + i].functions.Add(new Function("deposit1 [" + i.ToString() + "]", Color.Pink));
                 variablesVisualizer.parameters[1 + i].functions.Add(new Function("deposit2 [" + i.ToString() + "]", Color.Green));
                 variablesVisualizer.parameters[1 + i].functions.Add(new Function("exit [" + i.ToString() + "]", Color.LightSeaGreen));   */
            }

            // recurciveVariableAdding(expert.H, 0, name + "[0]");
            variablesVisualizer.refresh();
        }

        private int opt_inc;
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
                string new_save_folder;
                expert.copyExpertParametersToAlgorithms();
                for (int j = 0; j < expert.algorithms.Count; j++)
                {
                    new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[0]" + "\\" + expert.algorithms[j].getValueByName("model_name") + "\\";
                    Directory.CreateDirectory(new_save_folder);
                    expert.algorithms[j].h.setValueByName("save_folder", new_save_folder);
                    string predictionsFilePath = new_save_folder + "predictions.txt";
                    expert.algorithms[j].h.setValueByName("predictions_file_path", predictionsFilePath);
                    expert.algorithms[j].h.setValueByName("json_file_path", new_save_folder + "h.json");
                    File.WriteAllText(new_save_folder + "h.json", expert.algorithms[j].h.toJSON(0), System.Text.Encoding.Default);

                    expert.algorithms[j].train().Wait();
                }
                expert.copyHyperparametersFromAlgorithmsToExpert();
                expert.H.setValueByName("report_path", form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[0]");
                expert.H.setValueByName("code", "0");
                expert.Save(form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[0]");


                expert.test(date1, date2, rawDatasetFilePath);

                //отрисовка истрии баланса
                for (int i = 0; i < expert.deposit1History.Count; i++)
                {
                    variablesVisualizer.addPoint(expert.deposit2History[i] + (expert.deposit1History[i] * expert.closeValueHistory[i]), "exit [0]");
                    variablesVisualizer.addPoint(expert.deposit2History[i], "deposit2 [0]");
                    variablesVisualizer.addPoint(expert.closeValueHistory[i], "close [0]");
                }

                for (int i = 0; i < population_value; i++)
                {
                    expert.H.setValueByName("code", i.ToString());
                    population[i] = Copy(expert.H, form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[0]" + "\\", form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[" + i.ToString() + "]" + "\\");

                }

                variablesIDs.Clear();
                recurciveVariableAdding(population[0], 0, name + "[0]");
                /*   for (int i = 1; i < population_value; i++)
                   {
                       variablesIDs.Clear();
                       recurciveVariableAdding(population[i], 0, name + "[" + i.ToString() + "]");
                   }     */
                refreshEOTree();
                variablesVisualizer.refresh();
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

                for (int i = 0; i < population_value; i++)
                    File.WriteAllText(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\h.json", population[i].toJSON(0), System.Text.Encoding.Default);


                //   ВВЕДЕНО МНОГОКРАТНОЕ ТЕСТИРОВНИЕ ИНДИВИДОВ ВСЕХ КАТЕГОРИЙ ДЛЯ ПОВЫШЕНИЯ ПОВТОРЯЕМОСТИ РЕЗУЛЬТАТОВ

                //   target_functions - матрица результатов тестирования,
                //   где номер строки (первый индекс (i)) - индекс индивида, а номер столбца (второй индекс (j)) - итерация тестирования
                double[,] target_functions = new double[population_value, test_count];

                for (int tc = 0; tc < test_count; tc++)
                {
                    if (opt_inc == 2)
                    {
                        if (agentManager.agents.Count == 0)
                        {

                            if (multiThreadTraining)
                            {
                                if (form1.multiThreadTrainingRATE != 0)
                                    multiThreadTrainingRATE = form1.multiThreadTrainingRATE;
                                log("multiThreadTrainingRATE = " + multiThreadTrainingRATE.ToString(), Color.Yellow);
                                if (multiThreadTrainingRATE > population_value)
                                    multiThreadTrainingRATE = population_value;

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
                                    List<Expert> experts = new List<Expert>();
                                    for (int i = begin; i < end; i++)
                                    {
                                        Expert exp = Expert.Open(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]", name, form1);
                                        //   exp.H = population[i].Clone();
                                        experts.Add(exp);
                                    }

                                    Task[] trainTasks = new Task[end - begin];
                                    foreach (Expert exp in experts)
                                    {
                                        trainTasks[experts.IndexOf(exp)] = new Task(() => experts[experts.IndexOf(exp)].trainAllAlgorithms(false));
                                        trainTasks[experts.IndexOf(exp)].Start();
                                    }

                                    foreach (var task in trainTasks)
                                        task.Wait();

                                    for (int i = begin; i < end; i++)
                                    {
                                        // experts[i].copyHyperparametersFromAlgorithmsToExpert();
                                        experts[i - begin].Save(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]");
                                        population[i] = experts[i - begin].H.Clone();
                                    }
                                    log((end).ToString() + '/' + population_value.ToString() + " training comlete" + TimeSpan.FromSeconds((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()) - start1).ToString(), Color.LimeGreen);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < population_value; i++)
                                {
                                    expert = Expert.Open(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]", name, form1);
                                    expert.trainAllAlgorithms(false);
                                    expert.Save(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]");
                                    population[i] = expert.H.Clone();
                                    log((i + 1).ToString() + '/' + population_value.ToString() + " training comlete", Color.LimeGreen);
                                }

                            }
                        }
                        else
                        {
                            for (int i = 0; i < population_value; i++)
                            {

                                var algorithmBranches = population[i].getNodesByparentID(expert.committeeNodeID);
                                foreach (Node algorithmBranch in algorithmBranches)
                                {
                                    agentManager.tasks.Add(new AgentTask("train", new Hyperparameters(population[i].toJSON(algorithmBranch.ID), form1)));
                                }
                                Task.Factory.StartNew(() => { agentManager.work(); }).Wait();
                                //  СПИСОК ВЕТВЕЙ АЛГОРИТМОВ
                                List<Node> toDelete = population[i].getNodesByparentID(expert.committeeNodeID);
                                //удаление старых конфигураций алгоритмов
                                for (int j = 0; j < toDelete.Count; j++)
                                    population[i].deleteBranch(toDelete[j].ID);

                                //приращение новых конфигураций к узлу "committee"
                                for (int j = 0; j < agentManager.tasks.Count; j++)
                                {
                                    population[i].addBranch(agentManager.tasks[j].h, agentManager.tasks[j].h.nodes[0].name(), expert.committeeNodeID);
                                }
                                agentManager.tasks.Clear();

                                //   expert.synchronizeHyperparameters();
                                File.WriteAllText(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\h.json", population[i].toJSON(0), System.Text.Encoding.Default);
                            }
                        }
                    }
                    for (int i = 0; i < population_value; i++)
                    {
                        variablesVisualizer.Clear("committee response [" + i.ToString() + "]");
                        variablesVisualizer.Clear("deposit2 [" + i.ToString() + "]");
                        variablesVisualizer.Clear("deposit1 [" + i.ToString() + "]");
                        variablesVisualizer.Clear("exit [" + i.ToString() + "]");
                        variablesVisualizer.Clear("close [" + i.ToString() + "]");
                    }
                    //Тестирование (вычисление целевой функции)
                    for (int i = 0; i < population_value; i++)
                    {
                        expert = Expert.Open(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]", name, form1);
                        expert.test(date1, date2, rawDatasetFilePath);
                        log("test[" + i.ToString() + "]: " + expert.H.getValueByName("expert_target_function"), Color.Purple);
                        population[i] = expert.H.Clone();
                        File.WriteAllText(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\h.json", population[i].toJSON(0), System.Text.Encoding.Default);
                        target_functions[i, tc] = Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ','));

                        //отрисовка истрии баланса
                            for (int j = 0; j < expert.deposit1History.Count; j++)
                            {
                                variablesVisualizer.addPoint(expert.committeeResponseHistory[j][0], "committee response [" + i.ToString() + "]");
                                // variablesVisualizer.addPoint(expert.actionHistory[j], "actions [" + i.ToString() + "]");
                                variablesVisualizer.addPoint(expert.deposit2History[j] + (expert.deposit1History[j] * expert.closeValueHistory[j]), "exit [" + i.ToString() + "]");
                                variablesVisualizer.addPoint(expert.deposit1History[j], "deposit1 [" + i.ToString() + "]");
                                variablesVisualizer.addPoint(expert.closeValueHistory[j], "close [" + i.ToString() + "]");

                            }
                        variablesVisualizer.refresh();
                    }
                    log((tc + 1).ToString() + '/' + test_count.ToString() + " test comlete", Color.LimeGreen);

                }
                variablesVisualizer.refresh();
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
                    population[i].setValueByName("expert_target_function", (AVG - StdDev).ToString().Replace(',', '.'));
                    population[i].setValueByName("expert_target_function_AVG", (AVG).ToString().Replace(',', '.'));
                    population[i].setValueByName("expert_target_function_StdDev", (StdDev).ToString().Replace(',', '.'));
                    // File.WriteAllText(population[i].getValueByName("json_file_path"), population[i].toJSON(0), System.Text.Encoding.Default);
                }


                // сортировка
                Hyperparameters temp;
                for (int i = 0; i < population_value - 1; i++)
                {
                    for (int j = i + 1; j < population_value; j++)
                    {
                        double i_value = Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ','));
                        double j_value = Convert.ToDouble(population[j].getValueByName("expert_target_function").Replace('.', ','));
                        if (i_value < j_value || (double.IsNaN(i_value) && (!double.IsNaN(j_value))))
                        {
                            log(" [" + i.ToString() + "] <- [" + j.ToString() + "]: " + i_value + "<" + j_value, Color.Orchid);

                            string tempFolder = form1.pathPrefix + "Optimization\\" + name + "\\temp";
                            string path_to_i = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]";
                            string path_to_j = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + j.ToString() + "]";

                            temp = Copy(population[i], path_to_i, tempFolder);
                            population[i] = Copy(population[j], path_to_j, path_to_i);
                            population[j] = Copy(temp, tempFolder, path_to_j);

                            population[i].setValueByName("report_path", form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[" + i.ToString() + "]");
                            population[j].setValueByName("report_path", form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[" + j.ToString() + "]");
                        }
                    }
                }
            }

            for (int i = 0; i < population_value; i++) population[i].setValueByName("name", population[i].nodes[0].name() + "[" + i.ToString() + "]");

            log("Обновление отображаемых параметров", Color.Lime);

            refreshEOTree();
            // A.draw(0, form1.picBox, form1, 15, 150);

            variableChangeMonitoring();

            variablesVisualizer.addPoint(Convert.ToDouble(population[0].getValueByName("expert_target_function").Replace('.', ',')), "best_individ");
            double averegeBest = 0;
            for (int i = 0; i < population_value; i++)
            { var t_f = Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ','));

                if (i < population_value * elite_ratio)
                    averegeBest += t_f;

                variablesVisualizer.addPoint(t_f, " [" + population[i].getValueByName("code") + "]");
            }
            variablesVisualizer.addPoint((averegeBest/ (population_value * elite_ratio)).ToString(), "averege best");

            variablesVisualizer.refresh();
        }

        private void mutation()
        {

            int variableIndex = variablesIDs[r.Next(0, variablesIDs.Count)];
            int individIndex = r.Next(Convert.ToInt16(Math.Round(population_value * elite_ratio)), population_value);

            if (population[individIndex].nodes[variableIndex].getAttributeValue("variable") == "categorical")
            {
                var c = population[individIndex].nodes[variableIndex].getAttributeValue("categories");
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
            Hyperparameters child = old.Clone();

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

        private void recurciveVariableAdding(Hyperparameters h, int ID, string expertName)
        {

            List<Node> branches = h.getNodesByparentID(ID);
            if (branches.Count == 0)
            {
                if (h.getNodeById(ID).getAttributeValue("variable") != null)
                {
                    // добавить в список переменных
                    if (h.getNodeById(ID).getAttributeValue("variable") == "numerical")
                        variablesVisualizer.addParameter(expertName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Cyan, 200);

                    if (h.getNodeById(ID).getAttributeValue("variable") == "categorical")
                    {
                        variablesVisualizer.addParameter(expertName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Transparent, 60);
                        variablesVisualizer.parameters[variablesVisualizer.parameters.Count - 1].mainFontDepth = 12;
                    }
                    variablesNames.Add(expertName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString());
                    variablesIDs.Add(ID);
                }
            }
            else
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    recurciveVariableAdding(h, branches[i].ID, expertName);
                }
            }
        }

        private void variableChangeMonitoring()
        {
            foreach (int variableID in variablesIDs)
            {
                string variableName = name + "[0]" + " " + population[0].getNodeById(variableID).name() + " id=" + variableID.ToString();
                string value = population[0].nodes[variableID].getValue();
                variablesVisualizer.addPoint(value, variableName);
            }
            /*  for (int i = 0; i < population.Length; i++)
                  foreach (int variableID in variablesIDs)
                  {
                      string variableName = name + "[" + i.ToString() + "]" + " " + population[i].getNodeById(variableID).name() + " id=" + variableID.ToString();
                      string value = population[i].nodes[variableID].getValue();
                      variablesVisualizer.addPoint(value, variableName);
                  } */
        }

        private void refreshEOTree()
        {
            E = new Hyperparameters(form1, "Expert_Population");
            for (int i = 0; i < population_value; i++) { E.addBranch(population[i], population[i].nodes[0].name(), 0); }
        }

        public Hyperparameters Copy(Hyperparameters individ, string source, string destination)
        {
            Hyperparameters H = individ.Clone();

            var algorithmBranches = H.getNodesByparentID(H.getNodeByName("committee")[0].ID);

            List<Hyperparameters> hs = new List<Hyperparameters>();

            for (int j = 0; j < algorithmBranches.Count; j++)
            {
                var h = new Hyperparameters(H.toJSON(algorithmBranches[j].ID), form1);
                var new_save_folder = destination + "\\" + h.getValueByName("model_name") + "\\";
                //новые пути прописываются в h.json автоматически, если передать объект Hyperparameters по ссылке, а не по значению
                Algorithm.CopyFiles(h, h.getValueByName("save_folder"), new_save_folder);
                hs.Add(h);
            }

            //удаление старых записей
            for (int i = 0; i < algorithmBranches.Count; i++)
                H.deleteBranch(algorithmBranches[i].ID);

            //приращение новых записей к узлу  "committee"
            for (int i = 0; i < algorithmBranches.Count; i++)
            {
                H.addBranch(hs[i], hs[i].nodes[0].name(), H.getNodeByName("committee")[0].ID);
            }

            H.setValueByName("report_path", destination);
            H = H.Clone();
            File.WriteAllText(destination + "\\h.json", H.toJSON(0), System.Text.Encoding.Default);

            return H;
        }

        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
        }

        public static Color valueToColor(double min, double max, double val)
        {
            double R = 0;
            double G = 0;
            double B = 0;

            R = (max - val) / (max - min) * 255;
            G = (Math.Abs(((max + min) / 2) - val)) / ((max + min) / 2) * 255;
            B = (val - min) / (max - min) * 255;

            return Color.FromArgb(255, Convert.ToInt32(R), Convert.ToInt32(G), Convert.ToInt32(B));
        }

    }
}
