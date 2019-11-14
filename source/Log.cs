using System;
using System.Collections.Generic;
using System.Linq;
using CultureInfo=System.Globalization.CultureInfo;
using System.Diagnostics;

namespace Jde
{
	public class Log : Logging.LogBase
	{
		#region Constructors
		public Log():
			base(DefaultCategoryName)
		{ }
		#endregion
		#region Information
		//public static void WriteInformationKeyOnce( string key, string message )
		//{
		//	Logging.Log.WriteInformationKeyOnce( new System.Diagnostics.StackFrame(1), key, message, DefaultCategoryName );
		//}
		public static void WriteInformationKeyOnce( string key, string logMessage, params object[] args )
		{
			Logging.Log.WriteInformationKeyOnce( new System.Diagnostics.StackFrame(1), key, logMessage, DefaultCategoryName, args );
		}
		#endregion
		#region Error
		public static void WriteErrorOnce( string logMessage, params object[] args )
		{
			Logging.Log.WriteErrorOnce( new System.Diagnostics.StackFrame(1), logMessage, DefaultCategoryName, args );
		}

		public static void WriteErrorKeyOnce( string key, string logMessage, params object[] args )
		{
			Logging.Log.WriteErrorKeyOnce( new System.Diagnostics.StackFrame(1), key, logMessage, DefaultCategoryName, args );
		}
		#endregion
		#region Category
		const string DefaultCategoryName = "Jde";
		#endregion
		#region Default
		static Log _default;
		public static Log Default
		{
			get
			{
				if( _default==null )
					System.Threading.Interlocked.CompareExchange<Log>( ref _default, new Log(), null );
				return _default;
			}
		}
		#endregion
	}
}
