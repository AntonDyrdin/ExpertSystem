﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JsonView
{
    public partial class Form1 : Form
    {
        private string fileName = "";
        public Form1(string fileName)
        {
            InitializeComponent();

            this.fileName = fileName;
            DpiFix();
        }

        private void Save_button_Click(object sender, EventArgs e)
        {
            try
            {

                if (onlyValue.Checked)
                {
                    var oldValue = h.getNodeById(int.Parse(textBox1.Text)).getAttributeValue("value");
                    h.getNodeById(int.Parse(textBox1.Text)).setAttribute("value", textBox4.Text);
                    log("атрибут value изменён с " + oldValue + " на " + h.getNodeById(int.Parse(textBox1.Text)).getAttributeValue("value"));
                }
                else
                {
                    var oldValue = h.getNodeById(int.Parse(textBox1.Text)).getAttributeValue(textBox2.Text);
                    h.getNodeById(int.Parse(textBox1.Text)).setAttribute(textBox2.Text, textBox4.Text);
                    log("атрибут value изменён с " + oldValue + " на " + h.getNodeById(int.Parse(textBox1.Text)).getAttributeValue(textBox2.Text));
                }
            }
            catch { }
            refresh();
        }

        private int depth = 12;
        private int width = 120;
        private int rowH = 20;
        private Hyperparameters h;
        private Hyperparameters settings;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                settings = new Hyperparameters(System.IO.Path.GetDirectoryName(Application.ExecutablePath)+"\\settings.json", this, true);

                depth = int.Parse(settings.getValueByName("depth"));
                width = int.Parse(settings.getValueByName("width"));
                rowH = int.Parse(settings.getValueByName("rowH"));
            }
            catch
            {
                settings = new Hyperparameters(this, "SETTINGS");
            }
            h = new Hyperparameters(fileName, this, true);
            refresh();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (onlyValue.Checked)
                    textBox3.Text = h.getNodeById(int.Parse(textBox1.Text)).getAttributeValue("value");
                else
                    textBox3.Text = h.getNodeById(int.Parse(textBox1.Text)).getAttributeValue(textBox2.Text);
            }
            catch { }
        }
        public void log(String s, System.Drawing.Color col)
        {
            Invoke(new Action(() =>
            {
                logBox.SelectionColor = col;
                logBox.AppendText(s + '\n');
                logBox.SelectionColor = Color.White;
                logBox.SelectionStart = logBox.Text.Length;
                logBox.ScrollToCaret();
            }));
            var strings = new string[1];
            strings[0] = s;
        }
        public void log(string s)
        {
            log(s, Color.White);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                panel1.Size = new Size(this.Size.Width - 200, this.Size.Height - 80 - 30);
                picBox.Size = new Size(this.Size.Width - 200, this.Size.Height - 80 - 30);
                logBox.Location = new Point(200, this.Size.Height - 80 - 30);
                logBox.Size = new Size(this.Size.Width - 200, 80);
                refresh();
            }
            catch { }
        }

        private ViewSettings vs;
        private void Button3_Click(object sender, EventArgs e)
        {
            vs = new ViewSettings();
            vs.Show();
            vs.trackBar1.Scroll += TrackBar1_Scroll;
            vs.trackBar2.Scroll += TrackBar2_Scroll;
            vs.trackBar3.Scroll += TrackBar3_Scroll;
            vs.FormClosed += Vs_FormClosed;
        }

        private void Vs_FormClosed(object sender, FormClosedEventArgs e)
        {
            settings.Save("settings.json");
        }

        private void TrackBar3_Scroll(object sender, EventArgs e)
        {
            rowH = vs.trackBar3.Value;
            refresh();
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            width = vs.trackBar2.Value;
            refresh();
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            depth = vs.trackBar1.Value;
            refresh();
        }

        private void OnlyValue_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = true;

        }

        private void refresh()
        {
            settings.setValueByName("depth", depth);
            settings.setValueByName("width", width);
            settings.setValueByName("rowH", rowH);

            h.draw(0, picBox, this, depth, width, rowH);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                h.addByParentId(int.Parse(textBox8.Text), "name:" + textBox9.Text + ",value:" + textBox6.Text);
                log("добавлен узел parentID = " + textBox8.Text + "name:" + textBox9.Text + ",value:" + textBox6.Text);
            }
            else
            {
                h.addByParentId(int.Parse(textBox8.Text), "name:" + textBox9.Text + "," + textBox6.Text);
                log("добавлен узел parentID = " + textBox8.Text + "name:" + textBox9.Text + "," +textBox6.Text);
            }
                refresh();
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                textBox7.Enabled = false;
                textBox6.Enabled = true;
            }
            else
            {
                textBox7.Enabled = true;
                textBox6.Enabled = false;
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            h.Save(fileName);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            h.deleteBranch(int.Parse(textBox5.Text));
            log("удалён узел ID = " + textBox5.Text);
            refresh();
        }
        public static void DpiFix()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
        }
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
