using iJedha.Automation.EAP.Core;
using System;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.Environment
{
    public partial class MQReport : BaseComm
    {
        public void MQ_StatusCheck()
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.Enable) return;
                iJedha.Customized.MessageStructure.mStatusCheck _data = new Customized.MessageStructure.mStatusCheck();
                _data.messagename = "StatusCheck";
                _data.datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                iJedha.Customized.MessageStructure.mStatusCheck.status_data sd;
                foreach (var v in Environment.EAPEnvironment.commonLibrary.equipmentLibary.GetAllEquipmentModel())
                {
                    sd = new Customized.MessageStructure.mStatusCheck.status_data();
                    sd.data_item = v.EQName;
                    sd.data_value = v.ControlMode.ToString();
                    _data.status_list.Add(sd);
                }

                sd = new Customized.MessageStructure.mStatusCheck.status_data();
                sd.data_item = "MES";
                sd.data_value = Environment.EAPEnvironment.commonLibrary.commonModel.CurMode.ToString();
                _data.status_list.Add(sd);

                sd = new Customized.MessageStructure.mStatusCheck.status_data();
                sd.data_item = "EAP Version";
                sd.data_value = Application.ProductVersion;
                _data.status_list.Add(sd);

                string _xmldata, _outdata;
                if (Environment.EAPEnvironment.MQPublisherAp.SendMessage_topic(_data, string.Format("{0}.EAP.MQ.StatusCheck", Environment.EAPEnvironment.commonLibrary.LineName), out _xmldata))
                {
                    if (ConvertJSON(_data, out _outdata))
                        LogMsg(Log.LogLevel.Trace, string.Format("MQ Message<{0}> Send OK, Content<{1}>", System.Reflection.MethodBase.GetCurrentMethod().Name, _outdata));

                }
                else
                {
                    LogMsg(Log.LogLevel.Warn, string.Format("MQ Message<{0}> Send NG.", System.Reflection.MethodBase.GetCurrentMethod().Name));
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0} {1}", ex.ToString(), ex.StackTrace.ToString()));
            }
        }
    }
}
