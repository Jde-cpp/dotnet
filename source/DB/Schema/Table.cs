using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.IO;

namespace Jde.DB.Schema
{
	[Serializable, XmlType( "table_types" )]
	public enum TableType
	{ 
		[XmlEnum("none")]
		None=0,
		[XmlEnum("system")]
		System=1, 
		[XmlEnum("user")]
		User=2
	};
	
	[XmlRoot( Table.XmlElementName, IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class Table : IXmlSerializable
	{
		#region Constructors
		public Table( string name, TableType type )
		{ 
			_name=name; 
			_type=type; 
		}
		public Table()
		{}
		#endregion
		#region ObjectOverrides
		public override string ToString()
		{
			return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
		}
		#endregion
		#region Clone
		public Table Clone( DataSchema schema )
		{
			Table clone=new Table( Name, TableType);
			clone.Schema=schema;
			foreach( Column column in Columns )
				clone.Columns.Add( column );
			return clone;
		}
		#endregion
		#region CreateTable
		public string GetArchiveCreateStatement( SqlSyntax sql )
		{
			return GetCreateStatement( sql, Locations.Archive );
		}
		public string GetCreateStatement( SqlSyntax sql )
		{
			return GetCreateStatement(sql, Locations.Production);
		}

		public string GetCreateStatement( SqlSyntax sql, Locations location )
		{
			StringBuilder statement=new StringBuilder(string.Format(CultureInfo.InvariantCulture, "create table {0}(\n", Name));

			bool bFirst=true;
			foreach( var column in SortedColumns().Values )
			{
				if( !column.RealColumn )
					continue;
				if( bFirst )
					bFirst=false;
				else
					statement.Append(",");

				statement.Append(column.Name);
				statement.Append(" ");
				statement.Append( sql.GetTypeString(column.DataType, Schema.Unicode, false, column.MaxLength).ToLower(CultureInfo.InvariantCulture) );
//				if( DataTypes.HasLength(column.Type) )
//					statement.AppendFormat("({0})", );
				statement.Append(column.Nullable ? " null" : " not null");
				if( column.Default!=null )
				{
					switch( column.Default.DefaultType )
					{
					case DefaultType.UtcDate:
						statement.AppendFormat( " default {0}", sql.DefaultUtcDate );
						break;
					case DefaultType.Text:
						if( !DataTypes.HasLength(column.DataType) )
							statement.AppendFormat( CultureInfo.InvariantCulture, " default {0}", column.Default.Text );
						else
							statement.AppendFormat( CultureInfo.InvariantCulture, " default '{0}'", column.Default.Text );
						break;
					default:
						throw new NotSupportedException( string.Format(CultureInfo.InvariantCulture, "default type '{0}' is not supported.", column.Default.DefaultType) );
					}
				}
				if( sql.SupportsRowGuid && column.IsRowGuid() )
					statement.Append(" ROWGUIDCOL");
				if( sql.SupportsIdentity && column.Sequence!=null && (column.Sequence.Location & location)!=0 )
					statement.AppendFormat( " {0}", sql.IdentitySyntax(column.Sequence.Start, column.Sequence.Increment) );
			}
			statement.Append("\n)");

			return statement.ToString();
		}
		#endregion
		#region Drop
		public void Drop( DB.Database db )
		{
			foreach( var table in Schema.Tables )
			{
				List<ForeignKey> fks = table.ForeignKeys.ToList().FindAll( fk => fk.PrimaryTable.Name==Name );
				fks.ForEach( fk => fk.Drop(db) );
			}
			var existingTable = Schema.Tables.FirstOrDefault( (item)=>item.Name==Name );
			db.Execute( "drop table \"{Name}\"" );
			if( existingTable!=null )
				Schema.Tables.Remove( existingTable );
		}
		#endregion
		#region IXmlSerializable Members
		public const string XmlElementName = "table";
		public static XmlQualifiedName MySchema( XmlSchemaSet schemaSet )
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
			Name = reader.GetAttribute( "name" );
			SimpleName = reader.GetAttribute( "simple_name" );
			DefaultDisplay = reader.GetAttribute( "default_display" );
			InsertProcName = reader.GetAttribute( "insert_proc" );
			ClassName = reader.GetAttribute( "class_name" );
			BaseClass = reader.GetAttribute( "base_class" );

			string deletable = reader.GetAttribute( "deletable" );
			if( !string.IsNullOrEmpty(deletable) )
				Deletable = bool.Parse( deletable );

			string indexSize = reader.GetAttribute( "index_size" );
			if( !string.IsNullOrEmpty(indexSize) )
				IndexSize = int.Parse( indexSize );

			string location = reader.GetAttribute( "location" );
			if( !string.IsNullOrEmpty(location) )
				Location = DB.Schema.Location.Parse( location );
	
			string rowCount = reader.GetAttribute( "row_count" );
			if( !string.IsNullOrEmpty(rowCount) )
				RowCount = long.Parse( rowCount );

			string sld = reader.GetAttribute( "sealed" );
			if( !string.IsNullOrEmpty(sld) )
				Sealed = bool.Parse( sld );
	
			string tableSize = reader.GetAttribute( "table_size" );
			if( !string.IsNullOrEmpty(tableSize) )
				TableSize = long.Parse( tableSize );

			string type = reader.GetAttribute( "type" );
			if( !string.IsNullOrEmpty(type) )
				TableType = (TableType)Enum.Parse(typeof(TableType), type );

			Namespace = reader.GetAttribute( "namespace" );
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
					if( reader.LocalName=="columns" )
					{
						reader.ReadStartElement("columns");
						
						while( reader.NodeType!=XmlNodeType.EndElement )
						{
							if( reader.NodeType==XmlNodeType.Element )
								Columns.Add( Xml.XmlSerialization<Column>.Read(reader) );
							else
								reader.Read();
						}
						var iColumn = 0;
						foreach( var column in Columns )
						{
							if( column.Ordinal==default(int) )
								column.Ordinal = iColumn;
							++iColumn;
						}
						reader.ReadEndElement();
					}
					else if( reader.LocalName=="summary" )
						Summary = reader.ReadElementContentAsString();
					else if( reader.LocalName=="data" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(PersistentData) );
						Data = (PersistentData)ser.Deserialize( reader );
					}
					else if( reader.LocalName=="enumeration" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Enumeration) );
						Enumeration = (Enumeration)ser.Deserialize(reader);
					}
					else if( reader.LocalName=="foreign_keys" && !reader.IsEmptyElement )
					{
						reader.ReadStartElement("foreign_keys");
						while( reader.NodeType!=XmlNodeType.EndElement )
						{
							if( reader.NodeType==XmlNodeType.Element )
								ForeignKeys.Add( Xml.XmlSerialization<ForeignKey>.Read(reader) );
							else
								reader.Read();
						}
						reader.ReadEndElement();
					}
					else if( reader.LocalName=="indexes" && !reader.IsEmptyElement )
					{
						reader.ReadStartElement("indexes");
						
						while( reader.NodeType!=XmlNodeType.EndElement )
						{
							if( reader.NodeType==XmlNodeType.Element )
								Indexes.Add( Xml.XmlSerialization<Index>.Read(reader) );
							else
								reader.Read();
						}

						reader.ReadEndElement();
					}
					else
						reader.Read();
				}
				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			if( !string.IsNullOrEmpty(BaseClass) )
				writer.WriteAttributeString( "base_class", BaseClass );
			if( !string.IsNullOrEmpty(_className) )
				writer.WriteAttributeString("class_name", _className );
			if( !string.IsNullOrEmpty(Name) )
				writer.WriteAttributeString("name", Name );
			if( !string.IsNullOrEmpty(_simpleName) )
				writer.WriteAttributeString("simple_name", _simpleName );
			if( !string.IsNullOrEmpty(InsertProcName) )
				writer.WriteAttributeString("insert_proc", InsertProcName );
			if( !string.IsNullOrEmpty(DefaultDisplay) )
				writer.WriteAttributeString( "default_display", DefaultDisplay );
			if( Location!=Locations.Production )
				writer.WriteAttributeString( "location", Location.ToString() );
			if( !string.IsNullOrEmpty(Namespace) )
				writer.WriteAttributeString( "namespace", Namespace );

			if( IndexSize!=0 )
				writer.WriteAttributeString( "index_size", IndexSize.ToString(CultureInfo.InvariantCulture) );
			if( TableSize!=0 && TableSize!=long.MinValue )
				writer.WriteAttributeString( "table_size", TableSize.ToString(CultureInfo.InvariantCulture) );
			if( RowCount!=0 && RowCount!=long.MinValue )
				writer.WriteAttributeString( "row_count", RowCount.ToString(CultureInfo.InvariantCulture) );
			if( !Sealed )
				writer.WriteAttributeString( "sealed", Sealed.ToString() );
			if( TableType!=TableType.User )
				writer.WriteAttributeString( "type", TableType.ToString() );

			if( Deletable )
				writer.WriteAttributeString( "deletable", Deletable.ToString() );

			if( !string.IsNullOrEmpty(Summary) )
				writer.WriteElementString( "summary", Summary );
			if( Columns.Count>0 )
			{
				writer.WriteStartElement( "columns" );
				XmlSerializer ser = new XmlSerializer( typeof(Column) );
				foreach( Column item in Columns )
					ser.Serialize( writer, item );
				writer.WriteEndElement();
			}
			if( Data!=null )
			{
				XmlSerializer ser = new XmlSerializer( typeof(PersistentData) );
				ser.Serialize( writer, Data );
			}
			if( Enumeration!=null )
			{
				XmlSerializer ser = new XmlSerializer( typeof(Enumeration) );
				ser.Serialize( writer, Enumeration );
			}
			if( ForeignKeys.Count>0 )
			{
				writer.WriteStartElement( "foreign_keys" );
				XmlSerializer ser = new XmlSerializer( typeof(ForeignKey) );
				foreach( ForeignKey item in ForeignKeys )
					ser.Serialize( writer, item );
				writer.WriteEndElement();
			}
			if( Indexes.Count>0 )
			{
				writer.WriteStartElement( "indexes" );
				XmlSerializer ser = new XmlSerializer( typeof(Index) );
				foreach( Index item in Indexes )
					ser.Serialize( writer, item );
				writer.WriteEndElement();
			}
		}
		#endregion
		#region Assign
		internal void Assign()
		{
			foreach( Column column in Columns )
				column.ParentTable=this;
			foreach( ForeignKey fk in ForeignKeys )
			{
				fk.ForeignTable=this;
				fk.PrimaryTable = Schema.Tables.FirstOrDefault( (item)=>item.Name==fk.PrimaryTable.Name );
			}
			foreach( Index index in Indexes )
				index.ParentTable=this;
			if( Data!=null )
				Data.ParentTable = this;
		}
		#endregion
		#region AutoIncrement
		public bool AutoIncrement()
		{
			Collection<Column> columns = SurrogateKeys();
			return columns.Count==1 && columns[0].Sequence!=null;
		}
		#endregion
		#region BaseClass
		public string BaseClass{get;set;}
		#endregion
		#region BaseName
		public string BaseName()
		{
			StringBuilder className=new StringBuilder(Name.Length);
			int iStart=0;
			for( int iChar=iStart; iChar<Name.Length; ++iChar )
			{
				char ch=Name.ToCharArray()[iChar];
				if( iChar==Name.Length-1 && (ch=='s' || ch=='S') )
					break;
				else if( iChar==Name.Length-3 && string.Compare(Name.Substring(Name.Length-3, 3),"SES",StringComparison.OrdinalIgnoreCase)==0 )
				{
					if( Name.Substring( Name.Length-4 )=="SSES" )
						className.Append( "S" );
					else if( Name.Substring( Name.Length-4 )=="sses" )
						className.Append( "s" );
					break;
				}
				else if( iChar==Name.Length-3 && Name.Substring(Name.Length-3, 3)=="IES" )
				{
					className.Append( "Y" );
					break;
				}
				else if( iChar==Name.Length-3 && Name.Substring( Name.Length-3, 3 )=="ies" )
				{
					className.Append( "y" );
					break;
				}
				else if( iChar==Name.Length-3 && Name.Substring( Name.Length-3, 3 )=="xes" )
				{
					className.Append( "x" );
					break;
				}
				className.Append( ch );
			}
			return className.ToString();
		}
		#endregion
		#region Deletable
		public bool Deletable{ get; set; }
		#endregion
		#region GetKeys
		public LinkedList<Column> Keys()
		{
			LinkedList<Column> keys = new LinkedList<Column>();
			foreach( Column column in Columns )
			{
				if( column.Key!=null )
					keys.AddLast(column);
			}
			return keys;
		}
		#endregion
		#region GetProcedures
		string _insertProcName = string.Empty;
		[XmlAttribute("insert_proc")]
		public string InsertProcName
		{
			get
			{
				return _insertProcName=="none" ? null : string.IsNullOrEmpty(_insertProcName) && AutoIncrement() ? StdInsertProcName() : _insertProcName;
			}
			set{ _insertProcName=value;}
		}
		public string StdInsertProcName()
		{
			string baseName = BaseName();
			return baseName+"_insert";
		}
		public Procedure GetStdInsertProc()
		{
			return GetStdInsertProc(null);
		}
		
		bool HasInsertProc
		{
			get{ return !string.IsNullOrEmpty(InsertProcName);}
		}

		public Procedure GetStdInsertProc( Procedure proc )
		{
			if( !HasInsertProc )
				return null;
			if( proc==null )
				proc = new Procedure( InsertProcName, ProcedureType.Insert );
			Column surogateKey=null;
			int iParam=0;
			foreach( Column column in SortedColumns().Values )
			{
				if( column.SurrogateKey!=null && column.Sequence!=null )
					surogateKey=column;
				else if( column.RealColumn )
					proc.Parameters.Add(new SchemaParameter(iParam++, column));
			}
			if( surogateKey!=null )
				proc.Parameters.Add(new SchemaParameter(iParam++, surogateKey));
			proc.Table=this;
			return proc;
		}
		#endregion
		#region SurrogateKeys
		public Collection<Column> SurrogateKeys()
		{
			Collection<Column> surogateKeys = new Collection<Column>();
			foreach(Column column in Columns)
			{
				if( column.SurrogateKey!=null )
					surogateKeys.Add( column );
			}
			return surogateKeys;
		}
		#endregion
		#region ClassName
		string _className;
		public string ClassName
		{
			get{ return string.IsNullOrEmpty(_className) ? Name : _className; }
			set{ _className = value; } 
		}
		#endregion
		#region Columns
		Collection<Column> _columns=new Collection<Column>();

		[XmlArray( "columns" )]
		[XmlArrayItem( "column", typeof(Column) )] 
		public Collection<Column> Columns
		{
			get{ return _columns;}
			private set{ _columns=value;}
		}
		public SortedList<int, Column> SortedColumns()
		{
			SortedList<int, Column> columns=new SortedList<int,Column>();
			foreach( Column column in Columns )
				columns.Add( column.Ordinal, column );
			return columns;
		}
		public Column FindColumn(string columnName)
		{
			Column foundColumn=null;
			foreach( Column column in Columns )
			{
				if( column.Name==columnName )
				{
					foundColumn=column;
					break;
				}
			}
			return foundColumn;
		}
		public Column LastColumn()
		{
			Column lastColumn=null;
			foreach( Column column in Columns )
			{
				if( lastColumn==null || column.Ordinal>lastColumn.Ordinal )
					lastColumn = column;
			}
			return lastColumn;
		}
		#endregion
		#region Data
		PersistentData _data;
		[XmlElement( "data", typeof( PersistentData ) )]
		public PersistentData Data
		{
			get { return _data; }
			set { _data=value; }
		}
		public void Assign( PersistentData data )
		{
			Data = data;
			Data.ParentTable = this;
			if( Enumeration!=null )
				Enumeration.InitiateItems();
			foreach( PersistentRow row in Data.Items )
			{
				row.Data = data;
				EnumItem enumItem = Enumeration==null ? null : new EnumItem();
				foreach( DataItem item in row.Items )
				{
					Column column = FindColumn( item.Column.Name );
					if( column==null )
						throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "could not find column '{0}'.", item.Column.Name) );
					item.Column = column;
					if( enumItem!=null )
					{
						if( SurrogateKeys().Count==1 && SurrogateKeys()[0]==column )
							enumItem.Value = Convert.ToInt64( (ulong)item.ULongValue() );
						else
							enumItem.Name = item.StringValue;
					}
				}
				if( Enumeration!=null )
					Enumeration.Items.Add( enumItem );
			}
		}
		#endregion
		#region DefaultDisplay
		public string DefaultDisplay{ get; set; }
		#endregion
		#region Enumeration
		Enumeration _enumeration;
		[XmlElement("enumeration")]
		public Enumeration Enumeration
		{
			get { return _enumeration; }
			set { _enumeration=value; }
		}
		#endregion
		#region ForeignKeys
		Collection<ForeignKey> _foreignKeys = new Collection<ForeignKey>();
		[XmlArray( "foreign_keys" )]
		[XmlArrayItem( "foreign_key", typeof( ForeignKey ) )] 
		public Collection<ForeignKey> ForeignKeys
		{
			get{ return _foreignKeys;}
			private set{ _foreignKeys=value;}
		}
		public ForeignKey FindForeignKey(ForeignKey foreignKey)
		{
			ForeignKey foundKey=null;
			foreach( ForeignKey key in ForeignKeys )
			{
				if( key==foreignKey )
				{
					foundKey=key;
					break;
				}
			}
			return foundKey;
		}
		public ForeignKey FindForeignKey( string foreignKeyName )
		{
			ForeignKey foundKey=null;
			foreach( ForeignKey key in ForeignKeys )
			{
				if( key.Name==foreignKeyName )
				{
					foundKey=key;
					break;
				}
			}
			return foundKey;
		}
		public ForeignKey FindForeignKey( Column column )
		{
			return ForeignKeys.FirstOrDefault( fk => fk.Columns.Any(fkColumn => fkColumn.Name==column.Name) );
		}
		#endregion
		#region Indexes
		Collection<Index> _indexes=new Collection<Index>();
		[XmlArray( "indexes" )]
		[XmlArrayItem( "index", typeof( Index ) )] 
		public Collection<Index> Indexes
		{
			get{return _indexes;}
			private set{_indexes=value;}
		}
		public void SetIndexes( Collection<Index> indexes )
		{
			Indexes = indexes; 
		}

		public Index FindIndex( Index index )
		{
			Index foundIndex=null;
			foreach( Index item in Indexes )
			{
				if( index==item )
				{
					foundIndex=item;
					break;
				}
			}
			return foundIndex;
		}
		public Index FindIndex(string index)
		{
			Index foundIndex=null;
			foreach( Index item in Indexes )
			{
				if( index==item.Name )
				{
					foundIndex=item;
					break;
				}
			}
			return foundIndex;
		}
		bool IndexesExist( Index[] indexes )
		{
			bool bExists=true;
			foreach( Index index in indexes )
			{
				bExists = FindIndex(index)!=null;
				if( !bExists )
					break;
			}
			return bExists;
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute("name")]
		public string Name
		{
			get{ return _name; }
			set{ _name=value; }
		}
		#endregion
		#region Namespace
		public string Namespace{get;set;}
		#endregion
		#region Location
		Locations _location=Locations.Production;
		[XmlAttribute("location")]
		public Locations Location
		{
			get{ return _location; }
			set{ _location=value;}
		}
		#endregion
		#region Schema
		DataSchema _schema;
		[XmlIgnore]
		public DataSchema Schema
		{
			get{ return _schema;}
			set{ _schema=value; }
		}
		#endregion
		#region Sealed
		bool _sealed = true;
		public bool Sealed
		{
			get{ return _sealed; }
			set{ _sealed = value; }
		}
		#endregion
		#region Sequence
		/*Sequence _sequence=null;
		[XmlIgnore]
		public Sequence Sequence
		{
			set{ _sequence = value;}
		}*/
		public Sequence Sequence()
		{
			Sequence sequence = null;
			foreach( Column column in Columns )
			{
				if( column.Sequence!=null )
				{
					sequence = column.Sequence;
					if( sequence.Table==null )
						sequence.Table = this;
					break;
				}
			}
			return sequence;
		}/*
		public string GetStdSequenceName()
		{
			string prefix = string.Empty;
			if( MesType==MesTypes.Common )
				prefix = "CSEQ";
			else if( MesType==MesTypes.Quality )
				prefix = "QSEQ";
			else if( MesType==MesTypes.Material )
				prefix = "MSEQ";
			else if( MesType==MesTypes.Report )
				prefix = "RSEQ";

			return prefix+"_"+GetBaseName();
		}*/
		#endregion
		#region SimpleName
		string _simpleName;
		[XmlAttribute("simple_name")]
		public string SimpleName
		{
			get{ return string.IsNullOrEmpty(_simpleName) ? Name : _simpleName; }
			set{ _simpleName = value; }
		}
		#endregion
		#region Size
		long _indexSize;
		[XmlAttribute("index_size")]
		public long IndexSize
		{
			get{return _indexSize;}
			set{_indexSize=value;}
		}
		long _tableSize=long.MinValue;
		[XmlAttribute("table_size")]
		public long TableSize
		{
			get{return _tableSize;}
			set{_tableSize=value;}
		}
		long _rowCount=long.MinValue;
		[XmlAttribute("row_count")]
		public long RowCount
		{
			get{ return _rowCount;}
			set{ _rowCount=value;}
		}
		public bool InitiatedSize
		{
			get { return RowCount!=long.MinValue; }
		}
		#endregion
		#region Summary
		string _summary = string.Empty;
		[XmlElement("summary")]
		public string Summary
		{
			get{ return _summary;}
			set{ _summary=value;}
		}
		#endregion
		#region Types
		TableType _type = TableType.User;
		[XmlAttribute("type")]
		public TableType TableType
		{
			get{return _type;}
			set{_type=value;}
		}
		#endregion
	}

	public class TableComparer : IEqualityComparer<Table>
	{
		bool IEqualityComparer<Table>.Equals( Table x, Table y )
		{
			return /*x.Schema==y.Schema &&*/ x.Name==y.Name;
		}

		int IEqualityComparer<Table>.GetHashCode( Table obj )
		{
			return /*obj.Schema?.Name?.GetHashCode() ?? 0 ^ */obj.Name.GetHashCode();
		}
		public static TableComparer Default{get;} = new TableComparer();
	}
}
