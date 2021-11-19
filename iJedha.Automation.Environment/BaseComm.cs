//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Environment
//   文件概要 : BaseComm
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.ModelBase;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.Environment
{
    public partial class BaseComm : iJedha.Automation.EAP.Core.Controller.Controller_Base
    {
        private const char C_HEAD_CODE = (char)0x02;
        private const char C_TAIL_CODE = (char)0x03;

        private static object _LogListLock = new object();
        public BaseComm()
        {
        }
        public BaseComm(string id) : base(Path.Combine(Application.StartupPath, ConstLibrary.CONST_CLASSLIB_PATH), id)
        {

        }
        static string AsyVer()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();
        }

        [DllImport("Kernel32.dll")]
        static extern void GetLocalTime(clsSystemTime st);
        [DllImport("Kernel32.dll")]
        static extern void SetLocalTime(clsSystemTime st);

        class clsSystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDay;
            public ushort Whour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
            public ushort wDayOfWeek;
        }
        public static void SetSysTime(DateTime newdatetime)
        {
            clsSystemTime st = new clsSystemTime();
            st.wYear = (ushort)newdatetime.Year;
            st.wMonth = (ushort)newdatetime.Month;
            st.wDay = (ushort)newdatetime.Day;
            st.Whour = (ushort)newdatetime.Hour;
            st.wMinute = (ushort)newdatetime.Minute;
            st.wSecond = (ushort)newdatetime.Second;
            SetLocalTime(st);
        }
        public static string FilterEnterLine(string values)
        {
            if (string.IsNullOrEmpty(values))
            {
                return "";
            }

            string[] newValues = values.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join("", newValues);

        }
        /// <summary>
        /// 超时比对(秒）
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="overtime"></param>
        /// <returns></returns>
        public bool TimeComparisonSeconds(DateTime dt, int overtime)
        {
            try
            {
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = dt;
                TimeSpan ts = dt1.Subtract(dt2);
                double second = ts.TotalSeconds;
                return second > overtime;
            }
            catch (Exception ex)
            {
                LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 超时比对(分钟）
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="overtime"></param>
        /// <returns></returns>
        public bool TimeComparisonMin(DateTime dt, int overtime)
        {
            try
            {
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = dt;
                TimeSpan ts = dt1.Subtract(dt2);
                double min = ts.TotalMinutes;
                return min > overtime;
            }
            catch (Exception ex)
            {
                LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 超时比对(天）
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="overtime"></param>
        /// <returns></returns>
        public bool TimeComparisonDays(DateTime dt, int overtime)
        {
            try
            {
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = dt;
                TimeSpan ts = dt1.Subtract(dt2);
                double days = ts.TotalDays;
                return days > overtime;
            }
            catch (Exception ex)
            {
                LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

        /// <summary>
        /// 记日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="isMajor"></param>
        public static void LogMsg(Log.LogLevel level, string message, bool isMajor = true)
        {
            LogLibrary logLirary;
            logLirary = Environment.EAPEnvironment.commonLibrary.logLibrary;
            Log.Record(level, string.Format("{0} Version.{1}", message, AsyVer()), ConstLibrary.CFG_LOG_BLOCKNAME);

            #region
            string sb = string.Empty;
            if (true)
            {

            }
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
                case Log.LogLevel.Trace:
                    lvitem.ForeColor = Color.Blue;
                    break;
                default:
                    lvitem.ForeColor = Color.Black;
                    break;
            }
            if (level == Log.LogLevel.Info || level == Log.LogLevel.Warn)
            {
                Log.DisplayMessage = string.Format("{0}  {1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message);

                if (isMajor)
                {
                   EAPEnvironment.MQPublisherAp.MQ_ProcessMsg(message);
                }

            }

            if (Log.Dic_LogQueue.Count >= EAPEnvironment.commonLibrary.logLibrary.LogShowCount)
            {
                ListViewItem outlvitem;
                Log.Dic_LogQueue.TryDequeue(out outlvitem);
            }
            Log.Dic_LogQueue.Enqueue(lvitem);
            #endregion
        }
        /// <summary>
        /// EAP Error Msg Send
        /// </summary>
        /// <param name="errCode"></param>
        /// <param name="detailMsg"></param>
        public static void ErrorHandleRule(string errCode, string detailMsg, ref ErrorCodeModelBase errm)
        {
            try
            {
                errm = Environment.EAPEnvironment.commonLibrary.baseLib.errorCodeLibrary.GetErrorCodeModel(errCode);
                if (errm == null)
                {
                    LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Error, string.Format("Error Code<{0}> is not find. ", errCode));
                    return;
                }
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Warn, string.Format("<{0}> {1}. ", errCode, detailMsg));

                if (errm.DisplayTrigger == "Y")//发送信息到GUI,EMS
                {
                    EAPEnvironment.MQPublisherAp.MQ_ProcessError(errm.ErrorCode, errm.ErrorDesc, detailMsg, errm.ErrorSolve);
                    //History.Histroy_GUIErrorMsg(errm, detailMsg);
                }
                if (errm.HoldTrigger == "Y" && Environment.EAPEnvironment.commonLibrary.commonModel.currentProcessLotID != string.Empty)
                {
                }

            }
            catch (Exception ex)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        public static void Backup(object BackupObject, iJedha.Automation.EAP.Core.Controller.Controller_Base.eBackupStatement state)
        {
            iJedha.Automation.EAP.Core.Controller.Controller_Base.BackupItem backup = null;

            Task.Factory.StartNew(() => { EAPEnvironment.EAPAp.AddBackupStatement(ref backup); });
        }

        public static Func<iJedha.Automation.EAP.Core.Pipe.TransferProtocal, bool> PipeSend = (SndData) =>
        {
            try
            {
                return EAPEnvironment.EAPAp.BroadcastMsg(SndData, SndData.Recognition.Trim());
            }
            catch (Exception ex)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        };



        /// <summary>
        /// 将结构转成Json
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static bool ConvertJSON(object _object, out string _data)
        {
            _data = default(string);
            try
            {
                if (!new iJedha.Automation.EAP.Core.Serialize().SerializeJSON(_object, out _data))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return false;
            }
        }

        public static string ConvertJSON(object _object)
        {
            string _data;
            try
            {
                if (!new iJedha.Automation.EAP.Core.Serialize().SerializeJSON(_object, out _data))
                {
                    return string.Empty;
                }
                return _data;
            }
            catch (Exception ex)
            {
                LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return string.Empty;
            }
        }

        public static string GetProcessMemory()
        {
            try
            {
                Process obj_Process = new Process();
                obj_Process = Process.GetCurrentProcess();
                return (obj_Process.WorkingSet64 / 1024 / 1024).ToString();
            }
            catch
            {
                return "";
            }
        }

        public static void ProcessScenarioFlow(sFlowTag xTag, bool IsTrigger = false, bool IsReply = false)
        {
            sEngine xEngine = new sEngine(EAPEnvironment.FlowControl, xTag);
            if (sFlowModule.FlowEngine(ref xEngine, ref xTag, IsTrigger, IsReply))
            {
                ProcessScenarioFlow(xTag);
            }
        }
    }
}
