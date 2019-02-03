using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace Экспертная_система
{
    [Serializable]
    public class Expert
    {
        public double[,,] inputVector;
        public double[,] dataset;
        public double[,] dataset1;
        public double[,] dataset2;
        public double[,] dataset3;
        //критерий оптимальности
        public double target_function;

        //список алгоритмов(комитет)
        public List<Algorithm> algorithms;

        public Expert(Form1 form1)
        {
            this.form1 = form1;
            algorithms = new List<Algorithm>();
        }
        //█====================================================█
        //█            Обучить все алгоритмы                   █
        //█====================================================█
        public string trainAllAlgorithms()
        {
            for (int i = 0; i < algorithms.Count; i++)
                log(algorithms[i].train());

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

        //метод делающий из временного ряда (*.csv) датасет, пригодный для передачи в train.py скрипт
        //возвращает путь к файлу датасета
        public string prepareDataset(string inputFile, string dropColumn)
        {
            List<int> colDropInd;
            var allLines = File.ReadAllLines(inputFile);
            //пропуск пустых строк
            List<string> filledLines = new List<string>();
            foreach (string line in allLines)
            {
                if (line != "")
                {
                    filledLines.Add(line);
                }
            }
            allLines = new string[filledLines.Count];
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i] = filledLines[i];
            }

            // int windowSie = Convert.ToInt16(algorithms[a].getValueByName("windowSize"));
            string[] featuresNames = allLines[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            //drop column
            var dropColumnNames = dropColumn.Split(';');
            colDropInd = new List<int>();
            for (int c = 0; c < featuresNames.Length; c++)
            {
                for (int cind = 0; cind < dropColumnNames.Length; cind++)
                {
                    if (featuresNames[c] == dropColumnNames[cind])
                    {
                        colDropInd.Add(c);
                    }
                }
            }
            var firstDataRow = allLines[1].Split(';');
            for (int c = 0; c < featuresNames.Length; c++)
            {
                bool dropIt = false;
                for (int d = 0; c < colDropInd.Count; c++)
                {
                    if (colDropInd[d] == c)
                    {
                        dropIt = true;
                    }
                }

                if (!dropIt)
                {
                    //попытка преобразовать предиктор в тип double
                    try
                    {
                        var someDouble = Convert.ToDouble(firstDataRow[c]);
                    }
                    //в случае ошибки - весь столбец дропается
                    catch
                    {
                        log("столбец " + featuresNames[c] + " удалён");
                        colDropInd.Add(c);
                    }
                }
            }


            dataset = new double[allLines.Length - 1, featuresNames.Length - colDropInd.Count];
            //формирование матрицы dataset
            for (int i = 1; i < allLines.Length; i++)
            {
                string[] features = allLines[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                int shift = 0;
                for (int k = 0; k < features.Length; k++)
                {
                    bool dropIt = false;
                    for (int c = 0; c < colDropInd.Count; c++)
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


            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            dataset1 = normalize2(dataset);



            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            dataset2 = levelOff2(dataset1);


            string[] toWrite = new string[dataset2.GetLength(0) + 1];
            for (int k = 0; k < featuresNames.Length; k++)
            {
                bool dropIt = false;
                for (int c = 0; c < colDropInd.Count; c++)
                {
                    if (colDropInd[c] == k)
                    {
                        dropIt = true;
                    }
                }

                if (!dropIt)
                    toWrite[0] += featuresNames[k] + ';';
            }
            toWrite[0] = toWrite[0].Remove(toWrite[0].Length - 1, 1);


            for (int i = 0; i < dataset2.GetLength(0); i++)
            {
                for (int k = 0; k < dataset2.GetLength(1); k++)
                {
                    if ((dataset2[i, k]).ToString().Replace(',', '.').Length > 8)
                        toWrite[i + 1] += (dataset2[i, k]).ToString().Replace(',', '.').Substring(0, 8) + ';';
                    else
                        toWrite[i + 1] += (dataset2[i, k]).ToString().Replace(',', '.') + ';';
                }
                toWrite[i + 1] = toWrite[i + 1].Remove(toWrite[i + 1].Length - 1, 1);
            }


            File.WriteAllLines(inputFile.Replace(".txt", "-dataset.txt"), toWrite);
            return inputFile.Replace(".txt", "-dataset.txt");
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

        public Hyperparameters h()
        {
            return algorithms[0].h;
        }


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
        double[,] normalize1(double[,] inputDataset)
        {

            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            //первая строка датасета удаляется из-за нормализации типа i/(i-1)
            double[,] normalizedDataset2 = new double[inputDataset.GetLength(0) - 1, inputDataset.GetLength(1)];

            //заполнение строки previousLine для первой итерации алгоритма   ____i/(i-1)__
            double[] previousLine = new double[inputDataset.GetLength(1)];
            for (int j = 0; j < inputDataset.GetLength(1); j++)
                previousLine[j] = inputDataset[0, j];


            //___________i/(i-1)__________________
            for (int i = 0; i < inputDataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    if (previousLine[k] != 0)
                        normalizedDataset2[i, k] = Convert.ToDouble(inputDataset[i + 1, k]) / Convert.ToDouble(previousLine[k]);
                    else
                        normalizedDataset2[i, k] = 0;
                }
                for (int j = 0; j < inputDataset.GetLength(1); j++)
                    previousLine[j] = inputDataset[i + 1, j];
            }
            return normalizedDataset2;
        }
        double[,] normalize2(double[,] inputDataset)
        {

            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            //первая строка датасета удаляется из-за нормализации типа i/(i-1)
            double[,] normalizedDataset2 = new double[inputDataset.GetLength(0) - 1, inputDataset.GetLength(1)];

            //заполнение строки previousLine для первой итерации алгоритма   ____i/(i-1)__
            double[] previousLine = new double[inputDataset.GetLength(1)];
            for (int j = 0; j < inputDataset.GetLength(1); j++)
                previousLine[j] = inputDataset[0, j];


            //___________i-(i-1)__________________
            for (int i = 0; i < inputDataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    if (previousLine[k] != 0)
                        normalizedDataset2[i, k] = Convert.ToDouble(inputDataset[i + 1, k]) - Convert.ToDouble(previousLine[k]);
                    else
                        normalizedDataset2[i, k] = 0;
                }
                for (int j = 0; j < inputDataset.GetLength(1); j++)
                    previousLine[j] = inputDataset[i + 1, j];
            }
            return normalizedDataset2;
        }
        double[,] levelOff1(double[,] inputDataset)
        {
            double[,] levelOffDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            for (int i = 0; i < inputDataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    //приведение его к 0.5 - среднему делением на 2
                    levelOffDataset[i, k] = levelOffDataset[i, k] / 2;
                    //для увеличение стандартного отклонения сначала вычислим имеющееся i-ое отклонение, приведя к 0 - среднему
                    levelOffDataset[i, k] = levelOffDataset[i, k] - 0.5;
                    //а затем отмасштабируем
                    //levelOffDataset[i, k] = levelOffDataset[i, k] * (1 / (Math.Abs(levelOffDataset[i, k] + 0.5)));
                    //вернём к 0.5 - среднему
                    levelOffDataset[i, k] = levelOffDataset[i, k] + 0.5;
                    //и подровняем выбросы
                    if (levelOffDataset[i, k] > 1)
                    {
                        levelOffDataset[i, k] = 1;
                    }
                    else if (levelOffDataset[i, k] < 0)
                    {
                        levelOffDataset[i, k] = 0;
                    }
                }
            }
            return levelOffDataset;
        }
        double[,] levelOff2(double[,] inputDataset)
        {
            double[,] levelOffDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            for (int i = 0; i < inputDataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < inputDataset.GetLength(1); k++)
                {
                    //масштабирование
                    //  levelOffDataset[i, k] = levelOffDataset[i, k] * (1 / (Math.Abs(levelOffDataset[i, k] + 0.5)));
                    //0.5 - среднее
                    levelOffDataset[i, k] = inputDataset[i, k] + 0.5;
                    //выбросы
                    if (levelOffDataset[i, k] > 1)
                    {
                        levelOffDataset[i, k] = 1;
                    }
                    else if (levelOffDataset[i, k] < 0)
                    {
                        levelOffDataset[i, k] = 0;
                    }
                }
            }
            return levelOffDataset;
        }
        double[,] scale(double[,] inputDataset)
        {
            //масштабирование 

            double[,] scaleedDataset = new double[inputDataset.GetLength(0), inputDataset.GetLength(1)];
            var subt = new double[dataset.GetLength(1)];
            for (int j = 0; j < dataset.GetLength(1); j++)
                subt[j] = dataset[0, j];
            for (int i = 0; i < dataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    if (subt[k] > dataset[i + 1, k])
                    { subt[k] = dataset[i + 1, k]; }
                }
            }
            var div = new double[dataset.GetLength(1)];
            for (int j = 0; j < dataset.GetLength(1); j++)
                div[j] = dataset[0, j];
            for (int i = 0; i < dataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    if (div[k] < (dataset[i + 1, k] - subt[k]))
                    { div[k] = (dataset[i + 1, k] - subt[k]); }
                }
            }

            for (int i = 0; i < dataset.GetLength(0); i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    scaleedDataset[i, k] = (dataset[i, k] - subt[k]) / div[k];
                }
            }
            return scaleedDataset;
        }
    }
}
/*
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
         }   */
