using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
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
    public partial class frmTraceData : Form
    {
        Socket_DynamicLibraryBase dl;
        public frmTraceData()
        {
            InitializeComponent();
        }
        private void frmTraceData_Load(object sender, EventArgs e)
        {
            comboBoxEQList.Items.Clear();
            foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
            {
                comboBoxEQList.Items.Add(em.EQName);
            }
        }
        private void comboBoxEQList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void RefreshLocalVariable(string id, int trid)
        {
            ListViewVariable.Items.Clear();
            if (dl == null)
            {
                return;
            }
            int j = 0;
            foreach (Socket_TraceDataModelBase vm in dl.GetAllTraceDataModel(trid))
            {
                ListViewItem item = new ListViewItem();
                item = new ListViewItem(new string[] { vm.Sequence.ToString(), vm.ID, vm.Name, vm.DefaultUnit, vm.MaxValue, vm.MinValue, vm.Rule, vm.Value });
                if (j % 2 == 0)
                {
                    item.BackColor = Color.Azure;
                }
                ListViewVariable.Items.Add(item);
                j++;
            }
        }
        private void RefreshRemoteVariable(EquipmentModel em, int trid)
        {
            //ListViewVariable.Items.Clear();
            //int j = 0;
            //foreach (TraceDataModelBase vm in em.GetAllDefineTraceData(trid))
            //{
            //    ListViewItem item = new ListViewItem();
            //    item = new ListViewItem(new string[] { j.ToString(), vm.ID, vm.Name, vm.DefaultUnit, vm.MaxValue, vm.MinValue, vm.Rule, vm.Value });
            //    if (j % 2 == 0)
            //    {
            //        item.BackColor = Color.Azure;
            //    }
            //    ListViewVariable.Items.Add(item);
            //    j++;
            //}
        }
        private void RefreshKeyVariable(EquipmentModel em)
        {
            ListViewVariable.Items.Clear();
            int j = 0;
            foreach (WIPDataModel vm in em.List_KeyTraceDataSpec)
            {
                ListViewItem item = new ListViewItem();
                item = new ListViewItem(new string[] { j.ToString(), vm.ItemID, vm.WIPDataName, vm.DataType, vm.ItemMaxValue, vm.ItemMinValue, "", vm.DefaultValue });
                if (j % 2 == 0)
                {
                    item.BackColor = Color.Azure;
                }
                ListViewVariable.Items.Add(item);
                j++;
            }
        }
        private void frmTraceData_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = CheckValid(e.KeyChar, "0123456789");
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
        private void btnView_Click(object sender, EventArgs e)
        {
            if (comboBoxEQList.SelectedIndex == -1)
            {
                return;
            }
            if (cmbTraceGroup.Text == string.Empty)
            {
                cmbTraceGroup.Text = "0";
            }
            string id = comboBoxEQList.SelectedItem.ToString();
            EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);
            //if (radioButtonLocal.Checked)
            //{
                dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                RefreshLocalVariable(id, int.Parse(cmbTraceGroup.Text));
            //}
            if (radioButtonRemote.Checked)
            {
                //RefreshRemoteVariable(em,int.Parse(cmbTraceGroup.Text));
            }
            if (radioButtonKey.Checked)
            {
                RefreshKeyVariable(em);
            }
            toolStripStatusLabelCount.Text = ListViewVariable.Items.Count.ToString();
        }

        private void cmbTraceGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblInfo.Text = "";
            if (comboBoxEQList.SelectedIndex == -1)
            {
                return;
            }
            var v = Environment.EAPEnvironment.commonLibrary.baseLib.traceDataGroupLibrary.GetTraceDataGroup(comboBoxEQList.SelectedItem.ToString(), int.Parse(cmbTraceGroup.Text));
            if (v != null)
            {
                lblInfo.Text = "频率：" + v.SampleTimer;
            }
        }
    }
}
