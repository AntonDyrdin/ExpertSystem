using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.IO;

namespace Экспертная_система
{

    class Optimization
    {
        public string name;
        public Expert expert;
        public Hyperparameters[] population;
        public Hyperparameters O;
        Form1 form1;
        public DateTime date1;
        public DateTime date2;
        public int mutation_rate;
        public int population_value;
        public double elite_ratio;
        Random r = new Random();

        public Optimization(Expert expert, Form1 form1, DateTime date1, DateTime date2, int population_value, int mutation_rate, double elite_ratio)
        {
            this.elite_ratio = elite_ratio;
            this.mutation_rate = mutation_rate;
            this.population_value = population_value;
            population = new Hyperparameters[population_value];
            this.date1 = date1;
            this.date2 = date2;
            this.form1 = form1;
            for (int i = 0; i < population_value; i++)
            {
                population[i] = new Hyperparameters( expert.H.toJSON(0),form1);
            }
            name = "Expert";

            //   profitVisualizer = new ParameterVisualizer(form1.profit_optimization, form1, "profit", Color.Purple);
            individVisualizer = new MultiParameterVisualizer(form1.picBox, form1);

            List<Node> algorithmsBranches = expert.H.getNodesByparentID(expert.committeeNodeID);
            foreach (Node algorithmBranch in algorithmsBranches)
            {
                int parentID = algorithmBranch.ID;
                recurciveVariableAdding(parentID);
            }
            individVisualizer.addParameter("target_function", Color.LightCyan, 300);

            foreach (Node algorithmBranch in algorithmsBranches)
            {
                int parentID = algorithmBranch.ID;
                recurciveVariableChangeMonitoring(parentID);
            }

            individVisualizer.addPoint(0, "target_function");
            individVisualizer.refresh();
            // form1.stringDelegate = new Form1.StringDelegate(form1.set_date1_date2);
            // form1.date1_date2.Invoke(form1.stringDelegate, date1.ToShortDateString() + " - " + date2.ToShortDateString());
        }
        void recurciveVariableChangeMonitoring(int parentID)
        {
            List<Node> parameter = expert.H.getNodesByparentID(parentID);
            if (parameter == null)
            {
                if (expert.H.getNodeById(parentID).getAttributeValue("variable") != null)
                {
                    // новая точка на графике
                    individVisualizer.addPoint(expert.H.getNodeById(parentID).getValue(), expert.H.getNodeById(parentID).name());
                }
            }
            else
            {
                for (int i = 0; i < parameter.Count; i++)
                {
                    recurciveVariableAdding(parameter[i].ID);
                }
            }
        }
        void recurciveVariableAdding(int parentID)
        {
            List<Node> parameter = expert.H.getNodesByparentID(parentID);
            if (parameter == null)
            {
                if (expert.H.getNodeById(parentID).getAttributeValue("variable") != null)
                {
                    // добавить в список переменных
                    if (expert.H.getNodeById(parentID).getAttributeValue("variable") == "numerical")
                        individVisualizer.addParameter(expert.H.getNodeById(parentID).name(), Color.White, 200);

                    if (expert.H.getNodeById(parentID).getAttributeValue("variable") == "categorical")
                        individVisualizer.addParameter(expert.H.getNodeById(parentID).name(), Color.White, 100);
                }
            }
            else
            {
                for (int i = 0; i < parameter.Count; i++)
                {
                    recurciveVariableAdding(parameter[i].ID);
                }
            }
        }

        ParameterVisualizer profitVisualizer;
        MultiParameterVisualizer individVisualizer;

