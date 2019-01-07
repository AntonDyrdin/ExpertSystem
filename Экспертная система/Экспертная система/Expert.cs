using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace Экспертная_система
{
    [Serializable]
    public class Expert
    {
        private string pathPrefix = "";
        public double[,,] inputVector;
        public double[,] dataset;
        public double[,] normalizedDataset;
        //критерий оптимальности
        public double target_function;

        //список алгоритмов(комитет)
        public List<Algorithm> algorithms;

        public Expert(Form1 form1)
        {
            //чтене файла конфигурации
            var configLines = File.ReadAllLines("config.txt");
            foreach (string line in configLines)
            {
                if (line.Contains("pathPrefix="))
                {
                    pathPrefix = line.Split('=')[1];
                }
            }
            this.form1 = form1;
            algorithms = new List<Algorithm>();
        }
        //█====================================================█
        //█            Обучить все алгоритмы                   █
        //█====================================================█
        public string trainAllAlgorithms(string inputFile, int closeColumnIndex)
        {
            for (int i = 0; i < algorithms.Count; i++)
                log(algorithms[i].train(inputFile));

            return "";
        }
        //получить прогноз для одного окна  
        //inputVector - матрица входных данных, в которой нулевой столбец [i,0] - прогнозируемая величина, а остальные столбцы - предикторы.
        //каждая строка - значения предикторов в j-ый временной интервал
        //█====================================================█
        //█            Получить прогноз                        █
        //█====================================================█
        public double getPrediction(double[,] inputVector)
        {
            double sum = 0;
            //вызов getPrediction у каждого алгоритма прогнозирования 
            foreach (Algorithm algorithm in algorithms)
            {
                string prediction = algorithm.getPrediction(inputVector);
                if (!prediction.Contains("ошибка"))
                    sum = sum + Convert.ToDouble(prediction);
                else
                    log("метод getPrediction() вернул ошибку [" + prediction + "] при вызове с входным вектором" + inputVector.ToString(), System.Drawing.Color.Red);
            }
            //нахождение среднего арифметичкеского ответов комитета

            //возврат полуечнного значения

            return -1;
        }

        //█====================================================█
        //█   узнать решение системы принятия решений          █
        //█====================================================█
        //узнать решение системы принятия решений для одного окна
        //возвращает количество единиц котируемой валюты, которое нужно купить или продать (если возвращено значение меньше нуля)
        public double getDecision()
        {
            //Среднее арифметическое прогнозов алгоритмов
            //!!!!!!!!!!!    ТРЕБУЕТ ДАЛЬНЕЙШЕЙ ОПТИМИЗАЦИИ !!!!!!!!!!!
            double decision = 0;
            double sum = 0;
            foreach (Algorithm algorithm in algorithms)
            { sum += Convert.ToDouble(algorithm.lastPrediction); }
            decision = sum / algorithms.Count;
            return decision;
        }

        public void Add_Algorithm(Algorithm algorithm)
        {
            algorithms.Add(algorithm);
        }

        public Expert DeepClone()
        {
            using (MemoryStream memory_stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory_stream, this);

                memory_stream.Position = 0;
                return (Expert)formatter.Deserialize(memory_stream);
            }
        }
        public void prepareDataset(string inputFile, string dropColumn)
        {
            int[] colDropInd;
            var allLines = File.ReadAllLines(inputFile);
            //пропуск пустых строк
            List<string> filledLines = new List<string>();
            foreach (string line in allLines)
            {
                if (line != "")
                { filledLines.Add(line); }
            }
            allLines = new string[filledLines.Count];
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i] = filledLines[i];
            }

            for (int a = 0; a < algorithms.Count; a++)
            {

                int windowSie = Convert.ToInt16(algorithms[a].getValueByName("windowSize"));
                string[] featuresNames = allLines[0].Split(';');
                int inc = 0;
                var dropColumnNames = dropColumn.Split(';');
                //drop column
                for (int c = 0; c < featuresNames.Length; c++)
                {
                    foreach (string colName in dropColumnNames)
                    {
                        if (featuresNames[c] == colName)
                        {
                            inc++;
                        }
                    }

                }
                colDropInd = new int[inc];
                inc = 0;
                for (int c = 0; c < featuresNames.Length; c++)
                {
                    for (int cind = 0; cind < dropColumnNames.Length; cind++)
                    {
                        if (featuresNames[c] == dropColumnNames[cind])
                        {
                            colDropInd[inc] = c;
                        }
                    }
                    inc++;
                }

                dataset = new double[allLines.Length - 1, featuresNames.Length - colDropInd.Length];
                //формирование матрицы dataset
                for (int i = 1; i < allLines.Length; i++)
                {
                    string[] features = allLines[i].Split(';');

                    int shift = 0;
                    for (int k = 0; k < features.Length; k++)
                    {
                        bool dropIt = false;
                        for (int c = 0; c < colDropInd.Length; c++)
                        {
                            if (colDropInd[c] == k)
                            {
                                dropIt = true;
                                shift++;
                            }
                        }

                        if (!dropIt)
                            try
                            {
                                dataset[i - 1, k - shift] = Convert.ToDouble(features[k]);
                            }
                            catch
                            {
                                try
                                {
                                    dataset[i - 1, k - shift] = Convert.ToDouble(features[k]);
                                }
                                catch
                                {

                                    dataset[i - 1, k - shift] = 0;
                                }
                            }
                    }
                }
                //первая строка датасета удаляется из-за нормализации 
                normalizedDataset = new double[dataset.GetLength(0) - 1, dataset.GetLength(1)];
                double[] previousLine = new double[dataset.GetLength(1)];
                for (int j = 0; j < dataset.GetLength(1); j++)
                    previousLine[j] = dataset[0, j];


                //первая производная
                for (int i = 0; i < dataset.GetLength(0) - 1; i++)
                {
                    for (int k = 0; k < dataset.GetLength(1); k++)
                    {
                        normalizedDataset[i, k] = dataset[i + 1, k] / previousLine[k];
                    }
                    for (int j = 0; j < dataset.GetLength(1); j++)
                        previousLine[j] = dataset[i + 1, j];
                }

                /*   double max = double.MinValue;
                   double min = double.MaxValue; 
                   for (int i = 0; i < dataset.GetLength(0) - 1; i++)
                   {
                       for (int k = 0; k < dataset.GetLength(1); k++)
                       {
                           if (normalizedDataset[i, k] > max)
                           { max = normalizedDataset[i , k]; }
                           if (normalizedDataset[i , k] < min)
                           { min = normalizedDataset[i , k]; }
                       }
                   }*/

                //масштабирование к [0;1]
                for (int i = 0; i < dataset.GetLength(0) - 1; i++)
                {
                    for (int k = 0; k < dataset.GetLength(1); k++)
                    {
                        if (normalizedDataset[i, k] < 2)
                            normalizedDataset[i, k] = normalizedDataset[i, k] / 2;
                        else
                            normalizedDataset[i, k] = 1;
                    }
                }

                inputVector = new double[allLines.Length - windowSie, windowSie, allLines[1].Split(',').Length];
                //формирование входного вектора начинается с позиции последней строки первого окна
                //c учётом того, что первая строка - шапка таблицы!!!!!!!!!!!!!!!!!!!!!

                //запись в БД
                // int inputVectorID = Algorithms[a].h.add("name:inputVector,count:" + (allLines.Length - windowSie).ToString());

                for (int i = windowSie; i < allLines.Length; i++)
                {   //запись в БД
                    //  int windowID = Algorithms[a].h.addByParentId(inputVectorID, "name:" + (i - windowSie + 1).ToString() + "stWindow,count:"+ windowSie);

                    //затем от этой позиции заполняется первое окно
                    for (int j = 0; j < windowSie; j++)
                    {     //запись в БД
                          // int lineID = Algorithms[a].h.addByParentId(windowID, "name:" + (j + 1).ToString() + "stLine,count:" + allLines[1].Split(',').Length );
                        string[] features = allLines[i - windowSie + 1 + j].Split(';');
                        for (int k = 0; k < features.Length; k++)
                        {   //запись в БД
                            // Algorithms[a].h.addLeafByParentId(lineID, featuresNames[k] + ":" + features[k]);
                            try
                            {
                                inputVector[i - windowSie, j, k] = Convert.ToDouble(features[k]);
                            }
                            catch
                            {
                                try
                                {
                                    inputVector[i - windowSie, j, k] = Convert.ToDouble(features[k].Replace('.', ','));
                                }
                                catch
                                {

                                    inputVector[i - windowSie, j, k] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void normalizeInputVector(double[,] dataset)
        {
            for (int i = 0; i < dataset.GetLength(0); i++)
            {
                for (int j = 0; j < dataset.GetLength(1); j++)
                {

                }
            }
        }
        //inputVector - матрица входных данных, в которой нулевой столбец [i,0] - прогнозируемая величина, а остальные столбцы - предикторы.
        [NonSerializedAttribute]
        private Form1 form1;

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
