using iJedha.Automation.EAP.Core;
using System;
using System.Collections.Generic;
using System.Xml;

namespace iJedha.Automation.EAP.UI
{
    public class Language
    {
        #region [主界面]
        public string Menu_View = "";
        public string Menu_Setting = "";
        public string Menu_Operation = "";
        public string Menu_Log = "";
        public string Menu_Customize = "";
        public string Menu_About = "";
        public string Menu_Exit = "";

        public string Label_Status = "";
        public string Label_Mode = "";
        public string Label_StartTime = "";
        public string Label_NowTime = "";
        public string Label_Envirment = "";

        public string Listview_1 = "";
        public string Listview_2 = "";
        public string Listview_3 = "";
        public string Listview_4 = "";
        public string Listview_5 = "";
        public string Listview_6 = "";
        public string Listview_7 = "";
        #endregion

        protected Dictionary<string, string> DicLanguage = new Dictionary<string, string>();
        public Language()
        {
            XmlLoad(GlobalData.SystemLanguage);
            BindLanguageText();
        }

        /// <summary>
        /// 读取XML放到内存
        /// </summary>
        /// <param name="language"></param>
        protected void XmlLoad(string language)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                string address = AppDomain.CurrentDomain.BaseDirectory + "Languages\\" + language + ".xml";
                doc.Load(address);
                XmlElement root = doc.DocumentElement;

                XmlNodeList nodeLst1 = root.ChildNodes;
                foreach (XmlNode item in nodeLst1)
                {
                    DicLanguage.Add(item.Name, item.InnerText);
                }
            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name,ex.Message.ToString(), ex.StackTrace.ToString()));
            }
        }

        public void BindLanguageText()
        {
            Menu_View = DicLanguage["Menu_View"];
            Menu_Setting = DicLanguage["Menu_Setting"];
            Menu_Log = DicLanguage["Menu_Log"];
            Menu_Operation = DicLanguage["Menu_Operation"];
            Menu_Customize = DicLanguage["Menu_Customize"];
            Menu_About = DicLanguage["Menu_About"];
            Menu_Exit = DicLanguage["Menu_Exit"];

            Label_Status = DicLanguage["Label_Status"];
            Label_Mode = DicLanguage["Label_ProdMode"];
            Label_StartTime = DicLanguage["Label_StartTime"];
            Label_NowTime = DicLanguage["Label_NowTime"];
            Label_Envirment = DicLanguage["Label_Envirment"];

            Listview_1 = DicLanguage["Listview_1"];
            Listview_2 = DicLanguage["Listview_2"];
            Listview_3 = DicLanguage["Listview_3"];
            Listview_4 = DicLanguage["Listview_4"];
            Listview_5 = DicLanguage["Listview_5"];
            Listview_6 = DicLanguage["Listview_6"];
            Listview_7 = DicLanguage["Listview_7"];
        }
    }
}
