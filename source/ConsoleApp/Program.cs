using System;
using Microsoft.Extensions.DependencyInjection;  
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Jde
{
	using Jde.DB;
	class Program
	{
		static void Main(string[] args)
		{
			//var db = new Jde.DB.Dialects.MySqlDatabase(  );
			IConfiguration config =  new ConfigurationBuilder().AddJsonFile( "appsettings.json", true, true ).Build();
			var connectionString = "server=localhost;database=market;uid=market;password=Welcome$9;sslmode=none";
			var serviceProvider = new ServiceCollection()
			  .AddLogging()
			  .AddSingleton<System.Data.Common.DbProviderFactory,DB.Dialects.MySqlDbProviderFactory>()
			  .AddSingleton<Provider>()
			  .AddSingleton<Database,DB.Dialects.MySqlDatabase>((IServiceProvider provider)=>{return new DB.Dialects.MySqlDatabase(connectionString); } )
			  .AddSingleton<SqlSyntax,DB.Dialects.MySqlSnytax>()
			  .AddSingleton<DB.Schema.ISchema,DB.Dialects.MySqlSchema>()
			  .AddSingleton<DataSource>()
			  //.AddSingleton<DBConversionUtilities>()
			  .BuildServiceProvider();
			
			//serviceProvider.GetService<ILoggerFactory>().AddDebug();// LogLevel.Debug 

			var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
			logger.LogDebug( "Starting application" );

			var dataSource = serviceProvider.GetService<DataSource>();
			dataSource.ConnectionString = connectionString;

			var cmd = new TextCommand( "sec_contracts" ){ Database=dataSource.Database };
			cmd.LoadCount();
		}
	}
}
