using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	public enum DefaultType
	{
		Text=0,
		UtcDate=1,
		NewGuid=2,
		Date=3,
		UserName=4
	}

	[XmlRoot( Default.XmlElementName, Namespace=DataSchema.XmlNamespace ), XmlSchemaProvider("MySchema")]
	public class Default : IXmlSerializable
	{
		#region Constructors
		public Default() 
		{}
		public Default( DefaultType type ) 
		{
			DefaultType = type;
		}
		public Default( string text ):
			this( DefaultType.Text )
		{
			Text = text;
		}
		#endregion
		#region IXmlSerializable Members
		public const string XmlElementName = "default";
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
		void IXmlSerializable.ReadXml( System.Xml.XmlReader reader )
		{
			Locations = Location.Read( reader, LocationDefault );

			string type = reader.GetAttribute( "type" );
			if( !string.IsNullOrEmpty(type) )
				DefaultType = ParseDefaultType( type );
			
			Text = reader.GetAttribute( "text" );
		
			while( reader.NodeType==System.Xml.XmlNodeType.Attribute )
				reader.MoveToElement();
			if( !reader.IsEmptyElement )
			{
				reader.Read();
				while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
				{
/*					if( reader.NodeType!=XmlNodeType.Element )
					{
						reader.Read();
						continue;
					}
					else
*/						reader.Read();
				}
				reader.ReadEndElement();
			}
			else
				reader.Read();
		}

		void IXmlSerializable.WriteXml( System.Xml.XmlWriter writer )
		{
			if( Locations!=LocationDefault )
				Location.Write( writer, Locations );

			if( DefaultType!=DefaultTypeDefault )
				writer.WriteAttributeString( "type", DefaultType.ToString() );
			if( !string.IsNullOrEmpty(Text) )
				writer.WriteAttributeString( "text", Text );
		}
		#endregion
		#region DefaultType
		const DefaultType DefaultTypeDefault = DefaultType.Text;
		DefaultType _defaultType = DefaultTypeDefault;
		[XmlAttribute("type")]
		public DefaultType DefaultType
		{
			get{ return _defaultType; }
			set{ _defaultType=value;}
		}
		static DefaultType ParseDefaultType( string typeName )
		{
			DefaultType type = DefaultType.Text;
			if( typeName=="utc_date" )
				 type = DefaultType.UtcDate;
			else
				type = (DefaultType)Enum.Parse( typeof(DefaultType), typeName, true );

			return type;
		}
		#endregion
		#region Locations
		const Locations LocationDefault = Locations.Production;
		Locations _locations = LocationDefault;
		[XmlAttribute("location")]
		public Locations Locations
		{
			get{ return _locations; }
			set{ _locations=value;}
		}
		#endregion
		#region Text
		string _text;
		[XmlAttribute("text")]
		public string Text
		{
			get{ return _text; }
			set{ _text=value;}
		}
		#endregion
	}
}
