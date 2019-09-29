using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Экспертная_система
{
    public partial class TextBoxes : Form
    {
        public TextBoxes()
        {
            InitializeComponent();
        }

        private void TextBoxes_Load(object sender, EventArgs e)
        {

        }
        public void fillTextBox1(List<string> data)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.Text = "";

                foreach (string d in data)
                    richTextBox1.Text += d + '\n';
            }));
        }
        public void fillTextBox2(string[] data)
        {
            Invoke(new Action(() =>
            {
                richTextBox2.Text = "";

                foreach (string d in data)
                    richTextBox2.Text += d + '\n';
            }));
        }
        public void fillTextBox3(string[] data)
        {
            Invoke(new Action(() =>
            {
                richTextBox3.Text = "";

                foreach (string d in data)
                    richTextBox3.Text += d + "; ";
            }));
        }
    }
}
