using System.Configuration;

namespace iJedha.Automation.EAP.UI
{
    enum eLanguage
    {
        English = 0,
        Chinese = 1
    }

    public class GlobalData
    {
        /// <summary>
        /// 系统语言（Chinese（中文），English（英文）。。。）
        /// </summary>
        public static string SystemLanguage = ConfigurationManager.AppSettings["Language"];

        private static Language globalLanguage;
        public static Language GlobalLanguage
        {
            get
            {
                globalLanguage = new Language();
                return globalLanguage;
            }
        }

        public GlobalData()
        {

        }
        /// <summary>
        /// 修改AppSettings中配置
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="value">相应值</param>
        public static bool LanguageChange(string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings["Language"] != null)
                {
                    config.AppSettings.Settings["Language"].Value = value;
                }
                else
                {
                    config.AppSettings.Settings.Add("Language", value);
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                SystemLanguage = ConfigurationManager.AppSettings["Language"];
                globalLanguage = new Language();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
