using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Jde.Collections.Extensions
{
	public static class IEnumerableExtensions
	{
		#region ForEach
		public static void ForEach<T>( this IEnumerable<T> list, Action<T> action )
		{
			if( list==null )
				throw new ArgumentNullException( "list" );
			if( action==null )
				throw new ArgumentNullException( "action" );

			foreach( var item in list )
				action( item );
		}
		#endregion
		#region TestHasItems
		public static void TestHasItems<T>( IEnumerable<T> items, string name )
		{
			if( items==null )
				throw Exceptions.ExceptionHelper.ParameterNull( name );
			if( items.Count()==0 )
				throw Exceptions.ExceptionHelper.ParameterEmpty( name );
		}
		#endregion
		#region ToKeyDictionary
		public static Dictionary<T,T> ToKeyDictionary<T>( this IEnumerable<T> items, IEqualityComparer<T> equalityComparer )
		{
			if( items==null )
				throw new ArgumentNullException( "items" );

			var dictionary = new Dictionary<T,T>( equalityComparer );
			foreach( var item in items )
			{
				if( !dictionary.ContainsKey(item) )
					dictionary.Add( item, item );
			}

			return dictionary;
		}
		#endregion
		#region ToLinkedList
		public static LinkedList<T> ToLinkedList<T>( this IEnumerable<T> items )
		{
			var list = new LinkedList<T>();
			items.ForEach( item => list.AddLast(item) );
			
			return list;
		}
		#endregion
		#region ToCollection
		public static Collection<T> ToCollection<T>( this IEnumerable<T> items )
		{
			var list = new Collection<T>();
			items.ForEach( item => list.Add(item) );
			return list;
		}
		#endregion
		#region ToCommaDeliminatedString
		public static string ToCommaDeliminatedString<T>( this IEnumerable<T> list )
		{
			var sb = new System.Text.StringBuilder(); 
			list.ForEach( item=>sb.AppendFormat("{0},", item ) );
			if( sb.Length>0 )
				--sb.Length;
			return sb.ToString();
		}
		#endregion

		public static LinkedList<T> First<T>( this IEnumerable<T> items, int count )
		{
			if( items==null )
				throw new ArgumentNullException( "items" );
			var list = new LinkedList<T>();
			foreach( var item in items )
			{
				list.AddLast( item );
				if( list.Count==count )
					break;
			}
			if( list.Count<count )
				throw Exceptions.ExceptionHelper.InvalidOperation( "items.count='{0}', requested count='{1}'", list.Count, count );
			
			return list;
		}
	}
}
