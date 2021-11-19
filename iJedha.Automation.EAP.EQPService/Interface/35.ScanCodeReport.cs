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
    public partial class ScanCodeReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.ScanCodeReport msg = new SocketMessageStructure.ScanCodeReport();
                if (new Serialize<SocketMessageStructure.ScanCodeReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "ScanCodeReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "ScanCodeReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region 记录设备上报的物料ID信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                PortModel pm;
                if (em != null)
                {
                    #region 连线检查
                    if (em.isCheckConnect && em.isCheckControlMode)
                    {
                        if (em.ConnectMode == eConnectMode.DISCONNECT || em.ControlMode!=eControlMode.REMOTE)
                        {
                            BaseComm.LogMsg(Log.LogLevel.Error, $"设备ID[{em.EQID}],连线状态为[{em.ConnectMode.ToString()}],控制模式为[{em.ControlMode.ToString()}]");
                            return;
                        }
                    }
                    #endregion


                    pm = em.GetPortModelByPortID(msg.BODY.port_id.Trim());
                    if (pm != null)
                    {
                        //给物料Lot赋值
                        pm.MaterialLotID_PP = msg.BODY.code_id.Trim();
                        //添加上报次数，如果有相同的key值，先移除，再添加，给LoadCompleteReportCheck做检查
                        if (EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.ContainsKey(msg.BODY.port_id.Trim()))
                        {
                            EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Remove(msg.BODY.port_id.Trim());
                        }
                        EAPEnvironment.commonLibrary.commonModel.LoadCompleteCount.Add(msg.BODY.port_id.Trim(), msg.BODY.port_id.Trim());

                    }
                    else
                    {
                        LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}>  Function Name<ScanCodeReport> PortModel Error", msg.BODY.eqp_id.Trim()));
                    }
                    //MES下载信息
                    string MESDownloadInfo = string.Empty;
                    //设备上报信息
                    string EQPReportInfo = string.Empty;

                    //获取MESDownloadInfo和EQPReportInfo值方法
                    EAPEnvironment.commonLibrary.GetPPMaterialLotInfo(em, out MESDownloadInfo, out EQPReportInfo);
                    //下货完成后，如果生产过程中缺货，上料后检查叠构顺序
                    if (EAPEnvironment.commonLibrary.isSameWithInnerLotCount)
                    {
                        if (!string.IsNullOrEmpty(MESDownloadInfo) && !string.IsNullOrEmpty(EQPReportInfo) && MESDownloadInfo == EQPReportInfo)
                        {
                            LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>上报物料信息<{1}>与MES下载信息匹配.", msg.BODY.eqp_id.Trim(),msg.BODY.code_id.Trim()));
                        }
                        else
                        {
                            //如果不一致，清除物料Lot信息，并发送错误提示信息给设备
                            pm.MaterialLotID_PP = string.Empty;
                            string errMsg = string.Format("E0004:设备<{0}>上报物料信息<{1}>与MES下载信息不匹配.请检查物料位置是否正确.", msg.BODY.eqp_id.Trim(), msg.BODY.code_id.Trim());
                            LogMsg(Log.LogLevel.Warn, errMsg);
                            new HostService.HostService().CIMMessageCommand(msg.BODY.eqp_id,"0", errMsg, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        }
                    }
                }
                else
                {
                    LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}>  Function Name<ScanCodeReport> EquipmentModel Error", msg.BODY.eqp_id.Trim()));
                }
                #endregion

                #region Reply Message
                HostService.HostService.ScanCodeReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim(), "1");//0:NG 1:OK
                #endregion
            }
            catch (Exception ex)
            {
                LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "PanelInReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
