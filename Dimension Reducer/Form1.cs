using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dimension_Reducer
{
    public partial class Form1 : Form
    {

        string source_file;
        string result_file;
        string[] all_lines;
        string[] max_value_in_column;
        string[] min_value_in_column;
        string[] result_columns;
        string[] values_in_column;
        //bool[] freesed_coluns;
        List<string> reduced;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            reduced = new List<string>();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            source_file = openFileDialog1.FileName;
            textBox2.Text = source_file;
            result_file = source_file.Replace(".txt", DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString().Replace(":", "-") + ".txt");
            textBox1.Text = result_file;
            readData();


            showControls();
        }
        void readData()
        {
            all_lines = File.ReadAllLines(source_file);
            string[] columns = all_lines[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            max_value_in_column = new string[columns.Length];
            min_value_in_column = new string[columns.Length];
            values_in_column = new string[columns.Length];
            result_columns = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                values_in_column[i] = "";
                bool is_double = false;
                string[] first_row = all_lines[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                if (first_row[i].Contains(',') || first_row[i].Contains('.'))
                { is_double = true; }

                int max_value_i = int.MinValue;
                int min_value_i = int.MaxValue;
                double max_value_d = double.MinValue;
                double min_value_d = double.MaxValue;

                for (int l = 1; l < all_lines.Length; l++)
                {
                    string[] row = all_lines[l].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    if (!is_double)
                    {
                        if (max_value_i < int.Parse(row[i]))
                            max_value_i = int.Parse(row[i]);

                        if (min_value_i > int.Parse(row[i]))
                            min_value_i = int.Parse(row[i]);
                    }
                    else
                    {
                        if (max_value_d < double.Parse(row[i].Replace('.', ',')))
                            max_value_d = double.Parse(row[i].Replace('.', ','));

                        if (min_value_d > double.Parse(row[i].Replace('.', ',')))
                            min_value_d = double.Parse(row[i].Replace('.', ','));
                    }

                    bool is_new = true;
                    string[] values = values_in_column[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int v = 0; v < values.Length; v++)
                    {
                        if (values[v] == row[i])
                            is_new = false;
                    }
                    if (is_new)
                        values_in_column[i] += row[i] + "/";
                }
                if (!is_double)
                {
                    max_value_in_column[i] = max_value_i.ToString();
                    min_value_in_column[i] = min_value_i.ToString();
                }
                else
                {
                    max_value_in_column[i] = max_value_d.ToString();
                    min_value_in_column[i] = min_value_d.ToString();
                }
            }
        }
        void showControls()
        {
            int horizontal_step = 120;
            string[] columns = all_lines[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (columns.Length * horizontal_step + 200 > this.Width)
            {
                this.Width = columns.Length * horizontal_step + 200;
            }
            for (int i = 0; i < columns.Length; i++)
            {
                bool is_unic = true;
                for (int j = 0; j < columns.Length; j++)
                {
                    if (columns[j] == columns[i] && i != j)
                        is_unic = false;
                }
                if (!is_unic)
                    result_columns[i] = columns[i] + "_" + (i + 1).ToString();
                else
                    result_columns[i] = columns[i];

                TextBox column_name = new TextBox();
                column_name.Text = result_columns[i];
                column_name.Location = new System.Drawing.Point(200 + (horizontal_step * i), 60);
                column_name.Size = new System.Drawing.Size(100, 19);
                column_name.ReadOnly = true;
                this.Controls.Add(column_name);

                TextBox txtBox_min = new TextBox();
                txtBox_min.Text = min_value_in_column[i];
                txtBox_min.Location = new System.Drawing.Point(200 + (horizontal_step * i), 80);
                txtBox_min.Size = new System.Drawing.Size(49, 19);
                txtBox_min.ReadOnly = true;
                this.Controls.Add(txtBox_min);

                TextBox txtBox_max = new TextBox();
                txtBox_max.Text = max_value_in_column[i];
                txtBox_max.Location = new System.Drawing.Point(251 + (horizontal_step * i), 80);
                txtBox_max.Size = new System.Drawing.Size(49, 19);
                txtBox_max.ReadOnly = true;
                this.Controls.Add(txtBox_max);

                TextBox values = new TextBox();
                values.Text = values_in_column[i];
                values.Location = new System.Drawing.Point(200 + (horizontal_step * i), 100);
                values.Size = new System.Drawing.Size(100, 19);
                values.ReadOnly = true;
                this.Controls.Add(values);

                TextBox value = new TextBox();
                value.Location = new System.Drawing.Point(200 + (horizontal_step * i), 120);
                value.Size = new System.Drawing.Size(100, 19);
                value.Name = "nUD " + result_columns[i];
                this.Controls.Add(value);


                CheckBox chkBox = new CheckBox();
                chkBox.Text = "freeze";
                chkBox.Name = "freeze " + result_columns[i];
                chkBox.Location = new System.Drawing.Point(200 + (horizontal_step * i), 140);
                chkBox.CheckedChanged += new EventHandler(onFreeze);
                this.Controls.Add(chkBox);

            }

        }
        void onFreeze(object sender, EventArgs e)
        {
            /*   CheckBox chb = (CheckBox)sender;
               for (int i = 0; i < this.Controls.Count; i++)
               {
                   if (this.Controls[i].Name.Split(' ').Length > 1)
                       if (this.Controls[i].Name.Split(' ')[1] == chb.Name.Split(' ')[1])
                       {
                           string type = this.Controls[i].GetType().Name;
                           freesed_coluns[]
                       }
               }*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<int> freezed_columns = new List<int>();
            List<string> const_values = new List<string>();

            int x_ind = -1;
            int y_ind = -1;
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (this.Controls[i].Name.Split(' ')[0] == "freeze")
                {
                    CheckBox chb = (CheckBox)this.Controls[i];
                    if (chb.Checked)
                    {
                        for (int j = 0; j < result_columns.Length; j++)
                            if (this.Controls[i].Name.Split(' ').Length > 1)
                                if (result_columns[j] == this.Controls[i].Name.Split(' ')[1])
                                    freezed_columns.Add(j);

                        for (int n = 0; n < this.Controls.Count; n++)
                        {
                            if (this.Controls[n].Name.Split(' ')[0] == "nUD")
                            {
                                if (this.Controls[i].Name.Split(' ')[1] == this.Controls[n].Name.Split(' ')[1])
                                {
                                    TextBox value = (TextBox)this.Controls[n];
                                    const_values.Add(value.Text);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (x_ind == -1)
                        {
                            for (int n = 0; n < result_columns.Length; n++)
                                if (result_columns[n] == this.Controls[i].Name.Split(' ')[1])
                                { x_ind = n; }
                        }
                        else
                        {
                            if (y_ind == -1)
                            {
                                for (int n = 0; n < result_columns.Length; n++)
                                    if (result_columns[n] == this.Controls[i].Name.Split(' ')[1])
                                    { y_ind = n; }
                            }
                        }
                    }
                }
            }

            // удаление всех строк, которые не удовлетворяют заданным констанам
            List<string> new_lines = getLinesWichMatchConstants(all_lines, freezed_columns, const_values);

            if (result_columns.Length - freezed_columns.Count == 3)
            {
                if (surfacePlotly.Checked)
                {
                    // генерация файла для построения поверхности
                    // крайний правый столбец - Z
                    string head = ",";
                    List<string> X = new List<string>();
                    List<string> Y = new List<string>();
                    List<string> result = new List<string>();

                    for (int l = 1; l < new_lines.Count; l++)
                    {
                        string[] row = new_lines[l].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (X.IndexOf(row[x_ind]) == -1)
                        {
                            X.Add(row[x_ind]);
                            if (surfaceExcel.Checked)
                                result.Add(row[x_ind] + ";");
                            else
                                result.Add("");
                        }
                    }

                    for (int i = 0; i < X.Count; i++)
                    {
                        for (int l = 0; l < new_lines.Count; l++)
                        {
                            string[] row = new_lines[l].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (X[i] == row[x_ind])
                            {
                                if (Y.IndexOf(row[y_ind]) == -1)
                                {
                                    Y.Add(row[y_ind]);
                                    if (surfaceExcel.Checked)
                                        head += row[y_ind] + ";";
                                }
                            }
                        }
                    }

                    for (int i = 0; i < X.Count; i++)
                    {
                        for (int j = 0; j < Y.Count; j++)
                        {
                            for (int l = 0; l < new_lines.Count; l++)
                            {
                                string[] row = new_lines[l].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                if (X[i] == row[x_ind] & Y[j] == row[y_ind])
                                {
                                    result[i] += row[row.Length - 1].Replace(",",".") + ";";
                                    break;
                                }
                            }
                        }
                    }
                    if (surfaceExcel.Checked)
                        result.Insert(0, head);
                    File.WriteAllLines(textBox1.Text, result);
                }
                else
                { File.WriteAllLines(textBox1.Text, new_lines); }
            }
            else
            { File.WriteAllLines(textBox1.Text, new_lines); }


        }

        List<string> getLinesWichMatchConstants(string[] all_lines, List<int> freezed_columns, List<string> const_values)
        {
            List<string> new_lines = new List<string>();
            new_lines.Add(all_lines[0]);
            for (int l = 1; l < all_lines.Length; l++)
            {
                bool match_all_constants = true;

                string[] row = all_lines[l].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < row.Length; j++)
                {
                    for (int i = 0; i < freezed_columns.Count; i++)
                    {
                        if (j == freezed_columns[i] & row[j] != const_values[i])
                        {
                            match_all_constants = false;
                        }
                    }
                }
                if (match_all_constants)
                    new_lines.Add(all_lines[l]);
            }
            return new_lines;
        }
    }
}
