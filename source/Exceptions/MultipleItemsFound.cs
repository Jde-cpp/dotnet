using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jde.Exceptions
{
	using Utilities.Extensions;

	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors" ), Serializable]
	public sealed class MultipleItemsFoundException : Exception
	{
		public MultipleItemsFoundException( string item ):
			base( string.Format(CultureInfo.InvariantCulture, "'{0}' was not found", item) )
		{}

		public MultipleItemsFoundException( IEnumerable<string> items ):
			base( string.Format(CultureInfo.InvariantCulture, "'{0}' were not found.", items.ToDelimitedList()) )
		{}

	}
}
