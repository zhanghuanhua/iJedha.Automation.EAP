//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : ConstLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
namespace iJedha.Automation.EAP.Library
{
    /// <summary>
    /// 常量库
    /// </summary>
    public class ConstLibrary
    {
        #region [Rule Dll]
        public const string CONST_DLL_RULE_AliveCheck = "AliveCheck";
        public const string CONST_DLL_RULENAMESPACE = "iJedha.Automation.EAP.Rule";
        public const string CONST_DLL_RULE_SocketMsgHandle = "SocketMsgHandle";
        public const string CONST_DLL_RULE_RsMsgHandle = "RsMsgHandle"; 
        public const string CONST_DLL_RULE_SocketReConnect = "SocketReConnect";
        public const string CONST_DLL_RULE_EQPOnlineCheck = "EQPOnlineCheck";
        public const string CONST_DLL_RULE_TraceDataCollect = "TraceDataCollect";

        public const string CONST_DLL_RULE_TraceDataCollectTest = "TraceDataCollectTest";

        public const string CONST_DLL_RULE_JobDataDownload = "JobDataDownload";
        public const string CONST_DLL_RULE_JobDataDownloadKL = "JobDataDownloadKL";
        public const string CONST_DLL_RULE_WIPDataCheck = "WIPDataCheck";
        public const string CONST_DLL_RULE_EquipmentJobDataRequest = "EquipmentJobDataRequest";
        public const string CONST_DLL_RULE_EquipmentJobDataRequestKL = "EquipmentJobDataRequestKL";
        public const string CONST_DLL_RULE_AllProcessCompletionCheck = "AllProcessCompletionCheck";
        public const string CONST_DLL_RULE_AllProcessCompletionCheckKL = "AllProcessCompletionCheckKL";
        public const string CONST_DLL_RULE_ProductionConditionCheck = "ProductionConditionCheck";
        public const string CONST_DLL_RULE_ProductionConditionCheckKL = "ProductionConditionCheckKL";
        public const string CONST_DLL_RULE_LoadCompleteReportCheck = "LoadCompleteReportCheck";
        public const string CONST_DLL_RULE_ProductionConditionCheckInspectCheck = "ProductionConditionCheckInspectCheck";
        public const string CONST_DLL_RULE_TrackInCheck0 = "TrackInCheckNegative";
        public const string CONST_DLL_RULE_TrackInCheck1 = "TrackInCheckPositive";
        public const string CONST_DLL_RULE_CheckNextLotFlag = "CheckNextLotFlag";
        #endregion

        public const string CFG_DEV_FILE_NAME = "dev_startup.xml";
        public const string CFG_Config_FILE_NAME = "ConfigLibrary.xml";
        public const string CFG_Equipment_FILE_NAME = "EquipmentLibrary.xml";
        public const string CFG_Customized_FILE_NAME = "CustomizedLibrary.xml";
        public const string CFG_Scenario_FILE_NAME = "ScenarioLibrary.xml";
        public const string CFG_ErrorCode_FILE_NAME = "ErrorCode.xml";
        public const string CFG_PasswordKey_FILE_NAME = "key.dat";
        public const string CFG_Dynamic_NAME = "DynamicLibrary";
        public const string CFG_Dynamic_FILE_NAME = "DynamicLibrary.xml";
        public const string CFG_LOG_BLOCKNAME = "EAPTrace"; // Log 文件夹名称
        public const string CFG_PortEntity_FILE_NAME = "PortEntity.xml";


        public const string CONST_ERRORCODE_PATH = "ErrorCode";
        public const string CONST_BASELIB_PATH = "BaseDll";
        public const string CONST_CLASSLIB_PATH = "ClassLib";

        public const int CONST_WCFINTERFACE_TIMEOUT = 45;   // Time Out 时间设定值
        public const string CONST_WEBAPI_TIMEOUT_KEYWORD = "作業逾時";
        public const string CONST_ACL = "开料机";




    }

}
