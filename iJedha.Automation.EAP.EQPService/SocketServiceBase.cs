//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : SocketServiceBase
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/11 14:08:14
//******************************************************************

using System;
using System.Linq;
using System.Xml;
using System.Reflection;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Core;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class EQPService
    {
        public void HandleSocketEvent(byte[] rawdatas)
        {
            string evtName = "";
            try
            {
                byte[] recvCmd = new byte[rawdatas.Length - 2];
                rawdatas.ToList().CopyTo(1, recvCmd, 0, rawdatas.Length - 2);

                string data = Extensions.GetXmlData(recvCmd);
                //if (data.Contains("<AlarmReport>"))
                //{
                //    evtName = "AlarmReport";
                //}
                //else
                //    evtName = GetMsgName(data);

                if (!GetRootNodeName(data,ref evtName))
                {
                    evtName = GetMsgName(data);
                }
                if (!string.IsNullOrEmpty(evtName))
                {
                    CacheMethodDelegate<string>.DelegateCall(GetMsgType(evtName), "EventHandle", data);
                }
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("<{2}>{0} {1}", ex.ToString(), ex.StackTrace.ToString(), evtName));
            }
        }

        private static Type GetMsgType(string msgName)
        {
            try
            {
                return Type.GetType($"{MethodInfo.GetCurrentMethod().DeclaringType.Namespace}.{msgName.Replace(" ", "")}", true);
            }
            catch { return null; }
        }

        private string GetMsgName(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data)) return "";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);

                return xmlDoc.SelectSingleNode("//messagename").InnerText;
            }
            catch
            {
                return "";
            }
        }

        private bool GetRootNodeName(string data,ref string evtName)
        {
            try
            {
                if (string.IsNullOrEmpty(data)) return false;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                evtName=xmlDoc.DocumentElement.Name;
                if (evtName== "message")
                {
                    evtName = "";
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetRawDataLog(byte[] byts)
        {
            Func<byte[], string> BytetoString = (a) =>
            {
                string[] HexAry = a.Select(D => D.ToString("X").PadLeft(2, '0')).ToArray();
                return string.Join(" ", HexAry);
            };
            string log = string.Empty;
            string strRawData = BytetoString(byts);

            double s = 100;
            int result = 0;
            result = Convert.ToInt16(Math.Ceiling((double)strRawData.Length / 30));
            strRawData = strRawData.PadRight(30 * result);

            int column = 0;
            for (long i = 0; i < strRawData.Length; i += 30)
            {
                if (column != 0 && (column % 5) == 0) log += "\r\n\t";
                log += $" *>* {strRawData.Substring((int)(i / 30 * 30), 30)}";

                column++;
            }
            return log;
        }
    }
}
