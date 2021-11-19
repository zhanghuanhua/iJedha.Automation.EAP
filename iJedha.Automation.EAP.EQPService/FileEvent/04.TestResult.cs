//******************************************************************
//   系统名称 : iJedha.Automation.EAP.EQPService
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
    public partial class TestResult : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                EAPEnvironment.commonLibrary.testResult = new Library.TestResult();
                //Deserialize(EAPEnvironment.commonLibrary.alarmReport.GetType(), evtXml);
                if (new Serialize<Library.TestResult> ().DeSerializeXML(evtXml, out EAPEnvironment.commonLibrary.testResult) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "TestResult", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{0}>Socket Message<{1}> Recv OK , Content<{2}>",
                 EAPEnvironment.commonLibrary.alarmReport.eqp_id, "TestResult", evtXml));

                #endregion

                //根据设备No获取设备信息
                //EquipmentModel em = Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByNo(1);
                //根据设备ID获取设备模型
                EquipmentModel em = EAPEnvironment.commonLibrary.equipmentLibary.GetEquipmentModelByEqID(EAPEnvironment.commonLibrary.testResult.eqp_id);
                History.EAP_EQP_TESTRESULT_FILE(EAPEnvironment.commonLibrary.testResult, em);
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "TestResult", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }

    }
}
