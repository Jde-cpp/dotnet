using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;

namespace Jde.DB
{
	public abstract class Database
	{
		public Database(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public int Execute( string sql, List<DbParameter> parameters, CommandType commandType=CommandType.Text )
		{
			var connection = CreateConnection();
			var command = connection.CreateCommand();
			command.CommandTimeout = CommandTimeout;
			command.CommandText = sql;
			command.CommandType = commandType;
			foreach( var param in parameters )
				command.Parameters.Add( param );
			try
			{
				if( connection.State!=ConnectionState.Open )
					connection.Open();
				//Logger.Write( "<"+ds.DisplayName+">"+sql, "sql" );
				return command.ExecuteNonQuery();
			}
			catch( Exception e )
			{
				throw Provider.GetException( sql, e );
			}
		}

		public int Execute( string sql, SortedList<int, DbParameter> parameters )
		{
			return Execute( sql, null, parameters );
		}

		public int Execute( string sql, DbTransaction transaction=null, SortedList<int, DbParameter> parameters =null )
		{
			var connection = transaction==null ? CreateConnection() : transaction.Connection;
			var command=connection.CreateCommand();
			command.CommandTimeout=CommandTimeout;
			command.CommandText=sql;
			command.Transaction=transaction;
			if( parameters!=null )
			{
				if( parameters.Count>0 )
					command.CommandType=CommandType.StoredProcedure;
				foreach( var param in parameters.Values )
					command.Parameters.Add( param );
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
				throw Provider.GetException( e, sql, parameters );
			}
			finally
			{
				if( transaction==null )
					connection.Close();
			}
		}


		public abstract IDataReader ExecuteReader( string sql, CommandType commandType=CommandType.Text, DbTransaction t=null );
		public abstract IDataReader ExecuteReader( string sql, List<DbParameter> parameters );
		public abstract IDataReader ExecuteReader( DbCommand dbCommand );
		public abstract DbCommand GetStoredProcCommand( string sql );
		public virtual DbCommand GetSqlStringCommand( string commandText )
		{
			var command = ProviderFactory.CreateCommand();
			command.CommandText = commandText;
			command.CommandType = CommandType.Text;
			//command.Connection = CreateConnection();
			//command.Connection.ConnectionString = ConnectionString;
			return command;
		}
		public abstract DbConnection CreateConnection();
		//public abstract DbConnection CreateConnection();
		public abstract long? ExecuteScalarCount( DbCommand dbCommand, DbTransaction dbTransaction=null );
		public abstract int ExecuteNonQuery( DbCommand dbCommand, DbTransaction dbTransaction=null );
		public abstract string BuildParameterName( string name );
		public abstract void AddParameter( DbCommand dbCommand, string parameterName, DbType dbType, ParameterDirection direction, object _, DataRowVersion dataRowVersion, object value );
		protected string ConnectionString{get;set;}
		public int CommandTimeout{get;set;}
		public abstract SqlSyntax Syntax{get;}
		public abstract DbProviderFactory ProviderFactory{get;}
		#region Transactons
		public DbTransaction BeginTransaction()
		{
			var connection = CreateConnection();
			if( connection.State == ConnectionState.Closed )
				connection.Open();
			return connection.BeginTransaction();
		}
		#endregion
		public DataSet LoadDataSet( string sql, DbTransaction transaction=null )
		{
			if( string.IsNullOrEmpty(sql) )
				throw new ArgumentException( "sql is null or empty" );
			var connection = transaction==null ? CreateConnection() : transaction.Connection;
			try
			{
				//if( string.IsNullOrEmpty(tableName) )
				//	tableName = "table";
				var command=connection.CreateCommand();
				command.Transaction = transaction;
				command.CommandText=sql;
				command.CommandTimeout=CommandTimeout;
				//Logger.Write( sql, "sql" );
				return LoadDataSet( command );
			}
			catch(Exception e)
			{
				throw Provider.GetException( sql, e );
			}
			finally
			{
				if(transaction==null)
					connection.Close();
			}
		}
		protected DataSet LoadDataSet( DbCommand command, string tableName="table" )
		{
			var dataSet = new DataSet(){Locale = System.Globalization.CultureInfo.CurrentUICulture };
			var adapter = ProviderFactory.CreateDataAdapter();
			adapter.SelectCommand = command;//			spAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey; spDataSet.EnforceConstraints=false;
			adapter.Fill( dataSet, tableName );
			return dataSet;
		}


		public DataTable LoadDataTable( string sql, DbTransaction transaction=null )=>LoadDataSet( sql, transaction ).Tables[0];


	}
}