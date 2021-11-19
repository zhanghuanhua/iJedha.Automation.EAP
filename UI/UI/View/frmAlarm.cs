using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmAlarm : Form
    {
        Socket_DynamicLibraryBase d1;

        public frmAlarm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 获取机台名,清空下拉框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAlarm_Load(object sender, EventArgs e)
        {

            comboBoxEQList.Items.Clear();
            foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
            {
                comboBoxEQList.Items.Add(em.EQName);
            }
        }
        //显示全部的Alarm和指定Alarm查找
        void RefreshVariable()
        {
            try
            {
                ListViewData.Items.Clear();
                int j = 0;
                if (txtAlarmCode.Text == "")
                {
                    // 如果alarm Code显示为空 则显示全部
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(comboBoxEQList.SelectedItem.ToString());
                    d1 = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                    if (em == null)
                    {
                        return;
                    }
                    foreach (Socket_AlarmModelBase am in d1.GetAllAlarmModel())
                    {
                        switch (am.AlarmType)
                        {
                            case "L":
                                am.AlarmType = "一般";
                                break;
                            case "S":
                                am.AlarmType = "重大";
                                break;
                            default:
                                break;
                        }
                        ListViewItem item = new ListViewItem();
                        item = new ListViewItem(new string[] { am.Sequence.ToString(), am.ID, am.AlarmChineseText, am.AlarmEnglishText, am.AlarmType });
                        if (j % 2 == 0)
                        {
                            item.BackColor = Color.Azure;
                        }
                        ListViewData.Items.Add(item);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty( comboBoxEQList.SelectedItem.ToString()))
                    {
                        return;
                    }
                    //Alarm Code 不为空 则显示当前的Alarm ID
                    EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(comboBoxEQList.SelectedItem.ToString());
                    d1 = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                    if (em == null)
                    {
                        return;
                    }
                    foreach (Socket_AlarmModelBase am in d1.GetAllAlarmModel())
                    {
                        //判断ID是否一致
                        if (am.ID == txtAlarmCode.Text)
                        {
                            ListViewItem item = new ListViewItem();
                            item = new ListViewItem(new string[] { am.Sequence.ToString(), am.ID, am.AlarmChineseText, am.AlarmEnglishText, am.AlarmType });
                            if (j % 2 == 0)
                            {
                                item.BackColor = Color.Azure;
                            }
                            ListViewData.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            
        }
        private void btnBodyView_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshVariable();
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(comboBoxEQList.SelectedItem.ToString());
                if (txtAlarmCode.ToString() == "")
                {
                    //new HostService.HostService().S5F5(em, txtAlarmCode.ToString());
                }
                else
                {
                    //new HostService.HostService().S5F7(em);
                }
            }
            catch (Exception ex)
            {
                
            }
           
        }
    }
}
