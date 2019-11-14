using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
//using Microsoft.Practices.EnterpriseLibrary.Data;

using Fac=Jde.Facade;

namespace Jde.DB
{
	using Utilities.Extensions;

	public class UpdateCommand : Command
	{
		#region Constructors
		public UpdateCommand( Command.CommitHandler commitHandler, string tableName ):
			base( commitHandler )
		{
			TableName = tableName;
		}

		public UpdateCommand( Command.CommitHandler commitHandler, string tableName, Logging.Activity activity ):
			this( commitHandler, tableName )
		{
			Activity = activity;
		}

		public UpdateCommand( string sql )
		{
			Sql = sql;
		}
		#endregion
		#region ToString()
		public override string ToString()
		{
			using( var os = new System.IO.StringWriter(CultureInfo.InvariantCulture) )
			{
				foreach( var param in Parameters )
					os.WriteLine( string.Format(CultureInfo.InvariantCulture, "{0}={1}", param.Key, param.Value.Value) );
				os.WriteLine( CreateSql() );
				return os.ToString();
			}
		}
		#endregion
		#region AddAssignment
		public string AddAssignment<T>( string columnName, T value )
		{
			string paramName = AddParameter<T>( columnName, value );
			AddAssignmentParam( columnName, paramName );
			
			return paramName;
		}

		public string AddAssignment<T>( string columnName, T value, T existingValue )
		{
			string assignmentParam = AddAssignment<T>( columnName, value );
			string existingParam = AddParameter<T>( columnName+WhereParamSuffix, existingValue );
			
			AddWhere( columnName, assignmentParam, existingParam );
			return assignmentParam;
		}

		public string AddEnumAssignment( string columnName, Enum value )
		{
			string paramName = AddEnumParameter( columnName, value );
			AddAssignmentParam( columnName, paramName );
			return paramName;
		}

		public void AddEnumAssignment( string columnName, Enum value, Enum existingValue )
		{
			string assignmentParam = AddEnumAssignment( columnName, value );
			string existingParam = AddEnumParameter( columnName+WhereParamSuffix, existingValue );
			
			AddWhere( columnName, assignmentParam, existingParam );
		}
		#endregion
		#region CreateSql
		public override string CreateSql()
		{
			StringBuilder update = new StringBuilder( string.Format("update {0} set ", TableName) );
			
			if( Assignments.Count==0 )
				throw new InvalidOperationException( Properties.Resources.NoUpdateParamsToProcess );
			update.Append( Assignments.ToDelimitedList( ", ") );

			if( Where.Count==0 )
				throw new InvalidOperationException( Properties.Resources.UpdateStatementUnconstrained );
			update.Append( " "+SqlSyntax.Where+" " );
			update.Append( Where.ToDelimitedList(" and ") );

			return update.ToString();
		}
		#endregion
		#region Assignments
		LinkedList<string> _assignments = new LinkedList<string>();
		public LinkedList<string> Assignments
		{
			get{ return _assignments; }
			//private set{ _assignments = value; }
		}
		void AddAssignmentParam( string columnName, string paramName )
		{
			Assignments.AddLast( string.Format(CultureInfo.InvariantCulture, "{0}={1}", columnName, paramName) );
		}
		#endregion
		#region DBCommand
		protected override DbCommand CreateCommand()
		{
			return Database.GetSqlStringCommand( string.IsNullOrEmpty(Sql) ? CreateSql() : Sql );
		}
		#endregion
		#region Where
		void AddWhere( string columnName, string newParam, string existingParam )
		{
			Where.AddLast( string.Format(CultureInfo.InvariantCulture, "({0}={1} or {0}={2})", columnName, newParam, existingParam) );
		}
		#endregion
		#region WhereParamSuffix 
		const string WhereParamSuffix = "_existing";
		#endregion

		#region ActivityType
		public override Logging.ActivityItemTypes ActivityItemType{ get{ return Jde.Logging.ActivityItemTypes.Update;} }
		#endregion
	}
}
