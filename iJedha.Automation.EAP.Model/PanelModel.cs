//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Model
//   文件概要 : PanelModel
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace iJedha.Automation.EAP.Model
{
    /// <summary>
    /// Panel模块
    /// </summary>
    public class PanelModel
    {
        [CategoryAttribute("基本讯息"), DescriptionAttribute("批号")]
        [ReadOnlyAttribute(true)]
        public string LotID { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("料号")]
        [ReadOnlyAttribute(true)]
        public string PN { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("载具号")]
        [ReadOnlyAttribute(true)]
        public string CarrierID { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("序号")]
        [ReadOnlyAttribute(true)]
        public int SequenceNo { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("板号")]
        [ReadOnlyAttribute(true)]
        public string PanelID { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("板型")]
        [ReadOnlyAttribute(true)]
        public ePanelType PanelType { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("建立资料时间")]
        [ReadOnlyAttribute(true)]
        public string CreateTime { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("开始生产时间")]
        [ReadOnlyAttribute(true)]
        public string StartTime { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("结束生产时间")]
        [ReadOnlyAttribute(true)]
        public string EndTime { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("外码")]
        [ReadOnlyAttribute(true)]
        public string OutPanelID { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("内码")]
        [ReadOnlyAttribute(true)]
        public string InPanelID { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("子板列表")]
        [ReadOnlyAttribute(true)]
        public List<MessageModel.Strip> StripIDList { get; set; }

        [CategoryAttribute("基本讯息"), DescriptionAttribute("配套后套ID")]
        [ReadOnlyAttribute(true)]
        public List<MessageModel.BatchPnl> BatchIDList { get; set; }
        [CategoryAttribute("基本讯息"), DescriptionAttribute("配套后套ID")]
        [ReadOnlyAttribute(true)]
        public string  HolePnlID { get; set; }
        public PanelModel()
        {
            LotID = "";
            PN = "";
            CarrierID = "";
            SequenceNo = 0;
            PanelID = "";
            PanelType = ePanelType.OK;
            CreateTime = "";
            StartTime = "";
            EndTime = "";
            OutPanelID = InPanelID = string.Empty;
            StripIDList = new List<MessageModel.Strip>();
            BatchIDList = new List<MessageModel.BatchPnl>();
            HolePnlID = "";
        }
    }

    /// <summary>
    /// Panel类型
    /// </summary>
    public enum ePanelType
    {
        /// <summary>
        /// OK板
        /// </summary>
        OK = 0,
        /// <summary>
        /// NG板
        /// </summary>
        NG = 1,
        /// <summary>
        /// 虚拟板
        /// </summary>
        DUMMY = 2,
        /// <summary>
        /// 混批
        /// </summary>
        MIXLOT = 3,
        /// <summary>
        /// 混料
        /// </summary>
        MIXPRODUCT = 4  
    }
}
