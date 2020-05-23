using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Экспертная_система
{
    internal partial class Positions : Form
    {
        internal Positions()
        {
            InitializeComponent();
            Text = "Открыто позиций:";
        }

        private void Positions_Load(object sender, EventArgs e)
        {
            //   dataGridView1.Columns.Add("name", "Имя индивида");
            //   dataGridView1.Columns.Add("code", "Код");
            //   dataGridView1.Columns.Add("state", "Состояние");
            //   dataGridView1.Columns.Add("target_funtion", "Целевая функция");

            this.Show();
        }
        internal void refresh(List<Position> positions, double current_bid, double dues)
        {
            int total_opened = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                bool is_new = true;
                string[] data = new string[5];
                data[0] = positions[i].id.ToString();
                data[1] = positions[i].buyed_at.ToString();
                data[2] = positions[i].buy_price.ToString();
                if (positions[i].closed)
                    data[3] = positions[i].sell_price.ToString();
                else
                    data[3] = ((current_bid - positions[i].buy_price) * positions[i].quantity - (current_bid * positions[i].quantity * dues)*2).ToString();
                if (positions[i].sell_order_id == -1)
                {
                    data[4] = "Ошибка создания ордера на продажу";
                }
                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                {
                    
                    if (dataGridView1.Rows[j].Cells[0].Value as string == positions[i].id.ToString())
                    {
                        is_new = false;
                        dataGridView1.Rows[j].Cells[3].Value = data[3];
                        dataGridView1.Rows[j].Cells[4].Value = data[4];
                        if (!positions[i].closed)
                        {
                            total_opened++;
                            if (double.Parse(data[3]) < 0)
                            {
                                dataGridView1.Rows[j].Cells[3].Style.ForeColor = Color.Red;
                            }
                            else
                            {
                                dataGridView1.Rows[j].Cells[3].Style.ForeColor = Color.Lime;
                            }
                        }
                        else
                        {
                            dataGridView1.Rows[j].Cells[3].Style.ForeColor = Color.White;
                        }
                    }
                }
                if (is_new)
                {
                    dataGridView1.Rows.Add(data);
                }
            }
            Text = "Открыто позиций: " + total_opened.ToString();
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
        }

        private void Positions_SizeChanged(object sender, EventArgs e)
        {
            dataGridView1.Size = new System.Drawing.Size(Width, Height);
        }
    }
}
