using System.Collections.Generic;
using System.IO;
namespace DatasetScript
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var allLines = File.ReadAllLines(@"C:\Users\anton\Рабочий стол\MAIN\Временные ряды\timeSeries4.txt");
            List<string> newLines = new List<string>();
            foreach (string line in allLines)
            {
                if (line != "")
                {
                    var newline = line.Replace(',', ';');
                    newLines.Add(newline.Replace('.', ','));
                   // System.Console.WriteLine(newline);
                }
            }
            File.WriteAllLines(@"C:\Users\anton\Рабочий стол\MAIN\Временные ряды\timeSeries4.txt", newLines);
        }
    }
}
