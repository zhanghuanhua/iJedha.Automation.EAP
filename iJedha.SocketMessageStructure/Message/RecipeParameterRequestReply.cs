using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iJedha.SocketMessageStructure
{
	[Serializable]
 [XmlRoot("message")] //Version V1.0 - 2020/10/01
	public class RecipeParameterRequestReply : Message
	{
		[Serializable]
		public class crecipe_parameter
		{

            public string item_name { get; set; }

            public string item_value { get; set; }

            public crecipe_parameter()
            {
                item_name = string.Empty;
                item_value = string.Empty;
            }
        }


		[Serializable]
		public class TrxBody : Body
		{

			public string eqp_id { get; set; }

			public string recipe_id { get; set; }

			[XmlArray("recipe_parameter_list")]
			[XmlArrayItem("recipe_parameter")]
			public List<crecipe_parameter> recipe_parameter_list { get; set; }

			public TrxBody()
			{
				eqp_id = string.Empty;
				recipe_id = string.Empty;
				recipe_parameter_list = new List<crecipe_parameter>();
			}
		}

       [XmlElement("header")]
		public new Header HEADER { get { return _hearer; } set { _hearer = value; } }

             [XmlElement("body")]
		public TrxBody BODY { get; set; }

             [XmlElement("return")]
		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeParameterRequestReply()
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
