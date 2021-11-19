using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Model
{
    public class MessageModel
    {
        public class AliveCheckRequest
        {
            [DataMember]//线体设备
            public string MainEqpID = string.Empty;
            [DataMember]//设备IP
            public string IPAddress = string.Empty;
        }
        public class LotInfoRequest
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//载具ID
            public string CarrierID = string.Empty;
            [DataMember]//板号
            public string PnlID = string.Empty;
            [DataMember]//LotID充当物料ID
            public string LotID = string.Empty;
        }
        public class MaterialLotInfoRequest
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
        }
        /// <summary>
        /// MaterialLotInfo内容物信息
        /// </summary>
        public class MaterialLotInfo
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//物料料号
            public string CarrierID = string.Empty;
            [DataMember]//1：正向；0：逆向
            public string Direction = string.Empty;
        }
        /// <summary>
        /// LotInfo内容物信息
        /// </summary>
        public class LotInfo
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//载具中Sheet数量
            public string SheetTotalQty = string.Empty;
            [DataMember]//任务编号
            public string JobID = string.Empty;
            [DataMember]//任务总数
            public string JobTotalQty = string.Empty;
            [DataMember]//True:多批次 false：单批次
            public bool IsMutiLot = false;
            [DataMember]//批次号
            public string LotID = string.Empty;
            [DataMember]//WIPData
            public List<InnerLot> InnerLotID = new List<InnerLot>();
            [DataMember]//Lot状态（0：等待上机；1：部分上机；2：等待下机；3：部分下机；4：等待发送；5：等待接收）                                                            
            public string LotStatus = string.Empty;
            [DataMember]//投片数量
            public string LoadQty = string.Empty;
            [DataMember]//收板最大数量
            public string UnloadQty = string.Empty;
            [DataMember]//起始数（批次数量-搭配数*搭配次数）                                                                                                                 
            public string LotStartQty = string.Empty;
            [DataMember]//搭配数                                                                                                                                             
            public string LotMatchQty = string.Empty;
            [DataMember]//板号起始号                                                                                                                                         
            public string PnlStartSN = string.Empty;
            [DataMember]//产品号                                                                                                                                             
            public string PN = string.Empty;
            [DataMember]//产品版本                                                                                                                                           
            public string ProductRev = string.Empty;
            [DataMember]//载具中Panel 数量                                                                                                                                   
            public string PanelTotalQty = string.Empty;
            [DataMember]//WIPData
            public List<WipData> WIPDataList = new List<WipData>();
            [DataMember]//Param
            public List<Param> ParamList { get; set; } =new List<Param>();
            [DataMember]//Panel
            public List<Panel> PanelList = new List<Panel>();
            [DataMember]//工单类型(暂定Dummy板类型为1，以后扩展再议)                                                                                                         
            public string IsDummyLot = string.Empty;
            [DataMember]//层别                                                                                                                                               
            public string Layer = string.Empty;
            [DataMember]//FirstInspect
            public List<FirstInspect> FirstInspect = new List<FirstInspect>();
            [DataMember]//A,B,C                                                                                                                                        
            public string DummyType = string.Empty;

            public int MyProperty { get; set; }

            [DataMember]//前dummy数量                                                                                                                                        
            public string FrontDummyQty = string.Empty;
            [DataMember]//后dummy数量                                                                                                                                        
            public string AfterDummyQty = string.Empty;
            [DataMember]//板长
            public string PNLength = string.Empty;
            [DataMember]//板宽
            public string PNWidth = string.Empty;
            [DataMember]//板厚
            public string PNThick = string.Empty;

            [DataMember]//0：不旋转；1：旋转
            public string IsRotate = string.Empty;
            [DataMember]//例00010
            public string IsTurnoverGroup = string.Empty;
            [DataMember]//例00010
            public string IsSkipPunchingGroup = string.Empty;
            [DataMember] //需跳过设备，不生产
            public string SkipSubEqpID = string.Empty;
            [DataMember] //融铆合PP上料口任务数（芯板与芯板之间，芯板与铜箔之间）
            public string MaterialPortTaskQty = string.Empty;
            [DataMember]//1：正向；0：逆向
            public string Direction = string.Empty;
            [DataMember]//1：连线；0：非连线（前处理塞孔）
            public string IsConnectionStatus = string.Empty;
            [DataMember]//冲孔连棕化收的片数组合：60,60,60,30(尾号数)
            public string UnloadQtyGroup = string.Empty;
            [DataMember]//工单号
            public string WorkOrder = string.Empty;

            [DataMember]//工单数量
            public string WorkOrderPNLQty = string.Empty;
            [DataMember]//冲孔棕化内层总数
            public string InnerLotTotalQty = string.Empty;
        }
        /// <summary>
        /// DownloadLotInfo内容物信息
        /// </summary>
        public class LotInfoKL
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//大板ID                                                                                                                                             
            public string JobID = string.Empty;
            [DataMember]//载具中Sheet数量
            public string SheetTotalQty = string.Empty;
            [DataMember]//True:多批次 false：单批次
            public bool IsMutiLot = false;
            [DataMember] //批次列表
            public List<Lot> LotIDList = new List<Lot>();
        }
        public class Lot
        {
            [DataMember]//料号
            public string PN = string.Empty;
            [DataMember]//批次号
            public string LotID = string.Empty;
            [DataMember]//Lot状态（0：等待上机；1：部分上机；2：等待下机；3：部分下机；4：等待发送；5：等待接收）                                                            
            public string LotStatus = string.Empty;
            [DataMember]//投片数量
            public string LoadQty = string.Empty;
            [DataMember]//产品版本                                                                                                                                           
            public string ProductRev = string.Empty;
            [DataMember]//载具中Panel 数量                                                                                                                                   
            public string PanelTotalQty = string.Empty;
            [DataMember]//WIPData
            public List<WipData> WIPDataList = new List<WipData>();
            [DataMember]//Param
            public List<Param> ParamList = new List<Param>();
            [DataMember]//Panel
            public List<Panel> PanelList = new List<Panel>();
            [DataMember]//板长
            public string PNLength = string.Empty;
            [DataMember]//板宽
            public string PNWidth = string.Empty;
            [DataMember]//0：不旋转；1：旋转
            public string IsRotate = string.Empty;
            [DataMember]//工单号
            public string WorkOrder = string.Empty;
            [DataMember]//冲孔棕化内层总数
            public string InnerLotTotalQty = string.Empty;

        }
        /// <summary>
        /// 内容物信息
        /// </summary>
        public class WipData
        {
            [DataMember]//参数项目“AdHocWIPData”, “TrackOutLot”
            public string ServiceName = string.Empty;
            [DataMember]//参数名称
            public string WIPDataName = string.Empty;
            [DataMember]//参数值属性 (String, Number)
            public string DataType = string.Empty;
            [DataMember]//默认值
            public string DefaultValue = string.Empty;
            [DataMember]//参数下限
            public string ItemMinValue = string.Empty;
            [DataMember]//参数上限
            public string ItemMaxValue = string.Empty;
            [DataMember]//子设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//倍数
            public string TraceFactor = string.Empty;
        }
        /// <summary>
        /// SubEqp内容物信息
        /// </summary>
        public class Param
        {
            [DataMember]//子设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//参数名称
            public string ParamName = string.Empty;
            [DataMember]//参数值
            public string ParamValue = string.Empty;
            [DataMember]//参数类型
            public string ParamType = string.Empty;
        }
        /// <summary>
        /// 内容物信息
        /// </summary>
        public class Panel
        {
            [DataMember]//PanelList的PanelID信息
            public string PanelID = string.Empty;
            [DataMember]//Panel的striplist信息
            public List<Strip> StripList { get; set; }
            [DataMember]//外码ID
            public string OutCode = string.Empty;
            [DataMember]//配套后套ID
            public List<BatchPnl> BatchPnlList { get; set; }
            [DataMember]//通孔二维码ID，六位流水号
            public string HolePnlID { get; set; }
        }
        /// <summary>
        ///Strip内容物信息
        /// </summary>
        public class Strip
        {
            [DataMember]//striplist的StripID
            public string StripID = string.Empty;
        }
        /// <summary>
        ///BatchPnl内容物信息
        /// </summary>
        public class BatchPnl
        {
            [DataMember]//板号
            public string PnlID = string.Empty;
        }
        /// <summary>
        /// 首件信息
        /// </summary>
        public class FirstInspect
        {
            [DataMember]//首检批号
            public string FirstInspectLot = string.Empty;
            [DataMember]//首检批次数量
            public string FirstInspectLotQty = string.Empty;
        }
        /// <summary>
        /// 内层批次信息
        /// </summary>
        public class InnerLot
        {
            [DataMember]//内层批次号
            public string InnerLotID = string.Empty;
            [DataMember]//内层层别
            public string InnerLayer = string.Empty;
            [DataMember]//内层数量
            public string LoadQty = string.Empty;
            [DataMember]//Panel
            public List<Panel> PanelList = new List<Panel>();
            [DataMember]//0：不翻面；1：翻面，冲孔/PP裁切
            public string IsTurnover = string.Empty;
            [DataMember]//1：冲孔；0：不冲孔
            public string IsSkipPunching = string.Empty;
            [DataMember]//对应上料口位置
            public string SubEqpID = string.Empty;
            [DataMember]//物料序号
            public string MaterialSeq = string.Empty;

        }
      
        public class AlarmReport
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//故障代码
            public string ErrorCode = string.Empty;
            [DataMember]//故障原因
            public string ErrorReason = string.Empty;
            [DataMember]//故障发生/故障解除
            public string ErrorAction = string.Empty;
            [DataMember]//故障程度
            public string ErrorLevel = string.Empty;
            [DataMember]//备注信息
            public string Comments = string.Empty;
            [DataMember]//设备状态 Unknown/Idle/Run/Down/PM
            public string EqpStatus = string.Empty; 
            [DataMember]//警报时间
            public string ErrorDate = string.Empty;
        }
     
        public class LoadRequest
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//空载具Empty/实载具Full
            public string RequestType = string.Empty;
            [DataMember]//已生产数量
            public string LoadedQty = string.Empty;
            
        }
        public class LoadComplete
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//载具ID
            public string CarrierID = string.Empty;
        }
        public class UnLoadRequest
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(下料口:U01)
            public string PortID = string.Empty;
            [DataMember]//载具ID
            public string CarrierID = string.Empty;
            [DataMember]//空载具Empty/实载具Full
            public string RequestType = string.Empty;
            [DataMember]//批次号
            public string LotID = string.Empty;
        }
        public class UnLoadComplete
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:U01)
            public string PortID = string.Empty;
            [DataMember]//载具ID
            public string CarrierID = string.Empty;
        }
        public class LotTrackIn
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号
            public string PortID = string.Empty;
            [DataMember]//批次ID
            public string LotID = string.Empty;
        }
        public class LotTrackOut
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Port口编号(上料口:L01, 下料口:U01)
            public string PortID = string.Empty;
            [DataMember]//批次号
            public string LotID = string.Empty;
            [DataMember]//需要下机的数量
            public string PanelTotalQty = string.Empty;
            [DataMember]//false：此批非NG批次；true：此批为NG批次
            public bool NGFlag = false;
            [DataMember]//批次panellist信息
            public List<Panel> PanelList = new List<Panel>();
            [DataMember]//WIPData
            public List<WipData> WIPDataList = new List<WipData>();
            [DataMember]//任务编号
            public string JobID = string.Empty;
            [DataMember]//需下机任务数量
            public string JobTotalQty = string.Empty;
            [DataMember]//料号
            public string PN = string.Empty;
            [DataMember]//工单号
            public string WorkOrder = string.Empty;
        }
        public class StatusReport
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//状态代码
            public string EqpStatusCode = string.Empty;
            [DataMember]//变更原因(默认EAP)
            public string EqpStatusReason = "EAP";
        }

        
        public class EqpHoldInfo
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//变更原因(默认EAP)
            public string Reason = "EAP";
            [DataMember]//1：Hold；2：Release
            public string IsEqpHold = string.Empty;
            [DataMember]//1：停机械臂；0：停暂存口批次
            public string EqpHoldType = string.Empty;
        }
        public class EqpModeRequest
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
        }

        public class FirstInspResult
        {
            [DataMember]//线体ID
            public string MainEqpID = string.Empty;
            [DataMember]//批号
            public string LotID = string.Empty;
            [DataMember]//首件检结果（PASS,FAIL）
            public string InspResult = string.Empty;
        }
        public class LotHoldInfo
        {
            [DataMember]//批号
            public string LotID = string.Empty;
            [DataMember]//hold原因
            public string HoldReason = string.Empty;
            [DataMember] //是否查出所有和批次同工站同母批的批次
            public bool IsAll = false;
        }
        
        public class MaterialSetUp
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//物料条码
            public string MaterialSN = string.Empty;
            [DataMember]//载具ID
            public string CarrierID = string.Empty;
        }

        public class EquipmentSetUp
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//Tool条码
            public string ToolID = string.Empty;
        }

        public class RGVDispatch
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//料号
            public string PN = string.Empty;
            [DataMember]//目标设备ID（分板线）
            public string To_ID = string.Empty;
            [DataMember]//工单号
            public string WorkOrder = string.Empty;

        }
        public class TrainingRequest
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//人员编号
            public string Employee = string.Empty;
        }
        public class TaskCount
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//人员编号
            public string taskCount = string.Empty;
        }
        public class SafetySignal
        {
            [DataMember]//设备ID
            public string eqp_id = string.Empty;
            [DataMember]//设备端口ID
            public string port_id = string.Empty;
        }

        public class TransferComplete
        {
            [DataMember]//设备ID
            public string eqp_id = string.Empty;
            [DataMember]//设备端口ID
            public string port_id = string.Empty;
        }

        public class DataCollection
        {
            [DataMember]//设备ID
            public string SubEqpID = string.Empty;
            [DataMember]//点检采集对象
            public string WipDataSetUp = string.Empty;
            [DataMember]//WIP Data采集数据列表
            public List<WIPData> WIPDataList = new List<WIPData>();
            [DataMember]//TrackInLot；TrackOutLot；LotMoveOut；CompleteInsertion；AdHocWIPData（检测设备用）
            public string ServiceName = string.Empty;
            [DataMember]//批次号
            public string LotID = string.Empty;
            [DataMember]//板号
            public string PnlID = string.Empty;
        }
        public class WIPData
        {
            [DataMember]//采集项
            public string WipDataName = string.Empty;
            [DataMember]//采集值
            public string WipDataValue = string.Empty;
        }
        public class GUI_ControlModeCommand
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//控制模式(1：离线本地模式；2：在线远程模式)
            public string ControlMode = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_RemoteControlCommand
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//设备端口ID(无端口为空)
            public string PortID = string.Empty;
            [DataMember]//远程控制(1：Start 通知机台开始投板；2：Stop 通知机台停止板；3：Pause 通知机台暂停；4：Resume 通知机台复机；5：Open Buffer 通知机台启动暂存；6：Close Buffer 通知机台关闭暂存；7：Unload Carrier 通知机台下料；)
            public string RemoteCommand = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_JobDataModifyCommand
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//任务数量(旧)
            public string OldPanelCount = string.Empty;
            [DataMember]//任务名称(新)
            public string NewPanelCount = string.Empty;
            [DataMember]//修改状态(1：Updata 更新；2：Delete 删除；)
            public string ModifyType = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_JobDataDownloadCommand
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//设备端口ID(无端口为空)
            public string PortID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_TrackInCommand
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//设备端口ID(无端口为空)
            public string PortID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;

        }
        public class GUI_EqInfoRequest
        {
            [DataMember]//线体名称
            public string LineName = string.Empty;
            [DataMember]//自动刷新
            public bool isAutoRefresh = false;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
      
        public class ProductionModeDo
        {
            [DataMember]
            public string ProductionMode { get; set; }
        }

        public class GUI_JobDataDownload
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_CloseJobData
        {
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }

        public class GUI_LoadRequest
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//设备端口ID(无端口为空)
            public string PortID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_UnloadRequest
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//设备端口ID(无端口为空)
            public string PortID = string.Empty;
            [DataMember]//批次号
            public string LotID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }

        public class GUI_TrackOutCommand
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//任务名称
            public string JobID = string.Empty;
            [DataMember]//设备端口ID(无端口为空)
            public string PortID = string.Empty;
            [DataMember]//批次号
            public string LotID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
        public class GUI_LotInfoRequest
        {
            [DataMember]//设备名称
            public string EqpName = string.Empty;
            [DataMember]//Port口编号(上料口:L01)
            public string PortID = string.Empty;
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;

        }
        public class GUI_InitialLoadRequestFlag
        {
            [DataMember]//GUI IP
            public string GUIIP = string.Empty;
        }
    }
}
