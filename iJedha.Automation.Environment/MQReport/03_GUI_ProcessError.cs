using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.ModelBase;
using RabbitMQ.Client;
using System;

namespace iJedha.Automation.EAP.Environment
{
    public partial class MQReport : BaseComm
    {
        public void GUI_ProcessError(string alarmcode, string alarmdetail)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.Enable) return;
                ErrorCodeModelBase errm = Environment.EAPEnvironment.commonLibrary.baseLib.errorCodeLibrary.GetErrorCodeModel(alarmcode);
                if (errm == null) return;
                iJedha.Customized.MessageStructure.gProcessError _data = new Customized.MessageStructure.gProcessError();
                _data.messagename = "ProcessError";
                _data.datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _data.maineqpname = Environment.EAPEnvironment.commonLibrary.LineName;
                _data.subeqpname = "EAP";
                _data.alarmcode = alarmcode;
                _data.alarmmessage = errm.ErrorDesc;
                _data.alarmmessagedetail = alarmdetail;
                _data.alarmsolve = errm.ErrorSolve;

                string _xmldata, _outdata;
                if (Environment.EAPEnvironment.GUIMQPublisherAp.SendMessage_topic(_data, string.Format("ProcessError.{0}", Environment.EAPEnvironment.commonLibrary.LineName), out _xmldata))
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
