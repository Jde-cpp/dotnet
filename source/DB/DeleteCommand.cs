using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Jde.DB
{
	using Utilities.Extensions;

	public class DeleteCommand<TId> : Command
	{
		#region Constructors
		public DeleteCommand( string tableName, string columnName, TId id, Command.CommitHandler commit ):
			base( commit )
		{
			TableName = tableName;
			AddWhere<TId>( columnName, id );
		}
		public DeleteCommand( string tableName, string columnName, ICollection<TId> ids ):
			base( tableName, null )
		{
			AddWhereClauses<TId>( columnName, ids );
		}
		public DeleteCommand( string tableName ):
			base( tableName, null )
		{}
		#endregion
		#region ToString
		public override string ToString()
		{
			using( var os = new System.IO.StringWriter() )
			{
				foreach( var param in Parameters.Values )
					os.WriteLine( param.ToString() );

				var sql = Sql;
				if( string.IsNullOrEmpty(sql) )
					sql = Where.Count==0 ? "delete from "+TableName : CreateSql();
				os.Write( sql );
				return os.ToString();
			}
		}
		#endregion
		#region CreateSql
		public override string CreateSql()
		{
			StringBuilder sql = new StringBuilder( string.Format("delete from {0}", TableName) );
			
			if( Where.Count==0 )
				throw new InvalidOperationException( Properties.Resources.DeleteStatementUnconstrained );
			sql.Append( " "+SqlSyntax.Where+" " );
			sql.Append( Where.ToDelimitedList(" and ") );

			return sql.ToString();
		}
		#endregion
		#region CreateCommand
		protected override DbCommand CreateCommand()
		{
			return Database.GetSqlStringCommand( string.IsNullOrEmpty(Sql) ? CreateSql() : Sql );
		}
		#endregion
	}
}
