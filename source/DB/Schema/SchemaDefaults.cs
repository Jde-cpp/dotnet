using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jde.DB.Schema
{
	[System.Xml.Serialization.XmlRoot(SchemaDefaults.XmlElementName, Namespace=DataSchema.XmlNamespace),Serializable]
	[System.Xml.Serialization.XmlSchemaProvider("MySchema")]
	public class SchemaDefaults
	{
		#region MySchema
		public const string XmlElementName = "schema_defaults";
		public static System.Xml.XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new System.Xml.XmlUrlResolver();
			schemaSet.Add( DataSchema.XmlSchema );
			return new System.Xml.XmlQualifiedName( XmlElementName, DataSchema.XmlNamespace );
		}
		#endregion
		#region Table
		Table _table;
		[System.Xml.Serialization.XmlElement("table")]
		public Table Table
		{
			get{ return _table; }
			set{ _table = value; }
		}
		#endregion
	}
}
