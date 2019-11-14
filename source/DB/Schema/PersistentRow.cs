using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( PersistentRow.XmlElementName, IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class PersistentRow : IXmlSerializable
	{
		#region Constructors
		PersistentRow()
		{}
		PersistentRow( PersistentData data )
		{
			Data = data;
		}
		#endregion
		#region IXmlSerializable Members
		public const string XmlElementName = "row";
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
					if( reader.LocalName=="item" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(DataItem) );
						Items.Add( (DataItem)ser.Deserialize(reader) );
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

			if( Items!=null && Items.Count>0 )
			{
				XmlSerializer ser = new XmlSerializer( typeof(DataItem) );
				foreach( DataItem item in Items )
					ser.Serialize( writer, item );
			}
		}
		#endregion
		#region FindItem
		public DataItem FindItem( string columnName )
		{
			DataItem foundItem = null;
			foreach( DataItem item in Items )
			{
				if( item.Column.Name==columnName )
				{
					foundItem = item;
					break;
				}
			}
			return foundItem;
		}
		#endregion
		#region GetCountCommand
		public DB.TextCommand GetCountCommand( DB.Database db )
		{
			if( Data==null || Data.ParentTable==null )
				throw new InvalidOperationException( "Parent table has not been initialized." );
			var cmd = new TextCommand( TableName){ Database=db };
			foreach( DataItem item in Items )
			{
				if( item.Required )
					cmd.AddWhereParam( item.Column.Name, item.GetParameter(db.Syntax) );
			}
			return cmd;
		}
		#endregion
		#region GetCountSql
		public string GetCountSql( SqlSyntax sqlType )
		{
			if( Data==null || Data.ParentTable==null )
				throw new InvalidOperationException( "Parent table has not been initialized." );
			StringBuilder where = new StringBuilder();
			foreach( DataItem item in Items )
			{
				if( item.Required )
				{
					if( where.Length!=0 )
						where.Append( "\nand\t" );
					where.Append( item.FormatWhere(sqlType) );
				}
			}
			return where.Length==0
				? string.Empty
				: string.Format( CultureInfo.InvariantCulture, "select count(*) from {0} where {1}", Data.ParentTable.Name, where );
		}
		#endregion
		#region GetRowSql
		public string GetRowSql( SqlSyntax sqlType )
		{
			string surrogateWhere = GetSurogateWhere( sqlType );
			string where = string.IsNullOrEmpty(surrogateWhere) ? GetKeyWhere( sqlType ) : surrogateWhere;
			return string.IsNullOrEmpty(where) ? string.Empty : DB.SqlSyntax.GetSelectAll( Data.ParentTable.Name, where );
		}
		#endregion
		#region GetInsertStatement
		public string GetInsertStatement( SqlSyntax sqlType )
		{
			if( Data==null || Data.ParentTable==null )
				throw new InvalidOperationException( "Parent table has not been initialized." );

			StringBuilder columns = new StringBuilder();
			StringBuilder values = new StringBuilder();
			foreach( DataItem item in Items )
			{
				if( values.Length!=0 )
				{
					values.Append( "," );
					columns.Append( "," );
				}
				values.Append( item.FormatValue(sqlType) );
				columns.Append( item.Column.Name );
			}
			return string.Format( CultureInfo.InvariantCulture, "insert into {0}({1})values({2})", Data.ParentTable.Name, columns, values );
		}
		#endregion
		#region GetKeyWhere
		string GetKeyWhere( SqlSyntax sqlType )
		{
			if( Data==null || Data.ParentTable==null )
				throw new InvalidOperationException( "Parent table has not been initialized." );

			return GetWhere( sqlType, Data.ParentTable.Keys() );
		}
		#endregion
		#region GetSurogateWhere
		string GetSurogateWhere( SqlSyntax sqlType )
		{
			if( Data==null || Data.ParentTable==null )
				throw new InvalidOperationException( "Parent table has not been initialized." );

			Collection<Column> keys = Data.ParentTable.SurrogateKeys();
			return GetWhere( sqlType, keys );
		}
		#endregion
		#region GetUpdateStatement
		public string GetUpdateStatement( SqlSyntax sqlType, DataRow row )
		{
			if( Data==null || Data.ParentTable==null )
				throw new InvalidOperationException( "Parent table has not been initialized." );
			StringBuilder sql = new StringBuilder( DB.SqlSyntax.GetUpdatePrototype(Data.ParentTable.Name) );//string.Format("update {0} set ", ParentTable.Name)
			int emptySqlLength = sql.Length;
			foreach( DataItem item in Items )
			{
				if( !item.Required || item.Column.SurrogateKey!=null )
					continue;

				if( !item.TestValue(row[item.Column.Name]) )
				{
					if( sql.Length>emptySqlLength )
						sql.Append( "," );
					sql.AppendFormat( "{0}={1}", item.Column.Name, item.FormatValue(sqlType) );
				}
			}
			if( sql.Length>emptySqlLength )
				sql.AppendFormat( "\n{0} {1}", DB.SqlSyntax.Where, GetSurogateWhere(sqlType) );

			return sql.Length>emptySqlLength ? sql.ToString() : string.Empty;
		}
		#endregion
		#region GetWhere
		string GetWhere( SqlSyntax sqlType, ICollection<Column> keys )
		{
			StringBuilder sql = new StringBuilder(50);
			bool first = true;
			foreach( Column key in keys )
			{
				if( first )
					first = false;
				else
					sql.Append( "\nand\t" );
				DataItem item = FindItem( key.Name );
				if( item==null )//Surogate key isn't required.
				{
					sql.Length=0;
					break;
				}
				sql.Append( item.FormatWhere(sqlType) );
			}
			return sql.ToString();
		}
		#endregion
		#region Data
		public PersistentData Data{ get; set; }
		#endregion
		#region Location
		Locations _locations = Locations.Production;
		public Locations Locations
		{
			get{ return _locations;}
			set{ _locations = value;}
		}
		#endregion
		#region TableName
		public string TableName
		{
			get{ return Data.ParentTable.Name; }
		}
		#endregion
		#region Items
		Collection<DataItem> _items=new Collection<DataItem>();
		[XmlElement("item")]
		public Collection<DataItem> Items
		{
			get{return _items;}
			private set{_items=value;}
		}
		#endregion
	}
}
