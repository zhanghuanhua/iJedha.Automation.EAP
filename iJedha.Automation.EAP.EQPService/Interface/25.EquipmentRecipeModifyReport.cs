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
    public partial class EquipmentRecipeModifyReport : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.EquipmentRecipeModifyReport msg = new SocketMessageStructure.EquipmentRecipeModifyReport();
                if (new Serialize<SocketMessageStructure.EquipmentRecipeModifyReport>().DeSerializeXML(evtXml, out msg) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentRecipeModifyReport", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "EquipmentRecipeModifyReport", msg.WriteToXml(), msg.BODY.eqp_id.Trim()));
                //根据设备ID获取设备信息
                EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByID(msg.BODY.eqp_id.Trim());
                if (em == null)
                {
                    //error log
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Equipment ID<{0}> Function Name<EquipmentRecipeModifyReport> Find Error", msg.BODY.eqp_id.Trim()));
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
                HostService.HostService.RecipeModifyReportReply(msg.HEADER.TRANSACTIONID, msg.BODY.eqp_id.Trim());
                #endregion

                //记录设备操作状态
                switch (msg.BODY.modify_code)
                {
                    case "1"://1:新增 2:修改 3:删除
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 新增配方<{1}>完成.", msg.BODY.eqp_id.Trim(), msg.BODY.recipe_name));
                        break;
                    case "2":
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 修改配方<{1}>完成.", msg.BODY.eqp_id.Trim(), msg.BODY.recipe_name));
                        break;
                    case "3":
                        BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Equipment ID<{0}> 删除配方<{1}>完成.", msg.BODY.eqp_id.Trim(), msg.BODY.recipe_name));
                        break;
                    default:
                        BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("Equipment ID<{0}> 配方<{1}> Modify code<{2}> error.", msg.BODY.eqp_id.Trim(), msg.BODY.recipe_name,msg.BODY.modify_code));
                        break;
                }
                #endregion
                
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentRecipeModifyReport", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
