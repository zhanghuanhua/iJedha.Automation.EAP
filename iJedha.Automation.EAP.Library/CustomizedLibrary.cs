//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : CustomizedLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace iJedha.Automation.EAP.Library
{
    public class CustomizedLibrary
    {
        [CategoryAttribute("基本信息"), DescriptionAttribute("EQP Alive定时发送时间，单位秒")]
        [ReadOnlyAttribute(false)]
        [DisplayName("设备连线检查时间设定")]
        public int EQPAliveCheckTime { get; set; }  // EQP Alive定时发送时间，单位秒
        [CategoryAttribute("基本信息"), DescriptionAttribute("检查设备连线的时间")]
        [ReadOnlyAttribute(false)]
        [DisplayName("设备连线检查的时间")]
        public DateTime EQPAliveTime { get; set; }//检查设备连线的时间
        [CategoryAttribute("基本信息"), DescriptionAttribute("WIPData定时检查时间，单位秒")]
        [ReadOnlyAttribute(false)]
        [DisplayName("WIPData定时检查时间")]
        [Browsable(false)]
        public int WIPDataCheckTime { get; set; }  // WIPData定时检查时间，单位秒
        [CategoryAttribute("基本信息"), DescriptionAttribute("JobDataDownload定时检查时间，单位秒")]
        [ReadOnlyAttribute(false)]
        [DisplayName("任务下载等待时间")]
        public int JobDataDownloadCheckTime { get; set; }  //JobDataDownload定时检查时间，单位秒
        [CategoryAttribute("基本信息"), DescriptionAttribute("ProductionConditonPNCheck设定时间，单位秒。在换工单时使用")]
        [ReadOnlyAttribute(false)]
        [DisplayName("换料号节拍时间")]
        public int ProductionConditonCheckPNTime { get; set; }//ProductionConditonPNCheck设定时间，单位秒。在换工单时使用
        [CategoryAttribute("基本信息"), DescriptionAttribute("ProductionConditonLotCheck设定时间，单位秒。在换批次时使用")]
        [ReadOnlyAttribute(false)]
        [DisplayName("换批次节拍时间")]
        public int ProductionConditonCheckLotTime { get; set; }//ProductionConditonLotCheck设定时间，单位秒。在换批次时使用
        [CategoryAttribute("基本信息"), DescriptionAttribute("ProductionConditonCheckWorkOrder设定时间，单位秒。在换工单时使用")]
        [ReadOnlyAttribute(false)]
        [DisplayName("换工单节拍时间")]
        public int ProductionConditonCheckWorkOrderTime { get; set; }//ProductionConditonCheckWorkOrderTime设定时间，单位秒。在换批次时使用
        //[CategoryAttribute("基本信息"), DescriptionAttribute("该线体可使用的生产模式")]
        //[ReadOnlyAttribute(false)]
        //public string ProductionModeList { get; set; }  //该线体可使用的生产模式
        [CategoryAttribute("基本信息"), DescriptionAttribute("EqpModeRequestTime定时检查时间，单位秒")]
        [ReadOnlyAttribute(false)]
        [DisplayName("生产模式请求时间")]
        public int EqpModeRequestTime { get; set; }  //EqpModeRequestTime定时检查时间，单位秒
        [CategoryAttribute("基本信息"), DescriptionAttribute("JobDataDownload回复超时Retry次数")]
        [ReadOnlyAttribute(false)]
        [DisplayName("任务下载重复次数")]
        public int JobDataDownloadRetryCount { get; set; }//JobDataDownload回复超时Retry次数
        [CategoryAttribute("基本信息"), DescriptionAttribute("AMS Alive定时发送时间，单位秒")]
        [ReadOnlyAttribute(false)]
        [DisplayName("AMS系统检查时间")]
        public int AMSAliveCheckTime { get; set; }//AMS Alive定时发送时间，单位秒
        [CategoryAttribute("基本信息"), DescriptionAttribute("EQP Alive定时发送时间，单位秒")]
        [ReadOnlyAttribute(false)]
        [Browsable(false)]
        public DateTime AMSAliveTime { get; set; }//检查AMS连线的时间
        [CategoryAttribute("基本信息"), DescriptionAttribute("检查AMS连线的时间")]
        [ReadOnlyAttribute(false)]
        [Browsable(false)]
        public CustomizedLibrary()
        {
        }
    }
}
