using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
//using Microsoft.Practices.EnterpriseLibrary.Data;

using Jde.Facade;

namespace Jde.DB
{
	public class InsertCommand : Command
	{
		public InsertCommand( Command.CommitHandler commit, Command.SetNewIdHandler setNewId, string sql ):
			this( commit, sql )
		{
			AddSetNewIdHandler( setNewId );
		}

		public InsertCommand( Command.CommitHandler commit, Command.SetNewIntIdHandler setNewIntId, string sql, Logging.Activity activity ):
			this( commit, sql )
		{
			AddSetNewIntIdHandler( setNewIntId );
			Activity = activity;
		}

		InsertCommand( Command.CommitHandler commit, string sql ):
			base( commit )
		{
			Sql = sql;
		}

		public InsertCommand( string sql, Command.CommitHandler commit, Logging.Activity activity ):
			this( commit, sql )
		{
			Activity = activity;
		}

		public InsertCommand( string sql )
		{
			Sql = sql;
		}

		public string AddRefParameter( string columnName, IDataParameter parm )
		{
			var param = new Parameter<IDataParameter>( Database.Syntax, columnName, parm );
			Parameters.Add( columnName, param );
			return param.ParameterName;
		}
		protected override DbCommand CreateCommand()
		{
			return IsStoredProc ? Database.GetStoredProcCommand( Sql ) : Database.GetSqlStringCommand( Sql );
		}

		public override string CreateSql()
		{
			throw new InvalidOperationException( Properties.Resources.SqlNotSet );
		}

		public override string ToString()
		{
			var value = new StringBuilder( Sql );
			if( Parameters.Count>0 )
			{
				value.Append( "(" );
				int startLength = value.Length;
				foreach( var param in Parameters.Values )
				{
					if( value.Length!=startLength )
						value.Append( "," );
					value.Append( param.ToString() );
				}
				value.Append( ")" );
			}
			return value.ToString();
		}


		#region ActivityType
		public override Logging.ActivityItemTypes ActivityItemType{ get{ return Jde.Logging.ActivityItemTypes.Insert;} }
		#endregion
		public override bool IsStoredProc{get;set;} = true;
	}
}
