

namespace Jde.DB.Dialects
{
	public class MySqlStrings
	{
		public static string Columns
		{
			get{ return
				"select	t.TABLE_NAME, t.TABLE_TYPE, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, COLUMN_TYPE, CHARACTER_MAXIMUM_LENGTH, EXTRA='auto_increment' is_identity, 0 is_id, NUMERIC_PRECISION, NUMERIC_SCALE\n"+
				"from		INFORMATION_SCHEMA.TABLES t\n"+
				"	inner join INFORMATION_SCHEMA.COLUMNS c on t.TABLE_CATALOG=c.TABLE_CATALOG and t.TABLE_SCHEMA=c.TABLE_SCHEMA and t.TABLE_NAME=c.TABLE_NAME\n"+
				"where t.TABLE_SCHEMA='{0}'\n"+
				"	{1}\n"+
				"order by t.TABLE_NAME, ORDINAL_POSITION";
			}
		}

		public static string ForeignKeys
		{
			get
			{
				return
					"select	fk.CONSTRAINT_NAME name, fk.TABLE_NAME foreign_table, fk.COLUMN_NAME fk, pk.TABLE_NAME primary_table, pk.COLUMN_NAME pk, pk.ORDINAL_POSITION ordinal\n"+
					"from		INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS con\n"+
					"	join INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk on con.CONSTRAINT_NAME=fk.CONSTRAINT_NAME\n"+
					"	join INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk on pk.CONSTRAINT_NAME COLLATE utf8_general_ci=con.UNIQUE_CONSTRAINT_NAME and pk.ORDINAL_POSITION=fk.ORDINAL_POSITION and pk.TABLE_NAME=con.REFERENCED_TABLE_NAME\n"+
					"where pk.TABLE_SCHEMA='{0}'\n"+
					"order by name, ordinal";
			}
		}
	}
}
