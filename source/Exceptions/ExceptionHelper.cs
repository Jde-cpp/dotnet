using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace Jde.Exceptions
{
	public static class ExceptionHelper
	{
		public static Exception GetBaseInner(Exception exception)
		{
			Exception inner = exception;
			while (inner.InnerException != null)
				inner = inner.InnerException;
			return inner;
		}

		public static ArgumentException ParameterInvalid( string parameter )
		{
			return new ArgumentException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.ParameterInvalid, parameter), parameter );
		}

		public static ArgumentException ParameterNullOrEmpty( string parameter )
		{
			return new ArgumentException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.ParameterNullOrEmpty, parameter), parameter );
		}

		public static ArgumentException ParameterNull( string parameter )
		{
			return new ArgumentNullException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.ParameterNull, parameter), parameter );
		}

		public static ArgumentException ParameterEmpty( string parameter )
		{
			return new ArgumentException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.ParameterEmpty, parameter), parameter );
		}
		
		public static ArgumentException PropertyInvalid( string property )
		{
			return new ArgumentException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyInvalid, property), property );
		}

		public static ArgumentException PropertyNullOrEmpty( string property )
		{
			return new ArgumentException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.PropertyNullOrEmpty, property), property );
		}

		public static InvalidOperationException UnexpectedError( string methodName )
		{
			return new InvalidOperationException( string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnexpectedError, methodName) );
		}

		public static ArgumentException ArgumentException( string error, params object[] args )
		{
			return new ArgumentException( string.Format(CultureInfo.CurrentCulture, error, args) );
		}

		public static InvalidOperationException InvalidOperation( string error, params object[] args )
		{
			return new InvalidOperationException( string.Format(CultureInfo.CurrentCulture, error, args) );
		}

		public static NotSupportedException NotSupported( string error, params object[] args )
		{
			return new NotSupportedException( string.Format(CultureInfo.CurrentCulture, error, args) );
		}

		public static InvalidCastException InvalidCast( string error, params object[] args )
		{
			return new InvalidCastException( string.Format(CultureInfo.CurrentCulture, error, args) );
		}
	}
}
