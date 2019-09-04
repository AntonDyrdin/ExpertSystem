using System;
using System.Windows.Forms;

namespace Экспертная_система
{
    public partial class ExecutionProgress : Form
    {
        public ExecutionProgress()
        {
            InitializeComponent();
        }

        private void ExecutionProgress_SizeChanged(object sender, EventArgs e)
        {
            panel1.Height = Height;
            panel1.Width = Width;
        }

        private void ExecutionProgress_Load(object sender, EventArgs e)
        {

        }

        private int wasMaximized = 0;
        private void ExecutionProgress_Activated(object sender, EventArgs e)
        {
            if (wasMaximized <2)
            {
                WindowState = FormWindowState.Normal;
                WindowState = FormWindowState.Maximized;
                wasMaximized ++;
            }
        }
    }
}
