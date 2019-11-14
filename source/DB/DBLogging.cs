using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Jde.Logging;

namespace Jde.DB
{
	static class DBLogging
	{
        //public static void CommandExecuting( Microsoft.Practices.EnterpriseLibrary.Data.Instrumentation.CommandExecutedEventArgs e )
        //{
        //    Log.LogInformation( "sql_command_executing", e.StartTime.ToString() );
        //}

/*		public static void CommandExecuting( Command cmd )
		{
			Log.Default.WriteInformation( "sql", cmd.ToString() );
		}
		
		public static void ConnectionOpened()
		{
			Log.Default.WriteInformation( "DBConnection", string.Empty );
		}
		
		public static void Critical( string message, params object[] variables )
		{
			Log.Default.WriteCritical( "sql", message, variables );
		}
*/
        //public static void CommandFailed( CommandFailedEventArgs e )
        //{
        //    Log.LogError( "CommandText='{0}' error='{1}", "sql", e.CommandText, e.Exception );
        //}

        //public static void CommandExecuted( CommandExecutedEventArgs e )
        //{
        //    Log.LogInformation( "sql", e.StartTime.ToString() );
        //}
        //public static void ConnectionFailed( ConnectionFailedEventArgs e )
        //{
        //    var showConnectionStringSetting = System.Configuration.ConfigurationManager.AppSettings["ShowConnectionStringInLogs"];
        //    bool showConnectionString = false;
        //    if( !string.IsNullOrEmpty(showConnectionStringSetting) )
        //        bool.TryParse( showConnectionStringSetting, out showConnectionString );

        //    StringBuilder message = new StringBuilder( string.Format(CultureInfo.InvariantCulture, "Could not connect to the sql server. error='{0}'", e.Exception.ToString()) );
        //    if( showConnectionString )
        //        message.AppendLine( string.Format(CultureInfo.InvariantCulture, "ConnectionString='{0}'.", e.ConnectionString) );
        //    Log.LogError( "DBConnection", message.ToString() );
        //}
	}
}
