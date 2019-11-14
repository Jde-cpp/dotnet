using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

namespace Jde.DB.Schema
{
	[XmlRoot( "key", IsNullable=false, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class Key
	{
	}
}
