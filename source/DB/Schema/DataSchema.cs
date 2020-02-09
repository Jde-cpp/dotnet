using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	using Collections.Extensions;

	public interface ISchema
	{
		Schema.Table LoadTable( DataSource ds, string catalog, string tableName );
		Schema.DataSchema LoadSchema( DataSource ds, string catalog );
	}

	[XmlRoot("schema", IsNullable = true, Namespace=DataSchema.XmlNamespace),System.Serializable]
	[XmlSchemaProvider("MySchema")]
	public sealed class DataSchema : IXmlSerializable
	{
		#region Constructors
		public DataSchema()
		{}

		public static DataSchema Create( DataSource ds )
		{
			var creator = ds.Database as ISchema;
			if( creator==null )
				throw new ArgumentException( "DataSource '{0}'.Sql doesn't implement ISchema." );

			return creator.LoadSchema( ds, ds.InitialCatalog );
		}

		public static DataSchema CreateFromFiles( string schemaXml, string dataXml, string storedProcXml )
		{
			var schema = Xml.XmlSerialization<DB.Schema.DataSchema>.ReadFromFile( schemaXml );
			schema.Tables.ForEach( table => table.Data = new PersistentData() );
			if( !string.IsNullOrEmpty(dataXml) )
			{
				var data = Xml.XmlSerialization<DB.Schema.DataSchema>.ReadFromFile( dataXml );
				foreach( var table in data.Tables )
				{
					var existingTable = schema.Tables.FirstOrDefault( (item)=>item.Name==table.Name );
					if( existingTable==null )
						throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not find table '{0}' to add data to.", table.Name) );
					existingTable.Assign( table.Data );
				}
			}
			if ( storedProcXml!=null )
			{
				XmlDocument procDoc = new XmlDocument();
				procDoc.Load( storedProcXml );
				XmlNamespaceManager namespaceManager = new XmlNamespaceManager( procDoc.NameTable );
				namespaceManager.AddNamespace( "db", DataSchema.XmlNamespace );
				foreach( XmlNode node in procDoc.SelectNodes( "//db:schema/db:procedures/db:procedure", namespaceManager ) )
				{
					var proc = Xml.XmlSerialization<DB.Schema.Procedure>.Read( node );
					proc.FillValues();
					DB.Schema.Procedure appProc = schema.FindProc( proc.Name );
					if( appProc==null )
						schema.Procedures.Add( proc );
					else
					{
						appProc.Text = proc.Text;
						appProc.Prefix=proc.Prefix;
						appProc.Suffix=proc.Suffix;
					}
				}
			}
			return schema;
		}
		public static DataSchema Create( string schemaXml, string dataXml, string storedProcXml )
		{
			var schema = Xml.XmlSerialization<DataSchema>.Read( schemaXml );
//					schema.FillValues();

			schema.Tables.ForEach( table => table.Data = new PersistentData() );
			var data = Xml.XmlSerialization<DataSchema>.Read( dataXml );
			foreach( DB.Schema.Table dataTable in data.Tables )
			{
				var table = schema.Tables.FirstOrDefault( (item)=>item.Name==dataTable.Name );
				if( table==null )
					throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not find table '{0}' to add data to.", dataTable.Name) );
				table.Assign( table.Data );
			}

/*			doc.LoadXml( storedProcXml );
			Db.Schema.Schema data = (Db.Schema.Schema)serializer.Deserialize( new XmlNodeReader( doc.DocumentElement ) );
			foreach( Db.Schema.Table table in data.Tables.Values )
			{
				if( !schema.Tables.ContainsKey(table.Name) )
					throw new ApplicationException( string.Format("Could not find table '{0}' to add data to.", table.Name) );
				schema.Tables[table.Name].SetStoriedProcs( table.StoredProcs );
			}
*/
			XmlDocument procDoc = new XmlDocument();
			procDoc.LoadXml( storedProcXml );
			XmlNamespaceManager namespaceManager = new XmlNamespaceManager( procDoc.NameTable );
			namespaceManager.AddNamespace( "db", DataSchema.XmlNamespace );
			var serializer = new XmlSerializer( typeof( DB.Schema.Procedure ) );
			foreach( XmlNode node in procDoc.SelectNodes( "//db:schema/db:procedures/db:procedure", namespaceManager ) )
			{
				var proc = Xml.XmlSerialization<Procedure>.Read( node );
				proc.FillValues();
				var appProc = schema.FindProc( proc.Name );
				if( appProc==null )
					schema.Procedures.Add( proc );
				else
				{
					appProc.Text = proc.Text;
					appProc.Prefix=proc.Prefix;
					appProc.Suffix=proc.Suffix;
				}
			}
			return schema;
		}
		#endregion
		#region IXmlSerializable Members
		public const string XmlNamespace = "http://jde.data.db";
		static System.Xml.Schema.XmlSchema _xmlSchema;
		public static System.Xml.Schema.XmlSchema XmlSchema
		{
			get
			{
				if( _xmlSchema==null )
					System.Threading.Interlocked.CompareExchange<System.Xml.Schema.XmlSchema>(ref _xmlSchema, System.Xml.Schema.XmlSchema.Read(new System.IO.StringReader(Resources.Schema), null), null );
				return _xmlSchema;
			}
			set{ _xmlSchema = value; }
		}
		public static XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new XmlUrlResolver();
			schemaSet.Add( XmlSchema );
			return new XmlQualifiedName( "schema", XmlNamespace );
		}
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}
		void IXmlSerializable.ReadXml( System.Xml.XmlReader reader )
		{
			Name = reader.GetAttribute("name");
			string version = reader.GetAttribute("version");
			if( version!=null )
			{
//				throw new ApplicationException( string.Format("Schema '{0}' does not have a required version.", Name) );
				string[] versions = version.Split( '.' );

				if( versions.Length<2 )
					throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Schema '{0}' version ''{1}' does not have 2 parts.", Name) );
				MajorVersion = int.Parse( versions[0], CultureInfo.InvariantCulture );
				MinorVersion = int.Parse( versions[1], CultureInfo.InvariantCulture );
				if( versions.Length>2 )
					PatchVersion = int.Parse( versions[2], CultureInfo.InvariantCulture );
			}

			if( reader.IsEmptyElement )
				reader.Read();
			else
			{
				reader.Read();
				while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
				{
					if( reader.NodeType!=System.Xml.XmlNodeType.Element )
						reader.Read();
					else if( reader.Name=="tables" )
					{
						reader.ReadStartElement();
						while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
						{
							if( reader.NodeType==System.Xml.XmlNodeType.Element )
							{
								var table = Xml.XmlSerialization<Table>.Read( reader );
								table.Schema = this;
								Procedure proc = table.GetStdInsertProc();
								if( proc!=null )
									Procedures.Add( proc );
								Sequence seq = table.Sequence();
								if( seq!=null )
									Sequences.Add( seq.Name, seq );
								if( Tables.Contains(table) )
									throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Table '{0}' has been defined twice.", table.Name) );
								else
									Tables.Add( table );
							}
							else
								reader.Read();
						}
					}
					else if( reader.Name==SchemaDefaults.XmlElementName )
						SchemaDefaults = Xml.XmlSerialization<SchemaDefaults>.Read( reader );
					else
						reader.Read();
				}
			}
			foreach( Table table in Tables )
				table.Assign();
		}
		void IXmlSerializable.WriteXml( System.Xml.XmlWriter writer )
		{
			throw new NotImplementedException( "The method or operation is not implemented." );
		}
		#endregion
		#region ComputeSize
		/*		void ComputeSize( Table table, Provider provider )
		{
			if( table.InitiatedSize )
				return;

			if( conn.Database.HasSpaceUsedProc )
			{
				DataSet dataSet=conn.GetDataSet( string.Format("exec sp_spaceused {0}", table.Name), "space" );
				if(dataSet.Tables["space"].Rows.Count>0)
				{
					DataRow row=dataSet.Tables["space"].Rows[0];
					table.RowCount = ExtractNumeric( (string)row[1] );
					table.TableSize=ExtractNumeric( (string)row[3] );
					table.IndexSize=ExtractNumeric( (string)row[4] );
				}
			}
			else
				throw new InvalidOperationException("non-supported database type");
		}
*/
		#endregion
		#region Defaults
		public SchemaDefaults SchemaDefaults{get;set;}
		#endregion
		#region FillValues
		/*		public void FillValues()
		{
			foreach( Table table in Tables.Values )
			{
				table.Schema=this;
				table.FillValues();
				List<Column> keys = table.GetSurogateKeys();
				if( keys.Count==1 && keys[0].Sequence!=null )
				{
					Sequence sequence=new Sequence( table );
					table.Sequence=sequence;
					sequence.Table=table;
					Sequences.Add( sequence.Name, sequence );
					Procedure proc = FindProc( table.GetStdInsertProcName() );
					if( proc==null || proc.Prefix.Length>0 )
						Procedures.Add( table.GetStdInsertProc( proc ) );
				}
				foreach( ForeignKey fk in table.ForeignKeys )
				{
					Table primaryTable = Tables.ContainsKey(fk.PrimaryTable.Name) ?  Tables[fk.PrimaryTable.Name] : null;
					if( primaryTable==null )
						throw new ArgumentException( "lost table:  "+primaryTable);
					fk.PrimaryTable=primaryTable;
				}
			}
		}*/
		#endregion
		#region FindEnumeration
		Enumeration FindEnumeration( string tableName, string columnName )
		{
			Enumeration foundEnumeration=null;
			Table table = Tables.FirstOrDefault( (item)=>item.Name==tableName );
			if( table!=null )
			{
				Column column = table.FindColumn(columnName);
				if( column!=null )
					foundEnumeration=column.Enumeration;
			}
			return foundEnumeration;
		}
		#endregion
		#region FindForeignKey
		public ForeignKey FindForeignKey( string name )
		{
			Table foundTable = Tables.FirstOrDefault( table => table.FindForeignKey(name)!=null );
			return foundTable==null ? null : foundTable.FindForeignKey(name);
		}
		public List<ForeignKey> FindReferences( string tableName )
		{
			List<ForeignKey> refs = new List<ForeignKey>();
			foreach( Table table in Tables )
			{
				List<ForeignKey> fks = table.ForeignKeys.ToList().FindAll( fk => fk.PrimaryTable.Name==tableName );
				refs.AddRange( fks );
			}

			return refs;
		}
		#endregion
		#region FindIndexes
		public Collection<Index> FindIndexes(string indexName)
		{
			Collection<Index> existingIndexes = new Collection<Index>();
			foreach( Table table in Tables )
			{
				foreach( var index in table.Indexes )
				{
					if( indexName==index.Name )
						existingIndexes.Add(index);
				}
			}
			return existingIndexes;
		}
		#endregion
		#region FindProc
		public Procedure FindProc(string name)
		{
			Procedure found = null;
			foreach( Procedure proc in Procedures )
			{
				if( proc.Name==name )
				{
					found=proc;
					break;
				}
			}
			return found;
		}
		#endregion
		#region FindSequence
		public Sequence FindSequence(string name)
		{
			Sequence found = null;
			foreach( Sequence sequence in Sequences.Values )
			{
				if( sequence.Name==name )
				{
					found=sequence;
					break;
				}
			}
			return found;
		}
		public Sequence FindTableSequence(string tableName)
		{
			Sequence found = null;
			foreach( Sequence sequence in Sequences.Values )
			{
				if( sequence.Table.Name==tableName )
					found = sequence;
			}
			return found;
		}
		#endregion
		#region ParseVersion
		static void ParseVersion( string version, ref int major, ref int minor )
		{
			string[] versions = version.Split('.');
			if( versions.Length==2 )
			{
				major=int.Parse( versions[0], CultureInfo.InvariantCulture );
				minor=int.Parse( versions[1], CultureInfo.InvariantCulture );
			}
			else
				throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "version string {0}, is not valid.", version) );
		}
		#endregion
		#region Add
