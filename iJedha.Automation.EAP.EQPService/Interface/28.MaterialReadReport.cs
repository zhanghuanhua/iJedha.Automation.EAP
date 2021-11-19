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
using iJedha.Automation.EAP.WebAPI;
using System;

namespace iJedha.Automation.EAP.EQPService
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MaterialReadReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.MaterialReadReport msg = new SocketMessageStructure.MaterialReadReport();
                if (new Serialize<SocketMessageStructure.MaterialReadReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "MaterialReadReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "MaterialReadReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region EAP调用MES接口把物料信息上报给MES

                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());

                if (em != null)
                {
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

                    //上报MES物料上机
                    new WebAPIReport().EAP_MaterialSetUp(new MessageModel.MaterialSetUp()
                    {
                        SubEqpID = em.EQID,
                        MaterialSN = msg.BODY.material_id.Trim()
                    }, 1);
                    
                    #region 开料线需上报LOADCOMPLETE后，扫描物料上报EAP_LotInfoRequest
                    PortModel pm = em.GetPortModelByPortID(ePortID.L01.ToString());
                    if (pm == null)
                    {
                        //error log
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<CarrierReadReport> PortModel Find Error", msg.BODY.eqp_id.Trim()));
                        return;
                    }

                    //开料线端口状态为‘上料完成’时，向MES请求生产任务
                    if (pm.PortStatus == ePortStatus.LOADCOMPLETE)
                    {
                        MessageModel.LotInfoRequest lotRequest = new MessageModel.LotInfoRequest();
                        lotRequest.MainEqpID = EAPEnvironment.commonLibrary.MDLN;
                        lotRequest.SubEqpID = em.EQID;
                        lotRequest.PortID = ePortID.L01.ToString();
                        lotRequest.CarrierID = "";
                        lotRequest.LotID = msg.BODY.material_id.Trim();
                        new WebAPIReport().EAP_LotInfoRequest_KL(lotRequest, em, 1);
                    }
                    #endregion

                }
                else
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<MaterialReadReport> Find Error", msg.BODY.eqp_id.Trim()));
                    return;
                }
                #region Reply Message
                HostService.HostService.MaterialReadReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "MaterialReadReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
