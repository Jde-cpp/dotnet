using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jde.DB
{
	using Collections.Extensions;
	using Utilities.Extensions;

	public abstract class SqlSyntax
	{
		public abstract void ComputeStats( DataSource ds, string tableName );
		public abstract string LoadProcText( Database ds, string procName );
		public abstract void LoadSize( DataSource ds, string tableName, out int rowCount, out int tableSize, out int indexSize );
		public abstract void TestConnection( DataSource ds );
		public abstract string LoadVersion( DataSource ds );
		public abstract string GetDatabaseNameQuery();
		public abstract string IdentitySyntax( int start, int increment );
		public abstract Provider GetProvider();

		public virtual string DropIndexStatement( bool primaryKey, string parentTableName, string name )
		{
			string sql = string.Empty;
			if( primaryKey )
				sql = string.Format( CultureInfo.InvariantCulture, "alter table {0} drop constraint {1}", parentTableName, name );
			else if( SpecifyTableOnIndexDrop )
				sql = string.Format( CultureInfo.InvariantCulture, "drop index {0}.{1}", parentTableName, name );
			else
				sql = string.Format( CultureInfo.InvariantCulture, "drop index {0}", name );
			return sql;
		}
		public virtual string DropForeignKeyStatement( string tableName, string name )=>$"alter table \"{tableName}\" drop constraint \"{name}\"";
		#region AddColumnStatement
/*		public virtual string AddColumnStatement( Schema.Column column, bool unicode )
		{
			StringBuilder statement = new StringBuilder( string.Format(CultureInfo.InvariantCulture, "alter table {0} add {1} {2}", column.ParentTable.Name, column.Name, GetTypeString(column.DataType, unicode)) );
			if( Schema.DataTypes.HasLength(column.DataType) )
				statement.AppendFormat("({0})", column.MaxLength);

			statement.Append( column.Nullable ? " null" : " not null" );

			return statement.ToString();
		}
*/ 
		#endregion
		#region AlterColumnPrefix
		public virtual string AlterColumnPrefix
		{
			get { return "alter column"; }
		}
		#endregion
		#region FormatMaxDate
		public static string FormatMaxDate( string columnName )
		{
			return string.Format( "max({0})", columnName );
		}
		#endregion 
		#region FormatWhere

		public static string FormatWhere( string column, int value )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, value );
		}

		public static string FormatWhere( string column, Enum value )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, value.ToString("d") );
		}

		public static string FormatWhere( string column, long value )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, value.ToString(CultureInfo.InvariantCulture) );
		}

		public static string FormatWhere( string column, int? value )
		{
			return value==null
				? string.Format( CultureInfo.InvariantCulture, "{0} is null", column )
				: FormatWhere( column, (int)value );
		}

		public static string FormatWhere( string column, decimal value )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, value.ToString(CultureInfo.InvariantCulture) );
		}

		public static string FormatWhere( string column, double value )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, value.ToString(CultureInfo.InvariantCulture) );
		}

		public static string FormatWhere( string column, double? value )
		{
			return value==null
				? string.Format( CultureInfo.InvariantCulture, "{0} is null", column )
				: FormatWhere( column, (double)value );
		}

		public string FormatWhere( string column, DateTime? value )
		{
			return value==null
				? string.Format( CultureInfo.InvariantCulture, "{0} is null", column )
				: string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, Format(value) );
		}

		public string FormatWhereNot( string column, DateTime? value )
		{
			return value==null
				? string.Format( CultureInfo.InvariantCulture, "{0} is not null", column )
				: string.Format( CultureInfo.InvariantCulture, "{0} != {1}", column, Format(value) );
		}

		public string FormatWhereGreater( string column, DateTime value )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} > {1}", column, Format(value) );
		}

		public static string FormatWhere( string column, string value )
		{
			return string.IsNullOrEmpty(value)
				? string.Format( CultureInfo.InvariantCulture, "{0} is null", column )
				: string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, Format(value) );

		}

		public string FormatWhere( string column, Guid? value )
		{
			return value==null
				? string.Format( CultureInfo.InvariantCulture, "{0} is null", column )
				: string.Format( CultureInfo.InvariantCulture, "{0} = {1}", column, Format(value) );
		}

		public static string FormatWhere( string column, ICollection<int> values )
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat( "{0} in (", column );
			bool first=true;
			foreach( int i in values )
			{
				if( first )
					first=false;
				else
					sql.Append( "," );
				sql.Append( Format( i ) );
			}
			sql.Append( ")" );
			return sql.ToString();
		}

		public static string FormatWhere( string column, ICollection<long> values )
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat( "{0} in (", column );
			bool first=true;
			foreach( long i in values )
			{
				if( first )
					first=false;
				else
					sql.Append( "," );
				sql.Append( Format( i ) );
			}
			sql.Append( ")" );
			return sql.ToString();
		}

		public static string FormatWhere( string column, IEnumerable<string> strings )
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat( "{0} in (", column );
			bool first=true;
			foreach( string item in strings )
			{
				if( first )
					first=false;
				else
					sql.Append( "," );
				sql.Append( Format(item) );
			}
			sql.Append( ")" );
			return sql.ToString();
		}

		public string FormatWhere( string column, ICollection<DateTime> dates )
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat( "{0} in (", column );
			bool first=true;
			foreach( DateTime item in dates )
			{
				if( first )
					first=false;
				else
					sql.Append( "," );
				sql.Append( Format(item) );
			}
			sql.Append( ")" );
			return sql.ToString();
		}
		#endregion
		#region FormatWhereIn
		public static string FormatWherein( string column, string instatement )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} in ({1})", column, instatement );
		}
		#endregion
		#region FormatWhereFlags
		public static string FormatWhereBitsNotContain( string column, Enum bits )
		{
			return FormatWhereBits( column, bits, true );
		}
		public static string FormatWhereBitsContain( string column, Enum bits )
		{
			return FormatWhereBits( column, bits, false );
		}
		public static string FormatWhereBits( string column, Enum bits, bool not )
		{
			string operand = not ? "=" : "!=";
			return string.Format( CultureInfo.InvariantCulture, "{0} & {1}{2}0", column, Convert.ToInt32(bits, CultureInfo.InvariantCulture), operand );
		}
		#endregion
		#region Format
		static string FormatAssignment( string item, bool commaPrefix )
		{
			return commaPrefix ? ","+item : item;
		}
		public static string FormatAssignment( string column, string item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, int item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, int? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		
		public static string FormatAssignment( string column, uint item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}

		public static string FormatAssignment( string column, uint? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, long item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, long? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}

		public static string FormatAssignment( string column, ulong item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}

		public static string FormatAssignment( string column, ulong? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, decimal item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, decimal? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, double item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, double? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, Enum item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public string FormatAssignment( string column, DateTime? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		public static string FormatAssignment( string column, TimeSpan? item, bool commaPrefix )
		{
			return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		}
		
		//public string FormatAssignment( string column, Facade.IDataTypeId<int> item, bool commaPrefix )
		//{
		//   return FormatAssignment( string.Format(CultureInfo.InvariantCulture, "{0}={1}", column, Format(item)), commaPrefix );
		//}

		public static string Format( string item )
		{
			string format = string.Empty;
			if( !string.IsNullOrEmpty(item) )
			{
				format = item.Replace( "'", "''" );
				format = "'"+format+"'";
			}
			else
				format = "null";
			return format;
		}
		public static string Format( Enum item )
		{
			return Format( Convert.ToInt32(item, CultureInfo.InvariantCulture) );
		}

		public static string Format( int item )
		{
			return item.ToString(CultureInfo.InvariantCulture);
		}

		public static string Format( long item )
		{
			return item.ToString(CultureInfo.InvariantCulture);
		}

		public static string Format( long? item )
		{
			return item==null ? "null" : Format( (long)item );
		}

		public static string Format( ulong item )
		{
			return item.ToString(CultureInfo.InvariantCulture);
		}

		public static string Format( ulong? item )
		{
			return item==null ? "null" : Format( (ulong)item );
		}

		public static string Format( double item )
		{
			return item.ToString(CultureInfo.InvariantCulture);
		}

		public static string Format( double? item )
		{
			return item==null ? "null" : Format((double)item);
		}

		public static string Format( decimal item )
		{
			return item.ToString(CultureInfo.InvariantCulture);
		}

		public static string Format( decimal? item )
		{
			return item==null ? "null" : Format((decimal)item);
		}

		public static string Format( TimeSpan item )
		{
			return Format( item.Ticks );
		}

		public static string Format( TimeSpan? item )
		{
			return item==null ? "null" : Format( (TimeSpan)item );
		}

		public static string Format( uint item )
		{
			return Format( Convert.ToInt32(item) );
		}


		public static string Format( uint? item )
		{
			return item==null ? "null" : Format( (uint)item );
		}

		public virtual string Format( DateTime? item )
		{
			return item==null ? "null" : string.Format( CultureInfo.InvariantCulture, "'{0}'", ((DateTime)item).ToString("s", CultureInfo.InvariantCulture) );
		}

		public virtual string Format( Guid? item )
		{
			return item==null ? "null" : string.Format(CultureInfo.InvariantCulture, "'{0}'", ((Guid)item).ToString() );
		}
		//[CLSCompliant(false)]
		//public virtual string Format( Facade.IDataTypeId<int> item )
		//{
		//   return item==null ? "null" : Format( item.Id );
		//}
		#endregion
		#region GetDayRange
		public string GetDayRange( string column, DateTime time )
		{
			DateTime start = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
			DateTime end = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59, 999);
			return GetRange( column, start, end );
		}
		public string GetRange( string column, DateTime start, DateTime end )
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} between {1} and {2}", column, Format(start), Format(end) );
		}
		#endregion
		#region GetDbType
		public static DbType GetDBType( DataType type, bool unicode )
		{
		   DbType dbType = DbType.Byte;
		   switch( type )
		   {
		   case DataType.Int8:
		      dbType = DbType.Byte;
		      break;
		   case DataType.Char:
		      dbType = DbType.AnsiStringFixedLength;
		      break;
		   case DataType.Guid:
		      dbType = DbType.Guid;
		      break;
		   case DataType.Int:
		   case DataType.UInt:
		      dbType = DbType.Int32;
		      break;
		   case DataType.DateTime:
		      dbType = DbType.DateTime;
		      break;
		   case DataType.Float:
		      dbType = DbType.Double;
		      break;
		   case DataType.Long:
		   case DataType.ULong:
		   case DataType.TimeSpan:
		      dbType = DbType.Int64;
		      break;
		   case DataType.Money:
		      dbType = DbType.Currency;
		      break;
		   case DataType.TChar:
		      dbType = unicode ? DbType.StringFixedLength : DbType.AnsiStringFixedLength;
		      break;
		   case DataType.VarChar:
		      dbType = DbType.AnsiString;
		      break;
		   case DataType.VarTChar:
		      dbType = unicode ? DbType.String : DbType.AnsiString;
		      break;
		   case DataType.VarWChar:
		   case DataType.Text:
		      dbType = DbType.String;
		      break;
		   case DataType.WChar:
		      dbType = DbType.StringFixedLength;
		      break;
		   default:
		      throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "no type for '{0}' type.", type) );
		   }
		   return dbType;
		}
		#endregion
		#region GetDbUser
		public virtual string GetDBUser( DataSource ds )
		{
			return ds.User;
		}
		#endregion
		#region GetRenameStatement
		public abstract string GetRenameStatement( string fromTable, string toTable );
		#endregion
		#region GetSelectAll
		public static string GetSelectAll( string tableName )
		{
			return "select * from "+tableName;
		}
		public static string GetSelectAll( string tableName, string where )
		{
			string selectAll = GetSelectAll( tableName );
			return string.IsNullOrEmpty(where)
				? selectAll
				: string.Format( CultureInfo.InvariantCulture, "{0} {1} {2}", selectAll, Where, where );
		}
		public static string GetSelectAll( string tableName, ICollection<string> additionalColumns, ICollection<FromClause> fromClauses, string where )
		{
			var columns = tableName+".*";
			if( additionalColumns!=null && additionalColumns.Count>0 )
				columns = columns+", "+additionalColumns.ToDelimitedList( ", " );
			var selectAll = new StringBuilder( string.Format(CultureInfo.InvariantCulture, "select {0} from ", columns) );
			fromClauses.ForEach( clause => selectAll.AppendLine(clause.ToString()) );
			return string.IsNullOrEmpty( where )
				? selectAll.ToString()
				: string.Format( CultureInfo.InvariantCulture, "{0} {1} {2}", selectAll, Where, where );
		}
		//public static string GetSelectAll( string tableName, IEnumerable<string> whereClauses )
		//{
		//   return GetSelectAll( tableName, whereClauses, " and " );
		//}
		public static string GetSelectAll( string tableName, IEnumerable<string> whereClauses, string operand=" and " )
		{
			if( whereClauses==null )
				throw Exceptions.ExceptionHelper.ParameterNull( "whereClauses" );

			return GetSelectAll( tableName, whereClauses.Count()==0 ? string.Empty : whereClauses.ToDelimitedList(" "+operand+" ") );
		}

		public static string GetSelectAll( string tableName, ICollection<string> additionalColumns, ICollection<FromClause> fromClauses, IEnumerable<string> whereClauses, string operand=" and " )
		{
			if( whereClauses==null )
				throw Exceptions.ExceptionHelper.ParameterNull( "whereClauses" );

			return GetSelectAll( tableName, additionalColumns, fromClauses, whereClauses.Count()==0 ? string.Empty : whereClauses.ToDelimitedList(" "+operand+" ") );
		}
		#endregion
		#region GetSqlType
