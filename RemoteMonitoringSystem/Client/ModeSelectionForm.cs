using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ModeSelectionForm : Form
    {
        private string currentRole;
        private string currentUsername;
        public ModeSelectionForm(string username, string role)
        {
            InitializeComponent();

            currentUsername = username;
            currentRole = role;
        }
        private void btnShare_Click(object sender, EventArgs e)
        {
            MonitoringForm form = new MonitoringForm(currentUsername);

            form.Show();

            this.Hide();
        }
        private void btnMonitor_Click(object sender, EventArgs e)
        {
            MainForm form = new MainForm(currentUsername, currentRole);

            form.Show();

            this.Hide();
        }
    }
}
