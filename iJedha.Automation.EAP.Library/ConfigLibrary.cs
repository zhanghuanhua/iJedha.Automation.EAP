//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : ConfigLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
using System.Collections.Generic;

namespace iJedha.Automation.EAP.Library
{
    public class ConfigLibrary
    {
        public string AMSUrl { get; set; }//AMS Url设定

        public bool Enable_Set { get; set; }
        public string PortName { get; set; }
        public string BaudRate { get; set; }
        public string Parity { get; set; }
        public string DataBits { get; set; }
        public string StopBits { get; set; }

        public ConfigLibrary()
        {
            AMSUrl = string.Empty;
        }
    }
}
