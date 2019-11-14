using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jde.Logging
{
	public abstract class LogBase
	{
		protected LogBase( string defaultCategoryName )
		{
			CategoryConfigName = defaultCategoryName;
		}
		#region Category
		string _category;
		public string Category
		{
			get
			{ 
				if( _category==null )
					System.Threading.Interlocked.CompareExchange<string>( ref _category, Logging.Log.GetDefaultCategory(CategoryConfigName), null );
				return _category;
			}
		}
		#endregion
		#region CategoryName
		string _categoryConfigName;
		public string CategoryConfigName
		{
			get{ return _categoryConfigName; }
			set{ _categoryConfigName = value; }
		}
		#endregion
		#region Write
		public void Write( Logging.LogException exception )
		{
			Logging.Log.Write( Category, exception );
		}

		public void Write( System.Diagnostics.StackFrame stackFrame, System.Diagnostics.TraceEventType severity, string logMessage )
		{
			Logging.Log.Write( stackFrame, Category, severity, logMessage );
		}

		public void Write( System.Diagnostics.StackFrame stackFrame, System.Diagnostics.TraceEventType severity, string logMessage, params object[] args )
		{
			Logging.Log.Write( stackFrame, Category, severity, logMessage, args );
		}

		public void WriteCritical( Exception exception )
		{
			Logging.Log.LogCritical( exception, Category );
		}
		public void WriteCritical( string message, params object[] args )
		{
			Logging.Log.LogCritical( Category, message, args );
		}
		public void WriteActionError( Exception exception )
		{
			Logging.Log.WriteActionError( exception, Category );
		}
		public void WriteError( Logging.Activity2 activity, Exception exception )
		{
			Logging.Log.LogError( exception, Category );
		}
		public void WriteError( Exception exception )
		{
			Logging.Log.LogError( exception, Category );
		}
		public void WriteError( Exception exception, string message, params object[] args )
		{
			Logging.Log.LogError( exception, Category, message, args );
		}

		public void WriteError( string message, params object[] args )
		{
			Logging.Log.LogError( message, Category, args );
		}

		public void WriteWarning( string message, params object[] args )
		{
			Logging.Log.LogWarning( Category, message, args );
		}

		public void WriteWarning( Exception e, string message, params object[] args )
		{
			Logging.Log.LogWarning( Category, e, message, args );
		}

		#region WriteVerbose
		public void WriteVerbose( string message, params object[] args )
		{
			Logging.Log.WriteVerbose( new System.Diagnostics.StackFrame(1), Category, message, args );
		}
		public void WriteVerbose( Exception e )
		{
			Logging.Log.WriteVerbose( new System.Diagnostics.StackFrame(1), Category, e );
		}
		public void WriteVerbose( Exception e, string message, params object[] args )
		{
			Logging.Log.WriteVerbose( new System.Diagnostics.StackFrame(1), Category, e, message, args );
		}
		#endregion

		public void WriteInformation( Exception e )
		{
			Logging.Log.WriteInformation( Category, e );
		}

		public void WriteInformation( string message, params object[] args )
		{
			Logging.Log.WriteInformation( Category, message, args );
		}

		#endregion
	}
}
