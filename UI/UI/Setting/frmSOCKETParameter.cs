using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmSOCKETParameter : Form
    {
        SocketParaLibraryBase socketParaLibrary;
        public frmSOCKETParameter()
        {
            InitializeComponent();
        }

        private void frmHSMSParameter_Load(object sender, EventArgs e)
        {
            comboBoxEQList.Items.Clear();
            foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
            {
                comboBoxEQList.Items.Add(em.EQName);
            }

            cboConnectMode.Items.Clear();
            cboConnectMode.Items.Add("PASSIVE");
            cboConnectMode.Items.Add("ACTIVE");
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            btnEdit.Enabled = false;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnReopen.Enabled = false;
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
            if (socketParaLibrary == null)
            {
                return;
            }
            string id = comboBoxEQList.SelectedItem.ToString();
            EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);
            #region [先將設定填入變數裡]
            lock (socketParaLibrary)
            {
                socketParaLibrary.ConnectType = (eConnect_Mode)cboConnectMode.SelectedIndex;
                socketParaLibrary.PatternName = txtPath.Text.Trim();
                socketParaLibrary.RemotePort = uint.Parse(txtRemotePort.Text);
                socketParaLibrary.LocalIP = txtLocalIP.Text;
                socketParaLibrary.RemoteIP = txtRemoteIP.Text;
                socketParaLibrary.LocalPort = uint.Parse(txtLocalPort.Text);
                socketParaLibrary.Timeout = int.Parse(txtTime.Text);
                socketParaLibrary.Encoding = cboEncodeType.SelectedIndex.ToString();
                socketParaLibrary.PatternName = txtPath.Text;
            }
            #endregion
            socketParaLibrary.UpdateSocketParaLibrary(em.EQNo.ToString(), socketParaLibrary);
            EnableControls(true);
            btnEdit.Enabled = true;
            btnCancel.Enabled = false;
            btnSave.Enabled = false;
            btnReopen.Enabled = true;

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (socketParaLibrary == null)
            {
                return;
            }
            cboConnectMode.SelectedIndex = (int)socketParaLibrary.ConnectType;
            txtPath.Text = socketParaLibrary.PatternName;
            txtLocalPort.Text = socketParaLibrary.LocalPort.ToString();
            txtRemoteIP.Text = socketParaLibrary.RemoteIP.ToString();
            txtLocalIP.Text = socketParaLibrary.LocalIP.ToString();
            txtRemotePort.Text = socketParaLibrary.RemotePort.ToString();
            txtTime.Text = socketParaLibrary.Timeout.ToString();

            EnableControls(true);
            btnEdit.Enabled = true;
            btnCancel.Enabled = false;
            btnSave.Enabled = false;
            btnReopen.Enabled = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EnableControls(bool Switch)
        {
            cboConnectMode.Enabled = !Switch;
            cboEncodeType.Enabled = !Switch;
            txtRemotePort.ReadOnly = Switch;
            txtLocalIP.ReadOnly = Switch;
            txtRemoteIP.ReadOnly = Switch;
            txtLocalPort.ReadOnly = Switch;
            txtTime.ReadOnly = Switch;
        }

        /// <summary>
        /// 检查值是否符合要求
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            try
            {
                uint tmp;
                IPAddress ip;


                if (!uint.TryParse(txtRemotePort.Text, out tmp))
                {
                    MessageBox.Show("Port Format Error!");
                    return false;
                }

                if (!IPAddress.TryParse(txtLocalIP.Text, out ip))
                {
                    MessageBox.Show("IP Format Error!");
                    return false;
                }

                if (!IPAddress.TryParse(txtRemoteIP.Text, out ip))
                {
                    MessageBox.Show("IP Format Error!");
                    return false;
                }

                if (!uint.TryParse(txtTime.Text, out tmp))
                {
                    MessageBox.Show("Time Error!");
                    return false;
                }



                return true;
            }
            catch
            {
                return false;
            }
        }

        private void txtLocalPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = CheckValid(e.KeyChar, "0123456789");
        }

        private void txtIPAddress_KeyPress(object sender, KeyPressEventArgs e)
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

        private void btnDefault_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 获取对应设备的"rbmqParaLibrary",更新控件的值
        /// </summary>
        private void comboBoxEQList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEQList.SelectedIndex == -1)
            {
                return;
            }
            string id = comboBoxEQList.SelectedItem.ToString();
            EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);
            socketParaLibrary = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetSocketParaLibrary(em.EQID);
            txtPath.Text = socketParaLibrary.PatternName;
            if (socketParaLibrary.Enable)
            {
                pictureBoxEnable.Image = Properties.Resources.BitOn;
            }
            else
            {
                pictureBoxEnable.Image = Properties.Resources.BitOff;
            }
            cboConnectMode.SelectedIndex = (int)socketParaLibrary.ConnectType;
            txtPath.Text = socketParaLibrary.PatternName;
            txtLocalPort.Text = socketParaLibrary.LocalPort.ToString();
            txtRemoteIP.Text = socketParaLibrary.RemoteIP.ToString();
            txtLocalIP.Text = socketParaLibrary.LocalIP.ToString();
            txtRemotePort.Text = socketParaLibrary.RemotePort.ToString();
            txtTime.Text = socketParaLibrary.Timeout.ToString();
            cboEncodeType.SelectedIndex = int.Parse(socketParaLibrary.Encoding);

            //getNetStatus();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void frmHSMSParameter_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 重新连接设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReopen_Click(object sender, EventArgs e)
        {
            if (comboBoxEQList.SelectedIndex == -1)
            {
                return;
            }
            string id = comboBoxEQList.SelectedItem.ToString();
            EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);
            socket.SocketBasic socketBrick = Environment.EAPEnvironment.Dic_TCPSocketAp[em.EQID];
            socketBrick.Close();

        }

        private void cboDumpHex_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (txtPath.Text.Trim() == string.Empty)
                {
                    return;
                }
                System.Diagnostics.Process.Start("explorer.exe", txtPath.Text.Trim());
            }
            catch (Exception ex)
            {

            }
        }

        private void BtnNetStatusReflash_Click(object sender, EventArgs e)
        {
            try
            {
                //getNetStatus();
            }
            catch (Exception ex)
            {

            }

        }
        public void getNetStatus()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "netstat.exe";
                p.StartInfo.Arguments = "-a";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.Start();
                string result = p.StandardOutput.ReadToEnd();
                string[] strList = Regex.Split(result, "TCP", RegexOptions.IgnoreCase);
                foreach (var item in strList)
                {
                    if (!item.Trim().Contains(string.Format("{0}:{1}", txtLocalIP.Text, txtLocalPort.Text)))
                    {
                        continue;
                    }
                    else
                    {
                        string str = item.Trim();
                        if (str.Contains("LISTENING"))
                        {
                            lblNetStatus.Text = "LISTENING";
                        }
                        else if (str.Contains("ESTABLISHED"))
                        {
                            lblNetStatus.Text = "ESTABLISHED";
                        }
                        else if (str.Contains("TIME_WAIT"))
                        {
                            lblNetStatus.Text = "TIME_WAIT";
                        }
                        else
                        {
                            lblNetStatus.Text = string.Empty;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
