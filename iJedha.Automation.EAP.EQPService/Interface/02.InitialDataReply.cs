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
    public partial class InitialDataReply : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.InitialDataReply rpy = new SocketMessageStructure.InitialDataReply();
                
                if (new Serialize<SocketMessageStructure.InitialDataReply>().DeSerializeXML(evtXml, out rpy) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "InitialDataReply", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "InitialDataReply", rpy.WriteToXml(), rpy.BODY.eqp_id.Trim()));
                #endregion

                ////根据设备ID获取设备信息
                //EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(rpy.BODY.eqp_id.Trim());

                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByNo(1);

                #region EAP记录设备当前状况
                if (em != null)
                {
                    #region 系统对时
                    new HostService.HostService().DateTimeSyncCommand(em.EQID);
                    #endregion
                    em.EQID = rpy.BODY.eqp_id.Trim();
                   
                    em.ControlMode = (eControlMode)Enum.Parse(typeof(eControlMode), rpy.BODY.control_mode==""?"0": rpy.BODY.control_mode);
                    em.OperationMode = (eOperationMode)Enum.Parse(typeof(eOperationMode), rpy.BODY.operation_mode==""?"0": rpy.BODY.operation_mode);
                    em.EQStatus = (eEQSts)Enum.Parse(typeof(eEQSts), rpy.BODY.eqp_status==""?"0":rpy.BODY.eqp_status);
                    em.PPID = rpy.BODY.recipe_name;
                    em.CurrentLotID = rpy.BODY.job_id.Trim();
                    em.TotalPanelCount = int.Parse(rpy.BODY.total_panel_count==""?"0": rpy.BODY.total_panel_count);
                    em.ProcessPanelCount = int.Parse(rpy.BODY.process_panel_count==""?"0": rpy.BODY.process_panel_count);
                }
                else
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<InitialDataReply> Find Error", rpy.BODY.eqp_id.Trim()));
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "InitialDataReply", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
