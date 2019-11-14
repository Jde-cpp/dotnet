using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using ExceptionHelper=Jde.Exceptions.ExceptionHelper;
using System.Collections.ObjectModel;

namespace Jde.Utilities
{
	public static class StringHelper
	{
		public static int GetIndexOf( string token, string value )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );
			int start = value.IndexOf( token, StringComparison.Ordinal );
			if( start==-1 )
                throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not find '{0}' in '{1}'", token, value) );
			return start;
		}

		public static int GetIndexOf( string token, string value, int startIndex )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );
			int start = value.IndexOf( token, startIndex, StringComparison.Ordinal );
			if( start==-1 )
                throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not find '{0}' in '{1}, with a start index of '{2}'", token, value, startIndex) );
			return start;
		}

		public static string TryParseParentheses( string value )
		{
			return TryParseIndexes( value, "(" );
		}
		
		public static string TryParseIndexes( string value, string token )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );
			string item = string.Empty;
			int start = value.IndexOf( "(", StringComparison.Ordinal );
			if( start!=-1 )
			{
				string endToken = token;
				if( token=="(" )
					endToken = ")";
				else if( token=="<" )
					endToken = ">";
				int end = value.IndexOf( endToken, start, StringComparison.Ordinal );
				if( end!=-1 )
					item = value.Substring( start+1, end-start-1 );
			}
			return item;
		}

		public static string TryParsePrefix( string value )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );
			int iChar = 0;
			while( iChar<value.Length && Char.IsWhiteSpace(value[iChar]) )
				++iChar;
			StringBuilder prefix = new StringBuilder();
			while( iChar<value.Length && !Char.IsWhiteSpace(value[iChar]) )
				prefix.Append( value[iChar++] );

			return prefix.ToString();
		}

		public static string TryParseSuffix( string value )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );
			int iChar = value.Length-1;
			while( iChar>=0 && Char.IsWhiteSpace(value[iChar]) )
				--iChar;

			int end = iChar;
			while( iChar>=0 && !Char.IsWhiteSpace(value[iChar]) )
				--iChar;

			return value.Substring( iChar+1, end-iChar );
		}

		public static string GetSuffix( string value, string delimiter="." )
		{
			var lastIndex = value.LastIndexOf(delimiter);
			return lastIndex==-1 && lastIndex<value.Length-1 ? value : value.Substring(lastIndex+1);
		}

		public static string RemoveMultipleSpaces( string value )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );

			var newString = new StringBuilder();
			for( int i=0; i<value.Length; ++i )
			{
				var ch = value[i];
				newString.Append( ch );
				if( char.IsWhiteSpace(ch) )
				{
					while( i+1<value.Length && char.IsWhiteSpace(value[i+1]) )
						++i;
				}
			}
			return newString.ToString();
		}

		public static IEnumerable<string> Split( string line, string separator )
		{
			if( line==null )
				throw new ArgumentNullException( "value" );
			if( string.IsNullOrEmpty(separator) )
				throw new ArgumentNullException( "predicate" );

			var values = new System.Text.RegularExpressions.Regex(separator).Split( line );// data.Split( new string[] { separator }, StringSplitOptions.None ).ToList();
			return values.Where( value=>!string.IsNullOrEmpty( value ) );
		}

		public static LinkedList<string> Split( string line, char separator=',' )
		{
			if( line==null )
				throw new ArgumentNullException( "value" );
			//if( line=="62,1,1,\"Icard, Miss. Amelie\",female,38,0,0,113572,80,B28," )
				//System.Diagnostics.Trace.WriteLine( "here" );
			bool inParenthesis = false;
			var values = new LinkedList<string>();
			var value = new StringBuilder();
			foreach( char ch in line.ToCharArray() )
			{
				if( ch=='"' )
					inParenthesis = !inParenthesis;
				else if( inParenthesis || ch!=separator )
					value.Append( ch );
				else
				{
					values.AddLast( value.ToString() );
					value.Length=0;
				}
			}
			//if( value.Length>0 )
				values.AddLast( value.ToString() );
			//var values = new System.Text.RegularExpressions.Regex(separator).Split( line );// data.Split( new string[] { separator }, StringSplitOptions.None ).ToList();
			return values;
		}

		public static Collection<string> Split( string value, Predicate<char> predicate )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );
			if( predicate==null )
				throw new ArgumentNullException( "predicate" );

			var deliminatedList = new Collection<string>();
			var word = new StringBuilder();
			foreach( char ch in value.ToCharArray() )
			{
				if( predicate(ch) )
				{
					if( word.Length>0 )
					{
						deliminatedList.Add( word.ToString() );
						word.Length=0;
					}
				}
				else
					word.Append( ch );
			}
			if( word.Length>0 )
				deliminatedList.Add( word.ToString() );
			return deliminatedList;
		}

		public static Collection<string> SplitLines( string value )
		{
			if( value==null )
				throw new ArgumentNullException( "value" );

			var deliminatedList = new Collection<string>();
			var line = new StringBuilder();
			foreach( char ch in value.ToCharArray() )
			{
				if( ch=='\r' || ch=='\n' )
				{
					if( line.Length>0 )
					{
						deliminatedList.Add( line.ToString() );
						line.Length=0;
					}
				}
				else
					line.Append( ch );
			}
			if( line.Length>0 )
				deliminatedList.Add( line.ToString() );
			return deliminatedList;
		}

	}

	namespace Extensions
	{
		public static class StringCollectionExtensions
		{
			[System.Diagnostics.DebuggerStepThrough]
			public static LinkedList<string> ToLinkedList( this System.Collections.Specialized.StringCollection collection )
			{
				if( collection==null )
					throw new ArgumentNullException( "collection" );

				var list = new LinkedList<string>();
				foreach( var item in collection )
					list.AddLast( item );
				return list;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed" ), System.Diagnostics.DebuggerStepThrough]
			public static string ToDelimitedList<T>( this IEnumerable<T> list, string delimiter="," )
			{
				if( list==null || list.Count()==0 )
					throw ExceptionHelper.ParameterNullOrEmpty( "list" );
				if( string.IsNullOrEmpty(delimiter) )
					throw ExceptionHelper.ParameterNullOrEmpty( "deliminator" );

				StringBuilder deliminatedList = new StringBuilder();
				foreach( T item in list )
				{
					if( deliminatedList.Length!=0 )
						deliminatedList.Append( delimiter );
					deliminatedList.Append( item.ToString() );
				}

				return deliminatedList.ToString();
			}
		}
	}
}
