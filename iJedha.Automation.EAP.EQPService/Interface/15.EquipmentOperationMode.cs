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

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentOperationMode : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentOperationMode msg = new SocketMessageStructure.EquipmentOperationMode();
                if (new Serialize<SocketMessageStructure.EquipmentOperationMode>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentOperationMode", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentOperationMode", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

                #region 更新操作模式
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em==null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentOperationMode> Find Error", msg.BODY.eqp_id.Trim()));
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

                if (UpdataOperationMode(em, msg.BODY.operation_mode))
                {
                    #region Reply Message
                    HostService.HostService.EquipmentOperationModeReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                    #endregion
                }
                #endregion

            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentOperationMode", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        /// <summary>
        /// 更新模式
        /// </summary>
        /// <param name="em"></param>
        /// <param name="operationMode"></param>
        /// <returns></returns>
        public bool UpdataOperationMode(EquipmentModel em, string operationMode)
        {
            try
            {
                lock (operationMode)
                {
                    switch (operationMode)
                    {
                        case "2":
                            em.OperationMode = eOperationMode.AUTO;
                            Environment.BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>自动模式.", em.EQName));
                            //em.OnlineScenarioStep = Model.eOnlineScenarioStep.Initial;
                            break;
                        case "1":
                            em.OperationMode = eOperationMode.MANUAL;
                            Environment.BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>手动模式.", em.EQName));
                            break;
                        default:
                            em.OperationMode = eOperationMode.UNKNOW;
                            break;
                    }
                }
                return true;
                
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
    }
}
