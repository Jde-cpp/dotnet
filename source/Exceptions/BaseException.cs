using System;
using System.Collections.Generic;
using CultureInfo=System.Globalization.CultureInfo;
using System.Linq;
using System.Text;

namespace Jde.Exceptions
{
	[Serializable]
	public abstract class BaseException : Exception
	{
		protected BaseException( string message ):
			base( message )
		{}

		protected BaseException( string format, params object[] args ):
			base( string.Format(CultureInfo.CurrentCulture, format, args) )
		{}

		protected BaseException( string message, Exception inner ):
			base( message, inner )
		{}

		protected BaseException( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ):
			base( info, context )
		{}
	}
}
