﻿using System;
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
            //  var lines = File.ReadAllLines(args[0]);
            string file_name = "85123A-dataset";
            var lines = File.ReadAllLines(file_name+".txt");
            DateTime dt = new DateTime(2009, 12, 1);
            lines[0] = "<DATE>;" + lines[0];
            for (int i = 1; i < lines.Length; i++)
            {//01/09/98
                lines[i] = dt.ToString("dd/MM/yyyy")+';'+ lines[i];
                dt = dt.AddDays(1);
            }
            File.WriteAllLines(file_name + "+date.txt", lines);
        }
    }
}
