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
    public class EquipmentStatus
    {
        public string eqp_id { get; set; }
        public string eq_status { get; set; }
        public string green_towner { get; set; }
        public string yellow_towner { get; set; }
        public string red_towner { get; set; }
        
        public string DateTime { get; set; }

        public EquipmentStatus()
        {
            eqp_id = "";
            eq_status = "";
            green_towner = "";
            yellow_towner = "";
            red_towner = "";
            DateTime = "";
        }
      
    }
}
