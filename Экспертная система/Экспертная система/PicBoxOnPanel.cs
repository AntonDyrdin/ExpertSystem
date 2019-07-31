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
    public partial class PicBoxOnPanel : Form
    {
        public PicBoxOnPanel(Hyperparameters h)
        {
            InitializeComponent();
            Text = h.getValueByName("json_file_path");
            h.draw(0, picBox, 25, 300);
        }

        private void PicBoxOnPanel_Load(object sender, EventArgs e)
        {

        }

        private void PicBoxOnPanel_SizeChanged(object sender, EventArgs e)
        {
            panel1.Size = new Size(Width, Height);
        }
    }
}
