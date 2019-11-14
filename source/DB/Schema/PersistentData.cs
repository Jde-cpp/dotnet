using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "data", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class PersistentData : IXmlSerializable
	{
		#region Constructors
		public PersistentData()
		{ }
		#endregion
		#region IXmlSerializable Members
		public const string XmlElementName = "data";
		public static System.Xml.XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new System.Xml.XmlUrlResolver();
			schemaSet.Add( DataSchema.XmlSchema );
			return new System.Xml.XmlQualifiedName( XmlElementName, DataSchema.XmlNamespace );
		}
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
		{
			string location = reader.GetAttribute( "location" );
			if( !string.IsNullOrEmpty(location) )
				Locations = DB.Schema.Location.Parse( location );
			while( reader.NodeType==System.Xml.XmlNodeType.Attribute )
				reader.MoveToElement();

			if( reader.IsEmptyElement )
				reader.Read();
			else
			{
				reader.Read();
				while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
				{
					if( reader.NodeType!=System.Xml.XmlNodeType.Element )
					{
						reader.Read();
						continue;
					}
					if( reader.LocalName=="row" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(PersistentRow) );
						Items.Add( (PersistentRow)ser.Deserialize(reader) );
					}
					else
						reader.Read();
				}
				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			if( Locations!=Locations.Production )
				writer.WriteAttributeString( "location", Locations.ToString() );

			if( Items.Count>0 )
			{
				XmlSerializer ser = new XmlSerializer( typeof(PersistentRow) );
				foreach( PersistentRow item in Items )
					ser.Serialize( writer, item );
			}
		}
		#endregion
		#region Location
		Locations _locations = Locations.Production;
		public Locations Locations
		{
			get{ return _locations;}
			set{ _locations = value;}
		}
		#endregion
		#region Items
		Collection<PersistentRow> _items=new Collection<PersistentRow>();
		[XmlElement("row")]
		public Collection<PersistentRow> Items
		{
			get{return _items;}
			private set{_items=value;}
		}
		#endregion
		#region ParentTable
		Table _parentTable;
		public Table ParentTable
		{
			get{ return _parentTable;}
			set{ _parentTable = value; }
		}
		#endregion
	};
}
