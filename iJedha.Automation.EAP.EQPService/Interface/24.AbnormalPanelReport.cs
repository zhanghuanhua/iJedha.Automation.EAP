//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************


using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class AbnormalPanelReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.AbnormalPanelReport msg = new SocketMessageStructure.AbnormalPanelReport();
                if (new Serialize<SocketMessageStructure.AbnormalPanelReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "AbnormalPanelReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "AbnormalPanelReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion



                #region EAP做处理后，通知后面设备进行Job Data Modify Command，更新/删除任务信息
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                //根据设备上报板ID找批次信息
                LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByPanelID(msg.BODY.panel_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<AbnormalPanelReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #region 连线检查
                if (em.isCheckConnect && em.isCheckControlMode)
                {
                    if (em.ConnectMode == eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                        return;
                    }
                }
                #endregion
                #region Reply Message
                HostService.HostService.AbnormalPanelReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion

                if (lm==null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<AbnormalPanelReport> Find Lot Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                //根据不同的abnormal_code做不同的处理
                switch (msg.BODY.abnormal_code)
                {
                    case "1"://扣数
                        List<EquipmentModel> listEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo > em.EQNo).ToList();
                        if (listEm != null)
                        {
                            foreach (var v in listEm)
                            {
                                new HostService.HostService().JobDataModifyCommand(v.EQName, lm.LotID,lm.PanelTotalQty.ToString(), (lm.PanelTotalQty-int.Parse(msg.BODY.panel_count)).ToString(), eModifyType.Update);
                            }
                        }
                        LogMsg(Log.LogLevel.Info, string.Format($"设备<{em.EQName}>异常代码为<{msg.BODY.abnormal_code}>."));
                        break;
                    case "2"://通知投板机停止投板
                        foreach (var item in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r=>r.Type== eEquipmentType.L))
                        {
                            new HostService.HostService().RemoteControlCommand(item.EQID, "", eRemoteCommand.Stop.GetEnumDescription());
                        }
                        LogMsg(Log.LogLevel.Info, string.Format($"设备<{em.EQName}>异常代码为<{msg.BODY.abnormal_code}>."));
                        break;
                    case "3"://通知下一批次停止投板
                        Environment.EAPEnvironment.commonLibrary.StopNextLot = true;
                        LogMsg(Log.LogLevel.Info, string.Format($"设备<{em.EQName}>异常代码为<{msg.BODY.abnormal_code}>."));
                        break;
                    default:
                        break;
                }
                
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "AbnormalPanelReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
