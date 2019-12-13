using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Jde.DB.Dialects
{
	public class MySqlDatabase : Database
	{
		public MySqlDatabase(string connectionString):
			base(connectionString)
		{}


		public override IDataReader ExecuteReader( DbCommand dbCommand )
		{
			if( dbCommand.Connection.State!=ConnectionState.Open )
				dbCommand.Connection.Open();
			return dbCommand.ExecuteReader();
		}

		public override IDataReader ExecuteReader( string sql, CommandType commandType=CommandType.Text, DbTransaction t=null  )
		{
			var command = new MySqlCommand( sql, CreateMySqlConnection() ){ CommandType=commandType, Transaction=t as MySqlTransaction };
			return ExecuteReader( command );
		}
		public override IDataReader ExecuteReader( string sql, List<DbParameter> parameters )
		{
			var command = new MySqlCommand( sql, CreateMySqlConnection() );
			foreach( var param in parameters )
			{
				//var mysql = new MySqlParameter( param.ParameterName, param.DbType, param.Size, param.ParameterDirection, param.IsNullable, param.Precision, param.Scale, param.SourceColumn, param.SourceVersion, param.Value );
				command.Parameters.Add( (MySqlParameter)param );
			}
			return ExecuteReader( command );
		}


		public override DbCommand GetStoredProcCommand( string sql )
		{
			return new MySqlCommand( sql, CreateMySqlConnection() ){CommandType = CommandType.StoredProcedure };
		}

/*		public override DbCommand GetSqlStringCommand( string sql )
		{
			return new MySqlCommand( sql, CreateMySqlConnection() ){CommandType = CommandType.Text };
		}
*/
		public override DbConnection CreateConnection()=>CreateMySqlConnection();
		MySqlConnection CreateMySqlConnection()=>new MySqlConnection(ConnectionString);

		public override long? ExecuteScalarCount( DbCommand dbCommand, DbTransaction t=null )
		{
			dbCommand.Transaction = t;
			if( dbCommand.Connection.State!=ConnectionState.Open )
				dbCommand.Connection.Open();
			var value = dbCommand.ExecuteScalar();
			return value as long?;
		}

		public override int ExecuteNonQuery( DbCommand dbCommand, DbTransaction t=null )
		{
			
			if( t!=null )
			{
				dbCommand.Transaction = t;
				dbCommand.Connection = t.Connection;
			}
			if( dbCommand.Connection.State!= ConnectionState.Open )
				dbCommand.Connection.Open();
			return dbCommand.ExecuteNonQuery();
		}
		public override string BuildParameterName( string name )
		{
			return name;
		}

		public override void AddParameter( DbCommand dbCommand, string parameterName, DbType dbType, ParameterDirection direction, object _, DataRowVersion dataRowVersion, object value )
		{
			dbCommand.Parameters.Add( new MySqlParameter(parameterName, ToMySqlDbType(dbType)){ Direction=direction, SourceVersion=dataRowVersion, Value=value } );
		}
		public MySqlDbType ToMySqlDbType( DbType dbType )
		{
			switch( dbType )
			{
			case DbType.AnsiString:
				return MySqlDbType.VarChar;
			case DbType.AnsiStringFixedLength:
			case DbType.SByte:
			case DbType.Byte:
				return MySqlDbType.Binary;
			case DbType.Binary:
				return MySqlDbType.VarBinary;
			case DbType.Boolean:
				return MySqlDbType.Bit;
			case DbType.Currency:
			case DbType.Decimal:
				return MySqlDbType.Decimal;
			case DbType.Date:
				return MySqlDbType.Date;
			case DbType.DateTime:
			case DbType.DateTime2:
				return MySqlDbType.DateTime;
			case DbType.DateTimeOffset:
				return MySqlDbType.Int64;
			case DbType.Double:
				return MySqlDbType.Double;
			case DbType.Guid:
				return MySqlDbType.Guid;
			case DbType.Int16:
				return MySqlDbType.Int16;
			case DbType.Int32:
				return MySqlDbType.Int32;
			case DbType.Int64:
				return MySqlDbType.Int64;
			case DbType.Object:
				return MySqlDbType.Blob;
			case DbType.Single:
				return MySqlDbType.Float;
			case DbType.String:
			case DbType.StringFixedLength:
				return MySqlDbType.String;
			case DbType.Time:
				return MySqlDbType.Time;
			case DbType.UInt16:
				return MySqlDbType.Int16;
			case DbType.UInt32:
				return MySqlDbType.Int32;
			case DbType.UInt64:
				return MySqlDbType.UInt64;
			case DbType.VarNumeric:
				return MySqlDbType.Decimal;
			case DbType.Xml:
				return MySqlDbType.Text;
			}
			throw new Exceptions.ItemNotFoundException( dbType.ToString() );
		}
		public override SqlSyntax Syntax=>new Jde.DB.Dialects.MySqlSnytax();
		public override DbProviderFactory ProviderFactory => new MySqlDbProviderFactory();
		//MySqlConnection _dbConnection;
		//protected MySqlConnection MySqlConnection{get{return new MySqlConnection(ConnectionString);} }
		//protected override DbConnection DbConnection{get{return MySqlConnection;} }
	}
}
