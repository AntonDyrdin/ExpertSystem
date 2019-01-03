using System;
using System.IO;
namespace Экспертная_система
{
    public abstract class Algorithm
    {
        public Form1 form1;
        public string lastPrediction;
        public Hyperparameters h;

        public string pathPrefix = "";
        public Algorithm(Form1 form1, string name, int windowSize)
        {
            h = new Hyperparameters(form1);

            /////////чтене файла конфигурации///////////////////////
            var configLines = File.ReadAllLines("config.txt");
            foreach (string line in configLines)
            {
                h.add(line);
            }
            ////////////////////////////////////////////////////////
            this.form1 = form1;

            h.add("name", name);
            h.add("windowSize", windowSize);
            pathPrefix = h.getValueByName("pathPrefix");
        }

        //█==========================================█
        //█            get_prediction                █
        //█==========================================█
        //возвращает прогноз для одного окна
        //inputVector - матрица входных данных, в которой нулевой столбец [i,0] - прогнозируемая величина, а остальные столбцы - предикторы.
        //каждая строка - значения предикторов в j-ый временной интервал
        public string get_prediction(double[,] inputVector)
        {
            // lastPrediction=f(inputVector)

            return "ошибка: метод не реализован";
        }
        //█===========================================█
        //█            train_the_model                █
        //█===========================================█
        public string train_the_model(double[,,] inputVector)
        {
            return "обучение алгоритма " + h.getValueByName("name") + " - ошибка: метод не реализован";
        }



        public string getValueByName(string name)
        { return h.getValueByName(name); }
        public void setAttributeByName(string name, int value)
        { h.setAttributeByName(name, value); }
        public void setAttributeByName(string name, string value)
        { h.setAttributeByName(name, value); }
        public void variate(string name)
        { h.variate(name); }
        public void log(String s, System.Drawing.Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, new System.Diagnostics.StackTrace().ToString() + s, col);
        }
    }
}
