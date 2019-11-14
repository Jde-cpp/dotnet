using System;
using System.Collections.Generic;
using TraceEventType=System.Diagnostics.TraceEventType;
using System.Linq;


namespace Jde.Logging
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors" )]
	[Serializable]
	public sealed class LogException : Exceptions.BaseException
	{
		public LogException( Exception inner, string format, params object[] args ):
			base( string.Format(format, args), inner )
		{}

		public LogException( string format, params object[] args ):
			base( format, args )
		{}

		public LogException( TraceEventType type, string format, params object[] args ):
			this( format, args )
		{
			_traceEventType = type;
		}
		
		LogException( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ):
			base( info, context )
		{}

		//[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context )
		{
			base.GetObjectData( info, context );
		}

		public static LogException GetCritical( string format, params object[] args )
		{
			return new LogException( TraceEventType.Critical, format, args );
		}
		
		//public void Write()
		//{
		//	//Activity2.Write( TraceEventType, Message, 
		//}


		readonly TraceEventType _traceEventType = TraceEventType.Error;
		public TraceEventType TraceEventType
		{
			get{ return _traceEventType; }
			//private set{ _traceEventType = value; }
		}
	}
}
