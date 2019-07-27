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
    }
}
