using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace iJedha.SocketMessageStructure
{
    //<MESSAGE>
    //    <HEADER>
    //        <MESSAGENAME>AreYouThereRequest</MESSAGENAME>
    //        <TRANSACTIONID>20101129145858687500</TRANSACTIONID>
    //        <REPLYSUBJECTNAME>COMPANY.FACTORY.MES.PRD.FAB.PEMsvr</REPLYSUBJECTNAME>
    //        <INBOXNAME>_INBOX.0A46012D.4C81ECE61413A17.764</INBOXNAME>
    //        <LISTENER>PEMListener</LISTENER>
    //    </HEADER>
    //    <BODY>
    //       <LINENAME></LINENAME>
    //    </BODY>

    //</MESSAGE>
    [Serializable]
    public abstract class Message
    {
        //[XmlIgnore]
        //public MesSpec.DirType Direction
        //{
        //    get;
        //    set;
        //}

        //[XmlIgnore]
        //public string WaitReply
        //{
        //    get;
        //    set;
        //}

        [XmlIgnore]
        protected Header _hearer = null;
        [XmlIgnore]
        protected Return _return = null;


        [XmlIgnore]
        public Header HEADER
        {
            get { return _hearer; }
            set { _hearer = value; }
        }

        [XmlIgnore]
        public Return RETURN
        {
            get { return _return; }
            set { _return = value; }
        }

        public Message()
        {
            //Direction = MesSpec.DirType.UNKNOWN;
            //WaitReply = string.Empty;
            HEADER = new Header();
            RETURN = new Return();
        }

        /// <summary>
        /// Class To Xml
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Xml To Class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public T ReadFromXml<T>(string xml)
        {
            XmlDocument xdoc = new XmlDocument();

            try
            {
                xdoc.LoadXml(xml);
                XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(reader);
                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }

        public abstract Body GetBody();

        public Message Clone()
        {
            return (Message)this.MemberwiseClone();
        }
    }
}
