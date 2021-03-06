//******************************************************************
//   系统名称 : iJedha.SocketMessageStructure
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 10:40:34
//******************************************************************
using System.ComponentModel;

namespace iJedha.SocketMessageStructure
{
    public enum eSocketCommand
    {
        None = 0,
        [Description("AreYouThereRequest")]
        AreYouThereRequest,
        [Description("AreYouThereRequestReply")]
        AreYouThereRequestReply,
        [Description("AbnormalPanelReport")]
        AbnormalPanelReport,
        [Description("AbnormalPanelReportReply")]
        AbnormalPanelReportReply,
        [Description("CarrierReadReport")]
        CarrierReadReport,
        [Description("CarrierReadReportReply")]
        CarrierReadReportReply,
        [Description("CIMMessageCommand")]
        CIMMessageCommand,
        [Description("CIMMessageCommandReply")]
        CIMMessageCommandReply,
        [Description("ControlModeCommand")]
        ControlModeCommand,
        [Description("ControlModeCommandReply")]
        ControlModeCommandReply,
        [Description("DateTimeSyncCommand")]
        DateTimeSyncCommand,
        [Description("DateTimeSyncReply")]
        DateTimeSyncReply,
        [Description("EquipmentAlarmReport")]
        EquipmentAlarmReport,
        [Description("EquipmentAlarmReportReply")]
        EquipmentAlarmReportReply,
        [Description("EquipmentControlMode")]
        EquipmentControlMode,
        [Description("EquipmentControlModeReply")]
        EquipmentControlModeReply,
        [Description("EquipmentCurrentDateTime")]
        EquipmentCurrentDateTime,
        [Description("EquipmentCurrentDateTimeReply")]
        EquipmentCurrentDateTimeReply,
        [Description("EquipmentCurrentRecipe")]
        EquipmentCurrentRecipe,
        [Description("EquipmentCurrentRecipeReply")]
        EquipmentCurrentRecipeReply,
        [Description("EquipmentCurrentStatus")]
        EquipmentCurrentStatus,
        [Description("EquipmentCurrentStatusReply")]
        EquipmentCurrentStatusReply,
        [Description("EquipmentJobDataProcessReport")]
        EquipmentJobDataProcessReport,
        [Description("EquipmentJobDataProcessReportReply")]
        EquipmentJobDataProcessReportReply,
        [Description("EquipmentJobDataRequest")]
        EquipmentJobDataRequest,
        [Description("EquipmentJobDataRequestReply")]
        EquipmentJobDataRequestReply,
        [Description("EquipmentOperationMode")]
        EquipmentOperationMode,
        [Description("EquipmentOperationModeReply")]
        EquipmentOperationModeReply,
        [Description("EquipmentRecipeModifyReport")]
        EquipmentRecipeModifyReport,
        [Description("RecipeModifyReportReply")]
        RecipeModifyReportReply,
        [Description("EquipmentRecipeSetupReport")]
        EquipmentRecipeSetupReport,
        [Description("EquipmentRecipeSetupReportReply")]
        EquipmentRecipeSetupReportReply,
        [Description("InitialDataRequest")]
        InitialDataRequest,
        [Description("InitialDataReply")]
        InitialDataReply,
        [Description("JobDataDownload")]
        JobDataDownload,
        [Description("JobDataDownloadReply")]
        JobDataDownloadReply,
        [Description("JobDataModifyCommand")]
        JobDataModifyCommand,
        [Description("JobDataModifyCommandReply")]
        JobDataModifyCommandReply,
        [Description("MaterialReadReport")]
        MaterialReadReport,
        [Description("MaterialReadReportReply")]
        MaterialReadReportReply,
        [Description("OperatorLoginConfirm")]
        OperatorLoginConfirm,
        [Description("OperatorLoginConfirmReply")]
        OperatorLoginConfirmReply,
        [Description("OperatorLoginLogoutReport")]
        OperatorLoginLogoutReport,
        [Description("OperatorLoginLogoutReportReply")]
        OperatorLoginLogoutReportReply,
        [Description("PanelInReport")]
        PanelInReport,
        [Description("PanelInReportReply")]
        PanelInReportReply,
        [Description("PanelOutReport")]
        PanelOutReport,
        [Description("PanelOutReportReply")]
        PanelOutReportReply,
        [Description("PanelReadReport")]
        PanelReadReport,
        [Description("PanelReadReportReply")]
        PanelReadReportReply,
        [Description("PortStatusReport")]
        PortStatusReport,
        [Description("PortStatusReportReply")]
        PortStatusReportReply,
        [Description("ProcessDataReport")]
        ProcessDataReport,
        [Description("ProcessDataReportReply")]
        ProcessDataReportReply,
        [Description("RecipeParameterRequest")]
        RecipeParameterRequest,
        [Description("RecipeParameterRequestReply")]
        RecipeParameterRequestReply,
        [Description("RemoteControlCommand")]
        RemoteControlCommand,
        [Description("RemoteControlCommandReply")]
        RemoteControlCommandReply,
        [Description("RGVDispatchCommand")]
        RGVDispatchCommand,
        [Description("RGVDispatchCommandReply")]
        RGVDispatchCommandReply,
        [Description("RGVDispatchResultReport")]
        RGVDispatchResultReport,
        [Description("RGVDispatchResultReportReply")]
        RGVDispatchResultReportReply,
        [Description("ToolsStatusReport")]
        ToolsStatusReport,
        [Description("ToolsStatusReportReply")]
        ToolsStatusReportReply,
        [Description("TraceDataRequest")]
        TraceDataRequest,
        [Description("TraceDataRequestReply")]
        TraceDataRequestReply,
        [Description("TrayStatusReport")]
        TrayStatusReport,
        [Description("TrayStatusReportReply")]
        TrayStatusReportReply,

    }
}
