using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmWCFClientParameter : Form
    {
        WCFParaLibraryBase HostwcfParaLibrary;
        public frmWCFClientParameter()
        {
            InitializeComponent();
        }

        private void frmWCFClientParameter_Load(object sender, EventArgs e)
        {
            refreshData();
        }

        void refreshData()
        {
            HostwcfParaLibrary = Environment.EAPEnvironment.commonLibrary.baseLib.wcfParaLibrary;
            if (HostwcfParaLibrary == null) return;

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
                HostwcfParaLibrary.Client_Enable = chkClientEnable.Checked;
                HostwcfParaLibrary.RemoteIP = txtRemoteIP.Text;
                HostwcfParaLibrary.RemotePort = int.Parse(txtRemotePort.Text);
                HostwcfParaLibrary.AliveCheckInterval = int.Parse(txtAliveInterval.Text);
                HostwcfParaLibrary.RemoteUrlString = string.Format("net.tcp://{0}:{1}/", HostwcfParaLibrary.RemoteIP, HostwcfParaLibrary.RemotePort);
            }
            #endregion

            Environment.EAPEnvironment.commonLibrary.baseLib.UpdateWCFClientParaLibrary(HostwcfParaLibrary);

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
            txtRemoteIP.ReadOnly = Switch;
            txtRemotePort.ReadOnly = Switch;
            txtAliveInterval.ReadOnly = Switch;

            txtAliveInterval.ReadOnly = Switch;
            chkClientEnable.Enabled = !Switch;
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

        private void btnReopen_Click(object sender, EventArgs e)
        {
            EAPEnvironment.MESWCFClientStart();
            Environment.BaseComm.LogMsg(Core.Log.LogLevel.Warn, string.Format("MES WCF服务端重新开启."));
        }
    }
}
