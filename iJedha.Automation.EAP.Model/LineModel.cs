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
    /// Line模块
    /// </summary>
    public class LineModel
    {
        [CategoryAttribute("基本信息"), DescriptionAttribute("线体类别")]
        [ReadOnlyAttribute(true)]
        [DisplayName("线体类别")]
        public string LineType { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("线体名称")]
        [ReadOnlyAttribute(true)]
        [DisplayName("线体名称")]
        public string LineName { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("设定下载内层Lot Panel 列表 目前开料、PP裁切机使用")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        [DisplayName("是否使用内层Lot")]
        public bool isInnerLotPanelList { get; set; }
        /// <summary>
        /// Load Complete次数检查。目前冲孔连棕化，PP裁切机使用
        /// </summary>
        [CategoryAttribute("控制信息"), DescriptionAttribute("Load Complete次数检查。目前冲孔连棕化，PP裁切机使用")]
        [DisplayName("Load Complete次数检查")]
        [ReadOnlyAttribute(true)]
        public bool isLoadCompleteCountCheck { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute(" 是否使用SubLot，目前开料线使用")]
        [DisplayName("是否使用SubLot")]
        [ReadOnlyAttribute(true)]
        public bool isSubLot { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("TrackIn之前是否做检查，融铆合上机前检查")]
        [DisplayName("TrackIn之前是否做检查")]
        [ReadOnlyAttribute(true)]
        public bool isCheckBeforeTrackIn { get; set; }
        /// <summary>
        /// 线体多Load设备时设定  多投板机设备检查回复逻辑，目前融铆合使用
        /// </summary>
        [CategoryAttribute("控制信息"), DescriptionAttribute("多投板机设备检查回复逻辑，目前融铆合使用")]
        [DisplayName("是否为多Load设备")]
        [ReadOnlyAttribute(true)]
        public bool isMultiLoadEquipment { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("MES下载主线设备和设备名称相同")]
        [DisplayName("主设备")]
        [ReadOnlyAttribute(true)]
        public bool isMainEqpID { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("是否需要进行过账，用于MES手动过账工序  false:不需过账,true:需要过账")]
        [DisplayName("是否需要过账")]
        [ReadOnlyAttribute(false)]
        public bool isNeedPost { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("如果读取载具需要请求Lot信息，需要进行设定")]
        [DisplayName("读取载具是否请求Lot")]
        [ReadOnlyAttribute(true)]
        public bool isCarrierIDRequestLotInfo { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("By Lot清线时设定，如果Lot不同，全线清线")]
        [DisplayName("Lot不同时全线清线")]
        [ReadOnlyAttribute(true)]
        public bool isAllrocessCompletionByLot { get; set; }

        [CategoryAttribute("控制信息"), DescriptionAttribute("By 工单清线时设定，如果工单不同，清线")]
        [DisplayName("工单不同时清线")]
        [ReadOnlyAttribute(true)]
        public bool isAllrocessCompletionByWorkOrder { get; set; }

        [CategoryAttribute("控制信息"), DescriptionAttribute("MES主动下载生产任务后，EAP开始从哪一台设备开始下载生产任务")]
        [DisplayName("下载开始设备")]
        [ReadOnlyAttribute(false)]
        public string StartEquipmentID { get; set; }

        [CategoryAttribute("控制信息"), DescriptionAttribute("设备上报Scan Code检查")]
        [DisplayName("检查扫码信息")]
        [ReadOnlyAttribute(true)]
        public bool isCheckScanCodeReport { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("投板机CCD扫码后，EAP要记录扫码后的ID，供Process Data上报时使用。Hold AOI")]
        [DisplayName("记录板号")]
        [ReadOnlyAttribute(true)]
        public bool isQPanelList { get; set; }

        [CategoryAttribute("控制信息"), DescriptionAttribute("是否检查所有设备状态")]
        [DisplayName("状态检查开关")]
        [ReadOnlyAttribute(false)]
        public bool isCheckEquipmentStatus { get; set; }
        [CategoryAttribute("控制信息"), DescriptionAttribute("是否检查投板机正在投板状态")]
        [DisplayName("投板检查开关")]
        [ReadOnlyAttribute(false)]
        public bool isCheckLoadStatus { get; set; }
        /// <summary>
        /// false:自动下载生产任务，true:手动下载生产任务
        /// </summary>
        [CategoryAttribute("控制信息"), DescriptionAttribute("false:自动下载生产任务，true:手动下载生产任务")]
        [DisplayName("手动下载任务开关")]
        [ReadOnlyAttribute(false)]
        public bool isManualJobDataDoanload { get; set; }

        public LineModel()
        {
            LineType = string.Empty;
            LineName = string.Empty;
            isInnerLotPanelList = false;
            isLoadCompleteCountCheck = false;
            isSubLot = false;
            isCheckBeforeTrackIn = false;
            isMultiLoadEquipment = false;
            isMainEqpID = false;
            isNeedPost = false;
            isCarrierIDRequestLotInfo = false;
            isAllrocessCompletionByLot = false;
            isAllrocessCompletionByWorkOrder = false;
            StartEquipmentID =string.Empty;
            isCheckScanCodeReport = false;
            isQPanelList = false;
            isCheckEquipmentStatus = false;
            isCheckLoadStatus = false;
            isManualJobDataDoanload = false;
        }
    }
    
}
