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
            for (int i = 0; i < population_value; i++)
            {
                population[i] = new Hyperparameters(algorithm.h.toJSON(0), form1);
                population[i].setValueByName("model_name", population[i].nodes[0].name() + "[" + i.ToString() + "]");
            }
            name = algorithm.name;

            try { Directory.Delete(form1.pathPrefix + "Optimization\\" + name, true); } catch { }

            for (int i = 0; i < population_value; i++)
            {   //создание папок для индивидов
                Directory.CreateDirectory(form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]");
                //указание пути сохранения в параметрах
                population[i].setValueByName("save_folder", form1.pathPrefix + "Optimization\\" + name + "\\" + name + "[" + i.ToString() + "]"+"\\");
            }
            //добавление переменных в  MultiParameterVisualizer
            recurciveVariableAdding(0);
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 300);
            //добавление первых точек в  MultiParameterVisualizer
            variableChangeMonitoring();
            variablesVisualizer.addPoint(0, "target_function");
            variablesVisualizer.refresh();
        }

        void recurciveVariableAdding(int parentID)
        {
            List<Node> branches = algorithm.h.getNodesByparentID(parentID);
            var asdad = branches;
            if (branches.Count == 0)
            {
                if (algorithm.h.getNodeById(parentID).getAttributeValue("variable") != null)
                {
                    // добавить в список переменных
                    if (algorithm.h.getNodeById(parentID).getAttributeValue("variable") == "numerical")
                        variablesVisualizer.addParameter(algorithm.h.getNodeById(parentID).name(), Color.White, 200);

                    if (algorithm.h.getNodeById(parentID).getAttributeValue("variable") == "categorical")
                        variablesVisualizer.addParameter(algorithm.h.getNodeById(parentID).name(), Color.White, 100);

                    variablesNames.Add(algorithm.h.getNodeById(parentID).name());
                    variablesIDs.Add(algorithm.h.getNodeById(parentID).ID);
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
        int opt_inc;
        void optimization()
        {
            while (opt_inc < 10000)
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
                 algorithm.train().Wait();
            }
            else
            {
                Task[] trainTasks = new Task[population_value];
                for (int i = 0; i < population_value; i++)
                {
                    //проблема асинхронности
                    algorithm.h = new Hyperparameters(population[i].toJSON(0), form1);
                    trainTasks[i] = Task.Run(() => algorithm.train());
                }

                foreach (var task in trainTasks)
                    task.Wait();

                // сортировка по точности
                string temp;
                for (int i = 0; i < population_value - 1; i++)
                {
                    for (int j = i + 1; j < population_value; j++)
                    {
                        if (Convert.ToDouble(population[i].getValueByName("accuracy")) < Convert.ToDouble(population[j].getValueByName("accuracy")))
                        {
                            log("индивид [" + i.ToString() + "] 🢀 [" + j.ToString() + "]: " + population[i].getValueByName("accuracy") + "<" + population[j].getValueByName("accuracy"), Color.Orchid);

                            string tempFolder = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\temp";
                            string path_to_i = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + i.ToString() + "]";
                            string path_to_j = form1.pathPrefix + "Optimization\\" + algorithm.name + "\\" + algorithm.name + "[" + j.ToString() + "]";

                            try { Directory.Delete(tempFolder, true); } catch { }

                            try { Directory.CreateDirectory(tempFolder); } catch { }

                            foreach (string source in Directory.GetDirectories(path_to_i))
                            {
                                repeat:
                                try
                                {

                                    Directory.Move(source, tempFolder + "\\" + source.Split('\\')[source.Split('\\').Length - 1]);
                                }
                                catch { goto repeat; }
                            }
                            foreach (string source in Directory.GetDirectories(path_to_i))
                            {
                                try
                                {
                                    Directory.Delete(source);
                                }
                                catch { }
                            }
                            foreach (string source in Directory.GetDirectories(path_to_j))
                            {
                                repeat1:
                                try
                                {

                                    Directory.Move(source, path_to_i + "\\" + source.Split('\\')[source.Split('\\').Length - 1]);
                                }
                                catch
                                { goto repeat1; }
                            }
                            foreach (string source in Directory.GetDirectories(path_to_j)) { Directory.Delete(source); }

                            foreach (string source in Directory.GetDirectories(tempFolder))
                            {
                                repeat2:
                                try
                                {
                                    Directory.Move(source, path_to_j + "\\" + source.Split('\\')[source.Split('\\').Length - 1]);
                                }
                                catch { goto repeat2; }
                            }
                            temp = population[i].toJSON(0);
                            population[i] = new Hyperparameters(population[j].toJSON(0), form1);
                            population[j] = new Hyperparameters(temp, form1);
                        }
                    }
                }
            }
            //kill and concieve
            kill_and_conceive();
            //mutation
            for (int i = 0; i < mutation_rate; i++)
            { mutation(); }
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
                if (population[individIndex].nodes[variableID].getAttributeValue("variable") == "categorical")
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
                        population[inc] = get_child(population[j], population[j + 1]);
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
        Hyperparameters get_child(Hyperparameters parent1, Hyperparameters parent2)
        {
            Hyperparameters child = new Hyperparameters(parent1.toJSON(0), form1);

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
        void variableChangeMonitoring()
        {
            foreach (string variable in variablesNames)
            { variablesVisualizer.addPoint(population[0].getValueByName(variable), variable); }
        }

        void log(String s, Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }

    }
}