        public async void iteration_of_optimization()
        {


            //пересмотреть порядок селекции
            if (opt_inc != 1)
            {
                log("kill_and_conceive()", Color.LightBlue);
             //   kill_and_conceive();
                for (int i = 0; i < mutation_rate; i++)
                { mutation(); }
            }

            int start_train_index = 0;

            //если итерация первая, обучается только первый индивид и копируется в остальные, иначе - все новые 
            if (opt_inc == 1)
            { start_train_index = 0; }
            else
            { start_train_index = Convert.ToInt16(Math.Round(population_value * elite_ratio)); }

            //если итерация первая, обучаются все индивиды, иначе - только новые 
            /*   if (opt_inc == 1)
               { start_rewrite_index = 0; }
               else
               { start_rewrite_index = Convert.ToInt16(Math.Round(population_value * elite_ratio)); }*/


            //цикл по индивидам (комитетам)
            for (int i = start_train_index; i < population_value; i++)
            {
                expert.algorithms.Clear();
                //  СПИСОК ВЕТВЕЙ АЛГОРИТМОВ
                List<Node> algorithmBranches = population[i].getNodesByparentID(expert.committeeNodeID);
                //цикл по алгоритмам
                for (int j = 0; j < algorithmBranches.Count; j++)
                {
                    //  копирование параметров алгоритмов i-ого индивида в класс Expert для последующего обучения
                    if (algorithmBranches[i].name() == "LSTM_1")
                        expert.algorithms.Add(new LSTM_1(form1, "LSTM_1"));
                    if (algorithmBranches[i].name() == "LSTM_2")
                        expert.algorithms.Add(new LSTM_2(form1, "LSTM_2"));
                    if (algorithmBranches[i].name() == "ANN_1")
                        expert.algorithms.Add(new ANN_1(form1, "ANN_1"));
                    expert.algorithms[expert.algorithms.Count - 1].Open(new Hyperparameters(expert.H.toJSON(algorithmBranches[i].ID), form1));

                    //определение пути сохранения
                    //   population[i].prediction_Algorithms[j].get_Hyperparameters().set_parameter_by_name("save_folder",
                    //      @"D:\main\experts\" + name + "\\temp" + name + " [" + i.ToString() + ']' + "\\" + population[i].prediction_Algorithms[j].get_name() + " [" + j.ToString() + ']');
                    expert.algorithms[j].h.setValueByName("save_folder", expert.path_prefix + "experts\\" + expert.expertName + "\\" + expert.expertName + "[" + i.ToString() + ']' + "\\" + expert.algorithms[j].modelName);
                    //обучение
                    log("train " + algorithmBranches[i].name() + " [" + i.ToString() + "] " + '[' + j.ToString() + ']', Color.White);

                    await expert.algorithms[j].train();

                    //в случае ошибки обучения, дефектный индивид копирует случайного родителя
                    /*   if (!script_conclusion.Contains("successfully_trained"))
                       {
                           log(population[i].prediction_Algorithms[j].hyperparameters.ToString(), Color.Yellow);
                           int index = r.Next(0, Convert.ToInt16(Math.Round(population_value * elite_ratio)));
                           string save_to = population[i].prediction_Algorithms[j].hyperparameters.get_value_by_name("save_folder");
                           string save_from = population[index].prediction_Algorithms[j].hyperparameters.get_value_by_name("save_folder");

                           //копируются файлы родителя
                           foreach (string file in Directory.GetFiles(save_from))
                           {
                               var newPath = save_to + "\\" + file.Split('\\')[file.Split('\\').Length - 1];
                               File.Copy(file, save_to + "\\" + file.Split('\\')[file.Split('\\').Length - 1], true);
                           }

                           population[i].prediction_Algorithms[j] = population[index].prediction_Algorithms[j].DeepClone();
                           population[i].prediction_Algorithms[j].hyperparameters.set_parameter_by_name("save_folder", save_to);
                           log("индивид [" + i.ToString() + "] алгоритм " + population[i].prediction_Algorithms[j].get_name() + "-> заменён копией индивида [" + index.ToString() + "]: из-за ошибки при обучении", Color.Orchid);
                           log(script_conclusion, Color.Red);
                       }
                       else
                       {
                           //////////////////////////////////////
                           // log(script_conclusion, Color.White);
                           ////////////////////////////////////
                       }  */
                }

                population[i].setValueByName("target_function", expert.algorithms[0].accuracy.ToString());
            }

            //ТЕСТ
            for (int i = start_train_index; i < population_value; i++)
            {
                /* string s = population[i].get_profit_by_date1_date2(date1, date2);

                 if (s.IndexOf("Traceback") != -1)
                 {
                     int index = s.IndexOf("Traceback");
                     log(s.Substring(0, index), Color.White);
                     log(s.Substring(index, s.Length - index), Color.Red);
                     population[i].target_function = -1;
                 }
                 else if (s.IndexOf("error") != -1)
                 {
                     int index = s.IndexOf("error");
                     log(s.Substring(0, index), Color.White);
                     log(s.Substring(index, s.Length - index), Color.Red);
                     population[i].target_function = -1;
                 }
                 else    */

                log("target_function[" + i.ToString() + "] = " + population[i].getValueByName("target_function"), Color.LightCyan);
            }

            // СОРТИРОВКА
            string temp;
            for (int i = 0; i < population_value - 1; i++)
            {
                for (int j = i + 1; j < population_value; j++)
                {
                    if (Convert.ToDouble(population[i].getValueByName("target_function")) < Convert.ToDouble(population[j].getValueByName("target_function")))
                    {
                        log("индивид [" + i.ToString() + "] 🢀 [" + j.ToString() + "]: " + population[i].getValueByName("target_function") + "<" + population[j].getValueByName("target_function"), Color.Orchid);

                        string tempFolder = expert.path_prefix + "experts\\" + expert.expertName + @"\temp";
                        string path_to_i = expert.path_prefix + "experts\\" + expert.expertName + @"\" + expert.expertName + "[" + i.ToString() + "]";
                        string path_to_j = expert.path_prefix + "experts\\" + expert.expertName + @"\" + expert.expertName + "[" + j.ToString() + "]";
                        try
                        {
                            Directory.Delete(tempFolder, true);
                        }
                        catch { }

                        try
                        {
                            Directory.CreateDirectory(tempFolder);
                        }
                        catch { }

                        foreach (string source in Directory.GetDirectories(path_to_i))
                        {
                            repeat:
                            try
                            {

                                Directory.Move(source, tempFolder + "\\" + source.Split('\\')[source.Split('\\').Length - 1]);
                            }
                            catch
                            {
                                goto repeat;
                            }
                        }
                        foreach (string source in Directory.GetDirectories(path_to_i))
                        {

                            try
                            {
                                Directory.Delete(source);
                            }
                            catch
                            {
                            }
                        }
                        foreach (string source in Directory.GetDirectories(path_to_j))
                        {
                            repeat1:
                            try
                            {

                                Directory.Move(source, path_to_i + "\\" + source.Split('\\')[source.Split('\\').Length - 1]);
                            }
                            catch
                            {
                                goto repeat1;
                            }
                        }
                        foreach (string source in Directory.GetDirectories(path_to_j))
                        {
                            Directory.Delete(source);
                        }
                        foreach (string source in Directory.GetDirectories(tempFolder))
                        {
                            repeat2:
                            try
                            {
                                Directory.Move(source, path_to_j + "\\" + source.Split('\\')[source.Split('\\').Length - 1]);
                            }
                            catch
                            {
                                goto repeat2;
                            }
                        }
                        temp = population[i].toJSON(0);
                        population[i] = new Hyperparameters(population[j].toJSON(0), form1);
                        population[j] = new Hyperparameters(temp, form1);
                    }
                }
            }

         /*  //обновление save_folder после изменения расположения
            for (int j = 0; j < population_value; j++)
            {
                for (int i = 0; i < population[j].prediction_Algorithms.Count; i++)
                {
                    string directory = @"D:\main\experts\" + name + @"\temp\" + name + '[' + j.ToString() + ']' + @"\" + population[j].prediction_Algorithms[i].get_name() + '[' + i.ToString() + ']';
                    population[j].prediction_Algorithms[i].get_Hyperparameters().set_parameter_by_name("save_folder", directory);
                }
            }

            profitVisualizer.addPoint(population[0].target_function);
            // individVisualizer.addPoint(population[0].target_function, "target_function");
            populationvisualizer.draw(population);
            int inc = 0;
            foreach (Prediction_algorithm PA in population[0].prediction_Algorithms)
            {
                inc++;
                foreach (Parameter param in PA.hyperparameters.nodes)
                {
                    if (!param.is_categorical && !param.is_const)
                    {
                        individVisualizer.addPoint(param.value, param.name + '[' + inc.ToString() + ']');
                    }
                }
            }
            individVisualizer.refresh();   */
        }

