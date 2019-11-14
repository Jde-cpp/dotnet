using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( Column.XmlElementName, IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class Column : IXmlSerializable
	{
		#region Constructors
		public Column( string name, int ordinal, Table parentTable )
		{
			Name=name;
			ParentTable=parentTable;
			Ordinal=ordinal;
		}
		public Column()
		{ }
		#endregion
		#region Object Members
		public override string ToString()
		{
			StringBuilder text = new StringBuilder( string.Format(CultureInfo.CurrentCulture, "{0}-{1} {2}", Ordinal, Name, DataType) );
			if( MaxLength!=null )
				text.AppendFormat( CultureInfo.CurrentCulture, "({0})", MaxLength );

			if( !string.IsNullOrEmpty(Summary) )
				text.AppendFormat( CultureInfo.CurrentCulture, " - {0}", Summary );
			
			return text.ToString();
		}
		#endregion
		#region IXmlSerializable Members
		public const string XmlElementName = "column";
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
			string value = reader.GetAttribute( "boolean" );
			if( !string.IsNullOrEmpty(value) )
				Boolean = bool.Parse( value );

			value = reader.GetAttribute( "is_create_time" );
			if( !string.IsNullOrEmpty(value) )
				IsCreateTime = bool.Parse( value );

			value = reader.GetAttribute( "is_edit_time" );
			if( !string.IsNullOrEmpty(value) )
				IsEditTime = bool.Parse( value );

			value = reader.GetAttribute( "maxInclusive" );
			if( !string.IsNullOrEmpty(value) )
				MaxInclusive = (int?)int.Parse( value );

			value = reader.GetAttribute( "max_length" );
			if( !string.IsNullOrEmpty(value) )
				MaxLength = (int?)int.Parse( value );
			
			MemberName = reader.GetAttribute( "member_name" );

			value = reader.GetAttribute( "minInclusive" );
			if( !string.IsNullOrEmpty(value) )
				MinInclusive = (int?)int.Parse( value );
			
			Name = reader.GetAttribute( "name" );

			value = reader.GetAttribute( "nullable" );
			if( !string.IsNullOrEmpty(value) )
				Nullable = bool.Parse( value );

			value = reader.GetAttribute( "ordinal" );
			if( !string.IsNullOrEmpty(value) )
				Ordinal = int.Parse( value );

			value = reader.GetAttribute( "row_version" );
			if( !string.IsNullOrEmpty(value) )
				RowVersion = bool.Parse( value );

			value = reader.GetAttribute( "real_column" );
			if( !string.IsNullOrEmpty(value) )
				RealColumn = bool.Parse( value );

			value = reader.GetAttribute( "serialization" );
			if( !string.IsNullOrEmpty(value) )
				Serialization = Extensions.SerializationExtensions.Parse( value );

			value = reader.GetAttribute( "updateable" );
			if( !string.IsNullOrEmpty(value) )
				Updateable = bool.Parse( value );

			value = reader.GetAttribute( "virtual" );
			if( !string.IsNullOrEmpty(value) )
				Virtual = bool.Parse( value );

			value = reader.GetAttribute( "write" );
			if( !string.IsNullOrEmpty(value) )
				Write = bool.Parse( value );

			value = reader.GetAttribute( "type" );
			if( !string.IsNullOrEmpty(value) )
				DataType = DataTypes.Parse( value );

			XslType = reader.GetAttribute( "xslType" );

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
					else if( reader.LocalName==Default.XmlElementName )
						Default = Xml.XmlSerialization<Default>.Read( reader );
					else if( reader.LocalName=="enumeration" )
					{
						Enumeration = Xml.XmlSerialization<Enumeration>.Read( reader );
						Enumeration.Column = this;
					}
					else if( reader.LocalName=="sequence" )
						Sequence = Xml.XmlSerialization<Sequence>.Read( reader );
					else if( reader.LocalName=="surogate_key" )
						SurrogateKey = Xml.XmlSerialization<SurrogateKey>.Read( reader );
					else if( reader.LocalName=="key" )
						Key = Xml.XmlSerialization<Key>.Read( reader );
					else if( reader.LocalName=="summary" )
						Summary = reader.ReadElementContentAsString();
					else
						reader.Read();
				}
				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			if( Boolean )
				writer.WriteAttributeString( "boolean", "true" );
			if( IsCreateTime )
				writer.WriteAttributeString( "is_create_time", "true" );
			if( IsEditTime )
				writer.WriteAttributeString( "is_edit_time", "true" );
			if( !Nullable )
				writer.WriteAttributeString( "nullable", "false" );
			if( RowVersion )
				writer.WriteAttributeString( "row_version", "true" );
			if( !RealColumn )
				writer.WriteAttributeString( "real_column", "false" );
			if( Serialization!=Serialization.Attribute )
				writer.WriteAttributeString( "serialization", Extensions.SerializationExtensions.ToString(Serialization) );
			if( !Updateable )
				writer.WriteAttributeString( "updateable", "false" );
			if( Virtual )
				writer.WriteAttributeString( "virtual", "true" );
			if( !Write )
				writer.WriteAttributeString( "write", "false" );
			if( MaxInclusive!=null )
				writer.WriteAttributeString( "maxInclusive", ((int)MaxInclusive).ToString(CultureInfo.InvariantCulture) );
			if( MaxLength!=null )
				writer.WriteAttributeString( "max_length", ((int)MaxLength).ToString(CultureInfo.InvariantCulture) );
			if( !string.IsNullOrEmpty(MemberName) )
				writer.WriteAttributeString("member_name", MemberName );
			if( MinInclusive!=null )
				writer.WriteAttributeString( "minInclusive", ((int)MinInclusive).ToString(CultureInfo.InvariantCulture) );
			if( !string.IsNullOrEmpty(Name) )
				writer.WriteAttributeString( "name", Name );
			if( Ordinal!=int.MaxValue )
				writer.WriteAttributeString( "ordinal", Ordinal.ToString(CultureInfo.InvariantCulture) );

			writer.WriteAttributeString( "type", DataType.ToString() );
			if( !string.IsNullOrEmpty(XslType) )
				writer.WriteAttributeString( "xslType", XslType );

			if( Default!=null )
				Xml.XmlSerialization<Default>.Write( writer, Default );
			if( Enumeration!=null )
				Xml.XmlSerialization<Enumeration>.Write( writer, Enumeration );
			if( Sequence!=null )
				Xml.XmlSerialization<Sequence>.Write( writer, Sequence );
			if( SurrogateKey!=null )
				Xml.XmlSerialization<SurrogateKey>.Write( writer, SurrogateKey );
			if( Key!=null )
				Xml.XmlSerialization<Key>.Write( writer, Key );
			if( !string.IsNullOrEmpty(Summary) )
				writer.WriteElementString( "summary", Summary );
		}
		#endregion
		#region Drop
		public void Drop( DB.Database ds )
		{
			List<Index> dropIndexes = new List<Index>();
			foreach( Index index in ParentTable.Indexes )
			{
				if( index.FindColumn(Name)!=null )
					dropIndexes.Add( index );
			}
			dropIndexes.ForEach( index => index.Drop(ds) );
			foreach( ForeignKey fk in ParentTable.ForeignKeys )
			{
				if( fk.Columns.FirstOrDefault(column => column.Name==Name)!=null )
				{
					fk.Drop( ds );
					break;
				}
			}
			ds.Execute( $"alter table \"{ParentTable.Name}\" drop column \"{Name}\"" );
			ParentTable.Columns.Remove( this );
		}
		#endregion
		#region AlterStatement
		public string AlterStatement( SqlSyntax sql, Schema.Column appColumn, bool unicode )
		{
			StringBuilder statement = null;
			if( sql.GetTypeString( DataType, false, unicode, MaxLength, Precision, Scale )!=sql.GetTypeString(appColumn.DataType, unicode, false, appColumn.MaxLength ?? 256 )
				|| (Nullable && !appColumn.Nullable) 
				|| (!Nullable && appColumn.Nullable) )
			{
				statement = new StringBuilder( string.Format(CultureInfo.InvariantCulture, "alter table {0} {1} {2} ", appColumn.ParentTable.Name, sql.AlterColumnPrefix, appColumn.Name) );
				statement.Append( sql.GetTypeString(appColumn.DataType, unicode, false, appColumn.MaxLength) );
				if( appColumn.Nullable && (!sql.OnlySpecifyNullChanges || !Nullable) )
					statement.Append( " null" );
				else if( !appColumn.Nullable && (!sql.OnlySpecifyNullChanges || Nullable) )
					statement.Append( " not null" );
			}
			return statement==null ? string.Empty : statement.ToString();
		}
		public string AlterRowGuidColumn( SqlSyntax sql, Schema.Column appColumn )
		{
			string changeSql=string.Empty;
			if( sql.SupportsRowGuid )
			{
				if( IsRowGuid() && !appColumn.IsRowGuid() )
					changeSql = String.Format( CultureInfo.InvariantCulture, "alter table {0} alter column {1} drop ROWGUIDCOL", appColumn.ParentTable.Name, appColumn.Name );
				else if( !IsRowGuid() && appColumn.IsRowGuid() )
					changeSql = String.Format( CultureInfo.InvariantCulture, "alter table {0} alter column {1} add  ROWGUIDCOL", appColumn.ParentTable.Name, appColumn.Name );
			}
			return changeSql;
		}
		#endregion 
		#region IsRowGuid
		public bool IsRowGuid()
		{
			return ParentTable.SurrogateKeys().Count==1 && SurrogateKey!=null && DataType==DataType.Guid;
		}
		#endregion
		#region Clone
		public Column Clone( Table parent )
		{
			Column clone = new Column( Name, Ordinal, parent );
			clone.Boolean=Boolean;
			clone.Default=Default;
			clone.Enumeration=Enumeration;
			clone.MaxLength=MaxLength;
			clone.MemberName=MemberName;
			clone.Nullable=Nullable;
			clone.SurrogateKey=SurrogateKey;
//			clone.Sequenced=Sequenced;
			clone.Summary=Summary;
			clone.DataType=DataType;
			clone.Updateable=Updateable;
			clone.Write=Write;
			return clone;
		}
		#endregion
		#region Default
		Default _default;
		[XmlElement( "default" )]
		public Default Default
		{
			get { return _default; }
			set { _default=value; }
		}
		#endregion
		#region Enumeration
		Enumeration _enumeration;
		[XmlElement( "enumeration" )]
		public Enumeration Enumeration
		{
			get { return _enumeration; }
			set { _enumeration=value; }
		}
		#endregion
		#region Flags
		bool _boolean;
		[XmlAttribute("boolean")]
		public bool Boolean
		{
			get{ return _boolean;}
			set{_boolean=value;}
		}

		public bool IsCreateTime{get;set;}
		
		public bool IsEditTime{get;set;}

		bool _nullable = true;
		[XmlAttribute("nullable")]
		public bool Nullable
		{
			get{ return _nullable;}
			set{_nullable=value;}
		}

		bool _realColumn=true;
		[XmlAttribute("real_column")]
		public bool RealColumn
		{
			get{ return _realColumn;}
			set{_realColumn=value;}
		}

		bool _rowVersion;
		[XmlAttribute("row_version")]
		public bool RowVersion
		{
			get{ return _rowVersion;}
			set{_rowVersion=value;}
		}

		Sequence _sequence;
		[XmlElement("sequence")]
		public Sequence Sequence
		{
			get{ return _sequence;}
			set{_sequence=value;}
		}

		SurrogateKey _surrogateKey;
		[XmlElement( "surogate_key" )]
		public SurrogateKey SurrogateKey
		{
			get { return _surrogateKey; }
			set { _surrogateKey=value; }
		}

		bool _virtual;
		[XmlAttribute("virtual")]
		public bool Virtual
		{
			get{ return _virtual;}
			set{_virtual=value;}
		}

		bool _updateable = true;
		[XmlAttribute("updateable")]
		public bool Updateable
		{
			get{ return _updateable;}
			set{_updateable=value;}
		}

		bool _write = true;
		[XmlAttribute("write")]
		public bool Write
		{
			get{ return _write;}
			set{_write=value;}
		}
		#endregion
		#region Key
		Key _key;
		[XmlElement( "key" )]
		public Key Key
		{
			get { return _key; }
			set { _key=value; }
		}
		#endregion
		#region MaxInclusive
		int? _maxInclusive;
		[XmlAttribute("maxInclusive")]
		public int? MaxInclusive
		{
			get{ return _maxInclusive; }
			set{ _maxInclusive=value; }
		}
		#endregion
		#region MaxLength
		int? _maxLength;
		[XmlAttribute("max_length")]
		public int? MaxLength
		{
			get{ return _maxLength; }
			set{ _maxLength=value; }
		}
		#endregion
		#region MemberName
		string _memberName=string.Empty;
		[XmlAttribute( "member_name" )]
		public string MemberName
		{
			get { return _memberName; }
			set { _memberName=value; }
		}
		#endregion
		#region MinInclusive
		int? _minInclusive;
		[XmlAttribute("minInclusive")]
		public int? MinInclusive
		{
			get{ return _minInclusive; }
			set{ _minInclusive=value; }
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute( "name" )]
		public string Name
		{
			get { return _name; }
			set { _name=value; }
		}
		#endregion
		#region Ordinal
		int _ordinal;
		[XmlAttribute("ordinal")]
		public int Ordinal
		{
			get{ return _ordinal;}
			set{ _ordinal=value;}
		}
		#endregion
		[XmlAttribute("precision")]public ushort? Precision{get;set; }=null;
		[XmlAttribute("scale")]public ushort? Scale{get;set; }=null;
		#region Serialization
		Serialization _serialization = Serialization.Attribute;
		public Serialization Serialization
		{
			get{ return _serialization; }
			set{ _serialization = value;}
		}
		#endregion
		#region Summary
		string _summary=string.Empty;
		[XmlElement("summary")]
		public string Summary
		{
			get{ return _summary; }
			set{ _summary=value; }
		}
		#endregion
		#region Table
		Table _parentTable;
		[XmlIgnore]
		public Table ParentTable
		{
			get{ return _parentTable;}
			set
			{ 
				_parentTable=value;
				if(Sequence != null && Sequence.Table == null)
					Sequence.Table = _parentTable;
			}
		}
		#endregion
		#region Type
		DataType _type=DataType.None;
		[XmlAttribute( "type" )]
		public DataType DataType
		{
			get { return _type; }
			set { _type=value; }
		}
		#endregion
		#region XslType
		string _xslType;
		[XmlAttribute( "xslType" )]
		public string XslType
		{
			get { return _xslType; }
			set { _xslType=value; }
		}
		#endregion
	};
}
