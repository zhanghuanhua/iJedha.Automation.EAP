//******************************************************************
//   系统名称 : iJedha.Automation.EAP.Library
//   文件概要 : ConfigLibrary
//   作    者 : 张明哲
//   <更新履历>
//   1.0.0.0    2020/2/6 10:40:34
//******************************************************************
using iJedha.Automation.EAP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace iJedha.Automation.EAP.Library
{
    [Serializable]
    public class PortEntity
    {
        public string eap_id { get; set; }
        public ePortStatus L01 { get; set; }
        public ePortStatus L02 { get; set; }
        public ePortStatus L03 { get; set; }
        public ePortStatus L04 { get; set; }
        public ePortStatus U01 { get; set; }
        public ePortStatus U02 { get; set; }
        public ePortStatus U03 { get; set; }
        public ePortStatus U04 { get; set; }

        public PortEntity()
        {
            eap_id = "";
            L01 = ePortStatus.UNKNOW;
            L02 = ePortStatus.UNKNOW;
            L03 = ePortStatus.UNKNOW;
            L04 = ePortStatus.UNKNOW;
            U01 =ePortStatus.UNKNOW;
            U02 =ePortStatus.UNKNOW;
            U03 =ePortStatus.UNKNOW;
            U04 = ePortStatus.UNKNOW;
        }
        public string WriteToXml()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(GetType());
                XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces(new[] { new XmlQualifiedName(string.Empty, string.Empty), });
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                // 加入換行
                settings.Indent = true;
                settings.IndentChars = "    ";
                settings.NewLineOnAttributes = true;
                //
                StringWriter stringWriter = new StringWriter();
                XmlWriter w = XmlWriter.Create(stringWriter, settings);
                xmlSerializer.Serialize(w, this, xmlnsEmpty);

                return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + stringWriter.ToString();

                //StringWriterUTF8 sw = new StringWriterUTF8();
                //XmlSerializerNamespaces names = new XmlSerializerNamespaces();
                //names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd
                //XmlSerializer xmlwrite = new XmlSerializer(GetType());
                //xmlwrite.Serialize(sw, this, names);
                //return sw.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured in WriteToXml()", ex);
            }
        }
    }
}
