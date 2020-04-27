using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Jde.DB.Dialects
{
	using Schema;
	public class MySqlSchema : ISchema
	{
		Table ISchema.LoadTable( DataSource ds, string catalog, string tableName )
		{
			throw new NotImplementedException();
		}

		static HashSet<Table> LoadTables( DataSource ds, string tableName, string catalog )
		{
		   var tables = new HashSet<Table>( TableComparer.Default );

		   var dataSet=ds.LoadDataSet( GetColumnSql(tableName, catalog), "table" );
		   Schema.Table table=null;
		   int ordinal = 1;
		   foreach( System.Data.DataRow row in dataSet.Tables["table"].Rows )
		   {
		      string currentTableName = (string)row["TABLE_NAME"];
		      if( table==null || currentTableName!=table.Name )
		      {
		         string tableTypeName=(string)row["TABLE_TYPE"] as string;
		         var tableType = Schema.TableType.User;
		         if(tableTypeName!="BASE TABLE")
		         {
		            if(tableTypeName=="VIEW")
		               continue;
		            else
		            {
							System.Diagnostics.Debug.Assert(false);//need to implement
		               continue;
		            }
		         }
		         table = new Schema.Table( currentTableName, tableType );
		         tables.Add( table );
		         ordinal = 1;
		      }

		      var column=new Schema.Column( (string)row["COLUMN_NAME"], ordinal++, table ); //row["ORDINAL_POSITION"] isn't correct when you add drop then add a column.
				string dflt = row["COLUMN_DEFAULT"].GetType() == typeof(DBNull) ? null : (string)row["COLUMN_DEFAULT"];
				if( !string.IsNullOrEmpty(dflt) )
				{
					if( string.Compare(dflt, "(getutcdate())", StringComparison.OrdinalIgnoreCase)==0 )
						column.Default = new Schema.Default( Schema.DefaultType.UtcDate );
					else if( string.Compare(dflt, "(getdate())", StringComparison.OrdinalIgnoreCase)==0 )
						column.Default = new Schema.Default( Schema.DefaultType.Date );
					else if( string.Compare(dflt, "(user_name())", StringComparison.OrdinalIgnoreCase)==0 )
						column.Default = new Schema.Default( Schema.DefaultType.UserName );
					else
						column.Default = new Schema.Default( dflt );
				}
		      column.Nullable=(string)row["IS_NULLABLE"]=="YES";
		      var typeName = row["COLUMN_TYPE"] as string;
		      if( typeName==null ) //|| currentTableName=="sec_order_type_definitions" )//|| column.Name=="exchange_id"
		      {
					System.Diagnostics.Debug.Assert(false);//it can happen.
		         continue;
		      }
		      column.DataType = TryGetDataType( typeName );
				column.Precision = row["NUMERIC_PRECISION"].GetType()==typeof(DBNull) ? (ushort?)null : Convert.ToUInt16(row["NUMERIC_PRECISION"]);
				column.Scale = row["NUMERIC_SCALE"].GetType()==typeof(DBNull) ? (ushort?)null : Convert.ToUInt16( row["NUMERIC_SCALE"] );

		      column.MaxLength=row["CHARACTER_MAXIMUM_LENGTH"].GetType()==typeof(DBNull) ? null : (int?)Int32.Parse( row["CHARACTER_MAXIMUM_LENGTH"].ToString(), CultureInfo.InvariantCulture );

		      column.Sequence = row["is_identity"].GetType()!=typeof(DBNull) && Convert.ToInt32(row["is_identity"])==1 ? new DB.Schema.Sequence() : null;
		      if( Convert.ToInt32(row["is_id"])==1 )
		         column.SurrogateKey = new Jde.DB.Schema.SurrogateKey();

		      table.Columns.Add( column );
		   }//for each column.
		   return tables;
		}

		DataSchema ISchema.LoadSchema( DataSource ds, string schemaName )
		{
			var schema = new DataSchema();
			schema.SetTables( LoadTables( ds, string.Empty, schemaName ) );

		//indexes
		   var indexes = LoadIndexes( ds, schema.Tables, schemaName, null );
		   foreach( var index in indexes )
		   {
		      var table = schema.Tables.FirstOrDefault( (item)=>item.Name==index.ParentTable.Name );
		      if( table == null )
		      {
					System.Diagnostics.Debug.Assert( false, string.Format(CultureInfo.InvariantCulture, "Can not find table '{0}' for index '{1}'.", index.ParentTable.Name, index.Name) );
		         continue;
		      }
		      index.ParentTable = table;
		      table.Indexes.Add( index );
		   }

		   var fkDataSet=ds.LoadDataSet( string.Format(MySqlStrings.ForeignKeys, schemaName), "fk" );
		   Schema.Table pkTable = null;
		   var fkColumns = new Collection<Schema.Column>();
		   string currentName = null;
		   Schema.Table fkTable = null;
		   foreach( System.Data.DataRow row in fkDataSet.Tables["fk"].Rows )
		   {
		      var name = row["name"] as string;
		      if( name!=currentName && fkTable!=null )
		      {
		         fkTable.ForeignKeys.Add( new Schema.ForeignKey(currentName, fkTable, fkColumns, pkTable) );
		         fkColumns = new Collection<Schema.Column>();
		      }
		      var fkTableName = row["foreign_table"] as string;
		      fkTable = schema.FindTable( fkTableName );
		      if( fkTable==null )
		      {
					System.Diagnostics.Debug.Assert(false);
		         continue;
		      }
		      currentName=name;

		      var foreignKeyColumn=row["fk"] as string;
		      var foreignKey = fkTable.FindColumn( foreignKeyColumn );
		      if( foreignKey==null )
		      {
					System.Diagnostics.Debug.Assert(false);
		         continue;
		      }
		      fkColumns.Add( foreignKey );
		      var pkTableName = row["primary_table"] as string;
		      pkTable = schema.FindTable(pkTableName);
		      if( pkTable==null )
		      {
					System.Diagnostics.Debug.Assert(false);
		         continue;
		      }
		   }
		   if( fkTable!=null )
		      fkTable.ForeignKeys.Add( new Schema.ForeignKey( currentName, fkTable, fkColumns, pkTable ) );

		   var storedProcs=ds.Database.LoadDataTable( $"select SPECIFIC_NAME from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA='{schemaName}'" );
		   foreach( System.Data.DataRow row in storedProcs.Rows )
		      schema.Procedures.Add( new Schema.Procedure( row["SPECIFIC_NAME"] as string ) );

			return schema;
		}
		Collection<Schema.Index> LoadIndexes( DataSource ds, ISet<DB.Schema.Table> tables, string schema, string tableName )
		{
		   var indexes = new System.Collections.ObjectModel.Collection<Schema.Index>();

		   string sql = string.Format( "SELECT * FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{0}'", schema );
			if( !string.IsNullOrEmpty(tableName) )
				sql = string.Format( "{0} and TABLE_NAME='{0}'", sql, tableName );
		   var indexDataSet = ds.LoadDataSet( sql, "INDEXES" );
		   foreach( System.Data.DataRow row in indexDataSet.Tables["INDEXES"].Rows )
		   {
		      string indexName = (string)row["INDEX_NAME"];
		      string currentTableName = (string)row["TABLE_NAME"];
		      //string tableSchema = (string)row["TABLE_SCHEMA"];
		      //if( GetDBUser(ds)!=tableSchema )
		       //  continue;

		      Schema.Index foundIndex = indexes.FirstOrDefault( (item)=>item.Name==indexName && item.ParentTable.Name==currentTableName );
		      if( foundIndex==null )
		      {
		         bool clustered = false;//Boolean.Parse( row["CLUSTERED"].ToString() );
		         bool unique = Convert.ToInt32(row["NON_UNIQUE"])==0;//.ToString() );
		         bool primaryKey = indexName=="PRIMARY";//Boolean.Parse( row["PRIMARY_KEY"].ToString() );
					var table = tables.FirstOrDefault( (item)=>item.Name==currentTableName );
		         if( table==null )
		            throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not find table '{0}' for index '{1}'.", currentTableName, indexName) );
		         foundIndex = new Schema.Index( table, indexName, clustered, unique, primaryKey );
		         indexes.Add( foundIndex );
		      }

		      string columnName = row["COLUMN_NAME"] as string;
		      var column = foundIndex.ParentTable.FindColumn( columnName );
		      if( column==null )
		         throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not find column '{0}' for table '{1}' for index '{2}'.", columnName, currentTableName, indexName) );
				if( foundIndex.PrimaryKey )
					column.SurrogateKey = new DB.Schema.SurrogateKey();
		      foundIndex.Columns.Add( column );
		   }

		   return indexes;
		}

		static string GetColumnSql( string tableName, string catalog )
		{
			string tableQuery = string.IsNullOrEmpty(tableName) ? string.Empty : string.Format( CultureInfo.InvariantCulture, "\tand\tt.TABLE_NAME='{0}'\n", tableName );
			return string.Format( MySqlStrings.Columns, catalog, tableQuery );
		}

		static DataType TryGetDataType( string typeName )
		{
			DataType type = DataType.None;
			if( typeName=="datetime" || typeName=="datetime(6)" )
				type=DataType.DateTime;
			else if( typeName=="smalldatetime" )
				type=DataType.SmallDateTime;
			else if( typeName=="float" || typeName=="double" )
				type=DataType.Float;
			else if(typeName=="real")
				type=DataType.SmallFloat;
			else if( typeName=="int" || typeName=="int(11)" )
				type = DataType.Int;
			else if( typeName=="int(10) unsigned" || typeName=="int unsigned" )
				type = DataType.UInt;
			else if( typeName=="bigint(21) unsigned" || typeName=="bigint(20) unsigned" )
				type = DataType.ULong;
			else if(typeName.StartsWith("bigint"))
				type=DataType.Long;
			else if( typeName=="nvarchar" )
				type=DataType.VarWChar;
			else if(typeName=="nchar")
				type=DataType.WChar;
			else if (typeName.StartsWith("smallint(5) unsigned"))
				type = DataType.UInt16;
			else if (typeName.StartsWith("smallint"))
				type=DataType.Int16;
			else if( typeName == "tinyint(3) unsigned" || typeName == "tinyint unsigned" )
				type = DataType.UInt8;
//			else if (typeName.StartsWith("tinyint unsigned"))
//				type = DataType.UInt16;
			else if (typeName=="tinyint" || typeName=="tinyint(4)")
				type=DataType.Int8;
			else if(typeName=="uniqueidentifier")
				type=DataType.Guid;
			else if(typeName=="varbinary")
				type=DataType.VarBinary;
			else if(typeName.StartsWith("varchar", StringComparison.OrdinalIgnoreCase) )
				type=DataType.VarChar;
			else if(typeName=="ntext")
				type=DataType.NText;
			else if(typeName=="text")
				type=DataType.Text;
			else if(typeName=="char")
				type=DataType.Char;
			else if(typeName=="image")
				type=DataType.Image;
			else if(typeName.StartsWith("bit") )
				type=DataType.Bit;
			else if( typeName.StartsWith("binary") )
				type=DataType.Binary;
			else if(typeName.StartsWith("decimal") )
				type=DataType.Decimal;
			else if(typeName=="numeric")
				type=DataType.Numeric;
			else if(typeName=="money")
				type=DataType.Money;
			else
			{
#if DEBUG
				System.Diagnostics.Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, "Unknown datatype({0}).", typeName), "need to implement, no big deal if not our table.");
#endif
			}
			return type;
		}

		#region DbUser
		string _dbUser;
		public string GetDBUser( DataSource ds )
		{
			if( string.IsNullOrEmpty(_dbUser) )
			{
				var dataSet=ds.LoadDataSet( "select Database()", "db_user" );
				int cRow=dataSet.Tables["db_user"].Rows.Count;
				if(cRow==0)
					throw new InvalidOperationException( "lost user" );
				else
					_dbUser=(string)dataSet.Tables["db_user"].Rows[0][0];
			}
			return _dbUser;
		}
		#endregion
	}
}
