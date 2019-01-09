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
        public double[,] normalizedDataset1;
        public double[,] normalizedDataset2;
        public double[,] normalizedDataset3;
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
                { filledLines.Add(line); }
            }
            allLines = new string[filledLines.Count];
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i] = filledLines[i];
            }

            // int windowSie = Convert.ToInt16(algorithms[a].getValueByName("windowSize"));
            string[] featuresNames = allLines[0].Split(';');

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
                string[] features = allLines[i].Split(';');

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
            normalizedDataset1 = new double[dataset.GetLength(0) - 1, dataset.GetLength(1)];
            for (int i = 0; i < dataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    normalizedDataset1[i, k] = 0;
                }
            }
            //тупое масштабирование 
            /*    var subt = new double[dataset.GetLength(1)];
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
                normalizedDataset1 = new double[dataset.GetLength(0), dataset.GetLength(1)];

                for (int i = 0; i < dataset.GetLength(0) - 1; i++)
                {
                    for (int k = 0; k < dataset.GetLength(1); k++)
                    {
                        normalizedDataset1[i, k] = (dataset[i + 1, k] - subt[k]) / div[k];
                    }
                }
                   */
            ////////////////////////////////////////////////
            ///////////   НОРМАЛИЗАЦИЯ i/(i-1)   ///////////
            ////////////////////////////////////////////////
            //первая строка датасета удаляется из-за нормализации типа i/(i-1)
            normalizedDataset2 = new double[dataset.GetLength(0) - 1, dataset.GetLength(1)];

            //заполнение строки для первой итерации алгоритма   ____i/(i-1)__
            double[] previousLine = new double[dataset.GetLength(1)];
            for (int j = 0; j < dataset.GetLength(1); j++)
                previousLine[j] = dataset[0, j];


            //___________i/(i-1)__________________
            for (int i = 0; i < normalizedDataset1.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    if (previousLine[k]!=0)
                    normalizedDataset2[i, k] = dataset[i + 1, k] / previousLine[k];
                                           else
                        normalizedDataset2[i, k]=0;
                }
                for (int j = 0; j < dataset.GetLength(1); j++)
                    previousLine[j] = dataset[i + 1, j];
            }

            /////////////////////////////////
            //////     СГЛАЖИВАНИЕ    ///////
            /////////////////////////////////
            for (int i = 0; i < dataset.GetLength(0) - 1; i++)
            {
                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    //приведение его к 0.5 - среднему делением на 2
                    normalizedDataset2[i, k] = normalizedDataset2[i, k] / 2;
                    //для увеличение стандартного отклонения сначала вычислим имеющееся i-ое отклонение, приведя к 0 - среднему
                    normalizedDataset2[i, k] = normalizedDataset2[i, k] - 0.5;
                    //а затем отмасштабируем
                    normalizedDataset2[i, k] = normalizedDataset2[i, k] * (1 / (Math.Abs(normalizedDataset2[i, k] + 0.03)));
                    //вернём к 0.5 - среднему
                    normalizedDataset2[i, k] = normalizedDataset2[i, k] + 0.5;


                    //и подровняем выбросы
                    if (normalizedDataset2[i, k] > 1)
                    { normalizedDataset2[i, k] = 1; }
                    else if (normalizedDataset2[i, k] < 0)
                    { normalizedDataset2[i, k] = 0; }

                    if (normalizedDataset1[i, k] > 1)
                    { normalizedDataset1[i, k] = 1; }
                    else if (normalizedDataset1[i, k] < 0)
                    { normalizedDataset1[i, k] = 0; }
                }
            }

            string[] toWrite = new string[normalizedDataset2.GetLength(0) + 1];
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



            for (int i = 1; i < normalizedDataset2.GetLength(0); i++)
            {
                for (int k = 0; k < normalizedDataset2.GetLength(1); k++)
                {
                    if ((normalizedDataset2[i, k]).ToString().Replace(',', '.').Length > 4)
                        toWrite[i] += (normalizedDataset2[i, k]).ToString().Replace(',', '.').Substring(0, 4) + ';';
                    else
                        toWrite[i] += (normalizedDataset2[i, k]).ToString().Replace(',', '.') + ';';
                }
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
