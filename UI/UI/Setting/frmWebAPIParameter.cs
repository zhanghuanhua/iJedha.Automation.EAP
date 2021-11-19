using System;
using System.Net;
using System.Windows.Forms;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.LibraryBase;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmWebAPIParameter : Form
    {
        APIServerParaLibraryBase apiServerLib;
        APIClientParaLibraryBase apiClientLib;
        /// <summary>
        /// Http service.
        /// </summary>
        public frmWebAPIParameter()
        {
            InitializeComponent();
            
        }

        private void frmWebAPIParameter_Load(object sender, EventArgs e)
        {
            refreshData();
        }
        /// <summary>
        /// 获取"webAPIParaLibrary",更新控件的值
        /// </summary>
        void refreshData()
        {
            if (EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary == null)
            {
                return;
            }
            chkClientEnable.Checked = EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable;
            txtRemoteIP.Text = EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteIP;
            txtRemotePort.Text = EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemotePort.ToString();
            txtAliveInterval.Text = EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.AliveCheckInterval.ToString();
            txtRemoteUri.Text = EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteUrlString;

            if (EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary == null)
            {
                return;
            }
            chkServerEnable.Checked = EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary.Server_Enable;
            txtLocalIP.Text = EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary.LocalIP;
            txtLocalPort.Text = EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary.LocalPort.ToString();
            txtLocalUri.Text = EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary.LocalUrlString;
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
            if (EAPEnvironment.WebAPIServerAp == null)
            {
                return;
            }


            #region [先將設定填入变量]
            lock (EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary)
            {
                EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.Client_Enable = chkClientEnable.Checked;
                EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteIP = txtRemoteIP.Text;
                EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemotePort = int.Parse(txtRemotePort.Text);
                EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.AliveCheckInterval = int.Parse(txtAliveInterval.Text);
                EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteUrlString = string.Format("http://{0}:{1}/", EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemoteIP, EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary.RemotePort);
            }
            Environment.EAPEnvironment.commonLibrary.baseLib.UpdateAPIClientParaLibrary(EAPEnvironment.commonLibrary.baseLib.apiClientParaLibrary);

            //lock (EAPEnvironment.WebAPIServerAp)
            //{
            //    EAPEnvironment.WebAPIServerAp.IP = chkServerEnable.Checked;
            //    EAPEnvironment.WebAPIServerAp.Port = txtLocalIP.Text;
            //    EAPEnvironment.WebAPIServerAp. = int.Parse(txtLocalPort.Text);
            //    EAPEnvironment.WebAPIServerAp.LocalUrlString = txtLocalUri.Text;
            //    Environment.EAPEnvironment.commonLibrary.configLibrary.AMSUrl.UpdateAPIServerParaLibrary(apiServerLib);
            //}
            #endregion
            
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
            chkClientEnable.Enabled = !Switch;

            txtLocalIP.ReadOnly = Switch;
            txtLocalPort.ReadOnly = Switch;
            chkServerEnable.Enabled = !Switch;
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

        private void frmWebAPIParameter_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
