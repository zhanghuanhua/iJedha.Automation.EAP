using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
    //<RETURN>
    //    <RETURNCODE>0</RETURNCODE>
    //    <RETURNMESSAGE></RETURNMESSAGE>
    //</RETURN>
    [Serializable]
    public class Return
	{
        [XmlElement("returncode")]
        public string RETURNCODE
		{
			get;
			set;
		}

        [XmlElement("returnmessage")]
        public string RETURNMESSAGE
		{
			get;
			set;
		}

        public Return()
		{
			RETURNCODE = "0";
			RETURNMESSAGE = string.Empty;
		}

		public Return(string ReturnCode, string ReturnMessage)
		{
			RETURNCODE = ReturnCode;
			RETURNMESSAGE = ReturnMessage;
		}
	}
}
