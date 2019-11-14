using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace Jde.DB
{
	public class Provider
	{
		#region Constructors
		public Provider( DbProviderFactory factory )
		{
			//DbProviderFactories.GetFactoryClasses();
			Factory = factory;
		}
/*		public Provider( string invariantName )
		{
			if( string.IsNullOrEmpty(invariantName) )
				throw new ArgumentNullException( "invariantName" );
			InvariantName = invariantName;
			Factory = DbProviderFactories.GetFactory( InvariantName );
		}

		protected Provider( DbProviderFactory factory, string name, string description, string invariantName, string assemblyQualifiedName )
		{
			if( factory==null )
				throw new ArgumentNullException( "factory" );
			Factory = factory;
			Name=name;
			Description=description;
			InvariantName = invariantName;
			AssemblyQualifiedName = assemblyQualifiedName;
		}
		*/
		#endregion
		#region Connect
		protected DbConnection Connect( string connectionString )
		{
			DbConnection connection = Factory.CreateConnection();
			connection.ConnectionString = connectionString;
			return connection;
		}
		#endregion
		#region GetParameter
		public DbParameter GetParameter( DataSource ds, string name, DataType type, bool unicode, object value )
		{
			DbParameter parameter = Factory.CreateParameter();
			var prefix = name[0]=='@' ? "" : ds.Syntax.ProcedureParameterPrefix;
			parameter.ParameterName = prefix+name;
			parameter.DbType = (DbType)DB.SqlSyntax.GetDBType( type, unicode );
			string stringValue = value as string;
			if( value==null || (stringValue!=null && stringValue.Length==0) )
				parameter.Value=DBNull.Value;
			else
				parameter.Value = value is TimeSpan ? ((TimeSpan)value).Ticks : value;
			parameter.Direction=ParameterDirection.Input;
			return parameter;
		}

		public DbParameter GetParameter( DataSource ds, string name, DataType type, bool unicode )
		{
			return GetParameter( ds, name, type, 0, unicode );
		}

		public DbParameter GetParameter( DataSource ds, string name, DataType type, int size, bool unicode )
		{
			DbParameter parameter = Factory.CreateParameter();
			parameter.ParameterName = ds.Syntax.ProcedureParameterPrefix+name;
			parameter.DbType = (DbType)DB.SqlSyntax.GetDBType( type, unicode );
			if( size!=0 )
				parameter.Size = size;
			parameter.Direction=ParameterDirection.Output;
			return parameter;
		}
		#endregion
		#region Execute
		//public int Execute( DataSource ds, Command command )
		//{
		//   return Execute( ds, command, null );
		//}

		//public int Execute( DataSource ds, Command command, DbTransaction transaction )
		//{
		//   int result = Execute( ds, command.Sql, transaction, command.Parameters );
		//   command.SetDataTypeId();
		//   return result;
		//}

		//public static int Execute( DataSource ds, ICollection<Command> commands )
		//{
		//   int result = int.MinValue;
		//   using( Transaction transaction = new Transaction(ds) )
		//   {
		//      result = transaction.Execute( commands );
		//      transaction.Commit();
		//      foreach( Command command in commands )
		//         command.Commit();
		//   }
		//   return result;
		//}
		
		//public int Execute( DataSource ds, ICollection<Command> commands, DbTransaction transaction )
		//{
		//   StringBuilder sql = new StringBuilder();
		//   int result = 0;
		//   foreach( Command command in commands )
		//   {
		//      if( command.Parameters.Count>0 )
		//      {
		//         if( sql.Length>0 )//run any updates before inserts
		//         {
		//            result = Execute( ds, sql.ToString(), transaction );
		//            sql.Length=0;
		//         }
		//         result = Execute( ds, command, transaction );
		//      }
		//      else if( !string.IsNullOrEmpty(command.Sql) )
		//         sql.Append(command.Sql+";");
		//   }
		//   if( sql.Length>0 )
		//      result = Execute( ds, sql.ToString(), transaction );
		//   return result;
		//}
		public int Execute( DataSource ds, string sql )
		{
			return Execute( ds, sql, (DbTransaction)null );
		}
		
		public int Execute( DataSource ds, string sql, DbTransaction transaction )
		{
			return Execute( ds, sql, transaction, null );
		}

		public int InsertIdentity( DataSource ds, string sql, string tableName )
		{
			return InsertIdentity( ds, sql, tableName, null );
		}

		public int InsertIdentity( DataSource ds, string sql, string tableName, DbTransaction transaction )
		{
			return ds.Syntax.SupportsIdentity
				? Execute( ds, string.Format(CultureInfo.InvariantCulture, "set identity_insert {0} on;{1};set identity_insert {0} off", tableName, sql), transaction )
				: Execute( ds, sql, transaction );
		}

		public int Execute( DataSource ds, string sql, SortedList<int, DbParameter> parameters )
		{
			return Execute( ds, sql, null, parameters );
		}

		public int Execute( DataSource ds, string sql, DbTransaction transaction, SortedList<int, DbParameter> parameters )
		{
			var connection = transaction==null ? Connect(ds.ConnectionString) : transaction.Connection;
			var command=connection.CreateCommand();
			command.CommandTimeout=ds.CommandTimeout;
			command.CommandText=sql;
			command.Transaction=transaction;
			if( parameters!=null )
			{
				if( parameters.Count>0 )
					command.CommandType=CommandType.StoredProcedure;
				foreach( IDataParameter param in parameters.Values )
					command.Parameters.Add(param);
			}
			try
			{
				if(connection.State!=ConnectionState.Open)
					connection.Open();
				//Logger.Write( "<"+ds.DisplayName+">"+sql, "sql" );
				return command.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				throw GetException( e, sql, parameters );
			}
			finally
			{
				if( transaction==null )
					connection.Close();
			}
		}
		#endregion
		#region AssemblyQualifiedName
		string _assemblyQualifiedName = string.Empty;
		public string AssemblyQualifiedName
		{
			get{ return _assemblyQualifiedName;}
			set{ _assemblyQualifiedName = value;}
		}
		#endregion
		#region Factory
		DbProviderFactory _factory;
		public DbProviderFactory Factory
		{
			get{ return _factory;}
			set{ _factory=value;}
		}
		#endregion
		#region Description
		string _description = string.Empty;
		public string Description
		{
			get{ return _description;}
			set{ _description = value;}
		}
		#endregion
		#region InvariantName
		string _invariantName = string.Empty;
		public string InvariantName
		{
			get{ return _invariantName;}
			set{ _invariantName = value;}
		}
		#endregion
		#region Name
		string _name = string.Empty;
		public string Name
		{
			get{ return _name;}
			set{ _name = value;}
		}
		#endregion
		#region GetException
		public static Exception GetException( string sql, Exception exception )
		{
			return GetException( exception, sql, null );//TODO:  add location(ToString()).
		}
		public static Exception GetException( Exception exception, string sql, SortedList<int, DbParameter> parameters )
		{
			return new DBException( sql, exception, parameters );//TODO:  add location(ToString()).
		}
		#endregion
		#region GetProviders
/*		public static Provider Odbc()
		{
			return new Provider( "System.Data.Odbc" );
		}
		public static Provider OleDB()
		{
			return new Provider( "System.Data.OleDb" );
		}
		public static Provider Oracle()
		{
			return new Provider( "System.Data.OracleClient" );
		}
		public static Provider Sql()
		{
			return new Provider( "System.Data.SqlClient" );
		}
/ *		public static LinkedList<Provider> Providers()
		{
			LinkedList<Provider> providers = new LinkedList<Provider>();
			DataTable providerTable = DbProviderFactories.GetFactoryClasses();
			foreach( DataRow row in providerTable.Rows )
			{
				//try
				{
					providers.AddLast( new DB.Provider( DbProviderFactories.GetFactory(row), (string)row["Name"], (string)row["Description"], (string)row["InvariantName"], (string)row["AssemblyQualifiedName"]) );
				}
				//catch( Exception e )
				{
					//TODO:  find the specific exception.
//					Logger.Write( string.Format(CulureInfo.InvariantCulture, "Could not create '{0}' provider.\n"+e, row["Name"]), "warning" );
				}
			}
			return providers;
		}
		*/
		#endregion
/*		#region GetDefaultDatabase
		public Database GetDefaultDatabase( string connectionString )
		{
			return /*InvariantName=="System.Data.OracleClient" ? (Database)new Oracle.OracleDatabase( connectionString ) :* / (Database)new Sql.SqlDatabase( connectionString );
		}
		#endregion*/
		#region LoadDataSet
		public DataSet LoadDataSet( DataSource ds, string sql, string tableName, DbTransaction transaction )
		{
			if( string.IsNullOrEmpty(sql) )
				throw new ArgumentException( "sql is null or empty" );
			DbConnection connection = transaction==null ? Connect(ds.ConnectionString) : transaction.Connection;
			try
			{
				if( string.IsNullOrEmpty(tableName) )
					tableName = "table";
				DbCommand command=connection.CreateCommand();
				command.Transaction = transaction;
				command.CommandText=sql;
				command.CommandTimeout = ds.CommandTimeout;
				//Logger.Write( sql, "sql" );
				return LoadDataSet( command, tableName );
			}
			catch(Exception e)
			{
				throw GetException( sql, e );
			}
			finally
			{
				if(transaction==null)
					connection.Close();
			}
		}
		protected DataSet LoadDataSet( DbCommand command, string tableName )
		{
			DataSet dataSet = new DataSet(){Locale = CultureInfo.CurrentUICulture };
			var adapter = Factory.CreateDataAdapter();
			adapter.SelectCommand = command;
			//			spAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
			//			spDataSet.EnforceConstraints=false;
			adapter.Fill( dataSet, tableName );
			return dataSet;
		}
		#endregion
		#region LoadSources
/*		LinkedList<DataSource> _dataSources;
		public LinkedList<DataSource> LoadDataSources()
		{
			LinkedList<DataSource> dataSources = new LinkedList<DataSource>();

			DbDataSourceEnumerator sources = Factory.CreateDataSourceEnumerator();
			if( sources!=null )
			{
				DataTable sourceTable = sources.GetDataSources();
				
				foreach( DataRow row in sourceTable.Rows )
				{
					bool clustered = sourceTable.Columns.Contains("IsClustered") && !Convert.IsDBNull(row["IsClustered"]) ? (string)row["IsClustered"]!="No" : false;
					string factoryName = sourceTable.Columns.Contains("FactoryName") ? (string)row["FactoryName"] : string.Empty;
					string instanceName = sourceTable.Columns.Contains("InstanceName") ? Convert.IsDBNull(row["InstanceName"]) ? string.Empty : (string)row["InstanceName"] : string.Empty;
					string serverName = sourceTable.Columns.Contains("ServerName") ? (string)row["ServerName"] : string.Empty;
					string version = sourceTable.Columns.Contains("Version") && !Convert.IsDBNull(row["Version"]) ? (string)row["Version"] : string.Empty;
					dataSources.AddLast( new DataSource(this, clustered, factoryName, instanceName, serverName, version) );
				}
			}
			return dataSources;
		}

		public LinkedList<DataSource> DataSources
		{
			get{ return _dataSources; }
//			private set{_dataSources = value;}
		}
*/
#if Unused
		public DataSource GetDataSource( string server, string user, string password, string initialCatalog )
		{
			DataSource foundDataSource = null;
			if( DataSources!=null )
			{
				foreach( DataSource ds in DataSources )
				{
					if( ds.DisplayName==server && ds.InitialCatalog==initialCatalog && ds.User==user && ds.Password==password )
					{
						foundDataSource = ds;
						break;
					}
				}
			}
			return foundDataSource==null ? new DataSource(this, server, user, password, initialCatalog) : foundDataSource;
		}

		public DataSource GetDataSource( string server, string initialCatalog )
		{
			return GetDataSource( server, string.Empty, string.Empty, initialCatalog );
		}

		public DataSource GetDataSource( string connectionString )
		{
			DataSource foundDataSource = null;
			if( DataSources!=null )
			{
				foreach( DataSource ds in DataSources )
				{
					if( ds.ConnectionString==connectionString )
					{
						foundDataSource = ds;
						break;
					}
				}
			}
			return foundDataSource==null ? new DataSource(this, connectionString) : foundDataSource;
		}
#endif
		#endregion
	}
}
