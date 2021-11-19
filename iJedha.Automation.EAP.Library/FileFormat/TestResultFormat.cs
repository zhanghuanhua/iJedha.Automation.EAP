//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : ConfigLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace iJedha.Automation.EAP.Library
{
    [Serializable]
    public class TestResult
    {
        public string eqp_id { get; set; }
        public string PassQty { get; set; }
        public string FailQty { get; set; }
        
        public string DateTime { get; set; }

        public TestResult()
        {
            eqp_id = "";
            PassQty = "";
            FailQty = "";
            DateTime = "";
        }
      
    }
}
