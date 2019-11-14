using System;
using System.Collections.Generic;
using System.Linq;

namespace Jde.Collections.Extensions
{
	public static class LinkedListExtensions
	{
		public static void Append<T>( this LinkedList<T> list, IEnumerable<T> additionalItems )
		{
			if( list==null )
				throw new ArgumentNullException( "list" );
			if( additionalItems==null )
				throw new ArgumentNullException( "additionalItems" );

			foreach( var additionalItem in additionalItems )
				list.AddLast( additionalItem );
		}

		public static System.Collections.ObjectModel.Collection<T> ToCollection<T>( this LinkedList<T> list )
		{
			if( list==null )
				throw new ArgumentNullException( "list" );
			var collection = new System.Collections.ObjectModel.Collection<T>();
			foreach( var item in list )
				collection.Add( item );

			return collection;
		}

		public static LinkedList<T> Last<T>( this LinkedList<T> list, int count )
		{
			if( list==null )
				throw new ArgumentNullException( "list" );
			if( list.Count<count )
				Exceptions.ExceptionHelper.InvalidOperation( "list has '{0}' items, count is '{1}'", list.Count, count );
			var collection = new LinkedList<T>();
			int iItem = 0;
			for( var node = list.Last; iItem<count; node=node.Previous, ++iItem )
				collection.AddLast( node.Value );

			return collection;
		}

		public static LinkedList<string> ToLinkedList( this System.Collections.Specialized.StringCollection strings )
		{
			if( strings==null )
				throw new ArgumentNullException( "strings" );
			var results = new LinkedList<string>();
			foreach( var str in strings )
				results.AddLast( str );

			return results;
		}

		public static System.Collections.Specialized.StringCollection ToStringCollection( this LinkedList<string> strings )
		{
			if( strings==null )
				throw new ArgumentNullException( "strings" );
			var results = new System.Collections.Specialized.StringCollection();
			foreach( var str in strings )
				results.Add( str );

			return results;
		}

	}
}
