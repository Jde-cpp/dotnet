using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace Jde.DB.Dialects
{
	public class MySqlSnytax : SqlSyntax
	{
		public override string ProcedurePrefix =>"begin\n";
		public override string ProcedureSuffix =>"end\n";
		public override bool SpecifyParameterLength => true;

		public override void ComputeStats( DataSource ds, string tableName )
		{
			throw new NotImplementedException();
		}

		public override string GetDatabaseNameQuery()
		{
			throw new NotImplementedException();
		}


		//statement.AppendFormat( " identity({0},{1})", column.Sequence.Start.ToString(CultureInfo.InvariantCulture), column.Sequence.Increment.ToString(CultureInfo.InvariantCulture) );
		public override string IdentitySyntax( int start, int increment )=>"auto_increment primary key";

		#region DataType
		public override Enum GetDataType( DataType type, bool unicode )
		{
			var sqlType = MySqlDbType.Bit;
			switch( type )
			{
			case DataType.Binary:
				sqlType = MySqlDbType.Binary;
				break;
			case DataType.Bit:
				sqlType = MySqlDbType.Bit;
				break;
			case DataType.Char:
				sqlType = MySqlDbType.UByte;
				break;
			case DataType.DateTime:
				sqlType = MySqlDbType.DateTime;
				break;
			case DataType.Decimal:
				sqlType = MySqlDbType.Decimal;
				break;
			case DataType.Float:
				sqlType = MySqlDbType.Float;
				break;
			case DataType.Guid:
				sqlType = MySqlDbType.Binary;
				break;
			case DataType.Int8:
				sqlType = MySqlDbType.Byte;
				break;
			case DataType.Int16:
				sqlType = MySqlDbType.Int16;
				break;
			case DataType.Int:
				sqlType = MySqlDbType.Int32;
				break;
			case DataType.UInt:
				sqlType = MySqlDbType.UInt32;
				break;
			case DataType.Long:
			case DataType.TimeSpan:
				sqlType = MySqlDbType.Int64;
				break;
			case DataType.ULong:
				sqlType = MySqlDbType.UInt64;
				break;
			case DataType.Money:
				sqlType = MySqlDbType.Decimal;
				break;
			case DataType.TChar:
				sqlType = unicode ? MySqlDbType.VarChar : MySqlDbType.UByte;
				break;
			case DataType.Text:
				sqlType = MySqlDbType.Text;
				break;
			case DataType.VarChar:
				sqlType = MySqlDbType.VarChar;
				break;
			case DataType.Uri:
			case DataType.VarTChar:
				sqlType = MySqlDbType.VarChar;//unicode ? MySqlDbType.VarChar : MySqlDbType.VarChar;
				break;
			case DataType.VarWChar:
				sqlType = MySqlDbType.VarChar;;
				break;
			case DataType.WChar:
				sqlType = MySqlDbType.UInt16;
				break;
			case DataType.UInt16:
				sqlType = MySqlDbType.UInt16;
				break;
			case DataType.UInt8:
				sqlType = MySqlDbType.UByte;
				break;
			case DataType.RefCursor:
			case DataType.Cursor:
			case DataType.None:
			default:
				throw new ArgumentException( string.Format( System.Globalization.CultureInfo.InvariantCulture, "no type for '{0}' type.", type) );
			}
			return sqlType;
		}
		#endregion
		public override Provider GetProvider()=>new Provider(null){Factory=new MySql.Data.MySqlClient.MySqlClientFactory() };

		public override string GetRenameStatement( string fromTable, string toTable )
		{
			throw new NotImplementedException();
		}

		public override string GetTypeString( DataType type, bool unicode, bool unsigned=true, int? maxLength=null, ushort? precision=null, ushort? scale=null )
		{
			string typeString=null;
			switch( type )
			{
			case DataType.UInt8:
				typeString = "tinyint unsigned";
				break;
			case DataType.Decimal:
				typeString = string.Format( "decimal({0},{1})", precision, scale );
				break;
			case DataType.Int16:
				typeString = "smallint";
				break;
			case DataType.UInt16:
				typeString = "smallint unsigned";
				break;
			case DataType.Int:
				typeString = "int";
				break;
			case DataType.Long:
				typeString = "bigint";
				break;
			case DataType.UInt:
				typeString = "int unsigned";
				break;
			case DataType.RefCursor:
				typeString = "ref cursor";
				break;
			case DataType.Money:
				typeString = "decimal(19,4)";
				break;
			case DataType.ULong:
				typeString = "bigint unsigned";
				break;
			case DataType.TimeSpan:
				typeString = "bigint";
				break;
			default:
				typeString = GetDataType(type, unicode).ToString();
				break;
			}
			if( type==DataType.Guid )
				maxLength = 16;
			return maxLength!=null && (DataTypes.HasLength(type) || type==DataType.Guid)
				?	string.Format( CultureInfo.InvariantCulture, "{0}({1})", typeString, maxLength )
				:	typeString;
		}

		public override string LoadProcText( Database db, string procName )
		{
			//var dt = ds.LoadDataTable( $"select ROUTINE_DEFINITION from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME='{procName}'" );
			var dt = db.LoadDataTable( $"show create procedure {procName}" );
			var procText =  dt.Rows?[0]["Create Procedure"] as string;
			var pattern = "DEFINER=([`])(\\\\?.)*?\\1@([`])(\\\\?.)*?\\1";
			//var rgx = new System.Text.RegularExpressions.Regex( pattern );
			//proc_text = rgx.Replace( proc_text, string.Empty,  );
			return System.Text.RegularExpressions.Regex.Replace( procText, pattern, string.Empty );
				//System.Text.RegularExpressions.Rep
		}

		public override void LoadSize( DataSource ds, string tableName, out int rowCount, out int tableSize, out int indexSize )
		{
			throw new NotImplementedException();
		}

		public override string LoadVersion( DataSource ds )
		{
			throw new NotImplementedException();
		}

		public override void TestConnection( DataSource ds )
		{
			throw new NotImplementedException();
		}

		public override string DropIndexStatement( bool primaryKey, string tableName, string name )
		{
			return primaryKey 
				? $"alter table {tableName} drop constraint {name}"
				: $"alter table {tableName} drop index {name}";
		}

		public override string DropForeignKeyStatement( string tableName, string name )=>$"alter table `{tableName}` drop foreign key `{name}`";

		public override string AlterColumnPrefix => "modify column";
		public override string DefaultUtcDate => "current_timestamp";
		public override string AltDelimiter => "$$";
		public override string ProcedureParameterPrefix => "_";
		public override string LastInsertFormat => "select LAST_INSERT_ID() into {0};"; //"\tselect @{0}=@@identity", 
		public override bool? ProcPrefixDirection => null;
		public override bool CanAlterProcedures=>false;
		public override bool SpecifyIndexCluster=>false;
		
	}
}
