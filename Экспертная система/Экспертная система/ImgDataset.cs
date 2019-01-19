using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace Экспертная_система
{
    internal class ImgDataset
    {
        public string[] csvLines;
        public int Y = 0;
        public Bitmap bmp;

        public ImgDataset(string pathToImageFile, bool toInvertPixelsBrightness, Form1 form1)
        {
           
            this.form1 = form1;

            bmp = new Bitmap(Image.FromFile(pathToImageFile));

            csvLines = new string[bmp.Width+1];
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
            refresh();
        }

        public void Save(string path)
        {
            System.IO.File.WriteAllLines(path, csvLines);
        }

        public ImgDataset(string pathToDatasetFile, Form1 form1)
        {
            this.form1 = form1;

            csvLines = System.IO.File.ReadAllLines(pathToDatasetFile);
            bmp = new Bitmap(csvLines.Length , csvLines[0].Split(';').Length+1);
            for (int i = 1; i < csvLines.Length; i++)
            {
                var features = csvLines[i].Split(';');
                for (int j = 0; j < features.Length; j++)
                {
                    int AbsVal = Convert.ToInt16(Convert.ToDouble(features[j].Replace('.', ',')) * 255);

                    if (AbsVal > 255)
                        AbsVal = 255;
                    else if (AbsVal < 0)
                        AbsVal = 0;
                    bmp.SetPixel(i-1, j, Color.FromArgb(255, AbsVal, AbsVal, AbsVal));
                }
            }
            refresh();
        }


        ///////////ВИЗУАЛИЗАЦИЯ//////////////////
        public Form1 form1;
        public PictureBox picBox;

        public void drawImgWhithPredictions(string outputCSVFile, string predictionsColumnName, string split_point,string predColInd)
        {
            int predColIndINT = Convert.ToInt16(predColInd);

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

            int Xshift = Convert.ToInt16((allLines.Length - 1) * Convert.ToDouble(split_point.Replace('.', ',')));

            for (int i = 1; i < allLines.Length; i++)
            {
                string str1 = allLines[i].Split(';')[indCol + 1];

                int AbsVal = Convert.ToInt16(Convert.ToDouble(allLines[i].Split(';')[indCol + 1].Replace('.', ',')) * 255);

                if (AbsVal > 255)
                    AbsVal = 255;
                else if (AbsVal < 0)
                    AbsVal = 0;
                for (int j = predColIndINT; j < bmp.Height; j++)
                {
                    bmp.SetPixel(i, j, Color.FromArgb(255, AbsVal, AbsVal, AbsVal));
                }
            }
            refresh();
        }

        public void refresh()
        {
            picBox = form1.picBox;
            picBox.Image = new Bitmap(bmp,new Size(bmp.Width * 4, bmp.Height * 4));
        }
     
        public void log(String s, System.Drawing.Color col)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }
    }
}