     /*   void kill_and_conceive()
        {
            int inc = Convert.ToInt16((Math.Round(population_value * elite_ratio)));
            if (Convert.ToInt16((Math.Round(population_value * elite_ratio))) - 1 <= 1)
            { }
            else
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
        }   */


       /*   Expert get_child(Expert parent1, Expert parent2)
        {
          Expert child = new Expert(form1);
            //!!!!!!!!!!!!! ВОЗМОЖНА ОШИБКА !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            child = parent1.DeepClone();

            for (int i = 0; i < parent1.prediction_Algorithms.Count; i++)
            {
                for (int j = 0; j < child.prediction_Algorithms[i].get_Hyperparameters().nodes.Count; j++)
                {
                    Random r = new Random();
                    //родитель гена выбирается случайно
                    int parent_of_gene = r.Next(0, 2);
                    if (parent_of_gene == 0)
                        child.prediction_Algorithms[i].get_Hyperparameters().nodes[j].value = parent1.prediction_Algorithms[i].get_Hyperparameters().nodes[j].value;
                    else
                        child.prediction_Algorithms[i].get_Hyperparameters().nodes[j].value = parent1.prediction_Algorithms[i].get_Hyperparameters().nodes[j].value;
                }
            }
                          
            return child;    
        }                   */
        void mutation()
        {
         /*   int r1 = r.Next(Convert.ToInt16(Math.Round(population_value * elite_ratio)), population_value);
            int r2 = r.Next(0, population[r1].prediction_Algorithms.Count);
            int r3 = r.Next(0, population[r1].prediction_Algorithms[r2].get_Hyperparameters().nodes.Count);
            string param_name = population[r1].prediction_Algorithms[r2].get_Hyperparameters().nodes[r3].name;
            if (!population[r1].prediction_Algorithms[r2].get_Hyperparameters().get_parameter_by_name(param_name).is_const)
            {
                log("CHANGE: individ[" + r1.ToString() + "] in " + population[r1].prediction_Algorithms[r2].get_name(), Color.White);
                string temp = param_name + " = " + population[r1].prediction_Algorithms[r2].get_Hyperparameters().get_value_by_name(param_name);

                population[r1].prediction_Algorithms[r2].get_Hyperparameters().variate(param_name);

                log(temp + " => " + population[r1].prediction_Algorithms[r2].get_Hyperparameters().get_value_by_name(param_name), Color.Cyan);
            }   */
        }



