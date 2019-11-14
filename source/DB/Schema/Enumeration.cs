using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "enumeration", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class Enumeration : IXmlSerializable
	{
		#region IXmlSerializable Members
		public const string XmlElementName = "enumeration";
		public static XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new XmlUrlResolver();
			schemaSet.Add( DataSchema.XmlSchema );
			return new XmlQualifiedName( XmlElementName, DataSchema.XmlNamespace );
		}
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
		{
			string bitField = reader.GetAttribute("bit_field");
			BitField = string.IsNullOrEmpty(bitField) ? false : bool.Parse( bitField );
			Default = reader.GetAttribute( "default" );
			Description = reader.GetAttribute( "description" );
			Name = reader.GetAttribute( "name" );
			var isClass = reader.GetAttribute( "is_class" );
			if( !string.IsNullOrEmpty(isClass) )
				IsClass = bool.Parse(isClass);
			while( reader.NodeType==XmlNodeType.Attribute )
				reader.MoveToElement();

			if( reader.IsEmptyElement )
				reader.Read();
			else
			{
				reader.Read();
				while( reader.NodeType!=XmlNodeType.EndElement )
				{
					if( reader.NodeType!=XmlNodeType.Element )
					{
						reader.Read();
						continue;
					}
					else if( reader.LocalName=="column" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Column) );
						Column = (Column)ser.Deserialize( reader );
					}
					else if( reader.LocalName=="item" )
					{
						if( Items==null )
							Items = new Collection<EnumItem>();
						XmlSerializer ser = new XmlSerializer( typeof(EnumItem) );
						Items.Add( (EnumItem)ser.Deserialize(reader) );
					}
					else if( reader.LocalName=="table" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Table) );
						Table = (Table)ser.Deserialize( reader );
					}
					else
						reader.Read();
				}
				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			if( BitField )
				writer.WriteAttributeString( "name", true.ToString(CultureInfo.InvariantCulture) );
			if( !string.IsNullOrEmpty(Default) )
				writer.WriteAttributeString( "default", Default );
			if( !string.IsNullOrEmpty(Description) )
				writer.WriteAttributeString( "description", Description );
			if( !string.IsNullOrEmpty(Name) )
				writer.WriteAttributeString("name", Name );
			if( IsClass )
				writer.WriteAttributeString( "is_class", IsClass.ToString(CultureInfo.InvariantCulture) );
			if( Column!=null )
			{
				XmlSerializer ser = new XmlSerializer( typeof(Column) );
				ser.Serialize( writer, Column );
			}
			if( Items.Count>0 )
			{
				XmlSerializer ser = new XmlSerializer( typeof(EnumItem) );
				foreach( EnumItem item in Items )
					ser.Serialize( writer, item );
			}
			if( Table!=null )
			{
				XmlSerializer ser = new XmlSerializer( typeof(Table) );
				ser.Serialize( writer, Table );
			}
		}
		#endregion
		#region Object Members
		public override string ToString()
		{
			StringBuilder value = new StringBuilder( Name );
			if( Items!=null && Items.Count>0 )
			{
				value.Append( "{" );
				foreach( var item in Items )
					value.Append( item.ToString(BitField)+',' );
				value.Replace( ',', '}', value.Length-1, 1 );
			}
			return value.ToString();
		}
		#endregion
		#region BitField
		bool _bitField;
		[XmlAttribute( "bit_field" )]
		public bool BitField
		{
			get { return _bitField; }
			set { _bitField=value; }
		}
		#endregion
		#region IsClass
		public bool IsClass{ get; set; }
		#endregion
		#region Column
		Column _column;
		[XmlElement( "column" )]
		public Column Column
		{
			get { return _column; }
			set { _column=value; }
		}
		#endregion
		#region Default
		string _default=string.Empty;
		[XmlAttribute("default")]
		public string Default
		{
			get{ return _default;}
			set{ _default=value;}
		}
		#endregion
		#region Description
		string _description=string.Empty;
		[XmlAttribute( "description" )]
		public string Description
		{
			get { return _description; }
			set { _description=value; }
		}
		#endregion
		#region Items
		Collection<EnumItem> _items;
		[XmlElement( "item" )]
		public Collection<EnumItem> Items
		{
			get { return _items; }
			private set{ _items=value; }
		}
		public void InitiateItems()
		{
			Items = new Collection<EnumItem>();
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute( "name" )]
		public string Name
		{
			get { return string.IsNullOrEmpty(_name) && Column!=null ? Column.Name : _name; }
			set { _name=value; }
		}
		#endregion
		#region Table
		Table _table;
		[XmlElement( "table" )]
		public Table Table
		{
			get { return _table; }
			set { _table = value; }
		}
		#endregion
	}
}
