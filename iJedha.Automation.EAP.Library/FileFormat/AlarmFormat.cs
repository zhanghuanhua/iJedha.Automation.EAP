//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : ConfigLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;

namespace iJedha.Automation.EAP.Library
{
    [Serializable]
    public class AlarmReport
    {
        public string eqp_id { get; set; }
        public string alarm_type { get; set; }
        public string alarm_level { get; set; }
        public string alarm_text { get; set; }
        public string DateTime { get; set; }
        //需要厂商添加警报ID
        public string alarm_id { get; set; }

        public AlarmReport()
        {
            eqp_id = "";
            alarm_type = "";
            alarm_level = "";
            alarm_text = "";
            DateTime = "";
            alarm_id = "";
        }
       
    }
}
