using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Экспертная_система
{
    public class MyAlgorithmOptimization
    {
        //   
        //    х_______х_______х
        //    |               |
        //    |               |
        //    x       x       x       
        //    |               |
        //    |               |
        //    x_______x_______x
        //
        //

        // список точек, на которых была найдена целевая функция
        public List<TargetFunctionPoint> explored;

        // все ключевые точки текущего гиперкуба
        public List<TargetFunctionPoint> currentHypercube;

        // пул гиперпараметров
        public Hyperparameters[] pool;

        //текущая точка (value) и углы (min, max) гиперкуба
        public Hyperparameters P;

        public Algorithm algorithm;

        public MainForm form1;
        public int threads = 2;

        string report_file_name;

        private MultiParameterVisualizer variablesVisualizer;
        private int test_count = 3;
        int variablesCount;
        public enum TargetFunctionType
        {
            ACCURACY,
            STDDEV
        }
        TargetFunctionType target_function_type = TargetFunctionType.STDDEV;
        public MyAlgorithmOptimization(MainForm form1, Algorithm algorithm)
        {
            this.algorithm = algorithm;
            explored = new List<TargetFunctionPoint>();
            this.form1 = form1;
            variablesVisualizer = new MultiParameterVisualizer(form1.picBox, form1);
            variablesVisualizer.addParameter("max Q", Color.Cyan, 400);
            variablesVisualizer.addParameter("target_function", Color.LightCyan, 800);




            //VVVVVVVVVVV



            //    1)разобраться с косяками на графике всех индивидов

            //    2)почему при нахождении центра получаются близкие, но разные числа

            //    3)добиться одинакового числа точек в currentHypercube связано с (1)

            //    4) прикрутить красивые графики везде, где уместно


            // -параллелизм
            //for (int i = 0; i < population_value; i++)
            //     variablesVisualizer.parameters[1].functions.Add(new Function(" [" + i.ToString() + "]", valueToColor(0, population_value, population_value - i - 1)));

            report_file_name = form1.pathPrefix + "/MyAlgorithmOptimization/Optimization report " + DateTime.Now.ToString().Replace(':', '-') + ".txt";

            P = algorithm.h.Clone();
            string reportHeader = "";
            for (int i = 0; i < P.nodes.Count; i++)
            {
                if (P.nodes[i].getAttributeValue("variable") == "numerical")
                {
                    reportHeader += P.nodes[i].getAttributeValue("name") + ';';
                    variablesCount++;
                }
            }

            variablesVisualizer.enableGrid = false;

            int fucnCount = Convert.ToInt32(Math.Pow(3.0, Convert.ToDouble(variablesCount)));
            for (int i = 0; i < fucnCount; i++)
                variablesVisualizer.parameters[1].functions.Add(new Function(" [" + i.ToString() + "]", ParameterVisualizer.valueToColor(0, fucnCount, fucnCount - i - 1)));

            File.WriteAllText(report_file_name, reportHeader + "Q;" + '\n');

            initPool();
            variablesVisualizer.refresh();
        }

        TargetFunctionPoint lastP;
        public void run()
        {
            for (int it = 0; it < 20; it++)
            {
                currentHypercube = new List<TargetFunctionPoint>();

                iteration();

                // поиск максимума
                TargetFunctionPoint maxQ = new TargetFunctionPoint(null, 0);

                for (int i = 0; i < currentHypercube.Count; i++)
                {
                    variablesVisualizer.addPoint(currentHypercube[i].Q, " [" + i.ToString() + "]");

                    if (maxQ.Q < currentHypercube[i].Q)
                        maxQ = currentHypercube[i];
                }

                variablesVisualizer.addPoint(maxQ.Q, "max Q");

                form1.log(getReportLine(maxQ), Color.Magenta);

                bool isOptimumInCenter = true;

                for (int i = 0; i < P.nodes.Count; i++)
                {
                    if (P.nodes[i].getAttributeValue("variable") == "numerical")
                    {
                        string maxQVariableValue = maxQ.h.nodes[i].getValue();
                        string variableCenter = getVariableCenter(P.nodes[i]);
                        if (maxQVariableValue != variableCenter)
                        { isOptimumInCenter = false; }
                    }
                }


                if (isOptimumInCenter)
                {
                    //если максимум в центре - сжатие
                    form1.log("Сжатие", Color.Cyan);
                    variablesVisualizer.markLast("Сжатие", "max Q");
                    for (int i = 0; i < P.nodes.Count; i++)
                    {
                        if (P.nodes[i].getAttributeValue("variable") == "numerical")
                        {
                            string newMin = "";
                            string newMax = "";
                            if (P.nodes[i].getAttributeValue("max").Contains('.') || P.nodes[i].getAttributeValue("max").Contains(','))
                            {
                                double min_double = double.Parse(P.nodes[i].getAttributeValue("min").Replace('.', ','));
                                double max_double = double.Parse(P.nodes[i].getAttributeValue("max").Replace('.', ','));
                                // сужение интервала
                                if (min_double + ((max_double - min_double) / 4) > 0)
                                {
                                    newMin = (min_double + ((max_double - min_double) / 4)).ToString().Replace(',', '.');
                                }
                                else
                                {
                                    newMin = min_double.ToString().Replace(',', '.');
                                }
                                newMax = (max_double - ((max_double - min_double) / 4)).ToString().Replace(',', '.');
                            }
                            else
                            {
                                int min_int = int.Parse(P.nodes[i].getAttributeValue("min"));
                                int max_int = int.Parse(P.nodes[i].getAttributeValue("max"));
                                // сужение интервала
                                if (min_int + ((max_int - min_int) / 4) >= 2)
                                {
                                    newMin = (min_int + ((max_int - min_int) / 4)).ToString();
                                }
                                else
                                {
                                    newMin = min_int.ToString();
                                }
                                newMax = (max_int - ((max_int - min_int) / 4)).ToString();
                            }
                            P.nodes[i].setAttribute("min", newMin);
                            P.nodes[i].setAttribute("max", newMax);
                        }
                    }
                }
                else
                {
                    //иначе - перемещение центра в точку максимума
                    form1.log("Перемещение из " + getReportLine(lastP) + " в " + getReportLine(maxQ), Color.Cyan);
                    variablesVisualizer.markLast("Перемещение", "max Q");
                    for (int i = 0; i < P.nodes.Count; i++)
                    {
                        if (P.nodes[i].getAttributeValue("variable") == "numerical")
                        {
                            string newMin = "";
                            string newMax = "";
                            if (P.nodes[i].getAttributeValue("max").Contains('.') || P.nodes[i].getAttributeValue("max").Contains(','))
                            {
                                double min_double = double.Parse(P.nodes[i].getAttributeValue("min").Replace('.', ','));
                                double max_double = double.Parse(P.nodes[i].getAttributeValue("max").Replace('.', ','));
                                double value = double.Parse(maxQ.h.nodes[i].getValue().Replace('.', ','));
                                // перемещение центра
                                if ((value - ((max_double - min_double) / 2)) > 0)
                                {
                                    newMin = (value - ((max_double - min_double) / 2)).ToString().Replace(',', '.');
                                }
                                else
                                {
                                    newMin = min_double.ToString().Replace(',', '.');
                                }
                                newMax = (value + ((max_double - min_double) / 2)).ToString().Replace(',', '.');
                            }
                            else
                            {
                                int min_int = int.Parse(P.nodes[i].getAttributeValue("min"));
                                int max_int = int.Parse(P.nodes[i].getAttributeValue("max"));
                                int value = int.Parse(maxQ.h.nodes[i].getValue());
                                // перемещение центра
                                if (value - ((max_int - min_int) / 2) >= 2)
                                {
                                    newMin = (value - ((max_int - min_int) / 2)).ToString();
                                }
                                else
                                {
                                    newMin = min_int.ToString();
                                }
                                newMax = (value + ((max_int - min_int) / 2)).ToString();
                            }
                            P.nodes[i].setAttribute("min", newMin);
                            P.nodes[i].setAttribute("max", newMax);
                        }
                    }
                }
                lastP = maxQ;

                variablesVisualizer.refresh();
            }
        }
        int poolLoading = 0;
        public void hyperCubeRecursiveCalculation(int variableNodeId, string value)
        {
            if (form1.multiThreadTrainingRATE != threads && form1.multiThreadTrainingRATE != 0)
            {
                threads = form1.multiThreadTrainingRATE;
                initPool();
            }
            if (threads > Math.Pow(3.0, Convert.ToDouble(variablesCount)))
            {
                threads = Convert.ToInt32(Math.Pow(3.0, Convert.ToDouble(variablesCount)));
                initPool();
            }
            P.nodes[variableNodeId].setValue(value);

            

            for (int i = variableNodeId + 1; i < P.nodes.Count; i++)
            {
                if (P.nodes[i].getAttributeValue("variable") == "numerical")
                {
                    string min = P.nodes[i].getAttributeValue("min");
                    string max = P.nodes[i].getAttributeValue("max");

                    hyperCubeRecursiveCalculation(i, min);
                    hyperCubeRecursiveCalculation(i, max);
                    hyperCubeRecursiveCalculation(i, getVariableCenter(P.nodes[i]).Replace(',', '.'));

                    return;
                }
            }
            
            double Q = searchInExplored(P);
            if (Q == -1)
            {
                calculateTargetFunction(P);
            }
            else
            {// по идее среди этих точек не может быть максимума
                currentHypercube.Add(new TargetFunctionPoint(P.Clone(), Q));
            }
        }
        void iteration()
        {
            int firstVariableNodeId = -1;

            for (int i = 0; i < P.nodes.Count; i++)
            {
                if (P.nodes[i].getAttributeValue("variable") == "numerical")
                {
                    string min = P.nodes[i].getAttributeValue("min");
                    string max = P.nodes[i].getAttributeValue("max");

                    if (firstVariableNodeId == -1) firstVariableNodeId = i;

                    P.nodes[i].setValue(getVariableCenter(P.nodes[i]).Replace(',', '.'));
                }

            }

            string first_min = P.nodes[firstVariableNodeId].getAttributeValue("min");
            string first_max = P.nodes[firstVariableNodeId].getAttributeValue("max");
            string first_center = P.nodes[firstVariableNodeId].getAttributeValue("value");
            hyperCubeRecursiveCalculation(firstVariableNodeId, first_min);
            hyperCubeRecursiveCalculation(firstVariableNodeId, first_max);
            hyperCubeRecursiveCalculation(firstVariableNodeId, first_center);
        }
        void calculateTargetFunction(Hyperparameters h)
        {

            if (poolLoading < pool.Length)
            {
                string reportLine = "";
                for (int i = 0; i < h.nodes.Count; i++)
                {
                    if (h.nodes[i].getAttributeValue("variable") == "numerical")
                    {
                        pool[poolLoading].nodes[i] = h.nodes[i].Clone();
                        reportLine += h.nodes[i].getAttributeValue("value").Replace(',', '.') + ";";
                    }
                }
                form1.log(reportLine);

                poolLoading++;
                if(poolLoading == pool.Length)
                {
                    if (form1.test_count != 0)
                        test_count = form1.test_count;
                    double[,] target_functions = new double[pool.Length, test_count];

                    for (int tc = 0; tc < test_count; tc++)
                    {
                        var now2 = new DateTimeOffset(DateTime.Now);
                        var start2 = now2.ToUnixTimeSeconds();

                        List<Algorithm> algorithms = new List<Algorithm>();
                        for (int i = 0; i < pool.Length; i++)
                        {
                            Algorithm alg = Algorithm.newInstance(algorithm);
                            alg.h = pool[i].Clone();
                            algorithms.Add(alg);
                        }

                        List<Thread> trainThreads = new List<Thread>();

                        foreach (Algorithm alg in algorithms)
                        {
                            Thread t = new Thread(new ThreadStart(alg.train));
                            trainThreads.Add(t);
                            t.Start();
                        }

                        foreach (var t in trainThreads)
                            t.Join();


                        for (int i = 0; i < pool.Length; i++)
                        {
                            pool[i] = algorithms[i].h.Clone();

                            switch (target_function_type)
                            {
                                case TargetFunctionType.ACCURACY:
                                    {
                                        target_functions[i, tc] = Convert.ToDouble(pool[i].getValueByName("accuracy").Replace('.', ','));
                                        break;
                                    }
                                case TargetFunctionType.STDDEV:
                                    {
                                        target_functions[i, tc] = Convert.ToDouble(pool[i].getValueByName("stdDev").Replace('.', ','));
                                        break;
                                    }
                            }

                        }
                        form1.log((tc + 1).ToString() + '/' + test_count.ToString() + " test comlete" + TimeSpan.FromSeconds((new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()) - start2).ToString(), Color.LimeGreen);
                    }

                    string[] lines = new string[pool.Length];
                    for (int i = 0; i < pool.Length; i++)
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
                        pool[i].setValueByName("target_function", (AVG - StdDev).ToString().Replace(',', '.'));
                        pool[i].setValueByName("target_function_AVG", (AVG).ToString().Replace(',', '.'));
                        pool[i].setValueByName("target_function_StdDev", (StdDev).ToString().Replace(',', '.'));

                        double Q = double.Parse(pool[i].getValueByName("target_function").Replace('.', ','));

                        TargetFunctionPoint newPoint = new TargetFunctionPoint(pool[i].Clone(), Q);
                        currentHypercube.Add(newPoint);
                        explored.Add(newPoint);

                        form1.log(newPoint.Q.ToString(), Color.LimeGreen);

                        lines[i] = getReportLine(newPoint);
                    }

                    File.AppendAllLines(report_file_name, lines);
                    
                    poolLoading = 0;
                }
            }  
        }


        void initPool()
        {
            pool = new Hyperparameters[threads];
            for (int i = 0; i < pool.Length; i++)
                pool[i] = P.Clone();

            string name = algorithm.name;

            for (int i = 0; i < pool.Length; i++)
            {
                pool[i].setValueByName("code", i.ToString());
                pool[i].setValueByName("model_name", pool[i].nodes[0].name() + "[" + i.ToString() + "]");
                string new_save_folder = form1.pathPrefix + "MyAlgorithmOptimization\\" + name + "\\" + name + "[" + i.ToString() + "]" + "\\";
                Algorithm.CopyFiles(pool[i], algorithm.h.getValueByName("save_folder"), new_save_folder);
            }
        }

        double searchInExplored(Hyperparameters h)
        {
            for (int tfp = 0; tfp < explored.Count; tfp++)
            {
                bool coincidence = true;
                for (int i = 0; i < h.nodes.Count; i++)
                {
                    if (h.nodes[i].getAttributeValue("variable") == "numerical")
                    {
                        var a = explored[tfp].h.nodes[i].getValue();
                        var b = h.nodes[i].getValue();
                        if (explored[tfp].h.nodes[i].getValue() != h.nodes[i].getValue())
                        {
                            coincidence = false;
                            break;
                        }
                    }
                }
                if (coincidence) return explored[tfp].Q;

            }
            return -1;
        }
        string getVariableCenter(Node variable)
        {
            string center = "";
            if (variable.getAttributeValue("max").Contains('.') || variable.getAttributeValue("max").Contains(','))
            {
                double min_double = double.Parse(variable.getAttributeValue("min").Replace('.', ','));
                double max_double = double.Parse(variable.getAttributeValue("max").Replace('.', ','));
                center = ((max_double - min_double) / 2).ToString();
            }
            else
            {
                int min_int = int.Parse(variable.getAttributeValue("min"));
                int max_int = int.Parse(variable.getAttributeValue("max"));
                center = (min_int + ((max_int - min_int) / 2)).ToString();
            }
            return center;
        }

        string getReportLine(TargetFunctionPoint P)
        {
            if (P == null)
                return "";
            string reportLine = "";
            for (int n = 0; n < P.h.nodes.Count; n++)
            {
                if (P.h.nodes[n].getAttributeValue("variable") == "numerical")
                {
                    reportLine += P.h.nodes[n].getValue().Replace(',', '.') + ";";
                }
            }
            reportLine += P.Q.ToString().Replace(',', '.') + ';';

            return reportLine;
        }
    }
    public class TargetFunctionPoint
    {
        public Hyperparameters h;
        public double Q;

        public TargetFunctionPoint(Hyperparameters h)
        {
            this.h = h;
        }
        public TargetFunctionPoint(Hyperparameters h, double Q)
        {
            this.h = h;
            this.Q = Q;
        }

        public void setQ(string Q)
        {
            this.Q = double.Parse(Q);
        }
    }
}
