using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "alias", IsNullable=false, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class Alias
	{
		#region Name
		string _name=string.Empty;
		[XmlAttribute( "name" )]
		public string Name
		{
			get { return _name; }
			set { _name=value; }
		}
		#endregion
	}
}
