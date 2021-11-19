using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmInformation : Form
    {
        int _timer = 0;
        public frmInformation()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _timer++;
            if (_timer > 4)
            {
                this.Close();
            }
        }

        private void frmInformation_Load(object sender, EventArgs e)
        {
            listboxMessage.Items.Add("About iJDEAP:");
            listboxMessage.Items.Add("EAP for TCP Socket Standard");
            listboxMessage.Items.Add("Copyright ©  2020 Jedha Technology (Shanghai) Ltd.");
            listboxMessage.Items.Add("All rights reserved");
            listboxMessage.Items.Add(">");
            listboxMessage.Items.Add(string.Format("    update time：{0}", Application.ProductVersion));
            listboxMessage.Items.Add(">");
            timer1.Enabled = true;
        }
    }
}
