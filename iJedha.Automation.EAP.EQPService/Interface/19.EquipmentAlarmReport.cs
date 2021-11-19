//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.LibraryBase;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static iJedha.Automation.AMS.Service.AMSService;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentAlarmReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentAlarmReport msg = new SocketMessageStructure.EquipmentAlarmReport();
                if (new Serialize<SocketMessageStructure.EquipmentAlarmReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentAlarmReport", evtXml));
                    return;
                }
                #endregion

                //根据设备No获取设备信息
                //EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByNo(1);
                //根据设备ID获取设备模型
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(msg.BODY.eqp_id);

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentAlarmReport", msg.WriteToXml(), em.EQID));
                #endregion

                #region Reply Message
                HostService.HostService.EquipmentAlarmReportReply(msg.HEADER.TRANSACTIONID, em.EQID);
                #endregion

                #region EAP调用MES接口把设备警报上报给MES
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentAlarmReport> Find Error", em.EQID.Trim()));
                    return;
                }

                #region 连线检查
                if (em.isCheckConnect && em.isCheckControlMode)
                {
                    if (em.ConnectMode == Model.eConnectMode.DISCONNECT || em.ControlMode != eControlMode.REMOTE)
                    {
                        BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                        return;
                    }
                }
                #endregion
                //使用设备ID获取动态库数据
                Socket_DynamicLibraryBase d1 = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetDynamicLibrary(em.EQID);
                if (d1 == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentAlarmReport> Find DynamicLibraryBase Error", em.EQID.Trim()));
                    return;
                }

                //遍历设备上报的警报列表
                foreach (var item in msg.BODY.alarm_list)
                {
                    Socket_AlarmModelBase getAlarm = d1.GetAlarm(item.alarm_code);
                    if (getAlarm == null)
                    {
                        //error log
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentAlarmReport> Find Alarm Error", em.EQID.Trim()));
                        return;
                    }
                    //上报MES
                    //new WebAPIReport().EAP_AlarmReport(new MessageModel.AlarmReport()
                    //{
                    //    SubEqpID = em.EQID,
                    //    ErrorCode = item.alarm_code,
                    //    ErrorReason = getAlarm.AlarmChineseText,
                    //    ErrorAction = msg.BODY.report_type,
                    //    ErrorLevel = msg.BODY.alarm_type.ToUpper() == "S" ? "重大" : "一般",
                    //    Comments = getAlarm.Alias,
                    //    EqpStatus = em.EQStatus.ToString(),
                    //    ErrorDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    //}, 1);
                    #endregion

                    //#region To GUI
                    //EAPEnvironment.MQPublisherAp.MQ_EquipmentAlarm(em.EQName, msg.BODY.alarm_type.ToUpper() == "S" ? "重大" : "一般", item.alarm_code, getAlarm.AlarmChineseText, msg.BODY.report_type);
                    //#endregion

                
                    #region To History
                    History.EAP_EQP_ALM(em, item.alarm_code, getAlarm.AlarmChineseText, msg.BODY.report_type, msg.BODY.alarm_type.ToUpper() == "S" ? "重大" : "一般");
                    #endregion



                    //#region To AMS
                    //string aa = string.Empty;
                    //EAPEnvironment.MQPublisherAp.MQ_EquipmentStatus(Environment.EAPEnvironment.commonLibrary.MQ_EquipmentStatus());
                    //Environment.EAPEnvironment.AMSServiceAp.AlarmSendWithParam("黄石广合一厂", "EAP", em.EQID, item.alarm_code, item.alarm_code, getAlarm.AlarmChineseText,
                    //    msg.BODY.alarm_type.ToUpper() == "S" ? "重大" : "一般", "", new List<cInfoParam>(), new List<cInfoParam>(), out aa);
                    //#endregion

                    //#region 针对不同的警报代码和别名，EAP做不同的处理
                    //switch (getAlarm.Alias)
                    //{
                    //    case "STOPLOAD"://停止投板机投板
                    //        foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.Type == eEquipmentType.L))
                    //        {
                    //            new HostService.HostService().RemoteControlCommand(v.EQID, "", eRemoteCommand.Stop.GetEnumDescription());
                    //        }
                    //        break;
                    //    case "STOPLOT"://停止下个Lot投板
                    //        Environment.EAPEnvironment.commonLibrary.StopNextLot = true;
                    //        break;
                    //    case "DEDUCT"://扣数
                    //LotModel lm = EAPEnvironment.commonLibrary.commonModel.GetLotModelByLotID(em.CurrentLotID.Trim());
                    //if (em == null)
                    //{
                    //    //error log
                    //    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentAlarmReport> Find Error", em.EQID.Trim()));
                    //    return;
                    //}
                    //if (lm == null)
                    //{
                    //    //error log
                    //    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentAlarmReport> Find Lot Error", em.EQID.Trim()));
                    //    return;
                    //}

                    //List<EquipmentModel> listEm = EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.EQNo > em.EQNo).ToList();
                    //if (listEm != null)
                    //{
                    //    foreach (var v in listEm)
                    //    {
                    //        new HostService.HostService().JobDataModifyCommand(v.EQName, lm.LotID, lm.PanelTotalQty.ToString(), (lm.PanelTotalQty - int.Parse(msg.BODY.panel_count)).ToString(), eModifyType.Update);
                    //    }
                    //}
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    #endregion

                    //}
                    //#region[警报发生，添加EquipmentAlarmList；警报解除，减少EquipmentAlarmList]
                    //switch (msg.BODY.report_type)
                    //{
                    //    case "0":
                    //        foreach (var item in msg.BODY.alarm_list)
                    //        {
                    //            em.EquipmentAlarmList.Remove(item.alarm_code);
                    //        }
                    //        break;
                    //    case "1":
                    //        foreach (var item in msg.BODY.alarm_list)
                    //        {
                    //            em.EquipmentAlarmList.Add(item.alarm_code);
                    //            #region [单机超过20笔警报，清除之前上报的]
                    //            if (em.EquipmentAlarmList.Count > 20)
                    //            {
                    //                em.EquipmentAlarmList.RemoveAt(0);
                    //            }
                    //            #endregion
                    //        }
                    //        break;
                    //    default:
                    //        break;
                }
                        //#endregion

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentAlarmReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
