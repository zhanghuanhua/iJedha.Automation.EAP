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
    public partial class EquipmentStatus : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                EAPEnvironment.commonLibrary.equipmentStatus = new Library.EquipmentStatus();
                if (new Serialize<Library.EquipmentStatus>().DeSerializeXML(evtXml, out EAPEnvironment.commonLibrary.equipmentStatus) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "EquipmentStatus", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{0}>Socket Message<{1}> Recv OK , Content<{2}>",
                 EAPEnvironment.commonLibrary.equipmentStatus.eqp_id, "EquipmentStatus", evtXml));

                #endregion
                //根据设备No获取设备信息
                //var em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByNo(1);
                //根据设备ID获取设备模型
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(EAPEnvironment.commonLibrary.equipmentStatus.eqp_id);
                History.EAP_EQP_STATUS_FILE(EAPEnvironment.commonLibrary.equipmentStatus, em);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "EquipmentStatus", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }


    }
}
