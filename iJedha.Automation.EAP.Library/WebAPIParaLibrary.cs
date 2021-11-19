using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iJedha.Automation.EAP.Library
{
    public class WebAPIParaLibrary
    {
        public int AliveCheckInterval { get; set; }//定时检查MES连线的设定值
        public DateTime AliveCheckTime { get; set; }//检查MES连线的时间
        public bool Client_Enable { get; set; }//是否启用Client端
        public string RemoteIP { get; set; }//Clinet端的IP
        public int RemotePort { get; set; }//Client端的端口号
        public string RemoteUrlString { get; set; }//Client的地址
        public int RemoteRetrytime { get; set; }//Client的地址
        public bool Server_Enable { get; set; }//是否启用Server端
        public string LocalIP { get; set; }//Server端的IP
        public int LocalPort { get; set; }//Server端的Port
        public string LocalUrlString { get; set; }//Server的地址
        public bool WebAPIServer_Enable { get; set; }//是否启用WebAPIServer端
        public string WebAPIServerIP { get; set; }//WebAPIServer端的IP
        public int WebAPIServerPort { get; set; }//WebAPIServer端的Port
        public string WebAPIServerUrlString { get; set; }//WebAPIServer的地址
    }
}
