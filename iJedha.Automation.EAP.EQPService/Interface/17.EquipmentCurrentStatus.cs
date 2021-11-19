//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
using System;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentCurrentStatus : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentCurrentStatus msg = new SocketMessageStructure.EquipmentCurrentStatus();
                if (new Serialize<SocketMessageStructure.EquipmentCurrentStatus>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentCurrentStatus", evtXml));
                    return;
                }
                #endregion

                //根据设备NO获取设备信息
                //EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByNo(1);
                //根据设备ID获取设备模型
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(msg.BODY.eqp_id);

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentCurrentStatus", msg.WriteToXml(), em.EQID.Trim()));
                #endregion
                #region 更新设备当前状态
                
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentCurrentStatus> Find Error", em.EQID.Trim()));
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

                if (UpdataCurrentStatus(em, em.EQID, msg.BODY.eqp_status, msg.BODY.green_towner, msg.BODY.yellow_towner, msg.BODY.red_towner))
                {
                    #region Reply Message
                    HostService.HostService.EquipmentCurrentStatusReply(msg.HEADER.TRANSACTIONID, em.EQID.Trim());
                    #endregion
                }
                #endregion

                //#region EAP调用MES接口把当前设备状态上报给MES

                //new WebAPIReport().EAP_EqpStatusReport(new MessageModel.StatusReport()
                //{
                //    SubEqpID = em.EQID,
                //    EqpStatusCode = em.EQStatus.ToString()
                //}, 1);

                //#endregion
                
                EAPEnvironment.MQPublisherAp.MQ_EquipmentStatus(Environment.EAPEnvironment.commonLibrary.MQ_EquipmentStatus());
                if (!string.IsNullOrEmpty(msg.BODY.eqp_status))
                {
                    History.EAP_EQP_STATUS(em,msg.BODY.green_towner, msg.BODY.yellow_towner, msg.BODY.red_towner);
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentCurrentStatus", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        /// <summary>
        /// 更新设备状态
        /// </summary>
        /// <param name="em"></param>
        /// <param name="subEqID"></param>
        /// <param name="eqStatus"></param>
        /// <param name="greenTower"></param>
        /// <param name="yellowTower"></param>
        /// <param name="redTower"></param>
        /// <returns></returns>
        public bool UpdataCurrentStatus(EquipmentModel em, string subEqID, string eqStatus, string greenTower, string yellowTower, string redTower)
        {
            try
            {

                em.SUBEQID = subEqID;
                #region 记录设备状态切换
                switch (eqStatus)
                {
                    case "1":
                        em.EQStatus = eEQSts.Run;
                        Environment.BaseComm.LogMsg(Log.LogLevel.Info, string.Format("设备<{0}>当前状态为<{1}>.", em.EQName, em.EQStatus.ToString()));
                        if (em.EqNameKey== ConstLibrary.CONST_ACL)
                        {
                            Environment.EAPEnvironment.commonLibrary.StopNextLot = false;
                        }
                        break;
                    case "2":
                        em.EQStatus = eEQSts.Pause;
                        Environment.BaseComm.LogMsg(Log.LogLevel.Info,$"设备<{em.EQName}>当前状态为<{"暂停"}>.");
                        break;
                    case "3":
                        em.EQStatus = eEQSts.Idle;
                        Environment.BaseComm.LogMsg(Log.LogLevel.Info, $"设备<{em.EQName}>当前状态为<{"待机"}>.");
                        break;
                    case "4":
                        em.EQStatus = eEQSts.Down;
                        Environment.BaseComm.LogMsg(Log.LogLevel.Info, $"设备<{em.EQName}>当前状态为<{"宕机"}>.");
                        break;
                    case "5":
                        em.EQStatus = eEQSts.PM;
                        Environment.BaseComm.LogMsg(Log.LogLevel.Info, $"设备<{em.EQName}>当前状态为<{"保养"}>.");
                        break;
                    default:
                        em.EQStatus = eEQSts.Unknown;
                        break;
                }
                #endregion
                #region 记录信号灯切换
                switch (greenTower)
                {
                    case "1":
                        em.GreenTower = eGreenTower.LIGHTON;
                        break;
                    case "2":
                        em.GreenTower = eGreenTower.FLASH;
                        break;
                    case "3":
                        em.GreenTower = eGreenTower.LIGHTOFF;
                        break;
                    default:
                        em.GreenTower = eGreenTower.UNKNOW;
                        break;
                }
                switch (yellowTower)
                {
                    case "1":
                        em.YellowTower = eYellowTower.LIGHTON;
                        break;
                    case "2":
                        em.YellowTower = eYellowTower.FLASH;
                        break;
                    case "3":
                        em.YellowTower = eYellowTower.LIGHTOFF;
                        break;
                    default:
                        em.YellowTower = eYellowTower.UNKNOW;
                        break;
                }
                switch (redTower)
                {
                    case "1":
                        em.RedTower = eRedTower.LIGHTON;
                        break;
                    case "2":
                        em.RedTower = eRedTower.FLASH;
                        break;
                    case "3":
                        em.RedTower = eRedTower.LIGHTOFF;
                        break;
                    default:
                        em.RedTower = eRedTower.UNKNOW;
                        break;
                }
                #endregion
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
