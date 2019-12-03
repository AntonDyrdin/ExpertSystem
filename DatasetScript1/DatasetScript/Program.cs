using System;
using System.Collections.Generic;
using System.IO;
namespace DatasetScript
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var allLines = File.ReadAllLines(args[0]);
            List<string> newLines = new List<string>();

            string s = "";

            newLines.Add(allLines[0]);
           /* int window = 10;
            double avg = 0;
            for (int i = 1; i < allLines.Length-window; i++)
            {
                s = "";
                string[] features = allLines[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                double sum = 0;
                for (int j = 0; j < window; j++)
                {
                    features = allLines[i + j].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    double x = double.Parse(features[0].Replace('.', ','));
                    sum += x;
                }
                avg = sum / window;
                s += String.Format("{0:0.########}", avg).Replace(',', '.') + ';';
                newLines.Add(s);
            }*/
              for (int i = 0; i < allLines.Length; i++)
              {
                  s = "";
                  s = allLines[i].Replace(',','.');
                  
                  newLines.Add(s);
              }
            /*  foreach (string line in allLines)
              {
                  if (line != "")
                  {
                      var newline = line.Replace(',', ';');
                      newLines.Add(newline.Replace('.', ','));
                     // System.Console.WriteLine(newline);
                  }
              }*/
            File.WriteAllLines(args[0], newLines);
        }
    }
}
