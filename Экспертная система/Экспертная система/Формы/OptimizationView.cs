﻿using System;
using System.IO;
using System.Windows.Forms;

namespace Экспертная_система
{
    internal partial class OptimizationView : Form
    {
        internal AlgorithmOptimization AO;
        internal OptimizationView(AlgorithmOptimization AO)
        {
            InitializeComponent();
            Text = "Оптимизация " + AO.algorithm.h.nodes[1].getValue();
            this.AO = AO;
        }
        private void Optimization_Load(object sender, EventArgs e)
        {
         //   dataGridView1.Columns.Add("name", "Имя индивида");
         //   dataGridView1.Columns.Add("code", "Код");
         //   dataGridView1.Columns.Add("state", "Состояние");
         //   dataGridView1.Columns.Add("target_funtion", "Целевая функция");

            refresh();

            this.Show();
        }
        internal void refresh()
        {

            dataGridView1.Rows.Clear();

            for (int i = 0; i < AO.population_value; i++)
                if (AO.population[i] != null)
                {
                    dataGridView1.Rows.Add(new object[] {
                    AO.population[i].getValueByName("model_name"),
                    AO.population[i].getValueByName("code"),
                    AO.population[i].getValueByName("state"),
                    AO.population[i].getValueByName("parents"),
                    AO.population[i].getValueByName("target_function")
                     });
                }

        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells[0].ColumnIndex == 4)
            {
                Algorithm algorithm = Algorithm.newInstance(AO.algorithm);
                algorithm.Open(AO.population[dataGridView1.SelectedCells[0].RowIndex].getValueByName("json_file_path"));
                algorithm.h.setValueByName("show_train_charts", "True"); 
                algorithm.getAccAndStdDev(File.ReadAllLines(algorithm.h.getValueByName("predictions_file_path")));
            }

            if (dataGridView1.SelectedCells[0].ColumnIndex == 0)
            {
                PicBoxOnPanel picBoxOnPanel = new PicBoxOnPanel();
                picBoxOnPanel.Show();
                picBoxOnPanel.Text = AO.population[dataGridView1.SelectedCells[0].RowIndex].getValueByName("json_file_path");
                AO.population[dataGridView1.SelectedCells[0].RowIndex].draw(0, picBoxOnPanel.picBox, 25, 300);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            refresh();
        }

        private void OptimizationView_SizeChanged(object sender, EventArgs e)
        {
            dataGridView1.Size = new System.Drawing.Size(Width, Height);
        }
    }
}
