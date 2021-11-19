using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
namespace iJedha.Automation.EAP.Serial
{
    public class SerialPortService
    {
        public SerialPort com = new SerialPort();
        //定义端口类
        public SerialPort ComDevice = new SerialPort();
        public SerialPortService(string port, string baudrate, string databits, string stopbits, string checkbits)
        {
            if (ComDevice.IsOpen == false)
            {
                //设置串口相关属性
                ComDevice.PortName = port;
                ComDevice.BaudRate = Convert.ToInt32(baudrate);
                ComDevice.Parity = (Parity)Convert.ToInt32(checkbits);
                ComDevice.DataBits = Convert.ToInt32(databits);
                ComDevice.StopBits = (StopBits)Convert.ToInt32(stopbits);
            }
        }

    }
}
