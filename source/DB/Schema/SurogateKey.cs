using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "surogate_key", IsNullable=false, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class SurrogateKey
	{
	}
}
