using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Экспертная_система
{
    internal class ExpertOptimization
    {
        public string name;
        public Expert expert;
        public Hyperparameters[] population;
        public Hyperparameters A;
        private Form1 form1;
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
        public ExpertOptimization(Expert expert, Form1 form1, int population_value, int mutation_rate, double elite_ratio, int Iterarions, DateTime date1, DateTime date2, string rawDatasetFilePath)
        {
            r = new Random();
            this.Iterarions = Iterarions;
            this.rawDatasetFilePath = rawDatasetFilePath;
            this.form1 = form1;
            this.expert = expert;
            this.elite_ratio = elite_ratio;
            this.mutation_rate = mutation_rate;
            this.population_value = population_value;
            this.date1 = date1;
            this.date2 = date2;
            variablesVisualizer = new MultiParameterVisualizer(form1.picBox, form1);
            population = new Hyperparameters[population_value];
            variablesNames = new List<string>();
            variablesIDs = new List<int>();

            this.agentManager = form1.I.agentManager;

            for (int i = 0; i < population_value; i++) { population[i] = new Hyperparameters(expert.H.toJSON(0), form1); }

            name = expert.expertName;

            try { Directory.Delete(form1.pathPrefix + "Optimization\\" + name, true); } catch { }

            refreshAOTree();
            variablesVisualizer.enableGrid = false;
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 300);
            //добавление переменных в  MultiParameterVisualizer
            for (int i = 0; i < population_value; i++)
                variablesVisualizer.parameters[0].functions.Add(new Function(" [" + i.ToString() + "]", valueToColor(0, population_value, population_value - i - 1)));
            for (int i = 0; i < population_value; i++)
            {
                variablesVisualizer.addParameter("deposit history [" + i.ToString() + "]", Color.LightCyan, 300);
                variablesVisualizer.parameters.Last().functions.Add(new Function("deposit1 [" + i.ToString() + "]", Color.Pink));
                variablesVisualizer.parameters.Last().functions.Add(new Function("deposit2 [" + i.ToString() + "]", Color.Green));
                variablesVisualizer.parameters.Last().functions.Add(new Function("exit [" + i.ToString() + "]", Color.LightSeaGreen));
            }

            recurciveVariableAdding(expert.H, 0, name + "[0]");
            variablesVisualizer.refresh();
        }

        private int opt_inc;

        private void optimization()
        {
            while (opt_inc < Iterarions)
            {
                var now = new DateTimeOffset(DateTime.Now);

                var start = now.ToUnixTimeSeconds();
                opt_inc++;
                iteration_of_optimization();
                log(opt_inc.ToString() + "_ITERATION COMPLETE " + TimeSpan.FromSeconds((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()) - start).ToString(), Color.Green);
            }
        }
        public void iteration_of_optimization()
        {
            if (opt_inc == 1)
            {
                string new_save_folder;
                expert.synchronizeHyperparameters();
                for (int j = 0; j < expert.algorithms.Count; j++)
                {
                    new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[0]" + "\\" + expert.algorithms[j].name + "[" + j.ToString() + "]\\";
                    Directory.CreateDirectory(new_save_folder);
                    expert.algorithms[j].h.setValueByName("save_folder", new_save_folder);
                    string predictionsFilePath = new_save_folder + "predictions.txt";
                    expert.algorithms[j].h.setValueByName("predictions_file_path", predictionsFilePath);
                    expert.algorithms[j].h.setValueByName("json_file_path", new_save_folder + "json.txt");
                    File.WriteAllText(new_save_folder + "json.txt", expert.algorithms[j].h.toJSON(0), System.Text.Encoding.Default);

                    expert.algorithms[j].train().Wait();
                }
                expert.synchronizeHyperparameters();
                expert.Save(form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[0]");
                expert.test(date1, date2, rawDatasetFilePath);

                //отрисовка истрии баланса
                for (int i = 0; i < expert.deposit1History.Count; i++)
                {
                    variablesVisualizer.addPoint(expert.deposit2History[i] + expert.deposit1History[i] * expert.closeValueHistory[i], "exit [0]");
                    variablesVisualizer.addPoint(expert.deposit2History[i], "deposit2 [0]");
                  //  variablesVisualizer.addPoint(expert.deposit1History[i], "deposit1 [0]");
                }
                for (int i = 0; i < population_value; i++)
                {
                    population[i] = new Hyperparameters(expert.H.toJSON(0), form1);
                    population[i].setValueByName("name", population[i].nodes[0].name() + "[" + i.ToString() + "]");
                    for (int j = 0; j < expert.algorithms.Count; j++)
                    {
                        new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + expert.expertName + "[" + i.ToString() + "]" + "\\" + expert.algorithms[j].name + "[" + j.ToString() + "]\\";
                        //новые пути прописываются в json.txt автоматически
                        Algorithm.CopyFiles(expert.algorithms[j].h, expert.algorithms[j].h.getValueByName("save_folder"), new_save_folder);
                    }
                }
                for (int i = 1; i < population_value; i++)
                {
                    variablesIDs.Clear();
                  //  !!!!!!!!!!!!!!
                 //   recurciveVariableAdding(population[i], 0, population[i].getValueByName("model_name"));
                }

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

                if (agentManager.agents.Count == 0)
                {
                    log("Ожидание агентов...", Color.Yellow);
                    while (agentManager.agents.Count == 0) { Thread.Sleep(1000); }
                }
                else
                {
                    for (int i = Convert.ToInt16(Math.Round(population_value * elite_ratio)); i < population_value; i++)
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
                        for (int j = 0; j < toDelete.Count; i++)
                            population[i].deleteBranch(toDelete[i].ID);

                        //приращение новых конфигураций к узлу "committee"
                        for (int j = 0; j < agentManager.tasks.Count; j++)
                        {
                            population[i].addBranch(agentManager.tasks[j].h, agentManager.tasks[j].h.nodes[0].name(), expert.committeeNodeID);
                        }
                        agentManager.tasks.Clear();
                    }
                }
                /////TEST
              //  TEST
              //  TEST
                /*    //отрисовка истрии баланса
                    foreach (double value in expert.deposit1History)
                    {
                        variablesVisualizer.addPoint(Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ',')), "deposit1 [" + i.ToString() + "]");
                    }
                    variablesVisualizer.addPoint(Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ',')), "deposit2 [" + i.ToString() + "]");
                    variablesVisualizer.addPoint(Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ',')), "exit [" + i.ToString() + "]");
                    */
                // сортировка
                string temp;
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

                            temp = population[i].toJSON(0);
                            population[i] = new Hyperparameters(population[j].toJSON(0), form1);
                            population[j] = new Hyperparameters(temp, form1);

                            //пермещение файлов 
                            //  Algorithm.MoveFiles(population[j], path_to_i, tempFolder);
                            List<Node> toTempFolder = population[j].getNodesByparentID(expert.committeeNodeID);
                            foreach (Node algorithmBranch in toTempFolder)
                            {
                                var h = new Hyperparameters(population[j].toJSON(algorithmBranch.ID), form1);
                                Algorithm.MoveFiles(h, path_to_i + "\\" + h.nodes[0].name(), tempFolder + "\\" + h.nodes[0].name());
                            }
                            foreach (string file in Directory.GetFiles(path_to_i))
                            {
                                File.Move(file, tempFolder + Path.GetFileName(file));
                            }

                            //  Algorithm.MoveFiles(population[i], path_to_j, path_to_i);
                            List<Node> toI = population[i].getNodesByparentID(expert.committeeNodeID);
                            foreach (Node algorithmBranch in toI)
                            {
                                var h = new Hyperparameters(population[i].toJSON(algorithmBranch.ID), form1);
                                Algorithm.MoveFiles(h, path_to_j + "\\" + h.nodes[0].name(), path_to_i + "\\" + h.nodes[0].name());
                            }
                            foreach (string file in Directory.GetFiles(path_to_j))
                            {
                                File.Move(file, path_to_i + Path.GetFileName(file));
                            }

                            //Algorithm.MoveFiles(population[j], tempFolder, path_to_j);
                            List<Node> toJ = population[j].getNodesByparentID(expert.committeeNodeID);
                            foreach (Node algorithmBranch in toI)
                            {
                                var h = new Hyperparameters(population[j].toJSON(algorithmBranch.ID), form1);
                                Algorithm.MoveFiles(h, tempFolder + "\\" + h.nodes[0].name(), path_to_j + "\\" + h.nodes[0].name());
                            }
                            foreach (string file in Directory.GetFiles(tempFolder))
                            {
                                File.Move(file, path_to_j + Path.GetFileName(file));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < population_value; i++) population[i].setValueByName("name", population[i].nodes[0].name() + "[" + i.ToString() + "]");

            log("Обновление отображаемых параметров", Color.Lime);

            refreshAOTree();
            // A.draw(0, form1.picBox, form1, 15, 150);

            variableChangeMonitoring();

            for (int i = 0; i < population_value; i++)
            {
                variablesVisualizer.addParameter("deposit history [" + i.ToString() + "]", Color.LightCyan, 300);
                variablesVisualizer.parameters.Last().functions.Add(new Function("deposit1 [" + i.ToString() + "]", Color.Pink));
                variablesVisualizer.parameters.Last().functions.Add(new Function("deposit2 [" + i.ToString() + "]", Color.Green));
                variablesVisualizer.parameters.Last().functions.Add(new Function("exit [" + i.ToString() + "]", Color.LightSeaGreen));
            }
            for (int i = 0; i < population_value; i++)
            {
                variablesVisualizer.addPoint(Convert.ToDouble(population[i].getValueByName("expert_target_function").Replace('.', ',')), " [" + i.ToString() + "]");
            }

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

                    while (newValue > Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("min").Replace('.', ',')) && newValue < Convert.ToDouble(population[individIndex].nodes[variableIndex].getAttributeValue("max").Replace('.', ',')))
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
                    if (h.getNodeById(ID).getAttributeValue("variable") == "numerical")
                        variablesVisualizer.addParameter(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Cyan, 200);

                    if (h.getNodeById(ID).getAttributeValue("variable") == "categorical")
                    {
                        variablesVisualizer.addParameter(modelName + " " + h.getNodeById(ID).name() + " id=" + ID.ToString(), Color.Black, 40);
                        variablesVisualizer.parameters[variablesVisualizer.parameters.Count - 1].mainFontDepth = 12;
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
            foreach (Hyperparameters individ in population)
                foreach (int variableID in variablesIDs)
                {
                    string variableName = individ.getValueByName("model_name") + " " + individ.getNodeById(variableID).name() + " id=" + variableID.ToString();
                    string value = individ.nodes[variableID].getValue();
                    variablesVisualizer.addPoint(value, variableName);
                }
        }

        private void refreshAOTree()
        {
            A = new Hyperparameters(form1, "Algorithm_Population");
            for (int i = 0; i < population_value; i++) { A.addBranch(population[i], population[i].nodes[0].name(), 0); }
        }

        private void log(String s, Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
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

    }
}
