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
/// <summary>
/// 触发功能：
/// </summary>
namespace iJedha.Automation.EAP.Rule
{
    public partial class TrackInCheckNegative
    {
        /// <summary>
        /// TrackInCheckNegative前检查
        /// </summary>
        /// <param name="_DowryObj"></param>
        public void Invoke(object _DowryObj)
        {
            try
            {
                if (!Environment.EAPEnvironment.commonLibrary.commonModel.InTrackInCheckNegativeStart)
                {
                    return;
                }
                CommModel cm=(CommModel)_DowryObj;
                EquipmentModel em = cm.em;
                LotModel lm = cm.lm;
                string err = "";
                //逆向PP上报数量达到MES下达的任务数量后，进行以下逻辑处理
                if (int.Parse(lm.MaterialPortTaskQty==""?"0": lm.MaterialPortTaskQty) ==EAPEnvironment.commonLibrary.Negative.Count)
                {
                    #region 杀掉当前线程
                    Environment.EAPEnvironment.commonLibrary.commonModel.InTrackInCheckNegativeCheckInitial();
                    EAPEnvironment.EAPAp.DeleteThreadWork(ConstLibrary.CONST_DLL_RULE_TrackInCheck0);
                    //清掉逆向上报数据，以备下次使用
                    EAPEnvironment.commonLibrary.Negative.Clear();
                    #endregion
                    #region 如果需要过账，上报在制品上机
                    if (EAPEnvironment.commonLibrary.lineModel.isNeedPost)
                    {
                        new WebAPIReport().EAP_LotTrackIn(new MessageModel.LotTrackIn()
                        {
                            MainEqpID = Environment.EAPEnvironment.commonLibrary.LineName,
                            SubEqpID = em.EQID,
                            PortID = lm.LocalPortStation,
                            LotID = lm.LotID
                        }, lm, 1,out err);
                    }
                  
                    #endregion
                }
                else
                {
                    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}>物料上机检查数量<{1}>,物料上报数量<{2}>。",em.EQName, lm.MaterialPortTaskQty,
                        EAPEnvironment.commonLibrary.Negative.Count.ToString()));
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
