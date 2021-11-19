using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    public partial class frmRemoteCommand : Form
    {
        Socket_DynamicLibraryBase dl;
        SortedDictionary<string, string> commandFunction;

        public frmRemoteCommand()
        {
            InitializeComponent();
        }

        private void frmRemoteCommand_Load(object sender, EventArgs e)
        {
            comboBoxEQList.Items.Clear();
            foreach (EquipmentModel em in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
            {
                comboBoxEQList.Items.Add(em.EQName);
            }
        }

        /// <summary>
        /// 选择设备，生成相应的功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxEQList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEQList.SelectedIndex == -1)
            {
                return;
            }
            comboBoxCommandNameList.Items.Clear();
            InitialStreamFunction();
            foreach (KeyValuePair<string, string> sf in commandFunction)
            {
                comboBoxCommandNameList.Items.Add(sf.Key);
            }
            string id = comboBoxEQList.SelectedItem.ToString();
            EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);
            dl = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
        }

        private void InitialStreamFunction()
        {
            commandFunction = new SortedDictionary<string, string>();
            commandFunction.Add("InitialDataRequest", "Initial Data Request");
            commandFunction.Add("DateTimeSyncCommand", "Date Time Sync Command");
            commandFunction.Add("ControlModeCommand", "Control Mode Command");
            commandFunction.Add("CIMMessageCommand", "CIM Message Command");
            commandFunction.Add("RemoteControlCommand", "Remote Control Command");
            commandFunction.Add("JobDataDownload", "Job Data Download");
            commandFunction.Add("JobDataModifyCommand", "Job Data Modify Command");
            commandFunction.Add("RGVDispatchCommand", "RGV Dispatch Command");
            commandFunction.Add("RecipeParameterRequest", "Recipe Parameter Request");
            commandFunction.Add("EquipmentJobDataRequest", "Equipment Job Data Request");
            commandFunction.Add("TraceDataRequest", "Trace Data Request");
            commandFunction.Add("OperatorLoginConfirm", "Operator Login Confirm");
            commandFunction.Add("AreYouThereRequest", "Are You There Request");


        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxEQList.SelectedIndex == -1 || comboBoxEQList.Text == string.Empty)
                {
                    MessageBox.Show("设备选择错误!", "Error");
                    return;
                }

                if (comboBoxCommandNameList.SelectedIndex == -1 || comboBoxCommandNameList.Text == string.Empty)
                {
                    MessageBox.Show("功能选择错误!", "Error");
                    return;
                }


                string id = comboBoxEQList.SelectedItem.ToString();
                string sf = comboBoxCommandNameList.SelectedItem.ToString();


                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByName(id);

                switch (sf)
                {
                    case "DateTimeSyncCommand":
                        new HostService.HostService().DateTimeSyncCommand(em.EQID);
                        break;
                    case "ControlModeCommand":
                        var controlmode = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "control_mode").FirstOrDefault();
                        new HostService.HostService().ControlModeCommand(em.EQName, controlmode.Text);
                        break;
                    case "CIMMessageCommand":
                        var interval = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "interval_second_time").FirstOrDefault();
                        var cimmessage = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "cim_message").FirstOrDefault();
                        new HostService.HostService().CIMMessageCommand(em.EQID, interval.Text, cimmessage.Text, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        break;
                    case "RemoteControlCommand":
                        var portid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "port_id").FirstOrDefault();
                        var remotecommand = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "remote_command").FirstOrDefault();
                        new HostService.HostService().RemoteControlCommand(em.EQID, portid.Text, remotecommand.Text);
                        break;
                    case "JobDataDownload":
                        var Downloadjobid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "job_id").FirstOrDefault();
                        var Downloadportid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "port_id").FirstOrDefault();
                        var Downloadrecipepath = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "recipe_path").FirstOrDefault();
                        var Downloadcampath = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "cam_path").FirstOrDefault();
                        LotModel Downloadlm = em.GetLotModelByLotID(Downloadjobid.Text);
                        //for (int i = 0; i < 5; i++)
                        //{
                        //    PanelModel panelM = new PanelModel();
                        //    panelM.PanelID = "P01" + i.ToString();
                        //    panelM.SequenceNo = i;
                        //    Downloadlm.PanelList.Add(panelM);
                        //}

                        PortModel Downloadpm = em.GetPortModelByPortID(Downloadportid.Text);
                        List<MessageModel.Param> lparaMod = new List<MessageModel.Param>();

                        for (int i = 0; i < 5; i++)
                        {
                            MessageModel.Param paraMode = new MessageModel.Param();
                            paraMode.ParamName = "ITEM" + i.ToString();
                            paraMode.ParamValue = "1000" + i.ToString();
                            lparaMod.Add(paraMode);
                        }
                        new HostService.HostService().JobDataDownload(em, Downloadlm, Downloadlm.LocalPortStation, lparaMod);
                        break;
                    case "JobDataModifyCommand":
                        var Modifyeqpid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "eqp_id").FirstOrDefault();
                        var Modifyjobid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "job_id").FirstOrDefault();
                        var ModifyoldCount = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "old_panel_count").FirstOrDefault();
                        var ModifynewCount = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "new_panel_count").FirstOrDefault();
                        new HostService.HostService().JobDataModifyCommand(em.EQName, Modifyjobid.Text, ModifyoldCount.Text, ModifynewCount.Text, eModifyType.Update);
                        break;
                    case "RGVDispatchCommand":
                        var jobid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "job_id").FirstOrDefault();
                        var partid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "part_id").FirstOrDefault();
                        var toid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "to_id").FirstOrDefault();
                        new HostService.HostService().RGVDispatchCommand(em.EQID, jobid.Text, partid.Text, toid.Text);
                        break;
                    case "InitialDataRequest":
                        new HostService.HostService().InitialDataRequest(em.EQID);
                        break;
                    case "RecipeParameterRequest":
                        var recipeid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "recipe_id").FirstOrDefault();
                        new HostService.HostService().RecipeParameterRequest(em.EQID, recipeid.Text);
                        break;
                    case "TraceDataRequest":
                        new HostService.HostService().TraceDataRequest(em.EQID, em.SUBEQID);
                        break;
                    case "EquipmentJobDataRequest":
                        new HostService.HostService().EquipmentJobDataRequest(em.EQID);
                        break;
                    case "OperatorLoginConfirm":
                        var operatorid = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "operator_id").FirstOrDefault();
                        var result = panelPara.Controls.OfType<TextBox>().Where(r => r.Name == "result").FirstOrDefault();
                        new HostService.HostService().OperatorLoginConfirm(operatorid.Text, DateTime.Now.ToString("yyyyMMddHHmmssfff"), em.EQID, result.Text);
                        break;
                    case "AreYouThereRequest":
                        new HostService.HostService().AreYouThereRequest(DateTime.Now.ToString("yyyyMMddHHmmssfff"), em.EQName);
                        break;
                    default:

                        break;
                }
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// 选择功能，生成相应的命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxCommandNameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCommandNameList.SelectedIndex == -1)
            {
                return;
            }
            string id = comboBoxCommandNameList.SelectedItem.ToString();
            InitialCPNameForm(id);

        }

        private void frmRemoteCommand_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }



        public void InitialCPNameForm(string id)
        {
            PropertyInfo[] lotInfo = null;

            try
            {
                if (comboBoxCommandNameList.SelectedIndex == -1)
                {
                    return;
                }
                #region 获取类中字段
                switch (id)
                {
                    case "InitialDataRequest":
                        lotInfo = typeof(iJedha.SocketMessageStructure.InitialDataRequest.TrxBody).GetProperties();
                        break;
                    case "DateTimeSyncCommand":
                        lotInfo = typeof(iJedha.SocketMessageStructure.DateTimeSyncCommand.TrxBody).GetProperties();
                        break;
                    case "ControlModeCommand":
                        lotInfo = typeof(iJedha.SocketMessageStructure.ControlModeCommand.TrxBody).GetProperties();
                        break;
                    case "CIMMessageCommand":
                        lotInfo = typeof(iJedha.SocketMessageStructure.CIMMessageCommand.TrxBody).GetProperties();
                        break;
                    case "RemoteControlCommand":
                        lotInfo = typeof(iJedha.SocketMessageStructure.RemoteControlCommand.TrxBody).GetProperties();
                        break;
                    case "JobDataDownload":
                        lotInfo = typeof(iJedha.SocketMessageStructure.JobDataDownload.TrxBody).GetProperties();
                        break;
                    case "JobDataModifyCommand":
                        lotInfo = typeof(iJedha.SocketMessageStructure.JobDataModifyCommand.TrxBody).GetProperties();
                        break;
                    case "RGVDispatchCommand":
                        lotInfo = typeof(iJedha.SocketMessageStructure.RGVDispatchCommand.TrxBody).GetProperties();
                        break;
                    case "RecipeParameterRequest":
                        lotInfo = typeof(iJedha.SocketMessageStructure.RecipeParameterRequest.TrxBody).GetProperties();
                        break;
                    case "EquipmentJobDataRequest":
                        lotInfo = typeof(iJedha.SocketMessageStructure.EquipmentJobDataRequest.TrxBody).GetProperties();
                        break;
                    case "TraceDataRequest":
                        lotInfo = typeof(iJedha.SocketMessageStructure.TraceDataRequest.TrxBody).GetProperties();
                        break;
                    case "OperatorLoginConfirm":
                        lotInfo = typeof(iJedha.SocketMessageStructure.OperatorLoginConfirm.TrxBody).GetProperties();
                        break;
                    case "AreYouThereRequest":
                        lotInfo = typeof(iJedha.SocketMessageStructure.AreYouThereRequest.TrxBody).GetProperties();
                        break;
                    default:
                        break;
                }
                #endregion
                panelPara.Controls.Clear();
                int i = 1;
                foreach (var _r in lotInfo)
                {
                    #region [Label]
                    Label lbl1 = new Label();
                    lbl1.Font = new System.Drawing.Font("微软雅黑", 9, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    lbl1.Size = new System.Drawing.Size(130, 20);
                    lbl1.TextAlign = ContentAlignment.MiddleLeft;
                    lbl1.Top = (lbl1.Height + 13) * i;
                    lbl1.Left = 20;
                    lbl1.Text = _r.Name;
                    #endregion
                    #region [Add]
                    panelPara.Controls.Add(lbl1);
                    #endregion
                    #region [Text]
                    TextBox _txt = new TextBox();
                    _txt.Font = new System.Drawing.Font("微软雅黑", 9, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    _txt.Size = new System.Drawing.Size(150, 20);
                    _txt.Top = (_txt.Height + 10) * i;
                    _txt.Left = 150;
                    _txt.Tag = _r.Name;
                    _txt.Name = _r.Name;

                    #endregion
                    #region [Add]
                    panelPara.Controls.Add(_txt);
                    #endregion
                    #region [Label]
                    Label lbl2 = new Label();
                    lbl2.Font = new System.Drawing.Font("微软雅黑", 9, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    lbl2.Size = new System.Drawing.Size(50, 20);
                    lbl2.TextAlign = ContentAlignment.MiddleLeft;
                    lbl2.Top = (lbl2.Height + 13) * i;
                    lbl2.Left = 210;
                    lbl2.Text = _r.Name;
                    #endregion
                    #region [Add]
                    panelPara.Controls.Add(lbl2);
                    #endregion
                    i++;
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
