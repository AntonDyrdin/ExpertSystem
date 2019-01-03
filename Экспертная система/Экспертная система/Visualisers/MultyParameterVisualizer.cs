using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace Экспертная_система
{
    public class MultyParameterVisualizer
    {
        public List<ParameterVisualizer> parameters;
        public PictureBox pictureBox;
        public Graphics g;
        public Bitmap bitmap;
        public Form1 form1;
        public int hmin;
        public int Ymax;
        public int Xmax;
        double mainFontDepth;
        public MultyParameterVisualizer(PictureBox target_pictureBox, Form1 form1)
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
                if (visualizer.label == name)
                {
                    is_new = false;
                    visualizer.addPoint(value, name);
                }
            }
            if (is_new)
            {
                Random r = new Random();
                //  addParameter(name, Color.FromArgb(255, r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)));
                addParameter(name, Color.FromArgb(255,161,14,233));
                addPoint(value, name);
            }
        }
        public void addParameter(string label, Color color)
        {
            ParameterVisualizer newParameterVisualizer = new ParameterVisualizer(pictureBox, form1, label, color);
            newParameterVisualizer.needToRefresh.Stop();
            newParameterVisualizer.needToRefresh.Close();
            newParameterVisualizer.needToRefresh.Enabled = false;
            newParameterVisualizer.needToRefresh.Dispose();
            parameters.Add(newParameterVisualizer);


            for (int i = 0; i < parameters.Count; i++)
            {
                /*  int minEqualMaxCount = 0;
                    for (int j = 0; j < parameters.Count; j++)
                    {
                        if (parameters[j].maxY == parameters[j].minY)
                            minEqualMaxCount++;
                    }
                    if (parameters[i].maxY == parameters[i].minY && i == 0)
                    {
                        parameters[i].Ymax = hmin;
                        parameters[i].Ymin = 0;
                    }
                    else
                    if (parameters[i].maxY != parameters[i].minY && i == 0)
                    {
                        parameters[i].Ymax = (Ymax - (minEqualMaxCount * hmin)) / (parameters.Count - minEqualMaxCount);
                        parameters[i].Ymin = 0;
                    }
                    else
                    if (parameters[i].maxY != parameters[i].minY)
                    {
                        parameters[i].Ymin = parameters[i - 1].Ymax;
                        parameters[i].Ymax = parameters[i - 1].Ymax + ((Ymax - (minEqualMaxCount * hmin)) / (parameters.Count - minEqualMaxCount));
                    }
                    else
                    {
                        parameters[i].Ymin = parameters[i - 1].Ymax;
                        parameters[i].Ymax = parameters[i - 1].Ymax + hmin;
                    }
                       */
                parameters[i].yDownGap = 10;
                parameters[i].yUpGap = 25;
                parameters[i].Ymin = (Ymax / parameters.Count) * i;
                parameters[i].Ymax = Ymax / parameters.Count;
                parameters[i].multy = true;

            }
            parameters[parameters.Count - 1].multy = false;
        }
        public void refresh()
        {
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
        }

    }
}
