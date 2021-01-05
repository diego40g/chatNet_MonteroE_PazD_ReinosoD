using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatMonster_MonteroE_PazD_ReinosoD
{
    public partial class NotificacionForm : Form
    {
        string name, IP;
        public NotificacionForm(string name, string IP)
        {
            InitializeComponent();
            this.name = name;
            this.IP = IP;
        }

        private void NotificacionForm_Load(object sender, EventArgs e)
        {
            notificationTempLabel.Text = "File sending to " + IP + " " + name + "...";
        }
    }
}
