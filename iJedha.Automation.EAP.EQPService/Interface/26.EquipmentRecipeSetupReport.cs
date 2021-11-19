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
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentRecipeSetupReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentRecipeSetupReport msg = new SocketMessageStructure.EquipmentRecipeSetupReport();
                if (new Serialize<SocketMessageStructure.EquipmentRecipeSetupReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentRecipeSetupReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentRecipeSetupReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region 上报Setup状态之后EAP做处理
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentRecipeSetupReport> Find Error", msg.BODY.eqp_id.Trim()));
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
                HostService.HostService.EquipmentRecipeSetupReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
                //根据不同的上报内容，做不同的处理
                switch (msg.BODY.setup_result)
                {
                    case "0"://ng
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}> Recipe Setup NG", msg.BODY.eqp_id.Trim()));
                        em.JobDataDownloadChangeResult = eCheckResult.ng;
                        break;
                    case "1"://ok
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> Recipe Setup OK", msg.BODY.eqp_id.Trim()));
                        em.JobDataDownloadChangeResult = eCheckResult.ok;

                        em.OldEQParameter.Clear();
                        foreach (var item in em.NewEQParameter)
                        {
                            em.OldEQParameter.Add(item);
                        }
                        break;
                    case "3"://配方不存在
                        BaseComm.LogMsg(Log.LogLevel.Warn, $"设备<{msg.BODY.eqp_id.Trim()}> 不存在[{msg.BODY.process_id.Trim()}]配方.");
                        em.JobDataDownloadChangeResult = eCheckResult.ng;
                        break;
                    default:
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}>,Recipe Setup_result<{1}>, Recipe Setup Error", msg.BODY.eqp_id.Trim(), msg.BODY.setup_result));
                        em.JobDataDownloadChangeResult = eCheckResult.other;
                        break;
                }
                #endregion
                //#region 判断所有设备是否反馈OK，如果所有设备都反馈成功，EAP通知Lot上机
                //var v = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel().Where(r => r.JobDataDownloadChangeResult == eCheckResult.other
                //|| r.JobDataDownloadChangeResult == eCheckResult.nothing).ToList();
                //if (v.Count > 0)
                //{
                //    foreach (var item in v)
                //    {
                //        item.JobDataDownloadChangeResult = eCheckResult.wait;
                //    }
                //}
                //#endregion


            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentRecipeSetupReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
