using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmFSParameter : Form
    {
        FSParaLibraryBase fsParaLibrary;
        public frmFSParameter()
        {
            InitializeComponent();
        }

        private void frmFSParameter_Load(object sender, EventArgs e)
        {
            refreshData();
        }

        void refreshData()
        {
            fsParaLibrary = Environment.EAPEnvironment.commonLibrary.baseLib.fsParaLibrary;
            if (fsParaLibrary == null) return;
            chkEnable.Checked = fsParaLibrary.FS_Enable;
            txtServerIP.Text = fsParaLibrary.FS_ServerIP;
            txtSharePath.Text = fsParaLibrary.FS_SharePath;
            txtUserID.Text = fsParaLibrary.FS_UserID;
            txtPassword.Text = fsParaLibrary.FS_Password;
            txtLocalPath.Text = fsParaLibrary.FS_LocalPath;
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
            if (fsParaLibrary == null) return;

            #region 先將設定填入变量
            lock (fsParaLibrary)
            {
                fsParaLibrary.FS_Enable = chkEnable.Checked; ;
                fsParaLibrary.FS_ServerIP = txtServerIP.Text;
                fsParaLibrary.FS_SharePath = txtSharePath.Text;
                fsParaLibrary.FS_UserID = txtUserID.Text;
                fsParaLibrary.FS_Password = txtPassword.Text;
                fsParaLibrary.FS_LocalPath = txtLocalPath.Text;
            }
            #endregion

            Environment.EAPEnvironment.commonLibrary.baseLib.UpdateFSParaLibrary(fsParaLibrary);

            EnableControls(true);
            btnEdit.Enabled = true;
            btnCancel.Enabled = false;
            btnSave.Enabled = false;

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
            txtLocalPath.ReadOnly = Switch;
            txtPassword.ReadOnly = Switch;
            txtServerIP.ReadOnly = Switch;
            txtUserID.ReadOnly = Switch;
            txtSharePath.ReadOnly = Switch;
            chkEnable.Enabled = !Switch;
        }

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
                if (KeyIn == (char)8) return KeyIn;
            }
            return (char)0;
        }

        private void frmFSParameter_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
