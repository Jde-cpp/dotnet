using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "foreign_key", IsNullable=false, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	[System.Diagnostics.DebuggerDisplay("{Name}")]
	public class ForeignKey : ICloneable, IXmlSerializable
	{
		#region Constructors
		public ForeignKey( string name )
		{
			_name=name;
		}

		public ForeignKey( string name, Table foreignTable, Collection<Column> columns, Table primaryTable )
		{
			Name=name;
			Columns=columns;
			PrimaryTable=primaryTable;
			ForeignTable=foreignTable;
		}

		public ForeignKey()
		{}
		#endregion
		#region Drop
		public void Drop( DB.Database ds )
		{
			ds.Execute( ds.Syntax.DropForeignKeyStatement(ForeignTable.Name, Name) );
			ForeignTable.ForeignKeys.Remove( this );
		}
		#endregion
		#region IXmlSerializable Members
		public static System.Xml.XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new System.Xml.XmlUrlResolver();
			schemaSet.Add( DataSchema.XmlSchema );
			return new System.Xml.XmlQualifiedName( "foreign_key", DataSchema.XmlNamespace );
		}
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}
		void IXmlSerializable.ReadXml( System.Xml.XmlReader reader )
		{
			Name = reader.GetAttribute( "name" );
			PrimaryCollectionType = reader.GetAttribute( "primary_collection_type" );
			PrimaryMemberName = reader.GetAttribute( "primary_member_name" );
			string physicalLocations = reader.GetAttribute( "physical" );
			if( !string.IsNullOrEmpty(physicalLocations) )
				PhysicalLocations = Location.Parse( physicalLocations );

			var cardinality = reader.GetAttribute( "cardinality" );
			if( !string.IsNullOrEmpty(cardinality) )
				Cardinality = CardinalityExtensions.Parse( cardinality );

			var crossNamespace = reader.GetAttribute( "cross_namespace" );
			if( !string.IsNullOrEmpty(crossNamespace) )
				CrossNamespace = bool.Parse( crossNamespace );

			var excludePrimary = reader.GetAttribute( "exclude_primary" );
			if( !string.IsNullOrEmpty(excludePrimary) )
				ExcludePrimary = bool.Parse( excludePrimary );
			

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
					else if( reader.LocalName==Table.XmlElementName )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Table) );
						PrimaryTable = (Table)ser.Deserialize( reader );
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
			if( !string.IsNullOrEmpty(PrimaryCollectionType) )
				writer.WriteAttributeString("primary_collection_type", PrimaryCollectionType );
			if( !string.IsNullOrEmpty(PrimaryMemberName) )
				writer.WriteAttributeString("primary_member_name", Name );
			if( PhysicalLocations!=Locations.Production )
				writer.WriteAttributeString( "physical", PhysicalLocations.ToString() );
			if( Cardinality!=Cardinality.Child )
				writer.WriteAttributeString( "cardinality", Cardinality.ToString() );
			if( CrossNamespace )
				writer.WriteAttributeString( "cross_namespace", CrossNamespace.ToString(CultureInfo.InvariantCulture) );
			if( ExcludePrimary )
				writer.WriteAttributeString( "exclude_primary", ExcludePrimary.ToString(CultureInfo.InvariantCulture) );
			
			XmlSerializer ser = new XmlSerializer( typeof(Column) );
			foreach( Column column in Columns )
			{
				Column baseColumn = new Column( column.Name, column.Ordinal, null );
				ser.Serialize( writer, baseColumn );
			}
			ser = new XmlSerializer( typeof(Table) );
			Table table = new Table( PrimaryTable.Name, TableType.User );
			ser.Serialize( writer, table );
		}
		#endregion
		#region Equality Operators
		public static bool operator==( ForeignKey first, ForeignKey second )
		{
			bool equal = ReferenceEquals( first, second );
			if(!equal && (object)first != null && (object)second != null && first.Columns.Count==second.Columns.Count && first.ForeignTable.Name==second.ForeignTable.Name)
			{
				equal=true;
				foreach(Column aColumn in first.Columns)
				{
					foreach(Column bColumn in second.Columns)
						equal = aColumn.Name==bColumn.Name;
					if(!equal)
						break;
				}
			}
			return equal;
		}
		public static bool operator!=( ForeignKey first, ForeignKey second )
		{
			return !(first == second);
		}

		public override bool Equals( object obj )
		{
			return this==(ForeignKey)obj;
		}

		public override int GetHashCode()
		{
			return string.Format( CultureInfo.InvariantCulture, "{0}.{1}", ForeignTable.Name, Name ).GetHashCode();
		}
		#endregion
		#region CreateStatement
		public KeyValuePair<string,string> CreateStatement( DataSchema dbSchema )
		{
			if( Columns.Count==0 )
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Foreign key '{0}' has no columns.", Name) );
			if( PrimaryTable==null )
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Foreign key '{0}' has no primary table.", Name) );
			Collection<Column> surogateKeys = PrimaryTable.Columns;
			if( surogateKeys.Count==0 )
			{
				Table primaryTable = ForeignTable.Schema.Tables.FirstOrDefault(table=>table.Name==PrimaryTable.Name);// .TryGetValue[PrimaryTable.Name];
				if( primaryTable==null )
					throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Foreign key '{0}', could not find primary table '{1}'.", Name, PrimaryTable.Name) );

				surogateKeys = primaryTable.SurrogateKeys();

				if( surogateKeys.Count==0 )
					throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Foreign key '{0}' primary table '{1}' has '{2}' primary keys.", Name, PrimaryTable.Name, surogateKeys.Count) );
			}

			int suffix = 1;
			string name = Name;
			while( dbSchema.FindForeignKey(name)!=null )
				name = Name+suffix++;

			var columns = new System.Text.StringBuilder();
			foreach( var column in Columns )
			{
				if( columns.Length>0 )
					columns.Append(",");
				columns.Append( column.Name );
			}
			var foreignColumns = new System.Text.StringBuilder();
			foreach( var column in surogateKeys )
			{
				if( column.SurrogateKey==null )
					continue;
				if( foreignColumns.Length > 0 )
					foreignColumns.Append( "," );
				foreignColumns.Append( column.Name );
			}
			string createStatement = $"alter table {ForeignTable.Name} add constraint {name} foreign key({columns}) references {PrimaryTable.Name}({foreignColumns})";
			return new KeyValuePair<string,string>( name, createStatement );
		}
		#endregion
		#region Cardinality
		Cardinality _cardinality = Cardinality.Child;
		public Cardinality Cardinality
		{
			get{ return _cardinality; }
			set{ _cardinality = value; }
		}
		#endregion
		#region Columns
		Collection<Column> _columns = new Collection<Column>();
		[XmlElement( "column" )]
		public Collection<Column> Columns
		{
			get { return _columns; }
			private set { _columns=value; }
		}
		#endregion
		public bool CrossNamespace{get;set;}
		public bool ExcludePrimary{get;set;}
		#region ForeignTable
		Table _foreignTable;
		[XmlIgnore]
		public Table ForeignTable
		{
			get{ return _foreignTable;}
			set{ _foreignTable=value;}
		}
		#endregion
		#region Locations
		Locations _locations=Locations.ProductionArc;
		[XmlAttribute("locations")]
		public Locations Locations
		{
			get{ return _locations; }
			set{ _locations=value;}
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute("name")]
		public string Name
		{
			get{ return _name;}
			set{_name=value; }
		}
		#endregion
		#region PhysicalLocations
		Locations _physicalLocations = Locations.Production;
		[XmlAttribute( "physical" )]
		public Locations PhysicalLocations
		{
			get { return _physicalLocations; }
			set { _physicalLocations=value; }
		}
		#endregion
		#region PrimaryCollectionType
		public string PrimaryCollectionType{get;set;}
		#endregion
		#region PrimaryMemberName
		public string PrimaryMemberName{get;set;}
		#endregion
		#region PrimaryTable
		Table _primaryTable;
		[XmlElement("primary_table")]
		public Table PrimaryTable
		{
			get{ return _primaryTable;}
			set{ _primaryTable=value;}
		}
		#endregion
/*		#region Replicated
		bool _replicated=false;
		[XmlAttribute("replicated")]
		public bool Replicated
		{
			get{ return _replicated; }
			set{ _replicated=value; }
		}
		#endregion*/
		#region ICloneable Members
		public object Clone()
		{
			ForeignKey key = new ForeignKey(Name);
			foreach( Column col in Columns )
				key.Columns.Add( col.Clone(col.ParentTable) );
			key.ForeignTable = ForeignTable;
			key.Locations = Locations;
			key.PhysicalLocations = PhysicalLocations;
			key.PrimaryTable = PrimaryTable;

			return key;
		}
		#endregion
	}
}
