using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.Rule
{
    public abstract class BaseComm
    {
        public static void LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel level, string message)
        {
            iJedha.Automation.EAP.Core.Log.Record(level, string.Format("{0} Version.{1}", message, AsyVer()), ConstLibrary.CFG_LOG_BLOCKNAME);
            #region
            string sb = string.Empty;
            sb = string.Format("[{0}]  [{1}] {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), level.ToString(), message);
            ListViewItem lvitem = new ListViewItem(sb);

            switch (level)
            {
                case Log.LogLevel.Fatal:
                case Log.LogLevel.Error:
                    lvitem.ForeColor = Color.Red;
                    break;
                case Log.LogLevel.Warn:
                    lvitem.ForeColor = Color.DarkOrange;
                    break;
                default:
                    lvitem.ForeColor = Color.Blue;
                    break;
            }
            if (Log.Dic_LogQueue.Count >= 500)
            {
                ListViewItem outlvitem;
                Log.Dic_LogQueue.TryDequeue(out outlvitem);
            }
            Log.Dic_LogQueue.Enqueue(lvitem);
            if (level == Log.LogLevel.Info || level == Log.LogLevel.Warn) Log.DisplayMessage = string.Format("{0}  {1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message);
            #endregion
        }

        static string AsyVer()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();
        }

    }
}
