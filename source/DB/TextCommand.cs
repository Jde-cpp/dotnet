using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jde.DB
{
	using Utilities.Extensions;

	public class TextCommand : Command
	{
		#region Constructors
		public TextCommand()
		{}

		public TextCommand( string tableName, Transaction t=null ):
			base( tableName, t )
		{}

		public TextCommand( Transaction t ):
			base( t )
		{}

		public TextCommand( Database database ):
		   base( database )
		{}

		#endregion
		#region ToString
		public override string ToString()
		{
			using( var os = new System.IO.StringWriter() )
			{
				foreach( var param in Parameters.Values )
				{
					//os.WriteLine( string.Format(CultureInfo.InvariantCulture, "{0}={1};", param.ParameterName, param.ToString()) );
					os.WriteLine( param.ToString() );
				}

				os.Write( string.IsNullOrEmpty(Sql) ? CreateSql() : Sql );
				return os.ToString();
			}
		}
		#endregion
		#region CreateCommand
		protected override DbCommand CreateCommand()
		{
			return Database.GetSqlStringCommand( string.IsNullOrEmpty(Sql) ? CreateSql() : Sql );
		}
		#endregion
		#region CreateSql
		public override string CreateSql()
		{
			return FromClauses==null || FromClauses.Count==0 
				? SqlSyntax.GetSelectAll( TableName, Where, WhereOperand )
				: SqlSyntax.GetSelectAll( SelectTable, Columns, FromClauses, Where, WhereOperand );
		}
		#endregion
		#region ExecuteReader
		public System.Data.IDataReader ExecuteReader()
		{
			//DBLogging.CommandExecuting( this );
			return Transaction==null ? Database.ExecuteReader( GetDBCommand() ) : Transaction.ExecuteReader( GetDBCommand(Transaction) );
		}
		#endregion
		#region LoadCount
		public long LoadCount()
		{
			StringBuilder sql = new StringBuilder( string.Format(CultureInfo.InvariantCulture, "select count(*) from {0}", TableName) );
			if( Where.Count>0 )
				sql.AppendFormat( CultureInfo.InvariantCulture, " {0} {1}", SqlSyntax.Where, Where.ToDelimitedList(" "+WhereOperand+" ") );
			
			Sql = sql.ToString();

			return Convert.ToInt64( Database.ExecuteScalarCount(GetDBCommand()) );
		}
		#endregion
		public ICollection<FromClause> FromClauses{get;set;}
		public string SelectTable{get;set;}
		#region Columns
		public ICollection<string> Columns{get;set;}
		public void AddColumn( string columnName )
		{
			if( Columns==null )
				Columns = new LinkedList<string>();
			Columns.Add( columnName );
		}
		#endregion
	}
}
