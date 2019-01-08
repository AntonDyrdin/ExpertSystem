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
            foreach (string line in allLines)
            {
                if (line != "")
                {
                    var newline = line.Replace(',', ';');
                    newLines.Add(newline.Replace('.', ','));
                   // System.Console.WriteLine(newline);
                }
            }
            File.WriteAllLines(args[0], newLines);
        }
    }
}
