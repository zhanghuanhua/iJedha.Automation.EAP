//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Model
//   文件概要 : LotModel
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace iJedha.Automation.EAP.Model
{
    /// <summary>
    /// Lot Process Status枚举
    /// </summary>
    public enum eLotProcessStatus
    {
        /// <summary>
        /// 创建
        /// </summary>
        Create = 0,
        /// <summary>
        /// 就绪
        /// </summary>
        Ready = 1,
        /// <summary>
        /// 执行
        /// </summary>
        Run = 2,
        /// <summary>
        /// 完成
        /// </summary>
        Complete = 3,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 4
    }
   
    /// <summary>
    /// Data Source Type枚举
    /// </summary>
    public enum eDataSource
    {
        /// <summary>
        /// 自动
        /// </summary>
        Auto = 0,
        /// <summary>
        /// 手动
        /// </summary>
        Manual = 1
    }
    /// <summary>
    /// Run Type枚举
    /// </summary>
    public enum eRunType
    {
        Normal = 0,
        FirstInspWait = 1,
        FirstInspExt = 2,
        NormalDummy = 3,
        NormalLiveDummy = 4,
        FirstInspWaitDummy = 5,
        FirstInspExtDummy = 6,
        Fail = 7  //失败
    }
    /// <summary>
    /// Lot Type枚举
    /// </summary>
    public enum eLotType
    {
        /// <summary>
        /// 正常批次
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 虚拟批次
        /// </summary>
        Dummy = 1
    }
    /// <summary>
    /// 任务修改类型
    /// </summary>
    public enum eModifyType
    {
        /// <summary>
        /// 更新
        /// </summary>
        [Description("1")]
        Update = 1,
        /// <summary>
        /// 删除
        /// </summary>
        [Description("2")]
        Delete = 2,
        /// <summary>
        /// 更换
        /// </summary>
        [Description("3")]
        Change =3
    }
    /// <summary>
    /// 拆批，合批
    /// </summary>
    public enum eSCLot
    {
        /// <summary>
        /// 正常批
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 拆批
        /// </summary>
        Separate = 1,
        /// <summary>
        /// 合批
        /// </summary>
        Together = 2
    }

    /// <summary>
    /// Lot模块
    /// </summary>
    [Serializable]
    [CategoryAttribute("Basic"), DescriptionAttribute("Lot Model")]
    [ReadOnlyAttribute(true)]
    public class LotModel : ICloneable
    {
        //主设备ID 
        [CategoryAttribute("基本信息"), DescriptionAttribute("主设备")]
        [ReadOnlyAttribute(true)]
        public string MainEqpID { get; set; }
        //批次号 
        [CategoryAttribute("基本信息"), DescriptionAttribute("批次号")]
        [ReadOnlyAttribute(true)]
        public string LotID { get; set; }
        //端口号
        [CategoryAttribute("基本信息"), DescriptionAttribute("端口号")]
        [ReadOnlyAttribute(true)]
        public string PortID { get; set; }
        //载具ID 
        [CategoryAttribute("基本信息"), DescriptionAttribute("载具ID")]
        [ReadOnlyAttribute(true)]
        public string CarrierID { get; set; }
        //载具中Panel数量  
        [CategoryAttribute("基本信息"), DescriptionAttribute("载具中Panel数量")]
        [ReadOnlyAttribute(true)]
        public int PanelTotalQty { get; set; }
        //产品号
        [CategoryAttribute("基本信息"), DescriptionAttribute("产品号")]
        [ReadOnlyAttribute(true)]
        public string PN { get; set; }
        //Lot状态（0：等待上机；1：部分上机；2：等待下机；3：部分下机；4：等待发送；5：等待接收）
        [CategoryAttribute("基本信息"), DescriptionAttribute("Lots状态")]
        [ReadOnlyAttribute(true)]
        public string LotStatus { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("首件Lot ID")]
        [ReadOnlyAttribute(true)]
        public string FirstInspectLot { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("首件数量")]
        [ReadOnlyAttribute(true)]
        public string FirstInspQty { get; set; }

        [CategoryAttribute("基本信息"), DescriptionAttribute("Lot剩余数量")]
        [ReadOnlyAttribute(true)]
        public int LotQty { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("首件Flag")]
        [ReadOnlyAttribute(true)]
        public bool FirstInspFlag { get; set; }
        //前dummy数量 
        [CategoryAttribute("基本信息"), DescriptionAttribute("前dummy数量")]
        [ReadOnlyAttribute(true)]
        public string FrontDummyQty { get; set; }
        //后dummy数量 
        [CategoryAttribute("基本信息"), DescriptionAttribute("后dummy数量")]
        [ReadOnlyAttribute(true)]
        public string AfterDummyQty { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("批次生产状态")]
        [ReadOnlyAttribute(true)]
        public eLotProcessStatus LotProcessStatus { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("批次生产时间")]
        [ReadOnlyAttribute(true)]
        public DateTime ProcessTime { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("设备请求点")]
        [ReadOnlyAttribute(true)]
        public string LocalEQStation { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Local Port Station")]
        [ReadOnlyAttribute(true)]
        public string LocalPortStation { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Run Type")]
        [ReadOnlyAttribute(true)]
        public eRunType RunType { get; set; }
        //起始数（批次数量-搭配数*搭配次数）
        [CategoryAttribute("基本信息"), DescriptionAttribute("起始数")]
        [ReadOnlyAttribute(true)]
        public string LotStartQty { get; set; }
        //搭配数
        [CategoryAttribute("基本信息"), DescriptionAttribute("搭配数")]
        [ReadOnlyAttribute(true)]
        public string LotMatchQty { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Match Dummy Qty")]
        [ReadOnlyAttribute(true)]
        public string MatchDummyQty { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Panel List")]
        [ReadOnlyAttribute(true)]
        public List<PanelModel> PanelList { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("制程板列表")]
        [ReadOnlyAttribute(true)]
        public List<PanelModel> ProcessPanelList { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("批次参数列表")]
        [ReadOnlyAttribute(true)]
        public List<WIPDataModel> LotParameterList { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Sub Equipment List")]
        [ReadOnlyAttribute(true)]
        public List<SubEqp> SubEqpList { get; set; }

        [CategoryAttribute("基本信息"), DescriptionAttribute("内层Lot列表")]
        [ReadOnlyAttribute(true)]
        public List<InnerLotModel> InnerLotList { get; set; }

        [CategoryAttribute("基本信息"), DescriptionAttribute("开料LotList")]
        [ReadOnlyAttribute(true)]
        public List<Lot> LotList { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("错误信息")]
        [ReadOnlyAttribute(true)]
        public string ErrorMsg { get; set; }
        [CategoryAttribute("基本信息"), DescriptionAttribute("Data Source：Auto / Manual")]
        [ReadOnlyAttribute(true)]
        public eDataSource DataSource { get; set; }
        //产品版本 
        [CategoryAttribute("基本信息"), DescriptionAttribute("产品版本")]
        [ReadOnlyAttribute(true)]
        public string ProductRev { get; set; }
        // 合批 拆批旗标 正常：0，拆批：1，并批：2
        [CategoryAttribute("基本信息"), DescriptionAttribute("合批拆批旗标")]
        [ReadOnlyAttribute(true)]
        public eSCLot IntSCLot { get; set; }
        // Hold Reason
        [CategoryAttribute("基本信息"), DescriptionAttribute("批次Hold原因")]
        [ReadOnlyAttribute(true)]
        public string HoldReason { get; set; }
        //层别数
        [CategoryAttribute("基本信息"), DescriptionAttribute("层别数")]
        [ReadOnlyAttribute(true)]
        public string Layer { get; set; }
        //载具中Sheet数量
        [CategoryAttribute("基本信息"), DescriptionAttribute("载具中Sheet数量")]
        [ReadOnlyAttribute(true)]
        public string SheetTotalQty { get; set; }
        //收板最大数量
        [CategoryAttribute("基本信息"), DescriptionAttribute("收板最大数量")]
        [ReadOnlyAttribute(true)]
        public string UnloadQty { get; set; }
        //工单类型(暂定Dummy板类型为1，以后扩展再议)
        [CategoryAttribute("基本信息"), DescriptionAttribute("工单类型")]
        [ReadOnlyAttribute(true)]
        public eLotType IsDummyLot { get; set; }
        //板号起始号
        [CategoryAttribute("基本信息"), DescriptionAttribute("板号起始号")]
        [ReadOnlyAttribute(true)]
        public string PnlStartSN { get; set; }
        //任务编号
        [CategoryAttribute("基本信息"), DescriptionAttribute("任务编号")]
        [ReadOnlyAttribute(true)]
        public string JobID { get; set; }
        //任务总数
        [CategoryAttribute("基本信息"), DescriptionAttribute("任务总数")]
        [ReadOnlyAttribute(true)]
        public string JobTotalQty { get; set; }
        /// <summary>
        /// 板长  当边长<=28.5，Y为长边，X为短边；当边长>28.5，Y为短边，X为长边；
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("板长")]
        [ReadOnlyAttribute(true)]
        public string PNLength { get; set; }
        /// <summary>
        /// 板宽  当边长<=28.5，Y为长边，X为短边；当边长>28.5，Y为短边，X为长边；
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("板宽")]
        [ReadOnlyAttribute(true)]
        public string PNWidth { get; set; }
        //板厚
        [CategoryAttribute("基本信息"), DescriptionAttribute("板宽")]
        [ReadOnlyAttribute(true)]
        public string PNThick { get; set; }
        //0：不旋转；1：旋转
        [CategoryAttribute("基本信息"), DescriptionAttribute("是否旋转")]
        [ReadOnlyAttribute(true)]
        public string IsRotate { get; set; }
        ////True:多批次 false：单批次
        [CategoryAttribute("基本信息"), DescriptionAttribute("是否多批次")]
        [ReadOnlyAttribute(true)]
        public bool IsMutiLot { get; set; }
        
        /// <summary>
        /// 翻面信息
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("翻面信息")]
        [ReadOnlyAttribute(true)]
        public string IsTurnoverGroup { get; set; }
        /// <summary>
        /// 冲孔信息
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("冲孔信息")]
        [ReadOnlyAttribute(true)]
        public string IsSkipPunchingGroup { get; set; }
        
        /// <summary>
        ///  需跳过设备，不生产
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("需跳过设备")]
        [ReadOnlyAttribute(true)]
        public string SkipSubEqpID { get; set; }

        /// <summary>
        /// Dummy类型 A,B,C 
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("Dummy类型")]
        [ReadOnlyAttribute(true)]
        public string DummyType { get; set; }
        /// <summary>
        /// 投片数量
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("投片数量")]
        [ReadOnlyAttribute(true)]
        public string LoadQty { get; set; }
        /// <summary>
        /// 融铆合PP上料口任务数（芯板与芯板之间，芯板与铜箔之间）
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("融铆合PP任务数")]
        [ReadOnlyAttribute(true)]
        public string MaterialPortTaskQty { get; set; }

        /// <summary>
        /// 1：正向；0：逆向
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("1：正向；0：逆向")]
        [ReadOnlyAttribute(true)]
        public string Direction { get; set; }

        /// <summary>
        /// 1：连线；0：非连线
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("1：连线；0：非连线")]
        [ReadOnlyAttribute(true)]
        public string IsConnectionStatus { get; set; }
        /// <summary>
        /// 冲孔连棕化收的片数组合：60,60,60,30(尾号数)
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("冲孔连棕化收的片数组合：60,60,60,30(尾号数)")]
        [ReadOnlyAttribute(true)]
        public string UnloadQtyGroup { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("工单号")]
        [ReadOnlyAttribute(true)]
        public string WorkOrder { get; set; }
        /// <summary>
        /// 工单号数
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("工单数")]
        [ReadOnlyAttribute(true)]
        public string WorkOrderPNLQty { get; set; }
        /// <summary>
        /// 冲孔信息组合
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("冲孔信息组合")]
        [ReadOnlyAttribute(true)]
        public string PunchingInfo { get; set; }
        /// <summary>
        /// 冲孔棕化内层总数
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("冲孔棕化内层总数")]
        [ReadOnlyAttribute(true)]
        public int InnerLotTotalQty { get; set; }

        /// <summary>
        /// MES下载设备信息
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("MES设备信息")]
        [ReadOnlyAttribute(true)]
        public string MESInnerLotSubEqInfo { get; set; }

        /// <summary>
        /// 冲孔信息组合
        /// </summary>
        [CategoryAttribute("基本信息"), DescriptionAttribute("冲孔信息组合")]
        [ReadOnlyAttribute(true)]
        public List<string> PunchingList { get; set; }
        /// <summary>
        /// 层次 0203   0405
        /// </summary>
        public List<string> LayerLevel { get; set; }

        /// <summary>
        /// MES下载内层设备列表
        /// </summary>
        public List<string> MESInnerLotSubEqList { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        public LotModel()
        {
            MainEqpID = string.Empty;
            PortID = string.Empty;
            LotID = CarrierID = string.Empty;
            LocalEQStation = string.Empty;
            PN = string.Empty;
            LotStatus = string.Empty;
            FirstInspectLot = string.Empty;
            FirstInspQty = string.Empty;
            LotQty = 0;
            FrontDummyQty = string.Empty;
            AfterDummyQty = string.Empty;
            RunType = eRunType.Normal;
            LotStartQty = string.Empty;
            LotMatchQty = string.Empty;
            MatchDummyQty = string.Empty;
            ProductRev = string.Empty;
            IntSCLot = eSCLot.Normal;
            HoldReason = string.Empty;
            SheetTotalQty = string.Empty;
            UnloadQty = string.Empty;
            IsDummyLot = eLotType.Normal;
            PnlStartSN = string.Empty;
            JobID = string.Empty;
            JobTotalQty = string.Empty;
            PNLength = string.Empty;
            PNWidth = string.Empty;
            IsRotate = string.Empty;
            IsTurnoverGroup = string.Empty;
            IsSkipPunchingGroup = string.Empty;
            SkipSubEqpID = string.Empty;
            DummyType = string.Empty;
            LoadQty = string.Empty;
            IsMutiLot = false;
            ErrorMsg = LocalPortStation = string.Empty;
            MaterialPortTaskQty = string.Empty;
            DataSource = eDataSource.Manual;
            PanelTotalQty = 0;
            PanelList = new List<PanelModel>();
            ProcessPanelList = new List<PanelModel>();
            LotParameterList = new List<Model.WIPDataModel>();
            SubEqpList = new List<Model.SubEqp>();
            InnerLotList = new List<InnerLotModel>();
            Layer = string.Empty;
            LotList = new List<Lot>();
            Direction = string.Empty;
            IsConnectionStatus = string.Empty;
            UnloadQtyGroup = string.Empty;
            PunchingInfo = string.Empty;
            WorkOrder = string.Empty;
            WorkOrderPNLQty = string.Empty;
            InnerLotTotalQty = 0;
            MESInnerLotSubEqInfo = string.Empty;

        }
        public object Clone()
        {
            LotModel lot = (LotModel)this.MemberwiseClone();
            lot.LotParameterList = LotParameterList;
            lot.SubEqpList = SubEqpList;
            foreach (var v in PanelList)
            {
                lot.ProcessPanelList.Add(v);
            }
            return lot;
        }
        public LotModel DeepClone()
        {
            object obj = null;
            //将对象序列化成内存中的二进制流
            BinaryFormatter inputFormatter = new BinaryFormatter();
            MemoryStream inputStream;
            using (inputStream = new MemoryStream())
            {
                inputFormatter.Serialize(inputStream, this);
            }
            //将二进制流反序列化为对象
            using (MemoryStream outputStream = new MemoryStream(inputStream.ToArray()))
            {
                BinaryFormatter outputFormatter = new BinaryFormatter();
                obj = outputFormatter.Deserialize(outputStream);
            }
            return (LotModel)obj;
        }
        /// <summary>
        /// 根据Panel号检查Panel信息是否存在
        /// </summary>
        /// <param name="panelid"></param>
        /// <returns></returns>
        public bool CheckPanelExist(string panelid)
        {
            PanelModel panel = (from n in PanelList where n.PanelID.ToString() == panelid select n).FirstOrDefault();
            if (panel != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 根据Panel号获取Panel信息
        /// </summary>
        /// <param name="panelid"></param>
        /// <returns></returns>
        public PanelModel GetPanelModel(string panelid)
        {
            PanelModel panel = (from n in PanelList where n.PanelID.ToString() == panelid select n).FirstOrDefault();
            return panel;
        }
    }
    /// <summary>
    /// 下载参数模型
    /// </summary>
    [Serializable]
    public class ParameterModel : ICloneable
    {
        public string ItemName { get; set; }//参数名称
        public string ItemValue { get; set; }//参数值
        public string DataType { get; set; }//项目类型
        public string ItemCode { get; set; }//项目代码
        public ParameterModel()
        {
            ItemName = string.Empty;
            ItemValue = string.Empty;
            DataType = string.Empty;
            ItemCode = string.Empty;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
    /// <summary>
    /// 参数模型for Track Out WIP Data Report
    /// </summary>
    [Serializable]
    public class WIPDataModel : ICloneable
    {

        public string ItemID { get; set; }//项目号
        public string WIPDataName { get; set; }//项目名称
        public string DefaultValue { get; set; }//项目值
        public string ItemMaxValue { get; set; }//项目最大值
        public string ItemMinValue { get; set; }//项目最小值
        public string DataType { get; set; }//项目类型
        public string ServiceName { get; set; }//服务名称
        public string SubEqpID { get; set; }//设备号
        public string TraceFactor { get; set; }//倍数
        public WIPDataModel()
        {
            ItemID = string.Empty;
            WIPDataName = string.Empty;
            DefaultValue = string.Empty;
            ItemMaxValue = string.Empty;
            ItemMinValue = string.Empty;
            DataType = string.Empty;
            ServiceName = string.Empty;
            SubEqpID = string.Empty;
            TraceFactor = string.Empty;

        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }

    [Serializable]
    public class SubEqp : ICloneable
    {
        public string SubEqpID { get; set; }
        public List<ParameterModel> EQParameter { get; set; }
        public SubEqp()
        {
            SubEqpID = string.Empty;
            EQParameter = new List<ParameterModel>();

        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    [Serializable]
    public class InnerLotModel : ICloneable
    {
        public string InnerLotID { get; set; }//内层Lot ID
        public string InnerLayer { get; set; }//层别
        public string LoadQty { get; set; }//内层数量
        public string IsTurnover { get; set; }//翻面
        public string IsSkipPunching { get; set; }//跳过
        public string SubEqpID { get; set; }//子设备ID
        public string MaterialSeq { get; set; }//物料序号
        public List<PanelModel> ListPanel { get; set; }//Panel列表
        public InnerLotModel()
        {
            InnerLotID = string.Empty;
            InnerLayer = string.Empty;
            LoadQty = string.Empty;
            IsTurnover = string.Empty;
            IsSkipPunching = string.Empty;
            SubEqpID = string.Empty;
            ListPanel = new List<PanelModel>();
            MaterialSeq = string.Empty;

        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    [Serializable]
    public class Lot : ICloneable
    {
        public int LotSeq { get; set; }//Lot序号
        public string PN { get; set; }//料号
        public string LotID { get; set; }//LotID
        public string LotStatus { get; set; }//Lot状态
        public string LoadQty { get; set; }//投片数量
        public string ProductRev { get; set; }//产品版本
        public int PanelTotalQty { get; set; }//Panel总数
        public string PNLength { get; set; }//板长
        public string PNWidth { get; set; }//板宽
        public string IsRotate { get; set; }//是否旋转
        public string ErrorMsg { get; set; }
        public string PortID { get; set; }//端口ID
        public string WorkOrder { get; set; }//工单号
        public DateTime ProcessTime { get; set; }//制程开始时间
        public eLotProcessStatus LotProcessStatus { get; set; }//Lot状态
        public List<WIPDataModel> LotParameterList { get; set; }//参数列表
        public List<SubEqp> SubEqpList { get; set; }//子设备列表
        public List<PanelModel> PanelList { get; set; }//Panel列表
        public Lot()
        {
            LotSeq = 0;
            PN = string.Empty;
            LotID = string.Empty;
            LotStatus= string.Empty; 
            LoadQty= string.Empty; 
            ProductRev= string.Empty; 
            PanelTotalQty= 0 ;
            PNLength= string.Empty; 
            PNWidth= string.Empty; 
            IsRotate= string.Empty;
            ErrorMsg = string.Empty;
            PortID = string.Empty;
            WorkOrder = string.Empty;
            ProcessTime = DateTime.Now;
            LotParameterList = new List<WIPDataModel>();
            SubEqpList = new List<SubEqp>();
            PanelList = new List<PanelModel>();
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}
