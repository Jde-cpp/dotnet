using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.OleDb;
//using System.Data.OracleClient;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Jde.DB
{
	[Serializable]
	public class DBException : System.Data.Common.DbException
	{
		public DBException()
		{}

		public DBException( string error ):
			base( error )
		{}

/*		public DBException( System.Data.SqlClient.SqlException e ):
			base( e.Message, e.ErrorCode )
		{
			base.Source = e.Source;
			base.HelpLink = e.HelpLink;
		}
*/
		public DBException( string sql, Exception inner ): 
			this( sql, inner, null )
		{}

		public DBException( string sql, Exception inner, SortedList<int, DbParameter> parameters  ): 
			base( "Database error.", inner )
		{
			if( parameters!=null && parameters.Count>0 )
			{
				StringBuilder proc = new StringBuilder( sql+"(");
				bool first = true;
				foreach( DbParameter param in parameters.Values )
				{
					if( first )
						first = false;
					else
						proc.Append( ", ");
					proc.AppendFormat( "{0}={1}", param.ParameterName, param.Value );
				}
				Sql = proc.ToString();
			}
			 else
				Sql=sql;
			//Logger.Write( this.ToString(), "error" );
		}

		protected DBException( SerializationInfo info, StreamingContext context ):
			base( info, context )
		{
			Sql = info.GetString( "sql" );
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			base.GetObjectData(info, context);
			info.AddValue( "sql", Sql );
		}

		/// <summary></summary>
		public override string Message
		{
			get
			{
				StringBuilder text=new StringBuilder( InnerException==null ? 0 : InnerException.ToString().Length );
				text.AppendFormat( CultureInfo.InvariantCulture, "message:\r\n\"{0}\"\r\n", base.Message );
				if( !string.IsNullOrEmpty(Sql) )
					text.AppendFormat( "sql:\r\n\"{0}\"\r\n", Sql );
	//			text.AppendFormat( "type:\t{0}\r\n", Type );
				if( InnerException!=null )
				{
#if Unused
					if( InnerException.GetType()==typeof(OracleException) )
					{
						OracleException exp = (OracleException)InnerException;
						text.AppendFormat( "Oracle Error({0}): {1}\n", exp, exp.Message );
					}
					else 
#endif
/*					if( InnerException.GetType()==typeof(OdbcException) )
					{
						OdbcException exp = (OdbcException)InnerException;
						text.AppendFormat( "Odbc Error({0}):  {1} ", exp.Source, exp.Message );
						foreach( OdbcError error in exp.Errors )
							text.AppendFormat( CultureInfo.InvariantCulture, "\n\tError({0}):  {1}  Source:  {2} State:  {3}\n", error.NativeError, error.Message, error.Source, error.SQLState );
					}
					else if( InnerException.GetType()==typeof(OleDbError) )
					{
						OleDbException exp = (OleDbException)InnerException;
						text.AppendFormat( "OleDb Exception({0}):  {1}", exp.ErrorCode, exp.Message );
						foreach( OleDbError error in exp.Errors )
							text.AppendFormat( CultureInfo.InvariantCulture, "\n\tOleDb Error({0}):  {1}  Source:  {2}  State:  {3}", error.NativeError, error.Message, error.Source, error.SQLState );
					}
					else if( InnerException.GetType()==typeof(SqlException) )
					{
						SqlException exp = (SqlException)InnerException;
						text.AppendFormat( "SQL Exception Number({0}):  {1}\n", exp.Number, exp.Message );
						foreach( System.Data.SqlClient.SqlError error in exp.Errors )
						{
							text.AppendFormat( CultureInfo.InvariantCulture, "\tMessage({0}):  {1}\n\t\tServer:  {2}\n\t\tSource:  {3}\n\t\tState:  {4}\n\t\tSeverity:  {5}\n\t\tLineNumber:  {6}\n\t\tProcedure:  {7}\n",
								error.Number, error.Message,  error.Server, error.Source, error.State, error.Class, error.LineNumber, error.Procedure );
						}
					}
	/*					else
						text.AppendFormat("Inner Exception({0}):  {1}\n", InnerException.GetType(), InnerException.Message );
	*/				}
				return text.ToString();
			}
		}


		string _sql=string.Empty;
		/// <summary></summary>
		public string Sql
		{ 
			get{ return _sql; } 
			private set{ _sql = value; }
		}

	}
}
