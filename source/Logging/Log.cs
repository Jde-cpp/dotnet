using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

//using Ent=Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Jde.Logging
{
	#region LoggingCategories
	public static class LoggingCategories
	{
		public const string Callback = "Callback";
		public const string General = "General";
		public const string Presentation = "Presentation";
		public const string Process = "Process";
		public const string IB = "IB";
		public const string Remoting = "Remoting";
	}
	#endregion
	public static class Log
	{
		#region IB
		public static void LogIBError( string message, params object[] args )
		{
			LogError( message, LoggingCategories.IB, args );
		}
		public static void LogIBWarning( string message, params object[] args )
		{
			LogWarning( LoggingCategories.IB, message, args );
		}
		public static void LogIBWarning( Exception exception )
		{
			LogWarning( exception, LoggingCategories.IB );
		}
		public static void LogIBInformation( string message, params object[] args )
		{
			WriteInformation( LoggingCategories.IB, message, args );
		}
		public static void LogIBVerbose( string message, params object[] args )
		{
			WriteVerbose( LoggingCategories.IB, message, args );
		}
		#endregion
		#region Presentation
		public static void LogPresentationError( string message, params object[] args )
		{
			LogError( message, LoggingCategories.Presentation, args );
		}
		public static void LogPresentationError( Exception exception )
		{
			LogError( exception, LoggingCategories.Presentation );
		}
		
		public static void LogPresentationInformation( string logMessage, params object[] args )
		{
			WriteInformation( logMessage, LoggingCategories.Presentation, args );
		}
		#endregion
		#region General
		public static void LogGeneralError( Exception exception )
		{
			LogError( exception, LoggingCategories.General );
		}
		public static void LogGeneralError( string message, params object[] args )
		{
			LogError( message, LoggingCategories.General, args );
		}
		public static void LogGeneralWarning( string message, params object[] args )
		{
			LogWarning( LoggingCategories.General, message, args );
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Jde.Logging.Log.LogInformation(System.String,System.String,System.Object[])" )]
		public static void LogGeneralInformation( string message, params object[] args )
		{
			WriteInformation( LoggingCategories.General, message, args );
		}
		#endregion
		#region Process
		public static void LogProcessError( Exception exception, string message, params object[] args )
		{
			if( exception==null )
				throw new ArgumentNullException( "exception" );
			LogError( message+"\n"+exception.ToString(), LoggingCategories.Process, args );
		}
		public static void LogProcessError( string message, params object[] args )
		{
			LogError( message, LoggingCategories.Process, args );
		}
		public static void LogProcessWarning( string message, params object[] args )
		{
			LogWarning( LoggingCategories.Process, message, args );
		}
		#endregion
		#region Remoting
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
		public static void LogRemotingEvent( InvalidOperationException exception )
		{
			LogError( exception, LoggingCategories.Remoting );
		}
		#endregion
		#region Error
		public static void WriteActionError( Exception exception, string category )
		{
			AddAction( exception, category, TraceEventType.Error );
		}
		public static void LogError( Exception exception, string category )
		{
			Add( exception, category, TraceEventType.Error );
		}
		public static void LogError( Exception exception, string category, string message, params object[] args )
		{
			Add( exception, category, TraceEventType.Error, message, args );
		}
		public static void LogError( string message, string category, params object[] args )
		{
			Add( message, category, TraceEventType.Error, args );
		}
		static Dictionary<string,Dictionary<string,int>> _oneTimeErrors = new Dictionary<string,Dictionary<string,int>>();
		public static void WriteErrorOnce( System.Diagnostics.StackFrame method, string message, string category, params object[] args )
		{
			lock( _oneTimeErrors )
			{
				if( !_oneTimeErrors.ContainsKey(category) )
					_oneTimeErrors.Add( category, new Dictionary<string,int>() );
				var fullMessage = string.Format( CultureInfo.InvariantCulture, message, args );
				if( !_oneTimeErrors[category].ContainsKey(fullMessage) )
				{
					_oneTimeErrors[category].Add( fullMessage, 1 );
					Add( method, fullMessage, category, TraceEventType.Error );
				}
				else
					_oneTimeErrors[category][fullMessage]=_oneTimeErrors[category][fullMessage]+1;
			}
		}

		static Dictionary<string,Dictionary<string,string>> _oneTimeKeyErrors = new Dictionary<string,Dictionary<string,string>>();
		public static void WriteErrorKeyOnce( System.Diagnostics.StackFrame method, string key, string message, string category, params object[] args )
		{
			lock( _oneTimeKeyErrors )
			{
				if( !_oneTimeKeyErrors.ContainsKey(category) )
					_oneTimeKeyErrors.Add( category, new Dictionary<string,string>() );
				var fullMessage = string.Format( CultureInfo.InvariantCulture, message, args );
				if( !_oneTimeKeyErrors[category].ContainsKey(key) )
				{
					_oneTimeKeyErrors[category].Add( key, fullMessage );
					Add( method, fullMessage, category, TraceEventType.Error );
				}
				else if( _oneTimeKeyErrors[category][key]!=fullMessage )
				{
					_oneTimeKeyErrors[category][key] = fullMessage;
					Add( method, fullMessage, category, TraceEventType.Error );
				}
			}
		}
		#endregion
		#region Exceptions
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
/*		public static void Add( System.ServiceModel.CommunicationException exception )
		{
			Add( exception, LoggingCategories.Callback, TraceEventType.Error );
		}*/
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters" )]
		public static void Add( TimeoutException exception )
		{
			Add( exception, LoggingCategories.Callback, TraceEventType.Error );
		}
		#endregion
		#region Information
		//public static void WriteInformation( string category )
		//{
		//   WriteInformation( category, string.Empty );
		//}
		public static void WriteInformation( string category, string logMessage, params object[] args )
		{
			Add( logMessage, category, TraceEventType.Information, args );
		}
		public static void WriteInformation( string category, Exception e )
		{
			Add( e, category, TraceEventType.Information );
		}

		static Dictionary<string,Dictionary<string,string>> _oneTimeKeyInformation = new Dictionary<string,Dictionary<string,string>>();
		public static void WriteInformationKeyOnce( System.Diagnostics.StackFrame method, string key, string message, string category, params object[] args )
		{
			lock( _oneTimeKeyInformation )
			{
				if( !_oneTimeKeyInformation.ContainsKey(category) )
					_oneTimeKeyInformation.Add( category, new Dictionary<string,string>() );
				var fullMessage = string.Format( CultureInfo.InvariantCulture, message, args );
				if( !_oneTimeKeyInformation[category].ContainsKey(key) )
				{
					_oneTimeKeyInformation[category].Add( key, fullMessage );
					Add( method, fullMessage, category, TraceEventType.Information );
				}
				else if( _oneTimeKeyInformation[category][key]!=fullMessage )
				{
					_oneTimeKeyInformation[category][key] = fullMessage;
					Add( method, fullMessage, category, TraceEventType.Information );
				}
			}
		}
		#endregion
		#region Verbose
		public static void WriteVerbose( string category, string logMessage, params object[] args )
		{
			Add( logMessage, category, TraceEventType.Verbose, args );
		}
		public static void WriteVerbose( System.Diagnostics.StackFrame method, string category, string message, params object[] args )
		{
			Add( method, message, category, TraceEventType.Verbose, args );
		}
		public static void WriteVerbose( System.Diagnostics.StackFrame method, string category, Exception e )
		{
			Add( method, e, category, TraceEventType.Verbose );
		}
		public static void WriteVerbose( System.Diagnostics.StackFrame method, string category, Exception e, string message, params object[] args )
		{
			Add( method, e, category, TraceEventType.Verbose, message, args );
		}
		#endregion
		#region LogCritical
		public static void LogCritical( string category, string message, params object[] args )
		{
			Add( message, category, TraceEventType.Critical, args );
		}
		public static void LogCritical( Exception exception, string category )
		{
			Add( exception, category, TraceEventType.Critical );
		}
		#endregion
		#region Warning
		static void LogWarning( Exception e, string category )
		{
			Add( e, category, TraceEventType.Warning );
		}
		public static void LogWarning( string category, string message, params object[] args )
		{
			Add( message, category, TraceEventType.Warning, args );
		}
		public static void LogWarning( string category, Exception e, string message, params object[] args )
		{
			Add( e, category, TraceEventType.Warning, message, args );
		}
		#endregion
		#region Log
		public static void Write( string category, Logging.LogException exception )
		{
			if( exception==null )
				throw new ArgumentNullException( "exception" );

			Add( exception.Message, category, exception.TraceEventType );
		}

		public static void Write( System.Diagnostics.StackFrame method, string category, TraceEventType severity, string logMessage, params object[] args )
		{
			Add( method, string.Format(CultureInfo.InvariantCulture, logMessage, args), category, severity );
		}

		static void Add( System.Diagnostics.StackFrame method, string message, string category, TraceEventType severity, params object[] args )
		{
			Add( method, args.Length>0 ? string.Format(CultureInfo.InvariantCulture, message, args) : message, category, severity );
		}
		static void Add( System.Diagnostics.StackFrame stackFrame, string message, string category, TraceEventType severity )
		{
			Dictionary<string,object> properties = null;
			if( stackFrame!=null )
			{
				properties = new Dictionary<string,object>();
				var fileName = stackFrame.GetFileName();
				if( !string.IsNullOrEmpty(fileName) )
					properties.Add( "file", string.Format(CultureInfo.InvariantCulture, "{0} - Line {1}", fileName, stackFrame.GetFileLineNumber()) );
				var method = stackFrame.GetMethod();
				if( method!=null && method.DeclaringType!=null )
					properties.Add( "source", string.Format(CultureInfo.InvariantCulture, "{0}.{1}", method.DeclaringType.FullName, method.Name) );
			}
				
			//if( severity<TraceEventType.Information )
			//{
			//   var parameters = method.GetParameters();
			//   foreach( var parameter in parameters )
			//   {
			//      properties.Add( parameter.Name, parameter.
			//   }
			//}
			//var entry = new Ent.LogEntry( message, category, GetPriority(severity), -1, severity, User, properties );
			//Add( entry );
		}
		static void Add( System.Diagnostics.StackFrame method, Exception e, string category, TraceEventType severity )
		{
			Add( method, e.ToString(), category, severity );
		}
		static void Add( System.Diagnostics.StackFrame method, Exception e, string category, TraceEventType severity, string message, params object[] variables )
		{
			var formatedMessage = new StringBuilder( variables.Length>0 ? string.Format(CultureInfo.InvariantCulture, message, variables) : message );
			formatedMessage.AppendLine( e.ToString() );
			Add( method, formatedMessage.ToString(), category, severity );
		}

		static void Add( string message, string category, TraceEventType severity, params object[] args )
		{
			Add( args.Length>0 ? string.Format(CultureInfo.InvariantCulture, message, args) : message, category, severity );
		}
		static void Add( string logMessage, string category, TraceEventType severity )
		{
			//Add( new Ent.LogEntry(logMessage, category, GetPriority(severity), -1, severity, User, null) );
		}
		static void Add( Exception e, string category, TraceEventType severity )
		{
			Add( e.ToString(), category, severity );
		}
		static void Add( Exception e, string category, TraceEventType severity, string message, params object[] args )
		{
			Add( string.Format(CultureInfo.InvariantCulture, message, args)+"\n"+e.ToString(), category, severity );
		}
		static void AddAction( Exception e, string category, TraceEventType severity )
		{
			Add( "§"+e.ToString(), category, severity );
		}
		static bool _loggerSet;
		static object _addSync = new object();
		/*static void Add( Ent.LogEntry entry )
		{
			try
			{
				if( !_loggerSet )
					SetLogger();
				Ent.Logger.Write( entry );
			}
			catch( Exception e )
			{
				System.Diagnostics.Trace.WriteLine( e );
			}
		}*/
		#endregion
		#region ShouldLog
		public static bool ShouldLog( string category, System.Diagnostics.SourceLevels severity )
		{
			if( !_loggerSet )
				SetLogger();
			bool shouldLog = false;
			//Ent.LogSource source;
			//if( Ent.Logger.Writer.TraceSources.TryGetValue(category, out source ) )
				//shouldLog = source.Level!= SourceLevels.Off && source.Level<=severity;
			return shouldLog;
		}
		#endregion
		#region SetLogger
		static void SetLogger()
		{
			lock( _addSync )
			{
				if( !_loggerSet )
				{
					//Ent.Logger.SetLogWriter( new Ent.LogWriterFactory().Create() );
					_loggerSet = true;
				}
			}
		}
		#endregion
		#region GetDefaultCategory
		const string AppSetting = "Logging.Default.Category";
		public static string GetDefaultCategory( string defaultSetting )
		{
			//var defaultCategory = System.Configuration.ConfigurationManager.AppSettings[defaultSetting+"."+AppSetting];
			//if( string.IsNullOrEmpty(defaultCategory) )
			//	defaultCategory = System.Configuration.ConfigurationManager.AppSettings[AppSetting];
			//return string.IsNullOrEmpty(defaultCategory) ? defaultSetting : defaultCategory;
			return "default";
		}
		#endregion
		#region GetPriority
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TraceEventType" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Jde.Logging.Log.LogGeneralError(System.String,System.Object[])" )]
		static int GetPriority( TraceEventType severity )
		{
			int priority = 0;
			switch( severity )
			{
			case TraceEventType.Critical:
				priority=1;
				break;
			case TraceEventType.Error:
				priority=2;
				break;
			case TraceEventType.Information:
			case TraceEventType.Resume:
			case TraceEventType.Start:
			case TraceEventType.Stop:
			case TraceEventType.Suspend:
			case TraceEventType.Transfer:
			case TraceEventType.Verbose:
				priority=5;
				break;
			case TraceEventType.Warning:
				priority=3;
				break;
			default:
				LogGeneralError( "unknown TraceEventType: '{0}'", severity );
				priority = 1;
				break;
			}
			return priority;
		}
		#endregion
		#region user
		static string User
		{
			get{ return System.Threading.Thread.CurrentPrincipal.Identity.Name; }
		}
		#endregion
	}
}
