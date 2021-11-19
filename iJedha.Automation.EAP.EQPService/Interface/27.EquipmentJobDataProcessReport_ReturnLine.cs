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
using System.Collections.Generic;
using System.Linq;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EquipmentJobDataProcessReport_ReturnLine : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine msg = new SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine();
                if (new Serialize<SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentJobDataProcessReport_ReturnLine", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentJobDataProcessReport_ReturnLine", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                #endregion

               
                #region EAP收到设备上报信息后进行记录处理
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());

                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_ReturnLine>  Find Error", msg.BODY.eqp_id.Trim()));
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
                HostService.HostService.EquipmentJobDataProcessReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion

                switch (msg.BODY.process_code)
                {
                    case "1"://创建生产任务
                        CreateJob(em, msg);
                        break;
                    case "2"://更新生产任务
                        UpdateJob(em, msg);
                        break;
                    case "3"://删除生产任务
                        DeleteJob(em, msg);
                        break;
                    case "4"://开始生产任务
                        StartJob(em, msg);
                        break;
                    case "5"://完成生产任务
                        CompleteJob(em, msg);
                        break;
                    default:
                        em.ProcessStatus = eProcessCode.UNKNOW;
                        em.JobDataDownloadChangeResult = eCheckResult.other;
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 生产任务<UNKNOW>", msg.BODY.eqp_id.Trim()));
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_ReturnLine", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
        /// <summary>
        /// 创建生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void CreateJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Create;
                em.JobDataDownloadChangeResult = eCheckResult.ok;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 创建任务<{1}>OK", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
                em.LastLotID = msg.BODY.job_id.Trim();
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Create", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 更新生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void UpdateJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Update;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 更新任务<{1}>OK", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Update", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 删除生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void DeleteJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Delete;
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 删除任务<{1}>OK", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Delete", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 开始生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void StartJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Start;
                em.CurrentLotID = msg.BODY.job_id.Trim();
                if (em.isCurrentPN)
                {
                    //分板线四合一只有料号，job_id是料号
                    em.CurrentPN = msg.BODY.job_id;
                }
                em.OldEQParameter.Clear();
                foreach (var item in em.NewEQParameter)
                {
                    em.OldEQParameter.Add(item);
                }
                //如果是开料机，增加开始生产Lot信息
                if (em.EqNameKey == ConstLibrary.CONST_ACL)
                {
                    //在所有Lot里面找出符合条件的Lot信息
                    Lot lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelBySubLotID(msg.BODY.job_id.Trim());
                    if (lm == null)
                    {
                        //error log
                        BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_StartJob> LotModel Find Error.Job ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                        return;
                    }
                    //更改Lot状态
                    lm.LotProcessStatus = eLotProcessStatus.Run;
                    //把当前Lot加入到开始生产Lot里面
                    EAP.Environment.EAPEnvironment.commonLibrary.commonModel.AddStartProcessLotModelKL((Lot)lm.Clone());
                }

                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 开始生产任务<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Start", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
        /// <summary>
        /// 完成生产任务记录
        /// </summary>
        /// <param name="em"></param>
        /// <param name="msg"></param>
        public void CompleteJob(EquipmentModel em, SocketMessageStructure.EquipmentJobDataProcessReport_ReturnLine msg)
        {
            try
            {
                em.ProcessStatus = eProcessCode.Complete;
                string err = "";
                #region 回流线Track Out逻辑*****-----

                LotModel lm = Environment.EAPEnvironment.commonLibrary.commonModel.GetLotModelByBatchID(msg.BODY.job_id.Trim());
                if (lm == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentJobDataProcessReport_CompleteJob1> LotModel Find Error.Batch ID<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id.Trim()));
                    return;
                }
                new WebAPIReport().EAP_LotTrackOut(new MessageModel.LotTrackOut()
                {
                    MainEqpID = EAPEnvironment.commonLibrary.MDLN,
                    SubEqpID = em.EQID,
                    PortID = "",
                    LotID = em.CurrentLotID,
                    PanelTotalQty = msg.BODY.process_panel_count,
                    NGFlag = false,//先记录false??
                    PanelList = new List<MessageModel.Panel>(),//回流线不需要给Panel List
                    WIPDataList = new List<MessageModel.WipData>(),//回流线不需要上报WipData
                    JobID = msg.BODY.job_id.Trim(),
                    JobTotalQty = msg.BODY.process_panel_count,
                    PN = msg.BODY.part_id,
                    WorkOrder=lm.WorkOrder
                }, lm, em, 1,out err);

                #endregion
                BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 完成生产任务<{1}>", msg.BODY.eqp_id.Trim(), msg.BODY.job_id));
                EAPEnvironment.commonLibrary.CurrentLotCount = msg.BODY.process_panel_count;
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentJobDataProcessReport_Start", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }
     
    }
}