        Thread newthread;

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
        public void run_optimization()
        { /*
            try
            {
                Directory.Delete(@"D:\main\experts\" + name + @"\temp");
            }
            catch { }

            try
            {
                Directory.CreateDirectory(@"D:\main\experts\" + name + @"\temp");

                for (int j = 0; j < population_value; j++)
                {

                    for (int i = 0; i < population[j].prediction_Algorithms.Count; i++)
                    {
                        string directory = @"D:\main\experts\" + name + @"\temp\" + name + '[' + j.ToString() + ']' + @"\" + population[j].prediction_Algorithms[i].get_name() + '[' + i.ToString() + ']';

                        Directory.CreateDirectory(directory);

                        population[j].prediction_Algorithms[i].get_Hyperparameters().set_parameter_by_name("save_folder", directory);
                    }
                }
            }
            catch { }

            opt_inc = 0;
            newthread = new Thread(optimization);
            newthread.Start();
            log("OPTIMIZATION STARTED", Color.Cyan);    */
        }                                               

        public void stop_optimization()
        {

            newthread.Abort();
            log("OPTIMIZATION STOPPED", Color.Cyan);
        }
        void log(String s, Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.picBox.Invoke(form1.logDelegate, form1.picBox, s, col);
        }   
    }
}
/*        ////////////////// VARIATE //////////////////////////////
public void variate(string name)
{
    Node node = getNodeByName(name)[0];
    Random r = new Random();
    if (!Convert.ToBoolean(node.getAttributeValue("is_const")))
        if (Convert.ToBoolean(node.getAttributeValue("is_categorical")))
        {
            if (Convert.ToBoolean(node.getAttributeValue("categories") != null))
            {
                string lastValue = node.getValue();
                node.setAttribute("value", node.getAttributeValue("categories").Split('|')[r.Next(node.getAttributeValue("categories").Split('|').Length)]);
                if (lastValue == node.getValue())
                {
                    node.setAttribute("is_change", "true");
                }
                else
                {
                    node.setAttribute("is_change", "false");
                }
            }
            else
            {
                log("не удалось варьировать категориальный параметр: множество категорий пустое", System.Drawing.Color.Red);
            }
        }
        else
        {
            int last_value = Convert.ToInt32(node.getValue());
            int new_value = 0;

            new_value = r.Next(Convert.ToInt32(node.getAttributeValue("min")), Convert.ToInt32(node.getAttributeValue("max")));

            if (new_value > last_value)
                node.setAttribute("is_change_up_or_down", "true");

            if (new_value < last_value)
                node.setAttribute("is_change_up_or_down", "false");

            if (last_value != new_value)
            {
                node.setAttribute("is_change", "true");
            }
            else
            {
                node.setAttribute("is_change", "false");
            }
        }
}*/
