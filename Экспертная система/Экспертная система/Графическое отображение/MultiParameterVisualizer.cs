using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace Экспертная_система
{
    public class MultiParameterVisualizer
    {
        public List<ParameterVisualizer> parameters;
        public PictureBox pictureBox;
        public Graphics g;
        public Bitmap bitmap;
        public Form1 form1;
        public int hmin;
        public int Ymax;
        public int Xmax;
        private double mainFontDepth;
        public bool enableGrid = true;
        public bool enableRefresh = true;
        public bool lightsOn = false;
        public MultiParameterVisualizer(PictureBox target_pictureBox, Form1 form1)
        {
            this.form1 = form1;
            pictureBox = target_pictureBox;
            bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            g = Graphics.FromImage(bitmap);

            parameters = new List<ParameterVisualizer>();

            Xmax = pictureBox.Width;
            Ymax = pictureBox.Height;
            if ((Ymax / 10) < 14)
                mainFontDepth = Ymax / 10;
            else
                mainFontDepth = 14;
            hmin = Convert.ToInt16(mainFontDepth * 3);
        }
        public void addPoint(double value, string name)
        {
            bool is_new = true;
            foreach (ParameterVisualizer visualizer in parameters)
            {
                foreach (Function function in visualizer.functions)
                {
                    if (function.label == name)
                    {
                        is_new = false;
                        visualizer.addPoint(value, name);
                        break;
                    }
                }
                if (visualizer.label == name)
                {
                    is_new = false;
                    visualizer.addPoint(value, name);
                    break;
                }
            }
            if (is_new)
            {
                Random r = new Random();
                //  addParameter(name, Color.FromArgb(255, r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)));
                addParameter(name, Color.FromArgb(255, 161, 14, 233), 300);
                addPoint(value, name);
            }
        }

        public void addParameter(double[,] array, int index, string label, Color color, int H)
        {
            ParameterVisualizer newParameterVisualizer = new ParameterVisualizer(pictureBox, form1, label, color);
            newParameterVisualizer.needToRefresh.Stop();
            newParameterVisualizer.needToRefresh.Close();
            newParameterVisualizer.needToRefresh.Enabled = false;
            newParameterVisualizer.needToRefresh.Dispose();
            newParameterVisualizer.H = H;
            parameters.Add(newParameterVisualizer);

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].yDownGap = 10;
                parameters[i].yUpGap = 35;
                if (i == 0)
                    parameters[i].Ymin = 0;
                else
                {
                    parameters[i].Ymin = parameters[i - 1].Ymin + parameters[i].H;
                }
                if (i == parameters.Count - 1)
                    parameters[i].Ymax = H;
                parameters[i].multy = true;

            }
            parameters[parameters.Count - 1].multy = false;

            for (int i = 0; i < array.GetLength(0); i++)
            {
                addPoint(array[i, index], label);
            }
        }
        public void addParameter(string label, Color color, int H)
        {
            ParameterVisualizer newParameterVisualizer = new ParameterVisualizer(pictureBox, form1, label, color);
            newParameterVisualizer.needToRefresh.Stop();
            newParameterVisualizer.needToRefresh.Close();
            newParameterVisualizer.needToRefresh.Enabled = false;
            newParameterVisualizer.needToRefresh.Dispose();
            newParameterVisualizer.H = H;
            parameters.Add(newParameterVisualizer);


            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].yDownGap = 10;
                parameters[i].yUpGap = 35;
                if (i == 0)
                    parameters[i].Ymin = 0;
                else
                {
                    parameters[i].Ymin = parameters[i - 1].Ymin + parameters[i].H;
                }
                if (i == parameters.Count - 1)
                    parameters[i].Ymax = H;
                parameters[i].multy = true;

            }
            parameters[parameters.Count - 1].multy = false;
        }

        public void addCSV(string file, string name, string columnName, int H)
        {

            var allLines = File.ReadAllLines(file);
            int indCol = 0;
            if (columnName == "LAST_COLUMN")
            { indCol = allLines[0].Split(';').Length - 1; }
            else
            {
                try
                {
                    indCol = Convert.ToInt16(columnName);
                }
                catch
                {
                    for (int i = 0; i < allLines[0].Split(';').Length; i++)
                    {
                        var str = allLines[0].Split(';')[i];
                        if (allLines[0].Split(';')[i] == columnName)
                        {
                            indCol = i;
                            break;
                        }

                    }
                }
            }
            bool is_new = true;
            int parameterInd = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].label == name)
                {
                    is_new = false;
                    parameterInd = i;
                }

            }
            if (is_new)
            {
                addParameter(name, Color.White, H);
                parameters[parameters.Count - 1].functions[0].label = allLines[0].Split(';')[indCol];
                for (int i = 1; i < allLines.Length; i++)
                {
                    addPoint(Convert.ToDouble(allLines[i].Split(';')[indCol ].Replace('.', ',')), allLines[0].Split(';')[indCol]);
                }
            }
            else
            {
                for (int i = 1; i < allLines.Length; i++)
                {
                    var awerawr = allLines[0].Split(';');
                    parameters[parameterInd].addPoint(Convert.ToDouble(allLines[i].Split(';')[indCol ].Replace('.', ',')), allLines[0].Split(';')[indCol]);
                }
            }

        }
        public void addCSV(string file, int columnIndex, int H)
        {
            var allLines = File.ReadAllLines(file);
            int indCol = columnIndex;
            string columnName = allLines[0].Split(';')[indCol];

            addParameter(columnName, Color.White, H);
            for (int i = 1; i < allLines.Length; i++)
            {
                string str1 = allLines[i].Split(';')[indCol + 1];
                addPoint(Convert.ToDouble(allLines[i].Split(';')[indCol + 1].Replace('.', ',')), columnName);
            }
        }
        public void refresh()
        {
            if (!enableGrid)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].enableGrid = false;
                }
            }
            if (lightsOn)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].lightsOn = true;
                }
            }
            else
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].lightsOn = false;
                }
            }
            bool isFirstTime = true;
            drawHyperparametersAgain:
            for (int i = 0; i < parameters.Count; i++)
            {
                bitmap = parameters[i].multyRefresh(bitmap);
            }
            //  pictureBox.Image = null;
            pictureBox.Image = bitmap;
            // form1.voidDelegate = new Form1.VoidDelegate(form1.Refresh);
            //form1.richTextBox1.Invoke(form1.voidDelegate);
            bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            g = Graphics.FromImage(bitmap);
            if (isFirstTime)
            {
                isFirstTime = false;
                goto drawHyperparametersAgain;
            }
        }
        public void clear()
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                parameters.Clear();
            }
        }

    }
}
