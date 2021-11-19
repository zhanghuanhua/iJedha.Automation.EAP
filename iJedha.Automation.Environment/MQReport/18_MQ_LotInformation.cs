using iJedha.Automation.EAP.Core;
using RabbitMQ.Client;
using System;

namespace iJedha.Automation.EAP.Environment
{
    public partial class MQReport : BaseComm
    {
        public void MQ_LotInformation(string lotid, string pn, string panelcount, string message)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.baseLib.rbmqParaLibrary.Enable) return;
                iJedha.Customized.MessageStructure.mLot _data = new Customized.MessageStructure.mLot();
                _data.messagename = "LotInformation";
                _data.datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _data.maineqpname = Environment.EAPEnvironment.commonLibrary.LineName;
                _data.lotid = lotid;
                _data.pn = pn;
                _data.panelcount = panelcount;
                _data.message = message;

                string _xmldata, _outdata;
                if (Environment.EAPEnvironment.MQPublisherAp.SendMessage_topic(_data, string.Format("{0}.EAP.MQ.LotInformation", Environment.EAPEnvironment.commonLibrary.LineName), out _xmldata))
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
