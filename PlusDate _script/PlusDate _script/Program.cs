using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PlusDate__script
{
    class Program
    {
        static void Main(string[] args)
        {
            string file_name =args[0];
           // string file_name = "85123A-dataset";
            var lines = File.ReadAllLines(file_name);
            DateTime dt = new DateTime(2011, 1, 1);
            lines[0] = "<DATE>;" + lines[0];
            for (int i = 1; i < lines.Length; i++)
            {//01/09/98
                lines[i] = dt.ToString("dd/MM/yyyy")+';'+ lines[i];
                dt = dt.AddDays(1);
            }
            File.WriteAllLines(file_name.Replace(".txt","date.txt"), lines);
        }
    }
}
