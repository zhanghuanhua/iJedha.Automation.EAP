using System;
using System.Windows.Forms;

namespace iJedha.Automation.Tool.SocketSimulator
{
    public partial class SocketSettingForm : Form
	{
		private SocketConfigManager _configSManager = null;
        private bool _canModify = false;

        public SocketSettingForm(SocketConfigManager cfgManager, bool canModify)
		{
            _configSManager = cfgManager;
            _canModify = canModify;
			InitializeComponent();
		}

		private void SocketSettingForm_Load(object sender, EventArgs e)
		{
			try
			{
				txtIP.Text = _configSManager.IP;
                txtPort.Text = _configSManager.Port.ToString();
                cboEncoding.Text = _configSManager.Encoding == "1" ? "Unicode" : "Ascii";
                cboConnectType.Text = _configSManager.ConnectType == 1 ? "Client" : "Server";
                txtT3.Text = _configSManager.Timeout.ToString();
                chkHeader.Checked = _configSManager.isHaveHeader;
                chkBody.Checked = _configSManager.isHaveBody;
                chkReturn.Checked = _configSManager.isHaveReturn;
                txtHeader.Text = Global.FormatXMLString(_configSManager.HeaderXml);
                btnOK.Enabled = _canModify;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
            //if (txtHeader.Text == string.Empty)
            //{
            //    txtHeader.Focus();
            //    return;
            //}

            if (txtIP.Text == string.Empty)
			{
				txtIP.Focus();
				return;
			}

			if (txtPort.Text == string.Empty)
			{
				txtPort.Focus();
				return;
			}

            if (cboConnectType.Text == string.Empty)
            {
                cboConnectType.Focus();
                return;
            }

            if (cboEncoding.Text == string.Empty)
            {
                cboEncoding.Focus();
                return;
            }

            int t3 = 0;
			if (!int.TryParse(txtT3.Text, out t3))
			{
				txtT3.Focus();
				return;
			}

            _configSManager.IP = txtIP.Text;
            _configSManager.Port = uint.Parse(txtPort.Text);
            _configSManager.Encoding = cboEncoding.Text == "Unicode" ? "1" : "0";
            _configSManager.ConnectType = cboConnectType.Text == "Client" ? 1 : 0;
            _configSManager.Timeout = t3;
            _configSManager.HeaderXml = txtHeader.Text;
            _configSManager.isHaveHeader = chkHeader.Checked;
            _configSManager.isHaveBody = chkBody.Checked;
            _configSManager.isHaveReturn = chkReturn.Checked;

            DialogResult = DialogResult.OK;
			Close();
		}
	}
}
