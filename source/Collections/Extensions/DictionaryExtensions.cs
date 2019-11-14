using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jde.Collections.Extensions
{
	public static class DictionaryExtensions
	{
		public static void Append<TKey,TValue>( this Dictionary<TKey,TValue> dictionary, Dictionary<TKey,TValue> additionalItems )
		{
			if( dictionary==null )
				throw new ArgumentNullException( "dictionary" );
			if( additionalItems==null )
				throw new ArgumentNullException( "additionalItems" );

			foreach( var additionalItem in additionalItems )
				dictionary.Add( additionalItem.Key, additionalItem.Value );
		}

		public static Dictionary<TKey,TValue> Clone<TKey,TValue>( this Dictionary<TKey,TValue> original )
		{
			if( original==null )
				throw new ArgumentNullException( "original" );
			var clone = new Dictionary<TKey,TValue>();
			foreach( var item in original )
				clone.Add( item.Key, item.Value );

			return clone;
		}
		
		public static void AddForce<TKey,TValue>( this IDictionary<TKey,TValue> items, TKey key, TValue value, System.Diagnostics.TraceEventType severity=System.Diagnostics.TraceEventType.Error )
		{
			if( items==null )
				throw new ArgumentNullException( "items" );
			lock( items )
			{
				if( items.ContainsKey(key) )
				{
					//Log.Default.Write( new System.Diagnostics.StackFrame(1), severity, "collection has stale key '{0}' with value '{1}'", key, items[key] );
					items.Remove(key);
				}
				items.Add( key, value );
			}
		}

		public static Dictionary<TKey,TValue> ExceptValues<TKey,TValue>( this IDictionary<TKey,TValue> from, IDictionary<TKey,TValue> keys )
		{
			if( from==null )
				throw new ArgumentNullException( "from" );
			if( keys==null )
				throw new ArgumentNullException( "keys" );
			var missingItems = new Dictionary<TKey,TValue>();
			foreach( var item in from )
			{
				if( !keys.ContainsKey(item.Key) )
					missingItems.Add( item.Key, item.Value );
			}
			return missingItems;
		}

		public static string ToString<TKey,TValue>( this IReadOnlyDictionary<TKey,TValue> values )
		{
			var sb = new System.Text.StringBuilder();
			foreach( var value in values )
				sb.AppendFormat( "['{0}','{1}']", value.Key, value.Value);
			return sb.ToString();

		}

		public static bool AreEqual<TKey,TValue>( this IDictionary<TKey,TValue> first, IDictionary<TKey,TValue> second )
		{
			if( first==null )
				throw new ArgumentNullException( "first" );
			if( second==null )
				throw new ArgumentNullException( "keys" );
			bool equal = first.Count==second.Count;
			if( equal )
			{
				foreach( var item in first )
				{
					equal = second.ContainsKey(item.Key) && second[item.Key].Equals(item.Value);
					if( !equal )
						break;
				}
			}
			return equal;
		}
	}
}
