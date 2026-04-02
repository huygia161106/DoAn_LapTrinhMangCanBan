using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientA
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }



        // Ghi đè sự kiện đóng Form
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            // Lệnh này sẽ đóng toàn bộ các Form đang tàng hình và kết thúc process
            Application.Exit();
        }
    }
}
