using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmRBMQParameter : Form
    {
        RBMQParaLibraryBase rbmqParaLibrary;
        public frmRBMQParameter()
        {
            InitializeComponent();
        }

        private void frmRBMQParameter_Load(object sender, EventArgs e)
        {
            refreshData();
        }

        /// <summary>
        /// 获取"rbmqParaLibrary",更新控件的值
        /// </summary>
        void refreshData()
        {
            rbmqParaLibrary = Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary;
            if (rbmqParaLibrary == null)
            {
                return;
            }
            chkEnable.Checked = rbmqParaLibrary.Enable;
            txtServerUri.Text = rbmqParaLibrary.ServerUrlString;
            txtServerIP.Text = rbmqParaLibrary.ServerIP;
            txtServerPort.Text = rbmqParaLibrary.ServerPort.ToString();
            txtUserID.Text = rbmqParaLibrary.UserID;
            txtPassword.Text = rbmqParaLibrary.Password;
            txtExchangeName.Text = rbmqParaLibrary.ExchangeName;
            txtQueueName.Text = rbmqParaLibrary.QueueName;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            btnEdit.Enabled = false;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
        }

        /// <summary>
        /// 保存修改后的控件值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!CheckData())
            {
                return;
            }
            if (rbmqParaLibrary == null)
            {
                return;
            }

            #region [先將設定填入变量]
            lock (rbmqParaLibrary)
            {
                rbmqParaLibrary.Enable = chkEnable.Checked;
                rbmqParaLibrary.ServerIP = txtServerIP.Text;
                rbmqParaLibrary.ServerPort = int.Parse(txtServerPort.Text);
                rbmqParaLibrary.ExchangeName = txtExchangeName.Text;
                rbmqParaLibrary.QueueName = txtQueueName.Text;
                rbmqParaLibrary.UserID = txtUserID.Text;
                rbmqParaLibrary.Password = txtPassword.Text;
                rbmqParaLibrary.ServerUrlString = string.Format("amqp://{0}:{1}@{2}:{3}",
    rbmqParaLibrary.UserID, rbmqParaLibrary.Password, rbmqParaLibrary.ServerIP, rbmqParaLibrary.ServerPort);
                txtServerUri.Text = rbmqParaLibrary.ServerUrlString;
            }
            #endregion

            Environment.EAPEnvironment.commonLibrary.baseLib.UpdateRBMQParaLibrary(rbmqParaLibrary);

            EnableControls(true);
            btnEdit.Enabled = true;
            btnCancel.Enabled = false;
            btnSave.Enabled = false;

            Environment.EAPEnvironment.MQServiceStart();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            refreshData();
            EnableControls(true);
            btnEdit.Enabled = true;
            btnCancel.Enabled = false;
            btnSave.Enabled = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EnableControls(bool Switch)
        {
            txtExchangeName.ReadOnly = Switch;
            txtQueueName.ReadOnly = Switch;
            txtUserID.ReadOnly = Switch;
            txtPassword.ReadOnly = Switch;
            txtServerPort.ReadOnly = Switch;
            txtServerIP.ReadOnly = Switch;
            chkEnable.Enabled = !Switch;
        }

        /// <summary>
        /// 检查RemoteIP的值是否符合IP要求
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            try
            {
                uint tmp;
                IPAddress ip;
                if (!IPAddress.TryParse(txtServerIP.Text, out ip))
                {
                    MessageBox.Show("Server IP Format Error!");
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = CheckValid(e.KeyChar, "0123456789");
        }

        private void txtServerIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = CheckValid(e.KeyChar, ".0123456789");
        }

        private char CheckValid(char KeyIn, string ValidateString)
        {
            if (ValidateString.ToCharArray().Contains(KeyIn))
            {
                return KeyIn;
            }
            else
            {
                if (KeyIn == (char)8)
                {
                    return KeyIn;
                }
            }
            return (char)0;
        }

        private void frmRBMQParameter_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

     
    }
}
