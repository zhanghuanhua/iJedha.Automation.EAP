
namespace iJedha.Automation.EAP.Library
{
    public class LogLibrary
    {
        #region [Log Messagement]
        /// <summary>
        /// add Log Messagement
        /// 定义类
        /// </summary>
        public bool WarningLog { get; set; }
        public string Disk { get; set; }
        public bool TraceDataLog { get; set; }
        public bool InfoLog { get; set; }
        public bool DebugLog { get; set; }
        public bool ErrorLog { get; set; }
        public int LogShowCount { get; set; }//EAP主页面显示Log的最大数量
        public string CFG_LOG_PATH { get; set; }  // Log 存储路径
        #endregion
    }
}
