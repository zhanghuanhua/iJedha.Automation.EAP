//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Environment
//   文件概要 : Extensions
//   作    者 : 陈玉明
//   <更新履历>
//   1.0.0.0    2020/9/29 15:03:15
//******************************************************************

using iJedha.Automation.EAP.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace iJedha.Automation.EAP.Environment
{
    public static class Extensions
    {
        public const char C_HEAD_CODE = (char)0x02;
        public const char C_TAIL_CODE = (char)0x03;

        public static int transacNo { get; set; } = 1;

        public static byte[] GetTrxData(string xmlData, socket.SocketCommonData.ENCODEMODE type = socket.SocketCommonData.ENCODEMODE.ENCODEMODE_UNICODE)
        {
            List<byte> lstData = new List<byte>();
            byte[] stringBytes;
            lstData.Add((byte)C_HEAD_CODE);

            #region Encoding
            //< !--0: Ascii 1:Unicode-- >
            switch (type)
            {
                case Core.socket.SocketCommonData.ENCODEMODE.ENCODEMODE_UNICODE:
                    stringBytes = Encoding.Unicode.GetBytes(xmlData);
                    break;
                default:
                    stringBytes = Encoding.ASCII.GetBytes(xmlData);
                    break;
            }
            #endregion

            lstData.AddRange(stringBytes);

            lstData.Add((byte)C_TAIL_CODE);

            return lstData.ToArray();
        }

        public static string GetXmlData(byte[] TrxData, socket.SocketCommonData.ENCODEMODE type = socket.SocketCommonData.ENCODEMODE.ENCODEMODE_UNICODE)
        {
            string strData;

            #region Encoding

            switch (type)
            {
                case Core.socket.SocketCommonData.ENCODEMODE.ENCODEMODE_UNICODE:
                    strData = Encoding.Unicode.GetString(TrxData);
                    break;
                default:
                    strData = Encoding.ASCII.GetString(TrxData);
                    break;
            }
            #endregion

            return strData;
        }

        public static string GetEnumDescription(this System.Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }
}
