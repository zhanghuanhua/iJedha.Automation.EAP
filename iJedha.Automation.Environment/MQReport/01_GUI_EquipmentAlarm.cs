using iJedha.Automation.EAP.Core;
using System;

namespace iJedha.Automation.EAP.Environment
{
    public partial class MQReport : BaseComm
    {
        public void GUI_EquipmentAlarm( string eqname, string alarmtype, string alarmcode, string alarmmessage)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.Enable) return;
                iJedha.Customized.MessageStructure.gEquipmentAlarm _data = new Customized.MessageStructure.gEquipmentAlarm();
                _data.messagename = "EquipmentAlarm";
                _data.datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _data.maineqpname = Environment.EAPEnvironment.commonLibrary.LineName;
                _data.subeqpname = eqname;
                _data.alarmtype = alarmtype;
                _data.alarmcode = alarmcode;
                _data.alarmmessage = alarmmessage;

                string _xmldata, _outdata;
                if (Environment.EAPEnvironment.GUIMQPublisherAp.SendMessage_topic(_data, string.Format("EquipmentAlarm.{0}", Environment.EAPEnvironment.commonLibrary.LineName), out _xmldata))
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
