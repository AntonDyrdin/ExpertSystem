using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
namespace Экспертная_система
{
    class ImgDataset
    {
        string[] csvLines;
        int Y = 0;

        Bitmap bmp;
        ImgDataset(string pathToImageFile, bool toInvertPixelsBrightness, Form1 form1)
        {

            this.form1 = form1;

            bmp = new Bitmap(Image.FromFile(pathToImageFile));

            csvLines = new string[bmp.Width];
            string head = "";

            for (int j = 0; j < bmp.Height; j++)
            {
                head = head + '<' + j.ToString() + '>' + ';';
            }
            csvLines[0] = head;

            for (int i = 0; i < bmp.Width; i++)
            {
                string s = "";

                if (!toInvertPixelsBrightness)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        s = s + (bmp.GetPixel(i, j).B) + ';';
                    }
                else
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        s = s + (255 - bmp.GetPixel(i, j).B) + ';';
                    }

                csvLines[i + 1] = s;
            }
        }

        void Save(string path)
        {
            System.IO.File.WriteAllLines(path, csvLines);
        }

        ImgDataset(string pathToDatasetFile, Form1 form1)
        {
            csvLines = System.IO.File.ReadAllLines(pathToDatasetFile);
        }


        ///////////ВИЗУАЛИЗАЦИЯ//////////////////
        public Form1 form1;
        public PictureBox picBox;
        public Graphics g;
        public Bitmap bitmap;

        void drawImgWhithPredictions(string outputCSVFile, string predictionsColumnName,string split_point)
        {


            var allLines = File.ReadAllLines(outputCSVFile);
            int indCol = 0;
            if (predictionsColumnName == "LAST_COLUMN")
                indCol = allLines[0].Split(';').Length - 1;
            else
                for (int i = 0; i < allLines[0].Split(';').Length; i++)
                {
                    var str = allLines[0].Split(';')[i];
                    if (allLines[0].Split(';')[i] == predictionsColumnName)
                    {
                        indCol = i;
                        break;
                    }

                }

            int Xshift = Convert.ToInt16((allLines.Length - 1)*Convert.ToDouble(split_point.Replace('.',',')));
            for (int i = 1; i < allLines.Length; i++)
            {
                string str1 = allLines[i].Split(';')[indCol + 1];
                double point = Convert.ToDouble(allLines[i].Split(';')[indCol + 1].Replace('.', ','));
                                                                                                      bmp.SetPixel(Xshift  +
            }


            g.DrawImage(bmp, 0, 0);
        }

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
            y += Y;
            if (x > picBox.Width)
                picBox.Width = Convert.ToInt16(x);
            else
            if (y > picBox.Height)
                picBox.Height = Convert.ToInt16(y);
            else
                try
                {
                    g.DrawString(s, new Font("Consolas", Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                }
                catch { }
        }
        /////////////////////////////////Brushes.[Color]
        public void drawString(string s, Brush brush, double depth, double x, double y)
        {
            y += Y;
            if (x > picBox.Width)
                picBox.Width = Convert.ToInt16(x);
            else
            if (y > picBox.Height)
                picBox.Height = Convert.ToInt16(y);
            else
                try
                {
                    g.DrawString(s, new Font("Consolas", Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                }
                catch { }
        }

        public void drawLine(Color col, double depth, double x1, double y1, double x2, double y2)
        {
            y1 += Y;
            y2 += Y;
            if (x1 > picBox.Width)
                picBox.Width = Convert.ToInt16(x1);
            else
            if (x2 > picBox.Width)
                picBox.Width = Convert.ToInt16(x2);
            else
            if (y1 > picBox.Height)
                picBox.Height = Convert.ToInt16(y1);
            else
            if (y2 > picBox.Height)
                picBox.Height = Convert.ToInt16(y2);
            else
                g.DrawLine(new Pen(col, Convert.ToInt16(depth)), Convert.ToInt16(Math.Round(x1)), Convert.ToInt16(Math.Round(y1)), Convert.ToInt16(Math.Round(x2)), Convert.ToInt16(Math.Round(y2)));
        }
        private void log(String s, System.Drawing.Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }
    }
}
