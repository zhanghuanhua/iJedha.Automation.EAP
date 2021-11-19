//******************************************************************
//   系统名称 : iJedha.Automation.EAP.HistoryService
//   文件概要 : History
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************

/// <summary>
/// EAP to DB  Interface
/// </summary>
namespace iJedha.Automation.EAP.Environment
{
    public partial class History
    {
        private static string strReplace(string str)
        {
            string temp = str.Replace("'", "''");
            temp = temp.Replace(";", "；");
            temp = temp.Replace(",", "，");
            temp = temp.Replace("?", "？");
            return temp;
        }
    }
}
