using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
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
    public partial class frmWebAPIRequest : Form
    {
        public frmWebAPIRequest()
        {
            InitializeComponent();

        }

        /// <summary>
        /// EAP->MES 功能发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                EquipmentModel em;
                switch (this.cmbFuntion.Text)
                {
                    case "EAP_AliveCheckRequest":
                        new WebAPIReport().EAP_AliveCheckRequest(new MessageModel.AliveCheckRequest()
                        {
                            MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN,
                            IPAddress = Environment.EAPEnvironment.commonLibrary.baseLib.apiServerParaLibrary.LocalIP
                        }, 1);
                        break;
                    case "EAP_EqpModeRequest":
                        new WebAPIReport().EAP_EqpModeRequest(new MessageModel.EqpModeRequest()
                        {
                            MainEqpID = Environment.EAPEnvironment.commonLibrary.MDLN
                        }, 1);
                        break;
                    case "EAP_AlarmReport":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue1.Text);
                        if (em != null)
                        {
                            new WebAPIReport().EAP_AlarmReport(new MessageModel.AlarmReport()
                            {
                                SubEqpID = txtValue1.Text,
                                ErrorCode = txtValue2.Text,
                                ErrorReason = txtValue3.Text,
                                ErrorAction = txtValue4.Text,
                                ErrorLevel = txtValue5.Text,
                                Comments = txtValue6.Text,
                                EqpStatus = em.EQStatus.ToString()
                            }, 1);
                        }
                        break;
                    case "EAP_LotInfoRequest":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        string err;
                        if (em != null)
                        {
                            new WebAPIReport().EAP_LotInfoRequest(new MessageModel.LotInfoRequest()
                            {
                                MainEqpID = txtValue1.Text,
                                SubEqpID = txtValue2.Text,
                                PortID = txtValue3.Text,
                                CarrierID = txtValue4.Text,
                                PnlID = txtValue5.Text,
                                LotID = txtValue6.Text
                            }, em, 1,out err);
                        }
                        break;
                    case "EAP_LotInfoRequest_KL":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        if (em != null)
                        {
                            new WebAPIReport().EAP_LotInfoRequest_KL(new MessageModel.LotInfoRequest()
                            {
                                MainEqpID = txtValue1.Text,
                                SubEqpID = txtValue2.Text,
                                PortID = txtValue3.Text,
                                CarrierID = txtValue4.Text,
                                PnlID = txtValue5.Text,
                                LotID = txtValue6.Text
                            }, em, 1);
                        }
                        break;
                    case "EAP_LoadReuqest":
                        string errr = "";
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        if (em != null)
                        {
                            new WebAPIReport().EAP_LoadRequest(new MessageModel.LoadRequest()
                            {
                                MainEqpID = txtValue1.Text,
                                SubEqpID = txtValue2.Text,
                                PortID = txtValue3.Text,
                                RequestType = txtValue4.Text,
                                LoadedQty = txtValue5.Text
                            }, em, 1,out errr);
                        }
                        break;
                    case "EAP_LotTrackIn":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        if (em != null)
                        {
                            LotModel updateLot = EAPEnvironment.commonLibrary.commonModel.GetLotModelByInspectLotID(txtValue4.Text);
                            new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                            {
                                MainEqpID = txtValue1.Text,
                                SubEqpID = txtValue2.Text,
                                PortID = txtValue3.Text,
                                LotID = txtValue4.Text
                               
                            }, updateLot, 1,out err);
                        }
                        break;
                       
                    case "EAP_UnLoadRequest":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        if (em != null)
                        {
                            new WebAPIReport().EAP_UnLoadRequest(new MessageModel.UnLoadRequest()
                            {
                                MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = txtValue2.Text,
                                PortID = txtValue3.Text,
                                CarrierID = txtValue4.Text,
                                RequestType = txtValue5.Text,
                                LotID = txtValue6.Text
                            }, em, 1,out err);
                        }
                        break;
                    case "EAP_LotTrackOut":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        if (em != null)
                        {
                            new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                            {
                                MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                                SubEqpID = em.EQID,
                                PortID = txtValue3.Text,
                                LotID = txtValue4.Text,
                                PanelTotalQty = "",
                                NGFlag = false,//先记录false??
                                PanelList = new List<MessageModel.Panel>(),//回流线不需要给Panel List
                                WIPDataList = new List<MessageModel.WipData>(),//?
                                JobID = txtValue4.Text,
                                JobTotalQty = "",
                                PN = "",
                                WorkOrder = ""
                            }, new LotModel(), em, 1,out err);
                        }
                        break;
                    case "EAP_LoadComplete":
                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        if (em != null)
                        {
                            new WebAPIReport().EAP_LoadComplete(new MessageModel.LoadComplete()
                            {
                                MainEqpID = txtValue1.Text,
                                SubEqpID = txtValue2.Text,
                                PortID = txtValue3.Text,
                                CarrierID = txtValue4.Text
                            }, em, 1);
                        }
                        break;
                    case "EAP_EqpDefineInfo":
                        //new WebAPIReport().EAP_EqpDefineInfo(new MessageModel.EqpDeInfo()
                        //{
                        //    SubEqpID = txtValue1.Text,
                        //}, 1);
                        break;
                    case "EAP_LotHold":
                        new WebAPIReport().EAP_LotHold(new MessageModel.LotHoldInfo()
                        {
                            LotID = txtValue1.Text,
                            HoldReason = txtValue2.Text,
                            IsAll = bool.Parse(txtValue3.Text == "" ? "False" : "True")
                        }, 1);
                        break;
                    case "MaterialReadReport":

                        em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(txtValue2.Text);
                        new WebAPIReport().EAP_LotInfoRequest_KL(new MessageModel.LotInfoRequest()
                        {
                            MainEqpID = txtValue1.Text,
                            SubEqpID = txtValue2.Text,
                            PortID = txtValue3.Text,
                            CarrierID = txtValue4.Text,
                            PnlID = txtValue5.Text,
                            LotID = txtValue6.Text
                        }, em, 1);
                        break;

                    default:

                        break;
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 根据功能进行开放控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbFuntion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtValue1.Text = txtValue2.Text = txtValue3.Text = txtValue4.Text = txtValue5.Text = txtValue6.Text = txtValue7.Text = txtValue8.Text = "";
                lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = lblName7.Visible = lblName8.Visible = false;
                txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = txtValue7.Visible = txtValue8.Visible = false;
                switch (this.cmbFuntion.Text)
                {
                    case "EAP_AliveCheckRequest":
                        break;
                    case "EAP_AlarmReport":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = true;
                        lblName1.Text = "EqpID:";
                        lblName2.Text = "ErrorCode:";
                        lblName3.Text = "ErrorReason:";
                        lblName4.Text = "ErrorAction:";
                        lblName5.Text = "ErrorLevel:";
                        lblName6.Text = "Comments:";
                        break;
                    case "EAP_LotInfoRequest":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "CarrierID:";
                        lblName5.Text = "PnlID";
                        lblName6.Text = "LotID";
                        break;
                    case "EAP_LotInfoRequest_KL":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "CarrierID:";
                        lblName5.Text = "PnlID";
                        lblName6.Text = "LotID";
                        break;
                    case "EAP_LoadReuqest":

                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "RequestType:";
                        lblName5.Text = "LoadedQty:";
                        break;
                    case "EAP_LotTrackIn":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "LotID:";
                        break;
                    case "EAP_UnLoadRequest":
                      
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "CarrierID:";
                        lblName5.Text = "RequestType:";
                        lblName6.Text = "LotID";
                        break;
                  
                    case "EAP_LotTrackOut":

                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = lblName7.Visible = lblName8.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = txtValue7.Visible = txtValue8.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "LotID:";
                        lblName5.Text = "PanelTotalQty:";
                        lblName6.Text = "NGFlag";
                        lblName7.Text = "JobID";
                        lblName8.Text = "JobTotalQty";
                        break;
                    case "EAP_LoadComplete":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = true;
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName1.Text = "MainEqpID:";
                        lblName2.Text = "SubEqpID:";
                        lblName3.Text = "PortID:";
                        lblName4.Text = "CarrierID:";
                        break;
                    case "EAP_EqpDefineInfo":
                        lblName1.Visible = true;
                        txtValue1.Visible = true;
                        lblName1.Text = "SubEqpID:";
                        break;
                    case "EAP_LotHold":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = true;
                        lblName1.Text = "LotID:";
                        lblName2.Text = "HoldReason";
                        lblName3.Text = "IsAll";
                        txtValue3.Text = "True";
                        break;
                    case "API_ErrorMsg":
                        lblName1.Visible = lblName2.Visible = lblName3.Visible = lblName4.Visible = lblName5.Visible = lblName6.Visible = true;
                        txtValue1.Visible = txtValue2.Visible = txtValue3.Visible = txtValue4.Visible = txtValue5.Visible = txtValue6.Visible = true;
                        lblName1.Text = "EAP_ID:";
                        txtValue1.Text = EAPEnvironment.commonLibrary.MDLN;
                        lblName2.Text = "DATE_TIME:";
                        txtValue2.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ssffff");
                        lblName3.Text = "ERROR_DESC:";
                        lblName4.Text = "ERROR_TYPE:";
                        txtValue4.Text = "EAP";
                        lblName5.Text = "ERROR_CATEGORY:";
                        lblName6.Text = "ERROR_ROOTCAUSE:";
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
