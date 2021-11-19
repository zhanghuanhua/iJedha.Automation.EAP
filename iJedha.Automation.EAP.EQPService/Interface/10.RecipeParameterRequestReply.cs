//******************************************************************
//   系统名称 : iJedha.Automation.EAP.SocketService
//   文件概要 : 
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/10/1 14:28:28
//******************************************************************
using iJedha.Automation.EAP.Core;
using iJedha.Automation.EAP.Environment;
using System;
using System.Collections.Generic;

namespace iJedha.Automation.EAP.EQPService
{
    public partial class RecipeParameterRequestReply : BaseComm
    {
        public void EventHandle(string evtXml)
        {
            try
            {
                #region Decode Message
                SocketMessageStructure.RecipeParameterRequestReply rpy = new SocketMessageStructure.RecipeParameterRequestReply();
                if (new Serialize<SocketMessageStructure.RecipeParameterRequestReply>().DeSerializeXML(evtXml, out rpy) == false)
                {
                    BaseComm.LogMsg(Log.LogLevel.Error, string.Format("Socket Message<{0}> Decode Error, Content<{1}>", "RecipeParameterRequestReply", evtXml));
                    return;
                }
                #endregion

                #region Record Log
                BaseComm.LogMsg(Log.LogLevel.Trace, string.Format("<{2}>Socket Message<{0}> Recv OK , Content<{1}>",
                "RecipeParameterRequestReply", rpy.WriteToXml(), rpy.BODY.eqp_id.Trim()));
                #endregion

                #region 记录设备回复NG的Log
                //if (!rpy.RETURN.RETURNCODE.Equals("0"))
                //{
                //    BaseComm.LogMsg(Log.LogLevel.Warn, string.Format("设备<{0}>回复<{1}> 结果NG,回复内容<{2}> ", rpy.BODY.eqp_id.Trim(),
                //        "RecipeParameterRequestReply", rpy.RETURN.RETURNMESSAGE));
                //}
                //else
                //{
                    #region 拆解上报的Data_list存到字典内 
                    Dictionary<string, string> dicPara = new Dictionary<string, string>();
                    foreach (var item in rpy.BODY.recipe_parameter_list)
                    {
                        dicPara.Add(item.item_name, item.item_value);
                    }
                    #endregion
                //}
                #endregion
            }
            catch (Exception ex)
            {
                BaseComm.LogMsg(Log.LogLevel.Fatal, string.Format("{0}#{1}#{2}", "RecipeParameterRequestReply", ex.Message.ToString(), ex.StackTrace.ToString()));
            }
            finally
            {

            }
        }
    }
}
