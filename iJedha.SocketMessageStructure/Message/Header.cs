using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
    //Header
    //    MESSAGENAME
    //    SHOPNAME
    //    MACHINENAME
    //    TRANSACTIONID
    //    ORIGINALSOURCESUBJECTNAME
    //    SOURCESUBJECTNAME
    //    TARGETSUBJECTNAME
    //    EVENTUSER
    //    EVENTCOMMENT
    //    RETURNCODE
    //    RETURNMESSAGE

    [Serializable]
    public class Header
	{
        [XmlElement("messagename")]
        public string MESSAGENAME
		{
			get;
			set;
		}
        [XmlElement("transactionid")]
        public string TRANSACTIONID
        {
            get;
            set;
        }
        public Header()
		{
			MESSAGENAME = string.Empty;
            TRANSACTIONID = string.Empty;
        }
    }
}