/*		public static SqlType GetDefaultSqlType( Provider provider )
		{
			string sqlTypeName;
			if( provider.InvariantName=="System.Data.SqlClient" )
				sqlTypeName = "Jde.DB.SqlTypes.SqlMs, Jde.DB.SqlTypes.MsSql";
			else if( provider.InvariantName=="System.Data.OracleClient" )
				sqlTypeName = "Jde.DB.SqlTypes.SqlOracle";
			else
				throw new NotImplementedException( string.Format(CultureInfo.InvariantCulture, "No default sql type for provider '{0}'", provider.InvariantName) );

			var type = Type.GetType( sqlTypeName );
			if( type == null )
			{
				throw Exceptions.ExceptionHelper.InvalidOperation( "Could not find assembly '{0}'.", sqlTypeName );
			}
			return (SqlType)Activator.CreateInstance( type );
		}*/
		#endregion
		public abstract Enum GetDataType( DataType dataType, bool unicode );
		public abstract string GetTypeString( DataType dataType, bool unicode, bool unsigned=true, int? maxLength=null, ushort? precision=null, ushort? scale=null );
		#region GetUpdatePrototype
		public static string GetUpdatePrototype( string tableName )
		{
			return string.Format( CultureInfo.InvariantCulture, "update {0} set ", tableName );
		}
		#endregion
		#region OnlySpecifyNullChanges
		public virtual bool OnlySpecifyNullChanges
		{
			get { return false; }
		}
		#endregion
		#region ProcedureExact
		public virtual bool ProcedureExact
		{
			get { return true; }
		}
		#endregion
		public virtual string ProcedurePrefix{ get=>"as"; }
		public virtual bool? ProcPrefixDirection{ get=>false; }
		public abstract string ProcedureSuffix{ get; }
		public virtual string ProcedureParameterPrefix{ get=>"$"; }
		public virtual bool SpecifyIndexCluster{ get=>true;}
		#region SpecifyParamLength
		public abstract bool SpecifyParameterLength{get;}
		#endregion
		#region SpecifyTableOnIndexDrop
		public virtual bool SpecifyTableOnIndexDrop
		{
			get { return true; }
		}
		#endregion
		#region SupportsIdentity
		virtual public bool SupportsIdentity
		{
			get { return true; }
		}
		#endregion
		#region SupportsRowGuid
		public virtual bool SupportsRowGuid
		{
			get { return true; }
		}
		#endregion
		#region SupportsPackages
		public virtual bool SupportsPackages
		{
			get { return false; }
		}
		#endregion
		#region UniqueIndexNames
		public virtual bool UniqueIndexNames
		{
			get { return false; }
		}
		#endregion
		#region UseClusteredIndexes
		public virtual bool UseClusteredIndexes
		{
			get { return true; }
		}
		#endregion
		#region UseUpperCase
		public virtual bool UseUppercase
		{
			get { return false; }
		}
		#endregion
		#region Where
		const string _where = "where";
		static public string Where
		{
			get{ return _where; }
		}
		#endregion
		public abstract string DefaultUtcDate{get;}
		public abstract string AltDelimiter{get;}
		public abstract string LastInsertFormat{get;}
		public virtual bool CanAlterProcedures{get=>true;}
		public virtual string BuildParameterName( string name, bool isStoredProc=false )=> isStoredProc ? $"{ProcedureParameterPrefix}{name}" : $"@{name}";
	}
}
