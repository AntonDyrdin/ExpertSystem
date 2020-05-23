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
    public partial class ModeSelector : Form
    {
        public ModeSelector()
        {
            InitializeComponent();
            linkLabel1.Text = Environment.MachineName;

        }

        private void ModeSelector_Click(object sender, EventArgs e)
        {
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(linkLabel1.Text);
        }
    }
}
