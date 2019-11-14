using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jde.Exceptions
{
	using Jde.Utilities.Extensions;

	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors" )]
	[Serializable]
	public sealed class ItemNotFoundException : BaseException
	{
		public ItemNotFoundException( string item ):
			base( string.Format(CultureInfo.InvariantCulture, "'{0}' was not found", item) )
		{}

		public ItemNotFoundException( string item, params object[] args ):
			base( string.Format(CultureInfo.InvariantCulture, item, args) )
		{}

		public ItemNotFoundException( IEnumerable<string> items ):
			base( string.Format(CultureInfo.InvariantCulture, "'{0}' were not found.", items.ToDelimitedList()) )
		{}

		public ItemNotFoundException( IEnumerable<int> items, IEnumerable<int> itemsPresent ):
			base( string.Format(CultureInfo.InvariantCulture, "'{0}' were not found.", GetError(items, itemsPresent)) )
		{}

		public ItemNotFoundException( IEnumerable<string> items, IEnumerable<string> itemsPresent ):
			base( string.Format(CultureInfo.InvariantCulture, "'{0}' were not found.", GetError(items, itemsPresent)) )
		{}

		ItemNotFoundException( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ):
			base( info, context )
		{}

		static string GetError<T>( IEnumerable<T> items, IEnumerable<T> itemsPresent )
		{
			var missing = new LinkedList<string>();
			foreach( var item in items )
			{
				if( !itemsPresent.Contains(item) )
					missing.AddLast( item.ToString() );
			}
			return missing.ToDelimitedList( "," );
		}

		static string GetError( IEnumerable<int> items, IEnumerable<int> itemsPresent )
		{
			var missing = new LinkedList<string>();
			foreach( var item in items )
			{
				if( !itemsPresent.Contains(item) )
					missing.AddLast( item.ToString(CultureInfo.InvariantCulture) );
			}
			return missing.ToDelimitedList();
		}
	}
}
