using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmLogoutDetail : Form
    {
        string logDetail;

        public string LogDetail
        {
            get { return logDetail; }
            set
            {
                logDetail = value;
                richTextBox1.Text = "";
                richTextBox1.Text = LogDetail;
            }
        }
        public frmLogoutDetail()
        {
            InitializeComponent();
        }
    }
}
