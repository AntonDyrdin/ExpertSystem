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

        public ImgDataset(string pathToImageFile, bool toInvertPixelsBrightness, MainForm form1)
        {

            this.form1 = form1;

            bmp = new Bitmap(Image.FromFile(pathToImageFile));

            csvLines = new string[bmp.Width + 1];
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

        public ImgDataset(string pathToDatasetFile, MainForm form1)
        {
            this.form1 = form1;

            csvLines = System.IO.File.ReadAllLines(pathToDatasetFile);

            csvLines = Expert.skipEmptyLines(csvLines);

            bmp = new Bitmap(csvLines.Length, csvLines[0].Split(';').Length + 1);
            for (int i = 1; i < csvLines.Length; i++)
            {
                var features = csvLines[i].Split(';');
                for (int j = 0; j < features.Length; j++)
                {
                    int AbsVal = Convert.ToInt16(Convert.ToDouble(features[j]) * 255);

                    if (AbsVal > 255)
                        AbsVal = 255;
                    else if (AbsVal < 0)
                        AbsVal = 0;
                }
            }
            refresh();
        }


        ///////////ВИЗУАЛИЗАЦИЯ//////////////////
        public MainForm form1;
        public PictureBox picBox;

        public void drawImgWhithPredictions(string outputCSVFile, string predictionsColumnName, string split_point, string predColInd)
        {
            int predColIndINT = Convert.ToInt16(predColInd);

            var allLines = File.ReadAllLines(outputCSVFile);

            allLines = Expert.skipEmptyLines(allLines);
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

            // int Xshift = Convert.ToInt16((allLines.Length - 1) * Convert.ToDouble(split_point));

            for (int i = 1; i < allLines.Length-1; i++)
            {

                //реальное значение находится в будущем, поэтому [i+1]
                int realValue = Convert.ToInt16(Convert.ToDouble(allLines[i + 1].Split(';')[predColIndINT]) * 255);
                //прогноз получен сейчас, поэтому [i]
                int predictedValue = Convert.ToInt16(Convert.ToDouble(allLines[i].Split(';')[indCol]) * 255);

                if (predictedValue > 255)
                    predictedValue = 255;
                else if (predictedValue < 0)
                    predictedValue = 0;
                for (int j = predColIndINT; j < bmp.Height; j++)
                {


                    if (realValue > 128 && predictedValue > 128)
                    {
                        bmp.SetPixel(i, j, Color.FromArgb(255, 0, predictedValue, 0));
                    }
                    else
                    if (realValue < 128 && predictedValue < 128)
                    {
                        bmp.SetPixel(i, j, Color.FromArgb(255, 0, predictedValue, 0));
                    }
                    else
                    if (realValue > 128 && predictedValue < 128)
                    {
                        bmp.SetPixel(i, j, Color.FromArgb(255, predictedValue, 0, 0));
                    }
                    else
                    if (realValue < 128 && predictedValue > 128)
                    {
                        bmp.SetPixel(i, j, Color.FromArgb(255, predictedValue, 0, 0));
                    }
                }
            }
            refresh();
        }

        public void refresh()
        {
            picBox = form1.picBox;
            picBox.Image = new Bitmap(bmp, new Size(picBox.Width, picBox.Height));
        }

        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
        }
    }
}
