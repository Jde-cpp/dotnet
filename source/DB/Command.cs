using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace Jde.DB
{
	using Collections.Extensions;

	public abstract class Command : IDisposable
	{
		#region Constructors
		public Command()/*:
			this( null )*/
		{}
		
		public Command( CommitHandler commitHandler )
		{
			AddCommitHandler( commitHandler );
		}
		public Command( string tableName, Transaction t ):
			this( t )
		{
			TableName = tableName;
		}
		public Command( Transaction t )
		{
			Transaction = t;
		}
		public Command( Database database )
		{
		   Database = database;
		}
		#endregion
		#region IDisposable Members
		void IDisposable.Dispose()
		{
			if( _dbCommand!=null )
			{
				_dbCommand.Connection?.Dispose();
				_dbCommand.Dispose();
			}
		}
		#endregion
		#region Commit
		public delegate void CommitHandler();
		event CommitHandler _onCommit;
		public CommitHandler OnCommit
		{
			get{ return _onCommit; }
		}
		protected void AddCommitHandler( CommitHandler handler )
		{
			_onCommit+=handler;
		}
		public void Commit()
		{
			if( _onCommit!=null )
				_onCommit();
			Sql=null;
		}
		#endregion
/*		#region DataSource
		public static DataSource GetDefaultDataSource()
		{
			return GetDataSource( "MarketDatabaseConnection" );
		}
		
		public static DataSource GetDataSource( string appSetting )
		{
			var connection = System.Configuration.ConfigurationManager.AppSettings[appSetting];
			var marketConnection = System.Configuration.ConfigurationManager.ConnectionStrings[connection];
			if( marketConnection==null )
				throw new System.Configuration.ConfigurationErrorsException( string.Format(CultureInfo.InvariantCulture, "market connection string '{0}' not found.", connection) );
			return new DataSource( new Provider(marketConnection.ProviderName), marketConnection.ConnectionString );
		}
		#endregion*/
		#region Database
		public static Database DefaultDatabase{ get; set; }
		Database _database;
		public Database Database
		{
			get
			{ 
				Database db = _database==null ? DefaultDatabase : _database;
				if( db==null )
				{
					//db = _database = DefaultDatabase = GetDefaultDataSource().Database;
					//object eventHandler = db.GetInstrumentationEventProvider();
					//var events = eventHandler as Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.NewDataInstrumentationProvider;
					//if( events!=null )
					//{
						//events.commandExecuted += new EventHandler<Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.CommandExecutedEventArgs>( events_commandExecuted );
						//events.commandFailed +=new EventHandler<Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.CommandFailedEventArgs>( events_commandFailed );
						//events..connectionFailed += new EventHandler<Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.ConnectionFailedEventArgs>( events_connectionFailed );
						//events.connectionOpened += new EventHandler<EventArgs>(events_connectionOpened);
					//}
					//else
						//DBLogging.Critical( "Can't get event handler from database.  type='{0}'", eventHandler.GetType() );
				}
				return db;
			}
			set{ _database = value; }
		}
		#endregion
		#region DBCommand
		DbCommand _dbCommand;
		public DbCommand GetDBCommand()
		{
			return GetDBCommand( null ); 
		}
		public DbCommand GetDBCommand( Transaction t )
		{
			if( _dbCommand==null )
			{
				_dbCommand = CreateCommand();//Database.GetStoredProcCommand( Sql );
				_dbCommand.CommandTimeout = 0;
				foreach( DbParameter parameter in Parameters.Values )
					Database.AddParameter( _dbCommand, parameter.ParameterName, parameter.DbType, parameter.Direction, null, DataRowVersion.Default, parameter.Value );
				if( _dbCommand.Connection==null && t==null )
					_dbCommand.Connection = Database.CreateConnection();
			}

			return _dbCommand; 
		}
		protected DbCommand DBCommand 
		{
			//get
			//{ 
			//   if( _dbCommand==null )
			//      DBCommand = CreateCommand();//Database.GetStoredProcCommand( Sql );
			//   return _dbCommand; 
			//}
			set{ _dbCommand = value; }
		}
		protected abstract DbCommand CreateCommand();
		#endregion
		#region Parameters
		public Dictionary<string,DbParameter> Parameters{get;set;}= new Dictionary<string,DbParameter>();
		public virtual string AddParameter<T>( string name, T value )
		{
			string uniqueName = name;
			int i=0;
			while( Parameters.ContainsKey(uniqueName) )
				uniqueName = name+(++i).ToString(CultureInfo.InvariantCulture);

			var param = new Parameter<T>( Database.Syntax, uniqueName, value, IsStoredProc );
			Parameters.Add( uniqueName, param );
			return param.ParameterName;
		}

		string GetUniqueParamName( string name )
		{
			string uniqueName = name;
			bool unique = true;
			int index = 1;
			do
			{
				unique = true;
				foreach( var parameter in Parameters.Values )
				{
					if( string.Compare(parameter.ParameterName, uniqueName, StringComparison.OrdinalIgnoreCase)==0 )
					{
						uniqueName = name+'_'+index.ToString( CultureInfo.InvariantCulture );
						++index;
						unique = false;
					}
				}
			}while( !unique );

			return uniqueName;
		}

		public virtual string AddEnumParameter( string name, Enum value )
		{
			string paramName = null;
			if( value==null )
				paramName = AddParameter<DBNull>( name, DBNull.Value );
			else
			{
				switch( value.GetTypeCode() )
				{
				case TypeCode.Int32:
					paramName = AddParameter<int>( name, Convert.ToInt32(value) );
					break;
				case TypeCode.Int64:
					paramName = AddParameter<long>( name, Convert.ToInt64(value) );
					break;
				case TypeCode.UInt32:
					paramName = AddParameter<uint>( name, Convert.ToUInt32(value) );
					break;
				case TypeCode.UInt64:
					paramName = AddParameter<ulong>( name, Convert.ToUInt64(value) );
					break;
				default:
					throw new NotSupportedException( string.Format(CultureInfo.InvariantCulture, "enum type '{0}' is not supported.", value.GetTypeCode()) );
				}
			}
			return paramName;
		}

		public string AddOutParameter<T>( string name )
		{
			var param = new Parameter<T>( this.Database.Syntax, name, default(T), IsStoredProc ){ Direction= ParameterDirection.Output };
			Parameters.Add( name, param );
			if( OutParameter==null )
				OutParameter = param;
			return param.ParameterName;
		}

		public List<string> AddParameters<T>( string name, ICollection<T> items )
		{
			var parameters = new List<string>();
			int index = 0;
			var delimiterPosition = name.LastIndexOf( '.' );
			var paramPrefix = delimiterPosition>0 && delimiterPosition<name.Length-1 ? name.Substring( name.LastIndexOf('.')+1 ) : name;
			foreach( var item in items )
			{
				string itemName = index==0 ? paramPrefix : paramPrefix+index.ToString( CultureInfo.InvariantCulture );
				parameters.Add( AddParameter<T>(itemName, item) );
				++index;
			}

			return parameters;
		}
		#endregion
		#region DataType
		public delegate void SetNewIntIdHandler( int id );
		event SetNewIntIdHandler _setNewIntId;
		public SetNewIntIdHandler SetNewIntId
		{
			get{ return _setNewIntId; }
		}
		protected void AddSetNewIntIdHandler( SetNewIntIdHandler handler )
		{
			_setNewIntId+=handler;
		}

		public delegate void SetNewIdHandler( object id );
		event SetNewIdHandler _setNewId;
		public SetNewIdHandler SetNewId
		{
			get{ return _setNewId; }
		}
		protected void AddSetNewIdHandler( SetNewIdHandler handler )
		{
			_setNewId+=handler;
		}

		public void SetDataTypeId()
		{
			if( _dbCommand==null )
				return;
			if( _setNewId!=null || _setNewIntId!=null )
			{
				IDataParameter outParam = null;
				foreach( IDataParameter param in _dbCommand.Parameters )
				{
					if( param.Direction==ParameterDirection.Output )
					{
						outParam = param;
						break;
					}
				}
				if( outParam!=null )
				{
					OutParameter.Value = outParam.Value;
					if( _setNewId!=null )
						_setNewId( OutParameter.Value );
					else if( _setNewIntId!=null )
						_setNewIntId( Convert.ToInt32(OutParameter.Value) );
				}
			}
		}
		#endregion
		#region Where
		LinkedList<string> _where;
		public LinkedList<string> Where
		{
			get
			{ 
				if( _where==null )
					Where = new LinkedList<string>();
				return _where; 
			
			}
			private set{ _where = value; }
		}
		string _whereOperand = " and ";
		public string WhereOperand
		{
			get{ return _whereOperand; }
			set{ _whereOperand = value; }
		}
		LinkedList<string> SubWhereCommands{get;set;}
		string _subCommand;
		public void BeginOperand( string name )
		{
			if( SubWhereCommands==null )
				SubWhereCommands = new LinkedList<string>();
			else
				throw new NotSupportedException( "'or' in other 'or' clauses aren't supported" );
			_subCommand = name;
		}
		public void EndOperand()
		{
			var sql = new System.Text.StringBuilder( "(" );
			var startLength = sql.Length;
			foreach( var command in SubWhereCommands )
			{
				if( sql.Length>startLength )
					sql.Append( " "+_subCommand+" " );
				sql.Append( command );
			}
			if( sql.Length>startLength )
			{
				sql.Append( ")" );
				Where.AddLast( sql.ToString() );
			}
			SubWhereCommands=null;
		}

		public virtual void AddWhereNotNull( string column )
		{
			AddWhere( column+" is not null" );
		}

		public virtual void AddWhereNull( string column )
		{
			AddWhere( column+" is null" );
		}

		public virtual void AddWhere( string statement )
		{
			LinkedList<string> clause = SubWhereCommands==null ? Where : SubWhereCommands;
			clause.AddLast( statement );
		}

		public virtual string AddWhere<T>( string columnName, T value )
		{
			return AddWhere( columnName, value, "=" );
		}

		public virtual string AddWhereLess<T>( string columnName, T value )
		{
			return AddWhere( columnName, value, "<" );
		}

		public virtual string AddWhereGreater<T>( string columnName, T value )
		{
			return AddWhere( columnName, value, ">" );
		}
		public virtual string AddWhereGreaterOrEqual<T>( string columnName, T value )
		{
			return AddWhere( columnName, value, ">=" );
		}
		
		public virtual string AddWhereGreaterOrNull<T>( string columnName, T value )
		{
			return AddWhereOrNull( columnName, value, ">" );
		}

		public void AddWhereEnum( string columnName, Enum value )
		{
			if( value!=null )
			{
				var paramName = AddEnumParameter( columnName, value );
				AddWhereParam( columnName, paramName );
			}
			else
				AddWhereNull( columnName );
		}

		public void AddWhereEnum( string columnName, ICollection<Enum> values )
		{
			var numericValues = new LinkedList<long>();
			values.ForEach( value => numericValues.AddLast(Convert.ToInt64(value)) );
			AddWhereClauses( columnName, numericValues );
		}

		public virtual Tuple<string,string> AddWhereDayRange( string columnName, DateTime time )
		{
			DateTime start = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
			DateTime end = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59, 999);
			return AddWhereRange( columnName, start, end );
		}
		public virtual Tuple<string,string> AddWhereRange( string columnName, DateTime start, DateTime finish )
		{
			var startParam = AddParameter<DateTime>( columnName+"_start", start );
			var finishParam = AddParameter<DateTime>( columnName+"_finish", finish );
			var paramNames = new Tuple<string,string>( startParam, finishParam );

			LinkedList<string> clause = SubWhereCommands==null ? Where : SubWhereCommands;
			clause.AddLast( string.Format(CultureInfo.InvariantCulture, "{0} between {1} and {2}", columnName, paramNames.Item1, paramNames.Item2) );

			return paramNames;
		}

		public void AddWhereLike<T>( string columnName, T value )
		{
			AddWhere<T>( columnName, value, " like " );
		}

		protected virtual string AddWhere<T>( string columnName, T value, string oprtr )
		{
			string paramName = AddParameter<T>( columnName, value );
			AddWhereParam( columnName, paramName, oprtr );
			return paramName;
		}

		protected virtual string AddWhereOrNull<T>( string columnName, T value, string oprtr )
		{
			string paramName = AddParameter<T>( columnName, value );
			AddWhereOrNullParam( columnName, paramName, oprtr );
			return paramName;
		}

		public void AddWhereClauses<T>( string columnName, ICollection<T> values )
		{
			if( values.Count==0 )
				throw new ArgumentException( "values is empty" );
			if( values.Count>2000 )
			{
				var inClause = new System.Text.StringBuilder();
				foreach( var value in values )
				{
					if( inClause.Length>0 )
						inClause.Append( "," );
					var stringValue = value as string;
					if( stringValue!=null )
						inClause.Append( SqlSyntax.Format(stringValue) );
					else
					{
						int? intValue = value as int?;
						if( intValue!=null )
							inClause.Append( SqlSyntax.Format(intValue.Value) );
						else
							throw new NotImplementedException( "Add where clauses>2100 only implemented for ints & strings." );
					}
				}
				var clause = SubWhereCommands==null ? Where : SubWhereCommands;
				clause.AddLast( string.Format(CultureInfo.InvariantCulture, "{0} in ({1})", columnName, inClause) );
			}
			else
			{
				var parameters = AddParameters<T>( columnName, values );
				if( parameters.Count==1 )
					AddWhereParam( columnName, parameters[0] );
				else
				{
					System.Text.StringBuilder inClause = new System.Text.StringBuilder();
					parameters.ForEach( param => inClause.Append( param+",") );
					inClause.Remove( inClause.Length-1, 1 );
					var clause = SubWhereCommands==null ? Where : SubWhereCommands;
					clause.AddLast( string.Format(CultureInfo.InvariantCulture, "{0} in ({1})", columnName, inClause) );
				}
			}
		}

		public void AddWhereParam( string columnName, DbParameter param )
		{
			Parameters.Add( param.ParameterName, param );
			AddWhereParam( columnName, param.ParameterName );
		}

		protected void AddWhereParam( string columnName, string paramName )
		{
			AddWhereParam( columnName, paramName, "=" );
		}
		protected void AddWhereParam( string columnName, string paramName, string oprtr )
		{
			LinkedList<string> clause = SubWhereCommands==null ? Where : SubWhereCommands;
			clause.AddLast( string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", columnName, oprtr, paramName) );
		}
		protected void AddWhereOrNullParam( string columnName, string paramName, string oprtr )
		{
			LinkedList<string> clause = SubWhereCommands==null ? Where : SubWhereCommands;
			clause.AddLast( string.Format(CultureInfo.InvariantCulture, "({0}{1}{2} or {0} is null)", columnName, oprtr, paramName) );
		}
		#endregion
		#region WhereFlags
		public string AddWhereNotBits( string column, Enum bits )
		{
			return AddWhereBits( column, bits, true );
		}
		public string AddWhereBits( string columnName, Enum bits, bool not=false, bool or=false )
		{
			string operand = not ? "=" : "!=";
			string paramName = AddEnumParameter( columnName, bits );
			LinkedList<string> clause = SubWhereCommands==null ? Where : SubWhereCommands;
			var bitOperand = or ? "|" : "&";
			clause.AddLast( string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}{3}0", columnName, bitOperand, paramName, operand) );
			return paramName;
		}
		#endregion
		#region AddBetweenWhere
		public virtual Tuple<string,string> AddBetweenWhere<T>( string columnName, T from, T to )
		{
			var fromParam = AddParameter( "from_"+columnName, from );
			var toParam = AddParameter( "to_"+columnName, to );
			LinkedList<string> clause = SubWhereCommands==null ? Where : SubWhereCommands;
			clause.AddLast( string.Format(CultureInfo.InvariantCulture, "{0} between {1} and {2}", columnName, fromParam, toParam) );
			return new Tuple<string,string>( fromParam, toParam );
		}
		#endregion
		#region ExecuteCount
		public long? ExecuteCount()
		{
			long? count = 0;
			if( Transaction==null )
			{
				using( var connection = Database.CreateConnection() )
				{
					//DBLogging.CommandExecuting( this );
					connection.Open();
					var cmd = GetDBCommand();
					count = Database.ExecuteScalarCount( cmd ) as long?;
					//if( Activity!=null && count!=null )
					//	Activity[ActivityItemType].Count+=count.Value;
				}
			}
			else
				count = Transaction.ExecuteCount( this );

			return count;
		}
		#endregion

		#region ExecuteNonQuery
		public int ExecuteNonQuery()
		{
			int count = 0;
			if( Transaction==null )
			{
				using( var connection = Database.CreateConnection() )
				{
					//DBLogging.CommandExecuting( this );
					connection.Open();
					var cmd = GetDBCommand();
					count = Database.ExecuteNonQuery( cmd );
					//if( Activity!=null )
					//	Activity[ActivityItemType].Count+=count;
					SetDataTypeId();//need to set data type id before commiting so the item will have the new id.
					Commit();
				}
			}
			else
				count = Transaction.ExecuteNonQuery( this );

			return count;

		}
		#endregion
		#region Events
        //void events_connectionOpened( object sender, EventArgs e )
        //{
        //    DBLogging.ConnectionOpened();
        //}

        //void events_connectionFailed( object sender, Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.ConnectionFailedEventArgs e )
        //{
        //    DBLogging.ConnectionFailed( e );
        //}
		
        //void events_commandFailed( object sender, Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.CommandFailedEventArgs e )
        //{
        //    DBLogging.CommandFailed( e );
        //} 
        //void events_commandExecuted( object sender, Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.CommandExecutedEventArgs e )
        //{
        //    DBLogging.CommandExecuting( e );
        //}
		#endregion
		#region OutParameter
		IDataParameter _outParameter;
		public IDataParameter OutParameter
		{
			get{ return _outParameter; }
			set{ _outParameter = value; }
		}
		#endregion
		#region Sql
		string _sql;
		public string Sql
		{
			get{ return _sql; }
			set{ _sql=value; }
		}
		public abstract string CreateSql();
		#endregion
		#region TableName
		string _tableName;
		public string TableName
		{
			get{ return _tableName; }
			protected set{ _tableName = value; }
		}
		#endregion
		protected Transaction Transaction{ get; set; }
		public Logging.Activity Activity{get;set;}
		public virtual Logging.ActivityItemTypes ActivityItemType{ get{ return Jde.Logging.ActivityItemTypes.None;} }
		public virtual bool IsStoredProc{get;set;} = false;
	}
	#region Extensions
	namespace Extensions
	{
		public static class CommandExtensions
		{
			#region ExecuteNonQuery
			public static int ExecuteNonQuery( this ICollection<Command> commands )
			{
				int count = 0;
				if( commands.Count==1 )
				{
					foreach( var command in commands )
						count = command.ExecuteNonQuery();
				}
				else if( commands.Count>1 )
				{
					using( var t = new Transaction(commands.First().Database) )
					{
						count = commands.ExecuteNonQuery( t );
						t.Commit();
					}
				}
				return count;
			}
			
			public static int ExecuteNonQuery( this ICollection<Command> commands, Transaction t )
			{
				return t==null ? commands.ExecuteNonQuery() : t.ExecuteNonQuery( commands );
			}
			#endregion
			#region Commit
			public static void Commit( this ICollection<Command> commands )
			{
				foreach( var command in commands )
					command.Commit();
			}
			#endregion
			#region AddDelete
			//public static void AddDelete<T>( this ICollection<CommandEx> commands, string tableName, string columnName, T id )
			//{
			//   bool added = false;
			//   foreach( var command in commands )
			//   {
			//      if( command.TableName==tableName && command is Commands.DeleteCommand<T> )
			//      {
			//         var parameters = command.Parameters;
			//         commands.Remove( command );
			//         LinkedList<T> ids = new LinkedList<T>();
			//         foreach( var parameter in parameters )
			//            ids.AddLast( (T)parameter.Value );

			//         command.AddWhereClauses( columnName, ids );
			//         added = true;
			//         break;
			//      }
			//   }
			//   if( !added )
			//      commands.Add( new Commands.DeleteCommand<T>(tableName, columnName, id) );
			//}
			#endregion
		}
	}
	#endregion
}
