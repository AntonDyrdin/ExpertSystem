using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace Экспертная_система
{
    public class ParameterVisualizerFastest
    {
        PictureBox pictureBox;
        Graphics g;
        Bitmap bitmap;
        Form1 form1;
        double mainFontDepth;
        double functionDepth;
        int Xmax;
        int Ymax;
        int xZeroGap = 80;
        int yGap;
        double minY;
        double maxY;
        double dx;
        List<point> points;
        double zeroY;
        string label;

        public struct point
        {
            public double x;
            public double y;
        }
        void drawStatic()
        {
            if ((Ymax / 10) < 14)
                mainFontDepth = Ymax / 10;
            else
                mainFontDepth = 14;
            drawString(label, mainFontDepth, Xmax / 2 - (label.Length * mainFontDepth / 2), 0);

            drawLine(Color.White, 2, xZeroGap, 0, xZeroGap, Ymax);
            if (minY < 0)
            {
                drawLine(Color.White, functionDepth,
                          xZeroGap, Ymax - ((Ymax - (functionDepth * 4.0)) * (-minY)) / (maxY - minY) + functionDepth * 2.0,
                          Xmax, Ymax - ((Ymax - (functionDepth * 4.0)) * (-minY)) / (maxY - minY) + functionDepth * 2.0);
            }
            if (minY != maxY)
            {
                for (double i = minY; i < maxY; i = i + ((maxY - minY) / 10))
                {
                    if (i > 0)
                    {
                        if (i.ToString().Length > Convert.ToInt16(xZeroGap / mainFontDepth))
                            drawString(" " + i.ToString().Substring(0, Convert.ToInt16(xZeroGap / mainFontDepth)), mainFontDepth, 0, Ymax - (((Ymax - (functionDepth * 4.0)) * (i - minY)) / (maxY - minY) + functionDepth * 2.0) - mainFontDepth);
                        else
                            drawString(" " + i.ToString(), mainFontDepth, 0, Ymax - (((Ymax - (functionDepth * 4.0)) * (i - minY)) / (maxY - minY) + functionDepth * 2.0) - mainFontDepth);
                    }
                    else
                    {
                        if (i.ToString().Length > Convert.ToInt16(xZeroGap / mainFontDepth) + 1)
                            drawString(i.ToString().Substring(0, Convert.ToInt16(xZeroGap / mainFontDepth) + 1), mainFontDepth, 0, Ymax - (((Ymax - (functionDepth * 4.0)) * (i - minY)) / (maxY - minY) + functionDepth * 2.0) - mainFontDepth);
                        else
                            drawString(i.ToString(), mainFontDepth, 0, Ymax - (((Ymax - (functionDepth * 4.0)) * (i - minY)) / (maxY - minY) + functionDepth * 2.0) - mainFontDepth);
                    }
                    drawLine(Color.White, functionDepth,
                             xZeroGap - 5, Ymax - (((Ymax - (functionDepth * 4.0)) * (i - minY)) / (maxY - minY) + functionDepth * 2.0),
                             xZeroGap + 5, Ymax - (((Ymax - (functionDepth * 4.0)) * (i - minY)) / (maxY - minY) + functionDepth * 2.0));

                }
                try
                {
                    drawString("   0", mainFontDepth, 0, Ymax - ((Ymax - (functionDepth * 4.0)) * (-minY)) / (maxY - minY) + functionDepth * 2.0 - mainFontDepth);
                }
                catch { }
            }
            else
            {
                drawString("   0", mainFontDepth, 0, Ymax / 2);
            }
        }

        public ParameterVisualizerFastest(PictureBox target_pictureBox, Form1 form1, string label)
        {
            lastCount = 0;
            minY = double.MaxValue;
            maxY = double.MinValue;
            points = new List<point>();
            this.form1 = form1;
            this.pictureBox = target_pictureBox;
            bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            g = Graphics.FromImage(bitmap);
            System.Timers.Timer needToRefresh = new System.Timers.Timer();
            needToRefresh.Elapsed += new System.Timers.ElapsedEventHandler(checkForNewPoints);
            needToRefresh.AutoReset = true;
            needToRefresh.Interval = 500;
            needToRefresh.Start();

            Xmax = pictureBox.Width;
            Ymax = pictureBox.Height;
            this.label = label;
            yGap = Ymax / 10;
            zeroY = Ymax / 2;
            functionDepth = 10;

            refresh();
        }
        public void addPoint(double y)
        {
            point point = new point();
            point.x = points.Count + 1;
            point.y = y;
            points.Add(point);
        }
        public void addPoint(double inc, double y)
        {
            point point = new point();
            point.x = inc;
            point.y = y;
            points.Add(point);
        }
        public void refresh()
        {

            if (points != null)
            {
                if (points.Count > 1)
                {
                    dx = Convert.ToDouble(Xmax) / Convert.ToDouble(points.Count + 1);
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (points[i].y > maxY)
                            maxY = points[i].y;
                        if (points[i].y < minY)
                            minY = points[i].y;
                    }
                    for (int i = 1; i < points.Count; i++)
                    {
                        if (minY == maxY)
                        {
                            drawLine(Color.White, functionDepth,
                            xZeroGap + dx * (i - 1), Ymax / 2,
                            xZeroGap + dx * i, Ymax / 2);

                        }
                        else
                        {
                            drawLine(Color.Green, functionDepth,
                              xZeroGap + dx * (i - 1), Ymax - (((Ymax - (functionDepth * 4.0)) * (points[i - 1].y - minY)) / (maxY - minY) + functionDepth * 2.0),
                              xZeroGap + dx * i, Ymax - (((Ymax - (functionDepth * 4.0)) * (points[i].y - minY)) / (maxY - minY) + functionDepth * 2.0));
                        }
                    }
                }
            }
            drawStatic();
            pictureBox.Image = bitmap;
            form1.voidDelegate = new Form1.VoidDelegate(form1.Refresh);
            form1.logBox.Invoke(form1.voidDelegate);
            bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
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
        int lastCount;
        public void checkForNewPoints(object sender, EventArgs e)
        {
            if (points != null)
            {
                if (points.Count > lastCount)
                {
                    refresh();
                    lastCount = points.Count;
                }
            }
        }
    }
}
