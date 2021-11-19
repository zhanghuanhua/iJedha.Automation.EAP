//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Model
//   文件概要 : EquipmentModel
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace iJedha.Automation.EAP.Model
{
    /// <summary>
    /// 设备的连线状态
    /// </summary>
    public enum eConnectMode
    {
        /// <summary>
        /// 断线
        /// </summary>
        DISCONNECT = 0,
        /// <summary>
        /// 连线
        /// </summary>
        CONNECT = 1
    }
    /// <summary>
    /// 设备的连线状态
    /// </summary>
    public enum eEquipmentType
    {
        /// <summary>
        /// 投板机或者单机上料口
        /// </summary>
        L = 0,
        /// <summary>
        /// 收板机或者单机下料口
        /// </summary>
        U = 1,
        /// <summary>
        /// 检查机
        /// </summary>
        I = 2,
        /// <summary>
        /// 制程机
        /// </summary>
        P = 3,
        /// <summary>
        /// 传送机
        /// </summary>
        T = 4,
        /// <summary>
        /// 首件检查机
        /// </summary>
        IE = 5
    }
    /// <summary>
    /// 设备的控制状态
    /// </summary>
    public enum eControlMode
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 离线模式
        /// </summary>
        LOCAL = 1,
        /// <summary>
        /// 在线模式
        /// </summary>
        REMOTE = 2
    }
    /// <summary>
    /// 远程控制命令
    /// </summary>
    public enum eRemoteCommand
    {
        /// <summary>
        /// 未知
        /// </summary>
        [Description("0")]
        UNKNOW = 0,
        /// <summary>
        /// 开始命令
        /// </summary>
        [Description("1")]
        Start = 1,
        /// <summary>
        /// 停止命令
        /// </summary>
        [Description("2")]
        Stop = 2,
        /// <summary>
        /// 暂停命令
        /// </summary>
        [Description("3")]
        Pause = 3,
        /// <summary>
        /// 复机命令
        /// </summary>
        [Description("4")]
        Resume = 4,
        /// <summary>
        /// 打开暂存机命令
        /// </summary>
        [Description("5")]
        OpenBuffer = 5,
        /// <summary>
        /// 关闭暂存机命令
        /// </summary>
        [Description("6")]
        CloseBuffer = 6,
        /// <summary>
        /// 通知机台下料
        /// </summary>
        [Description("7")]
        UnloadCarrier = 7,
        /// <summary>
        /// 通知Panel正常
        /// </summary>
        [Description("8")]
        PanelOK = 8,
        /// <summary>
        /// 通知Panel异常
        /// </summary>
        [Description("9")]
        PanelNG = 9,
        /// <summary>
        /// AGV派送完成
        /// </summary>
        [Description("10")]
        AGVTransferComplete = 10,
        /// <summary>
        /// 首件OK
        /// </summary>
        [Description("11")]
        InspectOK = 11,
        /// <summary>
        /// 首件NG
        /// </summary>
        [Description("11")]
        InspectNG = 12
    }

    /// <summary>
    /// 操作模式
    /// </summary>
    public enum eOperationMode
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 手动
        /// </summary>
        MANUAL = 1,
        /// <summary>
        /// 自动
        /// </summary>
        AUTO = 2
    }


    /// <summary>
    /// 机台状态
    /// </summary>
    public enum eEQSts
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 运行
        /// </summary>
        Run = 1,
        /// <summary>
        /// 暂停
        /// </summary>
        Pause = 2,
        /// <summary>
        /// 待机
        /// </summary>
        Idle = 3,
        /// <summary>
        /// 宕机
        /// </summary>
        Down = 4,
        /// <summary>
        /// 保养
        /// </summary>
        PM = 5,
        /// <summary>
        /// 就绪
        /// </summary>
        Ready = 6
    }


    public enum eLotinfo
    {
        WaitingUp = 0, // 待上机
        PartUp = 2, // 上机
        WaitiongLower = 4,  //下机     
    }


    public enum eProtocol
    {
        /// <summary>
        /// 无
        /// </summary>
        NULL = 0,
        /// <summary>
        /// SECS/GEM通讯协议
        /// </summary>
        SECS = 1,
        /// <summary>
        /// Socket(TCP/IP)协议
        /// </summary>
        SOCKET = 2
    }
    /// <summary>
    /// 绿灯运行情况
    /// </summary>
    public enum eGreenTower
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 开灯
        /// </summary>
        LIGHTON = 1,
        /// <summary>
        /// 闪亮
        /// </summary>
        FLASH = 2,
        /// <summary>
        /// 关灯
        /// </summary>
        LIGHTOFF = 3
    }
    public enum eYellowTower
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 开灯
        /// </summary>
        LIGHTON = 1,
        /// <summary>
        /// 闪亮
        /// </summary>
        FLASH = 2,
        /// <summary>
        /// 关灯
        /// </summary>
        LIGHTOFF = 3
    }
    public enum eRedTower
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 开灯
        /// </summary>
        LIGHTON = 1,
        /// <summary>
        /// 闪亮
        /// </summary>
        FLASH = 2,
        /// <summary>
        /// 关灯
        /// </summary>
        LIGHTOFF = 3
    }
    public enum eProcessCode
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOW = 0,
        /// <summary>
        /// 创建
        /// </summary>
        Create = 1,
        /// <summary>
        /// 新增
        /// </summary>
        Update = 2,
        /// <summary>
        /// 删除
        /// </summary>
        Delete = 3,
        /// <summary>
        /// 开始
        /// </summary>
        Start = 4,
        /// <summary>
        /// 完成
        /// </summary>
        Complete = 5
    }
    public enum eEventName
    {
        UNKNOW = 0,
        /// <summary>
        /// 设备报上料请求
        /// </summary>
        EQP_LOADREQUEST = 1,
        /// <summary>
        /// 设备报上料完成
        /// </summary>
        EQP_LOADCOMPLETE = 2,
        /// <summary>
        /// 设备报下料请求
        /// </summary>
        EQP_UNLOADREQUEST = 3,
        /// <summary>
        /// 设备报下料完成
        /// </summary>
        EQP_UNLOADCOMPLETE = 4,
        /// <summary>
        /// EAP请求生产任务
        /// </summary>
        EAP_LOTINFOREQUEST = 5,
        /// <summary>
        /// EAP上机过账
        /// </summary>
        EAP_TRACKIN = 6,
        /// <summary>
        /// EAP下机过账
        /// </summary>
        EAP_TRACKOUT = 7,
        /// <summary>
        /// MES下载生产任务
        /// </summary>
        MES_TRACKININFO=8,
        /// <summary>
        /// AGV安全信号请求
        /// </summary>
        AGV_SAFETYSIGNALREQUEST=9,
        /// <summary>
        /// AGV搬送完成
        /// </summary>
        AGV_TRANSFERCOMPLETEREPORT=10


    }
    public enum eEventFlow
    {
        /// <summary>
        /// 设备->EAP
        /// </summary>
        EQP2EAP = 0,
        /// <summary>
        /// EAP->设备
        /// </summary>
        EAP2EQP = 1,
        /// <summary>
        /// EAP->MES
        /// </summary>
        EAP2MES = 2,
        /// <summary>
        /// MES->EAP
        /// </summary>
        MES2EAP = 3,
        /// <summary>
        /// AGV->EAP
        /// </summary>
        AGV2EAP=4,
    }

    [Serializable]
    public partial class EquipmentModel
    {
        #region 基本信息
        /// <summary>
        /// 设备编号
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备编号")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int EQNo { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备ID")]
        [ReadOnlyAttribute(true)]
        public string EQID { get; set; }
        /// <summary>
        /// 子设备ID
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("子设备ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string SUBEQID { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备名称")]
        [ReadOnlyAttribute(true)]
        public string EQName { get; set; }
        /// <summary>
        /// 设备定义别名
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备定义别名")]
        [ReadOnlyAttribute(true)]
        public string EqNameKey { get; set; }
        /// <summary>
        /// 设备类型(L：投板机 / P：制程设备 / I：量测设备 / U：收板机)
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备类型(L：投板机 / P：制程设备 / I：量测设备 / U：收板机 / T：传送机)")]
        [ReadOnlyAttribute(true)]
        public eEquipmentType Type { get; set; }
        /// <summary>
        /// 端口编号(L01：上料口 / U01：下料口)
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("端口编号(L01：上料口 / U01：下料口)")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string PortIDList
        {
            get
            {
                return portIDList;
            }

            set
            {
                portIDList = value;
                if (portIDList == string.Empty)
                {
                    return;
                }
                string[] sl = portIDList.Split(',');
                foreach (string s in sl)
                {
                    List_Port.Add(s, new PortModel());
                }

            }
        }
        private string portIDList;
        /// <summary>
        /// 端口物件群组
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("端口物件群组")]
        [ReadOnlyAttribute(true)]
        public Dictionary<string, PortModel> List_Port { get; set; }
        /// <summary>
        /// 通讯方式
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("通讯方式")]
        [ReadOnlyAttribute(true)]
        public eProtocol Protocol { get; set; }

        #endregion
        /// <summary>
        /// 批次列表
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("批次列表")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public Dictionary<string, LotModel> LotList { get; set; }
        /// <summary>
        /// MES Download Lot PanelID List Info
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("MES Download Lot PanelID List Info")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public Dictionary<string, List<string>> MES_LotList { get; set; }
        /// <summary>
        /// 底盘绑定信息
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("底盘绑定信息")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public Dictionary<string, string> TrayStatusInfo { get; set; }

        /// <summary>
        /// EQ<->HOST Connect Mode
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("EQ<->HOST Connect Mode")]
        [ReadOnlyAttribute(true)]
        public eConnectMode ConnectMode { get; set; }

        /// <summary>
        /// 之前连接状态
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("Old Connect Mode")]
        [ReadOnlyAttribute(true)]
        public eConnectMode OldConnectMode { get; set; }

        /// <summary>
        /// EQ<->HOST Control Mode
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("EQ<->HOST Control Mode")]
        [ReadOnlyAttribute(true)]
        public eControlMode ControlMode { get; set; }
        /// <summary>
        /// EQ<->HOST PP-Select Change Result
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("EQ<->HOST PP-Select Change Result")]
        [ReadOnlyAttribute(true)]
        public eCheckResult JobDataDownloadChangeResult { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设备状态")]
        [ReadOnlyAttribute(true)]
        public eEQSts EQStatus { get; set; }

        /// <summary>
        /// Online Scenario Step
        /// </summary>
        //[CategoryAttribute("状态"), DescriptionAttribute("Online Scenario Step")]
        //[ReadOnlyAttribute(true)]
        //public eOnlineScenarioStep OnlineScenarioStep { get; set; }

        /// <summary>
        /// 当前Panel ID
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("当前Panel ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string CurrentPanelID { get; set; }

        /// <summary>
        /// 当前Lot ID
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("当前Lot ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string CurrentLotID { get; set; }

        /// <summary>
        /// 当前完成Lot ID
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("当前完成Lot ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string CurrentCompleteLotID { get; set; }

        /// <summary>
        /// 最后Lot ID
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("最后Lot ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string LastLotID { get; set; }

        /// <summary>
        /// 当前产品ID
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("当前产品ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string CurrentPN { get; set; }

        /// <summary>
        /// 前一产品ID
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("前一产品ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string OldPN { get; set; }
        /// <summary>
        /// 前一工单号
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("前一工单号")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string OldWorkOrder { get; set; }
        

        /// <summary>
        /// 设备Hold状态   Ture: Equipment Hold / False: Equipment Not Hold
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("Ture: Equipment Hold / False: Equipment Not Hold")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isEquipmentHold { get; set; }

        /// <summary>
        /// 设备当前Panel数量
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设备当前Panel数量")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int PanelCount { get; set; }

        /// <summary>
        /// 进板数量
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("进板数量")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int PanelInCount { get; set; }

        /// <summary>
        /// 出板数量
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("出板数量")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int PanelOutCount { get; set; }

        /// <summary>
        /// 设备总Panel数量
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设备总Panel数量")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int TotalPanelCount { get; set; }

        /// <summary>
        /// 设备生产Panel数量
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设备生产Panel数量")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int ProcessPanelCount { get; set; }

        /// <summary>
        /// 设备当前操作模式
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设备当前操作模式")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public eOperationMode OperationMode { get; set; }

        /// <summary>
        /// Green Tower
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("Green Tower")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public eGreenTower GreenTower { get; set; }
        /// <summary>
        /// Yellow Tower
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("Yellow Tower")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public eYellowTower YellowTower { get; set; }
        /// <summary>
        /// Red Tower
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("Red Tower")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public eRedTower RedTower { get; set; }
        /// <summary>
        /// 清设备完成
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("清设备完成")]
        [ReadOnlyAttribute(true)]
        public bool isProcessCompletion { get; set; }
        /// <summary>
        /// 检查清整线
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("检查清整线")]
        [ReadOnlyAttribute(true)]
        public bool isAllrocessCompletion { get; set; }

        /// <summary>
        /// 设定检查清线设备
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定检查清线设备")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<string> ProcessCompletionEQ { get; set; }
        /// <summary>
        /// 设定任务下载设备
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定任务下载设备")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<string> DataDownloadEQ { get; set; }
        /// <summary>
        /// CCD读码下载任务
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("CCD读码下载任务")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isCCDDownloadData { get; set; }

        /// <summary>
        /// CCD读码下载设备任务
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("CCD读码下载设备任务")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string CCDDownloadDataEQ { get; set; }

        /// <summary>
        /// CCD读码变更任务
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("CCD读码变更任务")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isCCDChangeData { get; set; }

        /// <summary>
        /// 配方是否变更
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("配方是否变更")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isRecipeChange { get; set; }

        /// <summary>
        /// 设定任务下载回复方式。0:Socket直接回复;1:任务进展报告[建立生产任务];2:配方调用报告
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定任务下载回复方式")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string JobDataDownloadReplyCondition { get; set; }
        /// <summary>
        /// 设定是否下载PanelList
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定是否下载PanelList")]
        [ReadOnlyAttribute(false)]
        [Browsable(true)]
        public bool isDownloadPanelList { get; set; }
        /// <summary>
        /// 设定是否上报给MESLoadComplete
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定是否上报给MESLoadComplete")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isReportLoadComplete { get; set; }
        /// <summary>
        /// 设定是否下载内层Lot
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定是否下载内层Lot")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isDownloadInnerLot { get; set; }
        /// <summary>
        /// 设定是否下载生产任务
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("是否需要下载生产任务。true:不需要下载，false:需要下载")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isNotDownloadJob { get; set; }
        

        /// <summary>
        /// 设定是否启用检查层别
        /// </summary>
        [CategoryAttribute("检查项"), DescriptionAttribute("设定是否启用检查层别")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isCheckLayerNo { get; set; }
        /// <summary>
        /// 翻面规则
        /// </summary>
        [CategoryAttribute("检查项"), DescriptionAttribute("翻面规则")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isTurnoverGroup { get; set; }

        /// <summary>
        /// 设定是否为检测设备
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定是否为检测设备")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isInspectEquipment { get; set; }

        /// <summary>
        /// 是否为多端口,目前PP裁切使用
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("是否为多端口")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isMultiPort { get; set; }
        /// <summary>
        /// 设定是否为请求物料
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设定是否为请求物料")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isMaterialLoadComplete { get; set; }

        /// <summary>
        /// 扫描底盘下载生产任务，压机
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("扫描底盘下载生产任务")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isCarrierDownloadData { get; set; }

        /// <summary>
        /// 非连线工艺时，下载生产任务给设备 【前处理塞孔喷涂】
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("非连线工艺时，下载生产任务给设备 【前处理塞孔喷涂】")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isDisconnectionDataDownload { get; set; }

        /// <summary>
        /// 设备任务进展报告
        /// </summary>
        [CategoryAttribute("状态"), DescriptionAttribute("设备任务进展报告")]
        [ReadOnlyAttribute(true)]
        public eProcessCode ProcessStatus { get; set; }
        ///// <summary>
        ///// Link设备
        ///// </summary>
        //[CategoryAttribute("基本信息"), DescriptionAttribute("Link设备")]
        //[ReadOnlyAttribute(false)]
        //public bool isInitialLink { get; set; }

        [CategoryAttribute("基本信息"), DescriptionAttribute("当前PN")]
        [ReadOnlyAttribute(false)]
        [Browsable(false)]
        public bool isCurrentPN { get; set; }


        #region [Recipe]
        [CategoryAttribute("配方"), DescriptionAttribute("当前设备配方")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string PPID { get; set; } = string.Empty;

        [CategoryAttribute("配方"), DescriptionAttribute("下载设备配方")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string DownloadPPID { get; set; } = string.Empty;

        [CategoryAttribute("配方"), DescriptionAttribute("CAM档路径")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string CamPath { get; set; } = string.Empty;
        [CategoryAttribute("配方"), DescriptionAttribute("配方档全路径")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string RecipePath { get; set; } = string.Empty;

        [CategoryAttribute("配方"), DescriptionAttribute("检查配方参数")]
        [ReadOnlyAttribute(false)]
        [Browsable(false)]
        public bool isCheckRecipeParameter { get; set; }
        /// <summary>
        /// 如果不需下载配方任务，则为false
        /// </summary>
        [CategoryAttribute("配方"), DescriptionAttribute("false:不需下载配方参数,true:下载配方参数")]
        [ReadOnlyAttribute(false)]
        [Browsable(false)]
        public bool isRecipeParameterDownload { get; set; }
        #endregion

        #region [TraceData]
        [CategoryAttribute("检查项"), DescriptionAttribute("false:不检查关键参数,true:检查关键参数")]
        [ReadOnlyAttribute(false)]
        public bool isCheckTraceData { get; set; }

        //[CategoryAttribute("Trace Data"), DescriptionAttribute("Is Need to Generate History")]
        //[ReadOnlyAttribute(false)]
        //[Browsable(false)]
        //public bool isGenerateHistory { get; set; }

        [CategoryAttribute("Trace Data"), DescriptionAttribute("数据收集时间间隔(min)")]
        [ReadOnlyAttribute(false)]
        public int TraceDataTimer { get; set; }

        [CategoryAttribute("Trace Data"), DescriptionAttribute("关键数据收集时间")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public DateTime TraceDataCollectTime { get; set; }

        //public DateTime TraceDataCollectTimeTest { get; set; }

        //[CategoryAttribute("Trace Data"), DescriptionAttribute("Define Trace Data Spec")]
        //[ReadOnlyAttribute(true)]
        //[Browsable(false)]
        //public List<TraceDataModelBase> List_DefineTraceDataSpec { get; set; }

        [CategoryAttribute("Trace Data"), DescriptionAttribute("Key Trace Data Spec")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<WIPDataModel> List_KeyTraceDataSpec { get; set; }
        #endregion
        #region [Remote Control]

        [CategoryAttribute("检查项"), DescriptionAttribute("Is Need to Control Mode")]
        [ReadOnlyAttribute(false)]
        public bool isControlModeSelect { get; set; }
        #endregion
        [CategoryAttribute("Other"), DescriptionAttribute("DATA ID")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int DATAID { get; set; }

        [CategoryAttribute("Other"), DescriptionAttribute("Retry Count")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public int RetryCount { get; set; }


        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<byte> SocketMsgBuffer { get; set; } = new List<byte>();
        //public ConcurrentQueue<List<byte>> qSocketMsgBuffer { get; set; } = new ConcurrentQueue<List<byte>>();
        [CategoryAttribute("基本信息"), DescriptionAttribute("重连次数")]
        [ReadOnlyAttribute(true)]

        public int SetConnectCount { get; set; } = 0;
        [CategoryAttribute("基本信息"), DescriptionAttribute("Host类型")]
        [ReadOnlyAttribute(true)]
        public socket.SocketCommonData.HOSTMODE socketType { get; set; }

        /// <summary>
        /// 旧参数
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("旧参数")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<MessageModel.Param> OldEQParameter { get; set; }
        /// <summary>
        /// 下载参数
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("下载参数")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<MessageModel.Param> NewEQParameter { get; set; }
        /// <summary>
        /// 设备警报情况
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备警报情况")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public List<string> EquipmentAlarmList { get; set; }
        /// <summary>
        /// 叠板压合多投板机
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("叠板压合多投板机")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isMultiLoad { get; set; }

        /// <summary>
        /// 是否跳过设备
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("是否跳过设备")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isSkipEquipment { get; set; }

        /// <summary>
        /// 分板线四合一下载板起始号 
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("起始号")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isPnlStartSN { get; set; }

        /// <summary>
        /// VCP是否使用Dummy片 
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("是否使用Dummy片")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isUseDummy { get; set; }

        /// <summary>
        /// 是否下载冲孔信息给设备
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("是否下载冲孔信息给设备")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isPunching { get; set; }
        /// <summary>
        /// 是否为A陪镀板
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("是否为A陪镀板")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isAType { get; set; }
        /// <summary>
        /// 回流线标志
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("回流线")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public bool isReturnLine { get; set; }
        /// <summary>
        /// 投板机序号
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("回流线投板机序号")]
        [ReadOnlyAttribute(true)]
        [Browsable(false)]
        public string LoadEquipmentSequence { get; set; }

        /// <summary>
        /// 使用数据名称
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("false:使用DataCode，true:使用名称")]
        [ReadOnlyAttribute(false)]
        [Browsable(true)]
        public bool isUseDataName { get; set; }

        /// <summary>
        /// 设备商
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备商名称")]
        [ReadOnlyAttribute(false)]
        [Browsable(true)]
        public string EqVendor { get; set; }

        #region[检查开关]
        /// <summary>
        /// false:不检查连线,true:检查连线--连线检查
        /// </summary>
        [CategoryAttribute("检查项"), DescriptionAttribute("false:不检查连线,true:检查连线")]
        [ReadOnlyAttribute(false)]
        public bool isCheckConnect { get; set; }
        /// <summary>
        /// false:不检查控制模式,true:检查控制模式--在线、离线检查
        /// </summary>
        [CategoryAttribute("检查项"), DescriptionAttribute("false:不检查控制模式,true:检查控制模式")]
        [ReadOnlyAttribute(false)]
        public bool isCheckControlMode { get; set; }

        /// <summary>
        /// false:不检查Panel ID,true:检查Panel ID
        /// </summary>
        [CategoryAttribute("检查项"), DescriptionAttribute("false:不检查Panel ID,不下载停止投板指令,true:检查Panel ID")]
        [ReadOnlyAttribute(false)]
        public bool isCheckPanelID { get; set; }

        #endregion
        public EquipmentModel()
        {
            //List_DefineTraceDataSpec = new List<TraceDataModelBase>();
            List_KeyTraceDataSpec = new List<WIPDataModel>();
            List_Port = new Dictionary<string, PortModel>();
            LotList = new Dictionary<string, LotModel>();
            MES_LotList = new Dictionary<string, List<string>>();
            EQStatus = eEQSts.Down;
            ControlMode = eControlMode.UNKNOW;
            ConnectMode = eConnectMode.DISCONNECT;
            OldConnectMode = eConnectMode.DISCONNECT;
            OperationMode = eOperationMode.MANUAL;
            TraceDataCollectTime = DateTime.Now;
            //TraceDataCollectTimeTest = DateTime.Now;
            //OnlineScenarioStep = eOnlineScenarioStep.Initial;
            PortIDList = string.Empty;
            DATAID = 0;
            RetryCount = 0;
            PanelCount = 0;
            TotalPanelCount = 0;
            ProcessPanelCount = 0;
            PanelInCount = 0;
            PanelOutCount = 0;
            isEquipmentHold = false;
            isCurrentPN = false;
            CurrentPanelID = "";
            CurrentLotID = "";
            CurrentCompleteLotID = "";
            LastLotID = "";
            CurrentPN = "";
            OldPN = "";
            OldWorkOrder = "";
            Protocol = eProtocol.NULL;
            GreenTower = eGreenTower.UNKNOW;
            YellowTower = eYellowTower.UNKNOW;
            RedTower = eRedTower.UNKNOW;
            isProcessCompletion = false;
            isAllrocessCompletion = false;
            ProcessCompletionEQ = new List<string>();
            isCCDDownloadData = false;
            CCDDownloadDataEQ = string.Empty;
            isCCDChangeData = false;
            DataDownloadEQ = new List<string>();
            ProcessStatus = eProcessCode.UNKNOW;
            socketType = socket.SocketCommonData.HOSTMODE.HOSTMODE_SERVER;
            TraceDataTimer = 0;
            NewEQParameter = new List<MessageModel.Param>();
            OldEQParameter = new List<MessageModel.Param>();
            TrayStatusInfo = new Dictionary<string, string>();
            EquipmentAlarmList = new List<string>();
            isDownloadPanelList = false;
            isReportLoadComplete = false;
            isRecipeChange = false;
            isDownloadInnerLot = false;
            isNotDownloadJob = false;
            isCheckLayerNo = false;
            isTurnoverGroup = false;
            isInspectEquipment = false;
            isMultiPort = false;
            isCarrierDownloadData = false;
            isMaterialLoadComplete = false;
            isMultiLoad = false;
            isPnlStartSN = false;
            isSkipEquipment = false;
            isUseDummy = false;
            isDisconnectionDataDownload = false;
            isPunching = false;
            isReturnLine = false;
            LoadEquipmentSequence = string.Empty;
            isUseDataName = false;
            EqVendor = string.Empty;
            isCheckPanelID = false;
        }
        /// <summary>
        /// 获取DataID
        /// </summary>
        /// <returns></returns>
        public int GenerateDataID()
        {
            if (DATAID > 65535)
            {
                DATAID = 1;
            }
            else
            {
                DATAID++;
            }
            return DATAID;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public partial class EquipmentModel
    {
        public bool AddPortModel(PortModel port)
        {
            try
            {
                if (List_Port.ContainsKey(port.PortID.ToString()))
                {
                    List_Port.Remove(port.PortID.ToString());
                }
                List_Port.Add(port.PortID.ToString(), port);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 根据端口号取得端口内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PortModel GetPortModelByPortID(string id)
        {
            PortModel ret = null;
            try
            {
                PortModel port = (from n in List_Port.Values where n.PortID.ToString() == id select n).FirstOrDefault();
                return port;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// 根据载具号获取端口信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PortModel GetPortModelByCarrierID(string id)
        {
            PortModel port = (from n in List_Port.Values where n.CarrierID == id select n).FirstOrDefault();
            return port;
        }
        /// <summary>
        /// 根据端口号获取批次信息
        /// </summary>
        /// <param name="portid"></param>
        /// <returns></returns>
        public LotModel GetLotModel(string portid)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where l.LocalPortStation == portid select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据载具号与端口号获取批次信息
        /// </summary>
        /// <param name="portid"></param>
        /// <param name="carrierid"></param>
        /// <returns></returns>
        public LotModel GetLotModel(string portid, string carrierid)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where n.PortID.ToString() == portid && l.CarrierID == carrierid select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据端口号与批次号取得批次信息
        /// </summary>
        /// <param name="portid"></param>
        /// <param name="lotid"></param>
        /// <returns></returns>
        public LotModel GetLotModelByLotID(string portid, string lotid)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where n.PortID.ToString() == portid && l.LotID == lotid select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据载具号取得批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByCarrierID(string id)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where l.CarrierID == id select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据批次号取得批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByLotID(string id)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where l.LotID == id select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据批次号取得批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByFirstInspectLot(string id)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where l.FirstInspectLot == id select l).FirstOrDefault();
            return lot;
        }
        /// <summary>
        /// 根据Panel号取得批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByPanelID(string id)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values from w in l.PanelList where w.PanelID.ToString() == id select l).FirstOrDefault();
            return lot;
        }

        /// <summary>
        /// 根据Batch ID(JobID)取得批次信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LotModel GetLotModelByBatchID(string id)
        {
            LotModel lot = (from n in List_Port.Values from l in n.List_Lot.Values where l.JobID.ToString() == id select l).FirstOrDefault();
            return lot;
        }

        public IList<LotModel> GetAllLotModel()
        {
            var lot = (from n in List_Port.Values from l in n.List_Lot.Values select l).ToList();
            return lot;
        }


        /// <summary>
        /// 根据TRID获取TraceData的定义集
        /// </summary>
        /// <param name="trid"></param>
        /// <returns></returns>
        //public IList<TraceDataModelBase> GetAllDefineTraceData(int trid)
        //{
        //    try
        //    {
        //        //if (List_DefineTraceDataSpec == null)
        //        //{
        //        //    return null;
        //        //}
        //        //if (trid == 0)
        //        //{
        //        //    return (from o in List_DefineTraceDataSpec where o.isEnable == true select o).OrderBy(a => a.Sequence).ToList();
        //        //}
        //        //else
        //        //{
        //        //    return (from o in List_DefineTraceDataSpec where o.isEnable == true && o.TraceID == trid select o).OrderBy(a => a.Sequence).ToList();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}


    }
}
