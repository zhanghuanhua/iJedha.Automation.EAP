using System;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmLogout : Form
    {
        private frmMain Mon;
        private DateTime clearTime;
        public frmLogout(frmMain _Mon )
        {
            InitializeComponent();
            Mon = _Mon;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            Mon.WaitClose = true;
            this.Close();
            Mon.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                butOK_Click(sender, e);
            }
        }

        private void frmLogout_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            timer1.Enabled = true;
            btnCancel.Focus();
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
