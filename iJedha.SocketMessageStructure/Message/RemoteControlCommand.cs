using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
 [XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class RemoteControlCommand : Message
	{

		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string port_id { get; set; }

			public string remote_command { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				port_id = string.Empty;
				remote_command = string.Empty;
			}
		}

       [XmlElement("header")]
		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

             [XmlElement("body")]
		public TrxBody BODY { get; set; }

             [XmlElement("return")]
		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RemoteControlCommand()
		{
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