/*		public void Add( string tableName, ForeignKey key )
		{
			if( !Tables.ContainsKey(tableName) )
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Schema '{0}' does not contain table '{1}'.", Name, tableName) );
			if( !Tables.ContainsKey(key.PrimaryTable.Name) )
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Schema '{0}' does not contain primary table '{1}'.", Name, key.PrimaryTable.Name) );

			Table table = Tables[tableName];
			Table pkTable = Tables[key.PrimaryTable.Name];
			Collection<Column> columns = new Collection<Column>();
			foreach( Column keyColumn in key.Columns )
			{
				Column tableColumn = table.FindColumn( keyColumn.Name );
				if( tableColumn==null )
					throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Table '{0}'.'{1}' does not contain column '{2}'.", Name, tableName, keyColumn.Name) );
				columns.Add( tableColumn );
			}
			Tables[tableName].ForeignKeys.Add( new ForeignKey(key.Name, table, columns, pkTable) );
		}
*/
		public void Add( ForeignKey key, string fkName )
		{
			var table = Tables.FirstOrDefault( (item)=>item.Name==key.ForeignTable.Name );
			if( table==null )
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Schema '{0}' does not contain table '{1}'.", Name, key.ForeignTable.Name) );
			var pkTable = Tables.FirstOrDefault( (item)=>item.Name==key.PrimaryTable.Name );
			if( pkTable==null )
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Schema '{0}' does not contain primary table '{1}'.", Name, key.PrimaryTable.Name) );

			Collection<Column> columns = new Collection<Column>();
			foreach( Column keyColumn in key.Columns )
			{
				Column tableColumn = table.FindColumn( keyColumn.Name );
				if( tableColumn==null )
					throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "Table '{0}'.'{1}' does not contain column '{2}'.", Name, key.ForeignTable.Name, keyColumn.Name) );
				columns.Add( tableColumn );
			}
			table.ForeignKeys.Add( new ForeignKey(fkName, table, columns, pkTable) );
		}
		#endregion
		#region Connection
		[NonSerialized]
		DataSource _dataSource;
		public DataSource DataSource
		{
			get { return _dataSource; }
			set { _dataSource=value; }
		}
		#endregion
		[XmlAttribute("id")] public string Name{ get; set;}
		#region Tables
		[XmlArray( "tables" )]
		[XmlArrayItem( "table", typeof( Table ) )]
		public HashSet<Table> Tables{get;private set; } = new HashSet<Table>();
		public void SetTables( HashSet<Table> tables )
		{
			Tables = tables;
			Tables.ForEach( table => table.Schema=this );
		}
		public Table FindTable(string name)=>Tables.FirstOrDefault( (table)=>table.Name==name );
		#endregion
		#region Procedures
		Collection<Procedure> _procedures=new Collection<Procedure>();
		[XmlElement("procedure")]
		public Collection<Procedure> Procedures
		{
			get{ return _procedures; }
			private set{ _procedures=value; }
		}
		#endregion
		#region Sequences
		Dictionary<string,Sequence> _sequences=new Dictionary<string,Sequence>();
		[XmlElement("sequence")]
		public Dictionary<string,Sequence> Sequences
		{
			get{ return _sequences;}
			private set{ _sequences=value;}
		}
		#endregion
		#region MajorVersion
		int _majorVersion=int.MinValue;
		[XmlAttribute("major_version")]
		public int MajorVersion
		{
			get{ return _majorVersion; }
			set{ _majorVersion=value; }
		}
		#endregion
		#region MinorVersion
		int _minorVersion=int.MinValue;
		[XmlAttribute("minor_version")]
		public int MinorVersion
		{
			get{ return _minorVersion; }
			set{ _minorVersion=value; }
		}
		#endregion
		#region Packages
#if Unused
		Collection<Package> _packages=new Collection<Package>();
		[XmlElement("package")]
		public Collection<Package> Packages
		{
			get{ return _packages;}
			private set{ _packages=value;}
		}
#endif
		#endregion
		#region PatchVersion
		int _patchVersion;
		public int PatchVersion
		{
			get{ return _patchVersion; }
			set{ _patchVersion=value; }
		}
		#endregion
		#region Unicode
		bool _unicode;
		[XmlIgnore]
		public bool Unicode
		{
			get { return _unicode; }
			set { _unicode=value; }
		}
		#endregion
	};
}
