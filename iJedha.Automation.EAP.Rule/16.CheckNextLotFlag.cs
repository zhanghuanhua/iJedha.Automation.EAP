//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Rule
//   文件概要 : PPSelectCheck
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using iJedha.Automation.EAP.Library;
using iJedha.Automation.EAP.Model;
using iJedha.Automation.EAP.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class CheckNextLotFlag
    {
        /// <summary>
        /// 检查是否符合继续投板条件
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InCheckNextLotFlagStart)
                {
                    return;
                }
                //获取线程带进来的参数
                Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = (Dictionary<MessageModel.LotInfoKL, CommModel>)_DowryObj;
                #region [定义物件信息，并赋值]
                MessageModel.LotInfoKL lotInfo = new MessageModel.LotInfoKL();
                PortModel port = new PortModel();
                //LotModel lm = new LotModel();
                EquipmentModel em = new EquipmentModel();
                Lot subLot = new Lot();
                foreach (var item in Dic_Message)
                {
                    //lotInfo = item.Key;
                    port = item.Value.pm;
                    //lm = item.Value.lm;
                    em = item.Value.em;
                    subLot = item.Value.lot;
                }
                #endregion

                if ( !Environment.EAPEnvironment.commonLibrary.StopNextLot)
                {
                    #region 杀掉当前线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InCheckNextLotFlagInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_CheckNextLotFlag);
                    #endregion
                    #region 正在检查生产条件
                    //Dictionary<MessageModel.LotInfoKL, CommModel> Dic_Message = new Dictionary<MessageModel.LotInfoKL, CommModel>();
                    //CommModel cm = new CommModel();
                    //cm.lm = new LotModel();
                    //cm.pm = pm;
                    //cm.lot = ProcessLot.ElementAt(0).Value;
                    //cm.em = em;
                    EAPEnvironment.commonLibrary.isJobDataProcessDataTigger = true;
                    //Dic_Message.Add(new MessageModel.LotInfoKL(), cm);
                    if (!EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart)
                    {
                        if (EAPEnvironment.EAPAp.InitialThreadWork(ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, ConstLibrary.CONST_DLL_RULENAMESPACE, ConstLibrary.CONST_DLL_RULE_ProductionConditionCheckKL, 10000, true, Dic_Message))
                        {
                            BaseComm.LogMsg(Log.LogLevel.Info, string.Format("Lot<{0}>正在检查生产条件......", subLot.LotID));
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckStart = true;
                            EAPEnvironment.commonLibrary.commonModel.InProductionConditionCheckTime = DateTime.Now;
                        }
                    }

                    #endregion
                    EAPEnvironment.commonLibrary.MainLotID = subLot.LotID;
                    EAPEnvironment.commonLibrary.ShowLotInfoMessage = subLot.LotStatus = eLotinfo.WaitingUp.ToString();
                }



            }
            catch (Exception ex)
            {
                Environment.BaseComm.LogMsg(iJedha.Automation.EAP.Core.Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message.ToString(), ex.StackTrace.ToString()));
                return;
            }
            finally
            {

            }
        }
    }
}

    

