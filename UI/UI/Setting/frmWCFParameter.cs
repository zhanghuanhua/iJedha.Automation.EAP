using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace iJedha.Automation.EAP
{
    public partial class frmWCFParameter : Form
    {
        WCFParaLibrary HostwcfParaLibrary;
        public frmWCFParameter()
        {
            InitializeComponent();
        }

        private void frmWCFParameter_Load(object sender, EventArgs e)
        {
            refreshData();
        }

        void refreshData()
        {
            HostwcfParaLibrary = Environment.EAPEnvironment.commonLibrary.HostwcfParaLibrary;
            if (HostwcfParaLibrary == null) return;

            chkServerEnable.Checked = HostwcfParaLibrary.Server_Enable;
            txtLocalIP.Text = HostwcfParaLibrary.LocalIP;
            txtLocalPort.Text = HostwcfParaLibrary.LocalPort.ToString();
            txtConnectCount.Text = HostwcfParaLibrary.ConnectCount.ToString();
            txtLocalUrl.Text = HostwcfParaLibrary.LocalUrlString;

            chkClientEnable.Checked = HostwcfParaLibrary.Client_Enable;
            txtRemoteIP.Text = HostwcfParaLibrary.RemoteIP;
            txtRemotePort.Text = HostwcfParaLibrary.RemotePort.ToString();
            txtAliveInterval.Text = HostwcfParaLibrary.AliveCheckInterval.ToString();
            txtRemoteUrl.Text = HostwcfParaLibrary.RemoteUrlString;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            btnEdit.Enabled = false;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!CheckData()) return;
            if (HostwcfParaLibrary == null) return;

            #region 先將設定填入变量
            lock (HostwcfParaLibrary)
            {
                HostwcfParaLibrary.Server_Enable = chkServerEnable.Checked;
                HostwcfParaLibrary.LocalIP = txtLocalIP.Text;
                HostwcfParaLibrary.LocalPort = int.Parse(txtLocalPort.Text);
                HostwcfParaLibrary.ConnectCount = int.Parse(txtConnectCount.Text);
                HostwcfParaLibrary.LocalUrlString = string.Format("net.tcp://{0}:{1}/", HostwcfParaLibrary.LocalIP, HostwcfParaLibrary.LocalPort);

                HostwcfParaLibrary.Client_Enable = chkClientEnable.Checked;
                HostwcfParaLibrary.RemoteIP = txtRemoteIP.Text;
                HostwcfParaLibrary.RemotePort = int.Parse(txtRemotePort.Text);
                HostwcfParaLibrary.AliveCheckInterval = int.Parse(txtAliveInterval.Text);
                HostwcfParaLibrary.RemoteUrlString = string.Format("net.tcp://{0}:{1}/", HostwcfParaLibrary.RemoteIP, HostwcfParaLibrary.RemotePort);

            }
            #endregion

            Environment.EAPEnvironment.commonLibrary.UpdateMesWCFParaLibrary(HostwcfParaLibrary);

            EnableControls(true);
            btnEdit.Enabled = true;
            btnCancel.Enabled = false;
            btnSave.Enabled = false;
            refreshData();

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
            txtLocalIP.ReadOnly = Switch;
            txtLocalPort.ReadOnly = Switch;
            txtConnectCount.ReadOnly = Switch;

            txtRemoteIP.ReadOnly = Switch;
            txtRemotePort.ReadOnly = Switch;
            txtAliveInterval.ReadOnly = Switch;

            txtConnectCount.ReadOnly = txtAliveInterval.ReadOnly = Switch;
            chkServerEnable.Enabled = chkClientEnable.Enabled = !Switch;
        }

        private bool CheckData()
        {
            try
            {
                uint tmp;
                IPAddress ip;

                if (!IPAddress.TryParse(txtRemoteIP.Text, out ip))
                {
                    MessageBox.Show("WCF Remote IP Format Error!");
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
                if (KeyIn == (char)8) return KeyIn;
            }
            return (char)0;
        }

        private void frmWCFParameter_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
