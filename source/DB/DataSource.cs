using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Jde.DB
{
	public class DataSource : IDataSource
	{
		#region Constructors
/*		public DataSource()
		{}
*/
/*		public DataSource( Provider provider, bool clustered, string factoryName, string instanceName, string serverName, string version )
		{
			Provider = provider;
			Clustered = clustered;
			FactoryName = factoryName;
			InstanceName = instanceName;
			ServerName = serverName;
			Version = version;
			Database = Provider.GetDefaultDatabase( ConnectionString );
		}
*/
		public DataSource( Provider provider, Microsoft.Extensions.Logging.ILogger<DataSource> log, Database database, SqlSyntax dialect )
		{
			Provider = provider;
			//ConnectionString = connectionString;
			Database = database;//Provider.GetDefaultDatabase( ConnectionString );
			Syntax = dialect;
			Logger = log;
		}

/*
		public DataSource( SqlType sqlType, string server, string user, string password, string initialCatalog ):
			this( sqlType )
		{
			ServerName = server;
			User = user;
			if( string.IsNullOrEmpty(user) )
				IntegratedSecurity="SSPI";
			Password = password;
			InitialCatalog = initialCatalog;
		}
*/		
/*		DataSource( SqlType sqlType, string connectionString )
		{
			Sql = sqlType;
			Provider = sqlType.GetProvider();
			ConnectionString = connectionString;
			Database = Provider.GetDefaultDatabase( ConnectionString );
		}
*/	
/*		public DataSource( SqlType sqlType, string connectionString ):
			this( sqlType )
		{
			ConnectionString = connectionString;
		}
*/
/*		public DataSource( SqlType sqlType, string connectionString, string database ):
			this( sqlType, connectionString )
		{
			InitialCatalog = database;
		}
*/
		#endregion
		#region BeginTransaction
/*		public DbTransaction BeginTransaction()
		{
			return Provider.BeginTransaction(this);
		}*/
		#endregion
		#region ConnectionInfo
		public string ConnectionInfo
		{
			get
			{ 
				return !string.IsNullOrEmpty(InitialCatalog) 
					? string.Format( CultureInfo.InvariantCulture, "{0}.{1}.{2}", ServerName, InitialCatalog, Syntax.GetDBUser(this) )
					: string.Format( CultureInfo.InvariantCulture, "{0}.{1}", ServerName, Syntax.GetDBUser(this) );
			}
		}
		#endregion
		#region CommandTimeout
		[XmlAttribute( "command_timeout" )]
		public int CommandTimeout{get;private set;}=0;
		#endregion
		#region Database
		Database _database;
		public Database Database
		{
			get{ return _database; }
			private set{ _database = value; }
		}
		#endregion
		#region DisplayString
		string _displayString = string.Empty;
		public string DisplayString
		{
			get{ return _displayString;}
			set{ _displayString = value;}
		}
		#endregion
		#region GetParameter
		public DbParameter GetParameter( string name, DataType dataType, bool unicode )
		{
		   return Provider.GetParameter( this, name, dataType, unicode );
		}
		public DbParameter Param( string name, DataType type )
		{
			return Provider.GetParameter( this, name, type, 0, false );
		}
		public DbParameter Param( string name, string value )
		{
		   return Provider.GetParameter( this, name, DataType.VarTChar, false, value );
		}
		public DbParameter Param( string name, DateTime? value )
		{
		   return Provider.GetParameter( this, name, DataType.DateTime, false, value==null ? DBNull.Value : (object)value );
		}
		public DbParameter Param( string name, DateTime value )
		{
		   return Provider.GetParameter( this, name, DataType.DateTime, false, value );
		}
		public DbParameter Param( string name, int? id )
		{
			//var dataType = id.HasValue() ? DataType.Int : DataTy 
			object value = id.HasValue ? (object)id.Value : DBNull.Value;
		   return GetParameter( name, DataType.Int, false, value );
		}
		public DbParameter Param( string name, uint? id )
		{
			//var dataType = id.HasValue() ? DataType.Int : DataTy 
			object value = id.HasValue ? (object)id.Value : DBNull.Value;
		   return GetParameter( name, DataType.UInt, false, value );
		}

		public DbParameter GetParameter( string name, DataType dataType, bool unicode, object value )
		{
		   return Provider.GetParameter( this, name, dataType, unicode, value );
		}
		public DbParameter GetParameter( string name, DataType dataType, int size, bool unicode )
		{
		   return Provider.GetParameter( this, name, dataType, size, unicode );
		}
		#endregion
		#region ToString
		public override string ToString()
		{
			return !string.IsNullOrEmpty(DisplayString)
				? DisplayString
				: ConnectionInfo;
		}
		#endregion
		#region TestConnection
		public void TestConnection()
		{
			Syntax.TestConnection(this);
		}
		#endregion
		#region Sql
		SqlSyntax _sql;
		public SqlSyntax Syntax
		{
			get{ return _sql;}
			set{ _sql = value;}
		}
		#endregion
		#region Equals
		public override bool Equals(object obj)
		{
			bool equal = ReferenceEquals(this, obj);
			if( !equal )
			{
				equal = obj.GetType()==GetType();
				if( equal )
				{
					DataSource ds = (DataSource)obj;
					equal =	ServerName==ds.ServerName
							&&	InitialCatalog==ds.InitialCatalog
							&& Syntax.GetDBUser(this)==ds.Syntax.GetDBUser(this);
				}
			}
			return equal;
		}
		#endregion
		#region GetHashCode
		public override int GetHashCode()
		{
			return ConnectionString.GetHashCode();
		}
		#endregion
		#region Execute
		public int Execute( string sql, List<DbParameter> parameters )
		{
			Logger.LogTrace( sql );
			return Database.Execute( sql, parameters );
		}
		public int StoredProc( string sql, List<DbParameter> parameters )
		{
			Logger.LogTrace( sql );
			return Database.Execute( sql, parameters, CommandType.StoredProcedure );
		}

		//public int Execute( ICollection<string> sqlStatements )
		//{
		//   LinkedList<DB.Command> commands = new LinkedList<Command>();
		//   foreach( string sql in sqlStatements )
		//      commands.AddLast( new Command(sql) );

		//   return Execute( commands );
		//}
		//public int Execute( DB.Command command )
		//{
		//   return Execute( command, null );
		//}
		//public int Execute( DB.Command command, DbTransaction transaction )
		//{
		//   return Provider.Execute( this, command, transaction );
		//}
		//public int Execute( ICollection<Command> commands )
		//{
		//   int result = int.MaxValue;
		//   using( Transaction t = new Transaction(this) )
		//   {
		//      result = t.Execute( commands );
		//      t.Commit();
		//   }
		//   return result;
		//}
		//public int Execute( ICollection<Command> commands, DbTransaction transaction )
		//{
		//   return Provider.Execute( this, commands, transaction );
		//}
		#endregion
		#region LoadCount
		public long LoadCount( string sql )
		{
			DataSet dataSet = LoadDataSet( sql );
			return Convert.ToInt64( dataSet.Tables[0].Rows[0][dataSet.Tables[0].Rows[0].ItemArray.Length-1], CultureInfo.InvariantCulture );
		}
		#endregion
		#region LoadDataSet
		public DataSet LoadDataSet( string sql )
		{
			return LoadDataSet( sql, "table" );
		}
		public DataSet LoadDataSet( string sql, string tableName )
		{
			return LoadDataSet( sql, tableName, null );
		}
		public DataSet LoadDataSet( string sql, DbTransaction transaction )
		{
			return LoadDataSet( sql, string.Empty, transaction );
		}
		public DataSet LoadDataSet( string sql, string tableName, DbTransaction transaction )
		{
			return Provider.LoadDataSet( this, sql, tableName, transaction );
		}
		#endregion
		#region LoadDataTable
		//public DataTable LoadDataTable( string sql, DbTransaction transaction=null )
		//{
		//	return LoadDataSet( sql, transaction ).Tables[0];
		//}
		#endregion
		#region ExecuteReader
		public IDataReader ExecuteReader( string sql )
		{
			Logger.LogTrace( sql );
			return Database.ExecuteReader( sql );
		}
		public IDataReader ExecuteReader( string sql, List<DbParameter> parameters )
		{
			Logger.LogTrace( sql );
			return Database.ExecuteReader( sql, parameters );
		}
		public IDataReader ExecuteReader( string sql, DbTransaction t )
		{
			Logger.LogTrace( sql );
			return Database.ExecuteReader( sql, CommandType.Text, t );
			//return t.ExecuteReader( t, CommandType.Text, sql );
		}
		public IDataReader ExecuteReader( Command command )
		{
			Logger.LogTrace( command.Sql );
			return Database.ExecuteReader( command.GetDBCommand() );
		}
		#endregion
		#region LoadVersion
		public string LoadVersion()
		{
			return Syntax.LoadVersion(this);
		}
		#endregion
		#region Catalog
		string _initialCatalog = string.Empty;
		public string InitialCatalog
		{ 
			get { return string.IsNullOrEmpty(_initialCatalog) ? GetItem( "Database", "Initial Catalog" ) : _initialCatalog; }
			set
			{ 
				if( string.IsNullOrEmpty(_connectionString) )
					_initialCatalog = value; 
				else
					SetItem( "Database", value );
			}
		}
		#endregion
		#region Clustered
		bool _clustered;
		public bool Clustered
		{
			get{ return _clustered;}
			set{_clustered=value;}
		}
		#endregion
		#region ComputeStats
		public void ComputeStats( string tableName )
		{
			Syntax.ComputeStats( this, tableName );
		}
		#endregion
		#region ConnectionString
		string _connectionString=string.Empty;
		public string ConnectionString
		{
			get
			{
				if( string.IsNullOrEmpty(_connectionString) )
				{
					StringBuilder connection=new StringBuilder();
					connection.AppendFormat( "data source={0}", _serverName );
					if( !string.IsNullOrEmpty(_integratedSecurity) )
						connection.Append( string.Format(CultureInfo.InvariantCulture, ";Integrated Security={0}", IntegratedSecurity) );
					else
						connection.AppendFormat( ";Password={0};User ID={1}", _password, _user );
					if( !string.IsNullOrEmpty(_initialCatalog) )
						connection.AppendFormat( ";Database={0};", InitialCatalog );
					_connectionString = connection.ToString();
				}
				return _connectionString;
			}
			set
			{
				_connectionString=value;
			}
		}
		#endregion
		#region DisplayName
		public string DisplayName
		{
			get{ return string.IsNullOrEmpty(InstanceName) ? ServerName : string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", ServerName, InstanceName); }
		}
		#endregion
		#region FactoryName
		string _factoryName = string.Empty;
		public string FactoryName
		{
			get{ return _factoryName;}
			set{ _factoryName = value;}
		}
		#endregion
		#region GetItem
		protected string GetItem( string item, params string[] otherItems )
		{
			String value = String.Empty;
			String[] parameters = ConnectionString.Split( ';' );
			for( int iParam = 0; string.IsNullOrEmpty(value) && iParam<parameters.Length; ++iParam )
			{
				String[] items = parameters[iParam].Split( '=' );
				if(items.Length!=2)
					continue;
				if( string.Compare(items[0], item, StringComparison.OrdinalIgnoreCase)==0 )
					value = items[1];
				if( otherItems!=null )
				{
					foreach( string otherItem in otherItems )
					{
						if( string.Compare(items[0], otherItem, StringComparison.OrdinalIgnoreCase)==0 )
						{
							value = items[1];
							break;
						}
					}
				}
			}
			return value;
		}
		#endregion
		#region InsertIdentity
		public int InsertIdentity( string sql, string tableName )
		{
			return Provider.InsertIdentity( this, sql, tableName );
		}
		#endregion
		#region InstanceName
		string _instanceName = string.Empty;
		public string InstanceName
		{
			get{ return _instanceName;}
			set{ _instanceName = value;}
		}
		#endregion
		#region IntegratedSecurity
		string _integratedSecurity=string.Empty;
		[XmlAttribute( "integrated_security" )]
		public string IntegratedSecurity
		{
			get { return string.IsNullOrEmpty(_integratedSecurity) ? GetItem( "Integrated Security" ) : _integratedSecurity; }
			set { _integratedSecurity=value; }
		}
		#endregion
		#region LoadTable
/*		public Schema.Table LoadTable( string tableName )
		{
			return Sql.LoadTable( this, InitialCatalog, tableName );
		}
*/ 
		#endregion
		#region Password
		string _password=string.Empty;
		[XmlAttribute( "password" )]
		public string Password
		{
			get { return string.IsNullOrEmpty(_password) ? GetItem( "Password" ) : _password; }
			set { _password=value; }
		}
		#endregion
		#region Provider
		Provider _provider;
		public Provider Provider
		{
			get{ return _provider; }
			set
			{ 
				if( value==null )
					throw new ArgumentNullException( "value" );
				_provider = value;
			}
		}
		#endregion
		#region ServerName
		string _serverName = string.Empty;
		public string ServerName
		{
			get
			{ 
				return string.IsNullOrEmpty(_serverName)
					? GetItem("Data Source")
					: _serverName;
			}
			set{ _serverName = value;}
		}
		#endregion
		#region SetItem
		void SetItem( string item, string value )
		{
			int start = _connectionString.IndexOf( item );
			if( start!=-1 )
				_connectionString = _connectionString.Remove( start, Math.Max(_connectionString.IndexOf(";", start), _connectionString.Length-start) );
			_connectionString = string.Format( "{0};{1}={2}", _connectionString, item, value );
		}
		#endregion
		#region User
		string _user=string.Empty;
		[XmlAttribute( "user" )]
		public string User
		{
			get { return string.IsNullOrEmpty(_user) ? GetItem( "User ID" ) : _user; ;}
			set { _user=value; }
		}
		#endregion 
		#region Version 
		string _version  = string.Empty;
		public string Version 
		{
			get{ return _version ;}
			set{ _version = value;}
		}
		#endregion
		ILogger Logger{get;set; }
	}
}
