using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Экспертная_система
{
    public partial class AgentManagerView : Form
    {
        public AgentManagerView(AgentManager manager)
        {
            InitializeComponent();
            this.manager = manager;
        }
        internal AgentManager manager;
        private void AgentManagerView_Load(object sender, EventArgs e)
        {

            this.Show();
        }
        internal void refresh()
        {

            dataGridView1.Rows.Clear();

            for (int i = 0; i < manager.agents.Count; i++)
                if (manager.agents[i].hostName != null)
                {
                    if (manager.agents[i].task == null)
                    {
                        dataGridView1.Rows.Add(new object[] {
                           manager.agents[i].hostName ,
                           manager.agents[i].ip,
                           manager.agents[i].status,
                           "none"
                     });
                    }
                    else
                    {
                        dataGridView1.Rows.Add(new object[] {
                           manager.agents[i].hostName ,
                           manager.agents[i].ip,
                           manager.agents[i].status,
                           manager.agents[i].task.h.getValueByName("model_name")+"  code: "+manager.agents[i].task.h.getValueByName("code")
                     });
                    }
                }

            dataGridView2.Rows.Clear();

            for (int i = 0; i < manager.tasks.Count; i++)
            {
                if (manager.tasks[i] != null)
                {
                    string task = manager.tasks[i].h.getValueByName("model_name") + "  code: " + manager.tasks[i].h.getValueByName("code");
                    dataGridView2.Rows.Add(new object[] {
                      task,
                       manager.tasks[i].status
                     });
                }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            refresh();
        }
    }
}
