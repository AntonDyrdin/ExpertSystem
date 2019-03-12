using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Drawing;

namespace Экспертная_система
{
    class AlgorithmOptimization
    {
        public string name;
        public Algorithm algorithm;
        public Hyperparameters[] population;
        public Hyperparameters A;
        Form1 form1;
        public int mutation_rate;
        public int population_value;
        public double elite_ratio;
        Random r = new Random();
        MultiParameterVisualizer variablesVisualizer;
        List<string> variablesNames;
        List<int> variablesIDs;
        public AlgorithmOptimization(Algorithm algorithm, Form1 form1, int population_value, int mutation_rate, double elite_ratio)
        {
            this.form1 = form1;
            this.algorithm = algorithm;
            this.elite_ratio = elite_ratio;
            this.mutation_rate = mutation_rate;
            this.population_value = population_value;
            variablesVisualizer = new MultiParameterVisualizer(form1.picBox, form1);
            population = new Hyperparameters[population_value];
            variablesNames = new List<string>();
            variablesIDs = new List<int>();

            for (int i = 0; i < population_value; i++) { population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1); }

            name = algorithm.name;

            try { Directory.Delete(form1.pathPrefix + "Optimization\\" + name, true); } catch { }

            //добавление переменных в  MultiParameterVisualizer
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 300);
            recurciveVariableAdding(0);
            //добавление первых точек в  MultiParameterVisualizer
            variableChangeMonitoring();
            variablesVisualizer.addPoint(0, "target_function");
            variablesVisualizer.refresh();

