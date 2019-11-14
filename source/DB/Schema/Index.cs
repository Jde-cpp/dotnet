using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "index", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class Index : ICloneable, IXmlSerializable
	{
		#region Constructors
		public Index( Table table, string name, bool clustered, bool unique, bool primaryKey )
		{
			ParentTable=table;
			Name=name;
			Clustered=clustered;
			Unique=unique;
			PrimaryKey=primaryKey;
		}
		public Index() { }
		#endregion
		#region Clone
		Object ICloneable.Clone()
		{
			Index clone=new Index( ParentTable, Name, Clustered, Unique, PrimaryKey );
			foreach( Column column in Columns )
				clone.Columns.Add( column.Clone(ParentTable) );
			return clone;
		}
		
		public Index Clone( Table table, string name )
		{
			Index clone=new Index( table, name, Clustered, Unique, PrimaryKey );
			foreach(Column column in Columns)
				clone.Columns.Add( column.Clone( table ) );
			return clone;
		}
		#endregion
		#region Drop
		public List<ForeignKey> Drop( DB.Database db )
		{
			DataSchema schema = ParentTable.Schema;
			List<ForeignKey> droppedFks = new List<ForeignKey>();
			if( PrimaryKey )
			{
				List<ForeignKey> fks = schema.FindReferences( ParentTable.Name );
				fks.ForEach( fk => fk.Drop(db) );
				droppedFks.AddRange( fks );
			}
			db.Execute( DropStatement(db.Syntax) );

			return droppedFks;
		}

		public string DropStatement( SqlSyntax sqlType )=>sqlType.DropIndexStatement( PrimaryKey, ParentTable.Name, Name );
		#endregion
		#region IXmlSerializable Members
		public static System.Xml.XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new System.Xml.XmlUrlResolver();
			schemaSet.Add( DataSchema.XmlSchema );
			return new System.Xml.XmlQualifiedName( "index", DataSchema.XmlNamespace );
		}
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml( System.Xml.XmlReader reader )
		{
			Name = reader.GetAttribute( "name" );
			string clustered = reader.GetAttribute( "clustered" );
			if( !string.IsNullOrEmpty(clustered) )
				Clustered = bool.Parse( clustered );

			string locations = reader.GetAttribute( "locations" );
			if( !string.IsNullOrEmpty(locations) )
				Locations = Location.Parse( locations );

			string primaryKey = reader.GetAttribute( "primary_key" );
			if( !string.IsNullOrEmpty(primaryKey) )
				PrimaryKey = bool.Parse( primaryKey );

			string unique = reader.GetAttribute( "unique" );
			if( !string.IsNullOrEmpty(unique) )
				Unique = bool.Parse( unique );

			while( reader.NodeType==System.Xml.XmlNodeType.Attribute )
				reader.MoveToElement();
			if( !reader.IsEmptyElement )
			{
				reader.Read();
				while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
				{
					if( reader.NodeType!=System.Xml.XmlNodeType.Element )
					{
						reader.Read();
						continue;
					}
					if( reader.LocalName=="column" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Column) );
						Columns.Add( (Column)ser.Deserialize(reader) );
					}
					else
						reader.Read();
				}
				reader.ReadEndElement();
			}
			else
				reader.Read();
		}

		void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			if( !string.IsNullOrEmpty(Name) )
				writer.WriteAttributeString("name", Name );
			if( Clustered )
				writer.WriteAttributeString( "clustered", Clustered.ToString().ToLower(CultureInfo.InvariantCulture) );
			if( Locations!=Locations.ProductionArc )
				writer.WriteAttributeString( "locations", Locations.ToString() );
			if( PrimaryKey )
				writer.WriteAttributeString( "primary_key", PrimaryKey.ToString().ToLower(CultureInfo.InvariantCulture) );
			if( Unique )
				writer.WriteAttributeString( "unique", Unique.ToString().ToLower(CultureInfo.InvariantCulture) );

			
			XmlSerializer ser = new XmlSerializer( typeof(Column) );
			foreach( Column column in Columns )
			{
				Column baseColumn = new Column( column.Name, column.Ordinal, null );
				ser.Serialize( writer, baseColumn );
			}
		}
		#endregion
		#region Equality Operators
		public static bool operator==( Index first, Index second )
		{
			bool equal = ReferenceEquals( first, second );
			if( !equal && (object)first != null && (object)second != null && first.Columns.Count==second.Columns.Count && string.Compare(first.ParentTable.Name, second.ParentTable.Name, StringComparison.OrdinalIgnoreCase)==0 )
			{
				equal=true;
				for( int iColumn = 0; iColumn<first.Columns.Count; ++iColumn )
				{
					equal = string.Compare(first.Columns[iColumn].Name, second.Columns[iColumn].Name, StringComparison.OrdinalIgnoreCase)==0;
					if(!equal)
						break;
				}
			}
			return equal;
		}
		public static bool operator!=( Index first, Index second )
		{
			return !(first == second);
		}

		public override bool Equals( object obj )
		{
			return this==(Index)obj;
		}

		public override int GetHashCode()
		{
			return string.Format( CultureInfo.InvariantCulture, "{0}.{1}", ParentTable.Name, Name ).GetHashCode();
		}
		#endregion
		#region CreateStatement
		/// <summary>Index was just dropped, so you know the name is unique.</summary>
		/// <param name="index">Index to create.</param>
		public KeyValuePair<string,string> CreateStatement( SqlSyntax sqlType )
		{
			return CreateStatement( sqlType, null );
		}

		public KeyValuePair<string,string> CreateStatement( SqlSyntax sqlType, DataSchema dbSchema )
		{
			StringBuilder createStatement=new StringBuilder();
			string indexName = dbSchema!=null ? GetUniqueName(dbSchema, sqlType.UniqueIndexNames) : Name;

			string unique = Unique ? "unique" : string.Empty;
			if( PrimaryKey )
				createStatement.AppendFormat( "alter table {0} add constraint {1} primary key {2} (", ParentTable.Name, indexName, Clustered || !sqlType.SpecifyIndexCluster ? "" : "nonclustered" );
			else
			{
				createStatement.AppendFormat( "CREATE {0} {1} INDEX {2} on ", (Clustered && sqlType.SpecifyIndexCluster ? "clustered" : ""), unique, indexName );
				createStatement.AppendFormat( "{0}(", ParentTable.Name );
			}

			bool firstColumn=true;
			foreach(Schema.Column column in Columns)
			{
				if(firstColumn)
					firstColumn=false;
				else
					createStatement.Append( "," );
				createStatement.Append( column.Name );
			}
			createStatement.Append( ")" );
			return new KeyValuePair<string,string>( indexName, createStatement.ToString() );
		}
		#endregion
		#region GetName
		public string GetUniqueName( DataSchema dbSchema, bool uniqueName )
		{
			String indexName=Name;
			if( PrimaryKey || uniqueName )
			{
				for( int iIndex=1; ;++iIndex )
				{
					Collection<Index> existingIndexes = dbSchema.FindIndexes( indexName );
					bool bExists=false;
					foreach( Index existingIndex in existingIndexes )
					{
						bExists = string.Compare( existingIndex.Name, indexName, StringComparison.OrdinalIgnoreCase )==0;
						if( bExists )
							break;
					}
					indexName = bExists ? string.Format( CultureInfo.InvariantCulture, "{0}{1}", Name, iIndex.ToString(CultureInfo.InvariantCulture) ) : indexName;
					if( !bExists )
						break;
				}
			}
			return indexName;
		}
		#endregion
		#region Clustered
		bool _clustered;
		[XmlAttribute( "clustered" )]
		public bool Clustered
		{
			get { return _clustered; }
			set { _clustered=value; }
		}
		#endregion
		#region Columns
		Collection<Column> _columns=new Collection<Column>();
		[XmlElement( "column" )]
		public Collection<Column> Columns
		{
			get { return _columns; }
			private set { _columns=value; }
		}
		public Column FindColumn( string columnName )
		{
			Column foundColumn = null;
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
		#endregion
		#region Locations
		Locations _locations = Locations.ProductionArc;
		public Locations Locations
		{
			get{ return _locations;}
			set{ _locations = value;}
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute("name")]
		public string Name
		{
			get{ return _name;}
			set{_name=value;}
		}
		#endregion
		#region ParentTable
		Table _parentTable;
		[XmlIgnoreAttribute]
		public Table ParentTable
		{
			get{ return _parentTable;}
			set{_parentTable=value;}
		}
		#endregion
		#region PrimaryKey
		bool _primaryKey;
		[XmlAttribute( "primary_key" )]
		public bool PrimaryKey
		{
			get { return _primaryKey; }
			set { _primaryKey=value; }
		}
		#endregion
		#region Unique
		bool _unique;
		[XmlAttribute("unique")]
		public bool Unique
		{
			get{ return _unique;}
			set{ _unique=value;}
		}
		#endregion
	}
}