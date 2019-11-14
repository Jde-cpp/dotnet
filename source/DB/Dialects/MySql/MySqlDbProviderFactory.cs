using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Jde.DB.Dialects
{
	public class MySqlDbProviderFactory : DbProviderFactory
	{
		public static MySql.Data.MySqlClient.MySqlClientFactory Instance{get;private set;} = new MySql.Data.MySqlClient.MySqlClientFactory();

		public MySqlDbProviderFactory():
			base()
		{}

		public override DbCommand CreateCommand()=>Instance.CreateCommand();
		public override DbConnection CreateConnection()=>Instance.CreateConnection();
		public override DbConnectionStringBuilder CreateConnectionStringBuilder()=>Instance.CreateConnectionStringBuilder();
		public override DbParameter CreateParameter()=>Instance.CreateParameter();
		public override DbDataAdapter CreateDataAdapter()=>new MySql.Data.MySqlClient.MySqlDataAdapter();
	}
}
