using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlotlyNET
{
    public partial class Form1 : Form
    {
        string source_file;
        string python_path = @"C:\Program Files\Python36\python.exe";
        string path_prefix = @"E:\Anton\Desktop\MAIN\";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            source_file = openFileDialog1.FileName;
            textBox2.Text = source_file;

            var all_lines = File.ReadAllLines(source_file);
            richTextBox1.Text = all_lines[0];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            runProcess(path_prefix + @"PlotlyNET\3d_plot.py", "--file_path \"" + source_file + "\" " + command.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            runProcess(path_prefix + @"PlotlyNET\3d_plot.py", "--file_path \"" + source_file + "\" " + command.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            runProcess(path_prefix + @"PlotlyNET\surface_plot.py" , "--file_path \"" + source_file+"\"");
        }

        public Process runProcess(string scriptFile, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = python_path;
            start.Arguments = '"' + scriptFile + '"' + " " + args;

            Process process = Process.Start(start);
            return process;
        }
    }
}