            refreshAOTree();
        }

        int opt_inc;
        void optimization()
        {
            while (opt_inc < 20)
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
            //асинхронное обучение индивидов
            if (opt_inc == 1)
            {
                string new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[0]" + "\\";
                Directory.CreateDirectory(new_save_folder);
                algorithm.h.setValueByName("save_folder", new_save_folder);
                string predictionsFilePath = new_save_folder + "predictions.txt";
                algorithm.h.setValueByName("predictions_file_path", predictionsFilePath);
                File.WriteAllText(new_save_folder + "json.txt", algorithm.h.toJSON(0), System.Text.Encoding.Default);

                algorithm.train().Wait();

                for (int i = 0; i < population_value; i++)
                {
                    population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1);
                    population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");

                    new_save_folder = form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\";
                    Algorithm.CopyFiles(population[i], algorithm.h.getValueByName("save_folder"), new_save_folder);
                }
            }
            else
            {
                //kill and concieve
                kill_and_conceive();

                //mutation
                for (int i = 0; i < mutation_rate; i++)
                { mutation(); }


                for (int i = Convert.ToInt16(Math.Round(population_value * elite_ratio)); i < population_value; i++)
                {
                    //проблема асинхронности
                    algorithm.h = new Hyperparameters(population[i].toJSON(0), form1);
                    algorithm.train().Wait();
                    population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1);
                    File.WriteAllText(population[i].getValueByName("json_file_path"), population[i].toJSON(0), System.Text.Encoding.Default);
                }

                // сортировка по точности
                string temp;
                for (int i = 0; i < population_value - 1; i++)
                {
                    for (int j = i + 1; j < population_value; j++)
                    {
                        double i_value = Convert.ToDouble(population[i].getValueByName("target_function").Replace('.', ','));
                        double j_value = Convert.ToDouble(population[j].getValueByName("target_function").Replace('.', ','));
                        if (i_value < j_value || (double.IsNaN(i_value) && (!double.IsNaN(j_value))))
                        {
                            log("индивид [" + i.ToString() + "] 🢀 [" + j.ToString() + "]: " + i_value + "<" + j_value, Color.Orchid);

                            string tempFolder = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\temp";
                            string path_to_i = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                            string path_to_j = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + j.ToString() + "]";

                            temp = population[i].toJSON(0);
                            population[i] = new Hyperparameters(population[j].toJSON(0), form1);
                            population[j] = new Hyperparameters(temp, form1);

                            Algorithm.MoveFiles(population[j], path_to_i, tempFolder);
                            Algorithm.MoveFiles(population[i], path_to_j, path_to_i);
                            Algorithm.MoveFiles(population[j], tempFolder, path_to_j);
                        }
                    }
                }

            }

            for (int i = 0; i < population_value; i++) population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");

            log("Обновление отображаемых параметров", Color.Lime);

            //refreshAOTree();
            // A.draw(0, form1.picBox, form1, 15, 150);
            variableChangeMonitoring();
            variablesVisualizer.addPoint(Convert.ToDouble(population[0].getValueByName("target_function").Replace('.', ',')), "target_function");
            variablesVisualizer.refresh();
        }
        void mutation()
        {
            foreach (int variableID in variablesIDs)
            {
                Random r = new Random();
                int individIndex = r.Next(Convert.ToInt16(Math.Round(population_value * elite_ratio)), population_value);
                if (population[individIndex].nodes[variableID].getAttributeValue("variable") == "categorical")
                {
                    int categoryIndex = r.Next(0, population[individIndex].nodes[variableID].getAttributeValue("categories").Split(',').Length);
                    string newValue = population[individIndex].nodes[variableID].getAttributeValue("categories").Split(',')[categoryIndex];
                    population[individIndex].nodes[variableID].setAttribute("value", newValue);
                }
                if (population[individIndex].nodes[variableID].getAttributeValue("variable") == "numerical")
                {
                    if (population[individIndex].nodes[variableID].getValue()[0] != '0')
                    {
                        int newValue = r.Next(Convert.ToInt16(population[individIndex].nodes[variableID].getAttributeValue("min")), Convert.ToInt16(population[individIndex].nodes[variableID].getAttributeValue("max")));
                        population[individIndex].nodes[variableID].setAttribute("value", newValue.ToString());
                    }
                    else
                    {
                        double newValue = r.NextDouble();

                        while (newValue > Convert.ToDouble(population[individIndex].nodes[variableID].getAttributeValue("min").Replace('.', ',')) && newValue < Convert.ToDouble(population[individIndex].nodes[variableID].getAttributeValue("max").Replace('.', ',')))
                        {
                            newValue = r.NextDouble();
                        }
                        population[individIndex].nodes[variableID].setAttribute("value", newValue.ToString().Replace(',', '.'));
                    }
                }
            }
        }
        void kill_and_conceive()
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
        Hyperparameters get_child(Hyperparameters parent1, Hyperparameters parent2, Hyperparameters old)
        {
            Hyperparameters child = new Hyperparameters(old.toJSON(0), form1);

            foreach (int variableID in variablesIDs)
            {
                Random r = new Random();
                //родитель гена выбирается случайно
                int parent_of_gene = r.Next(0, 2);
                if (parent_of_gene == 0)
                    child.nodes[variableID].setAttribute("value", parent1.nodes[variableID].getValue());
                else
                    child.nodes[variableID].setAttribute("value", parent2.nodes[variableID].getValue());
            }
            return child;
        }

        Thread newthread;
        public void run()
        {
            opt_inc = 0;
            newthread = new Thread(optimization);
            newthread.Start();
            log("OPTIMIZATION STARTED", Color.Cyan);
        }
        void recurciveVariableAdding(int ID)
        {
            List<Node> branches = algorithm.h.getNodesByparentID(ID);
            var asdad = branches;
            if (branches.Count == 0)
            {
                if (algorithm.h.getNodeById(ID).getAttributeValue("variable") != null)
                {
                    // добавить в список переменных
                    if (algorithm.h.getNodeById(ID).getAttributeValue("variable") == "numerical")
                        variablesVisualizer.addParameter(algorithm.h.getNodeById(ID).name() + '[' + ID.ToString() + ']', Color.Cyan, 200);

                    if (algorithm.h.getNodeById(ID).getAttributeValue("variable") == "categorical")
                    {
                        variablesVisualizer.addParameter(algorithm.h.getNodeById(ID).name() + '[' + ID.ToString() + ']', Color.Black, 50);
                        variablesVisualizer.parameters[variablesVisualizer.parameters.Count - 1].mainFontDepth = 14;
                    }

                    variablesNames.Add(algorithm.h.getNodeById(ID).name() + '[' + ID.ToString() + ']');
                    variablesIDs.Add(ID);
                }
            }
            else
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    recurciveVariableAdding(branches[i].ID);
                }
            }
        }
        void variableChangeMonitoring()
        {
            foreach (int variableID in variablesIDs)
            { variablesVisualizer.addPoint(population[0].nodes[variableID].getValue(), population[0].nodes[variableID].name() + '[' + variableID.ToString() + ']'); }
        }
        void refreshAOTree()
        {
            A = new Hyperparameters(form1, "Algorithm_Population");
            for (int i = 0; i < population_value; i++) { A.addBranch(population[i], population[i].getValueByName("model_name"), 0); }
        }
        void log(String s, Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }

    }
}
