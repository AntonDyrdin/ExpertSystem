using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace Экспертная_система
{
    public class PopulationVisualizer
    {
        PictureBox picBox;
        Graphics g;
        Bitmap bitmap;
        Form1 form1;
        double elite_ratio;
        public PopulationVisualizer(PictureBox target_pictureBox, Form1 form1, double elite_ratio)
        {
            this.elite_ratio = elite_ratio;
            this.form1 = form1;
            picBox = target_pictureBox;
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
        }
      /*  public void draw(Algorithm obj, int x, int y, int Width, int Height)
        {
            int inc = 0;

            List<Parameter> hyperparameters = obj.get_Hyperparameters().nodes;
            var Count = 0;
            foreach (Parameter node in hyperparameters)
            {
                if (!node.is_const)
                { Count++; }
            }

            int Hinc = Height / (Count + 2) - 3;
            int depth = Hinc - 2;
            if (depth > 14)
                depth = 14;
            int gap = 2;
            foreach (Parameter node in hyperparameters)
            {
                if (!node.is_const)
                {
                    if (!node.is_categorical)
                    {
                        drawString(node.name + " = ", Brushes.Gray, depth, x, y + 5 + inc * Hinc + inc * 2);
                        drawString(node.value.ToString(), depth, x + Math.Pow(node.name.Length + 2, 0.92) * depth, y + 5 + inc * Hinc + inc * gap);
                    }
                    else
                    {
                        if ((node.category.Length + node.name.Length) * depth < Width)
                        {
                            drawString(node.name + " = ", Brushes.Gray, depth, x, y + 5 + inc * Hinc + inc * 2);
                            drawString(node.category.ToString(), depth, x + Math.Pow(node.name.Length + 2, 0.92) * depth, y + 5 + inc * Hinc + inc * gap);
                        }
                        else
                        {
                            drawString(node.name + " = ", Brushes.Gray, Width / (node.category.Length + node.name.Length - 3) * 1.1, x, y + 5 + inc * Hinc + inc * gap);
                            drawString(node.category.ToString(), Width / (node.category.Length + node.name.Length - 3) * 1.1, x + node.name.Length * Width / (node.category.Length + node.name.Length - 3) * 1.1, y + 5 + inc * Hinc + inc * gap);
                        }
                    }
                    if (node.is_change)
                    {
                        if (!node.is_categorical)
                        {
                            if (node.is_change_up_or_down)
                            { drawString("⯅", Brushes.Green, depth, x + Width - depth, y + 5 + inc * Hinc + inc * gap); }
                            else
                            { drawString("⯆", Brushes.Red, depth, x + Width - depth, y + 5 + inc * Hinc + inc * gap); }
                        }
                        else
                        {
                            drawString("⬤", Brushes.YellowGreen, depth, x + Width - depth, y + 5 + inc * Hinc + inc * gap);
                        }
                    }
                    inc++;
                }
            }
            drawString("degree_of_trust = " + obj.get_degree_of_trust().ToString(), Brushes.Cyan, depth, x, y + 5 + inc * Hinc + inc * 2);

        }
        public void draw(Expert obj, int x, int y, int Width, int Height)
        {
            double del = 0;
            int linesCount = 1;

            for (int i = 1; i < 10; i++)
            {
                if (Math.Abs(1 - del) > Math.Abs(1 - (Convert.ToDouble(obj.prediction_Algorithms.Count) * Convert.ToDouble(Height)) / (Convert.ToDouble(Width) * i * i)))
                {
                    linesCount = i;
                }
                del = Math.Abs((Convert.ToDouble(obj.prediction_Algorithms.Count) * Convert.ToDouble(Height)) / (Convert.ToDouble(Width) * i * i));
            }
            int XperIndivid = Width / (obj.prediction_Algorithms.Count / linesCount);
            int YperIndivid = Height / linesCount;
            int inc = 0;
            for (int i = 0; i < obj.prediction_Algorithms.Count / linesCount; i++)
            {
                for (int j = 0; j < linesCount; j++)
                {
                    //  drawLine(Color.Red, 1, i * XperIndivid, j * YperIndivid, (i + 1) * XperIndivid, j * YperIndivid);
                    //  drawLine(Color.Red, 1, i * XperIndivid, j * YperIndivid, i * XperIndivid, (j + 1) * YperIndivid);

                    draw(obj.prediction_Algorithms[inc], x + i * XperIndivid, y + j * YperIndivid, XperIndivid, YperIndivid);
                    inc++;
                }
            }
            drawString( obj.target_function.ToString(), Brushes.Purple, Height / 20, x, y + Height - (Height / 20 * 1.8));

        }
        public void draw(Expert[] obj)
        {
            double del = 0;
            int linesCount = 1;

            for (int i = 1; i < 10; i++)
            {
                if (Math.Abs(1 - del) > Math.Abs(1 - (Convert.ToDouble(obj.Length) * Convert.ToDouble(picBox.Height)) / (Convert.ToDouble(picBox.Width) * i * i)))
                {
                    linesCount = i;
                }
                del = Math.Abs((Convert.ToDouble(obj.Length) * (Convert.ToDouble(picBox.Height))) / (Convert.ToDouble(picBox.Width) * i * i));
            }
            int XperIndivid = picBox.Width / (obj.Length / linesCount);
            int YperIndivid = picBox.Height / linesCount;
            int inc = 0;
            for (int i = 0; i < obj.Length / linesCount; i++)
            {
                for (int j = 0; j < linesCount; j++)
                {
                    drawLine(Color.Red, 1, i * XperIndivid, j * YperIndivid, (i + 1) * XperIndivid, j * YperIndivid);
                    drawLine(Color.Red, 1, i * XperIndivid, j * YperIndivid, i * XperIndivid, (j + 1) * YperIndivid);
                    if (inc < Convert.ToInt16((Math.Round(obj.Length * elite_ratio))))
                    {
                        drawString("[" + inc.ToString() + "]", Brushes.Green, 10, (i + 1) * XperIndivid - 25, (j + 1) * YperIndivid - 20);
                    }
                    else
                    {
                        drawString("[" + inc.ToString() + "]", Brushes.Red, 10, (i + 1) * XperIndivid - 25, (j + 1) * YperIndivid - 20);
                    }

                    draw(obj[inc], i * XperIndivid, j * YperIndivid, XperIndivid, YperIndivid);
                    inc++;
                }
            }
            refresh();
        }*/
        public void refresh()
        {
            picBox.Image = bitmap;
            form1.voidDelegate = new Form1.VoidDelegate(form1.Refresh);
            form1.logBox.Invoke(form1.voidDelegate);
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
        }
        public void drawString(string s, double depth, double x, double y)
        {
            g.DrawString(s, new Font("Consolas", Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
        }
        /////////////////////////////////Brushes.[Color]
        public void drawString(string s, Brush brush, double depth, double x, double y)
        {
            g.DrawString(s, new Font("Consolas", Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
        }

        public void drawLine(Color col, double depth, double x1, double y1, double x2, double y2)
        {
            g.DrawLine(new Pen(col, Convert.ToInt16(depth)), Convert.ToInt16(Math.Round(x1)), Convert.ToInt16(Math.Round(y1)), Convert.ToInt16(Math.Round(x2)), Convert.ToInt16(Math.Round(y2)));
        }

    }
}
