using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jde.Utilities
{
	public static class Number
	{
		public static int? TryParsePrefix( string number, bool allowTextRepresentation )
		{
			string prefix = StringHelper.TryParsePrefix( number );

			int result = int.MaxValue;
			if( !int.TryParse(prefix, out result) && allowTextRepresentation )
			{
				if( string.Compare(prefix, "one", StringComparison.OrdinalIgnoreCase)==0 )
					result = 1;
				else if( string.Compare(prefix,"two", StringComparison.OrdinalIgnoreCase)==0 )
					result = 2;
				else if( string.Compare(prefix,"three", StringComparison.OrdinalIgnoreCase)==0 )
					result = 3;
				else if( string.Compare(prefix,"four", StringComparison.OrdinalIgnoreCase)==0 )
					result = 4;
				else if( string.Compare(prefix,"five", StringComparison.OrdinalIgnoreCase)==0 )
					result = 5;
				else if( string.Compare(prefix,"six", StringComparison.OrdinalIgnoreCase)==0 )
					result = 6;
				else if( string.Compare(prefix,"seven", StringComparison.OrdinalIgnoreCase)==0 )
					result = 7;
				else if( string.Compare(prefix,"eight", StringComparison.OrdinalIgnoreCase)==0 )
					result = 8;
				else if( string.Compare(prefix,"nine", StringComparison.OrdinalIgnoreCase)==0 )
					result = 9;
				else if( string.Compare(prefix,"ten", StringComparison.OrdinalIgnoreCase)==0 )
					result = 10;
				else if( string.Compare(prefix,"elevin", StringComparison.OrdinalIgnoreCase)==0 )
					result = 11;
				else if( string.Compare(prefix,"twelve", StringComparison.OrdinalIgnoreCase)==0 )
					result = 12;
				else
					result = int.MaxValue;
			}
			return result==int.MaxValue ? null : (int?)result;
		}

		/// <summary>Returns the first number in a string.</summary>
		/// <param name="number">The string to extract the first number.</param>
		/// <returns>The string representation of the first number, string.Empty if no number exists.</returns>
		/// <example>"text 1" returns "1". "555 alpha" returns 555.  "test" returns string.Empty.</example>
		public static string GetFirstNumber( string number )
		{
			if( number==null )
				throw new ArgumentNullException( "number" );
			int iChar=0;
			for( ; iChar<number.Length; ++iChar )
			{
				if( Char.IsDigit(number[iChar]) )
					break;
			}
			
			return iChar<number.Length ? StringHelper.TryParsePrefix(number.Substring(iChar)) : string.Empty;
		}
		
		public static string ExtractNonnumeric( string partialNumber )
		{
			if( partialNumber==null )
				throw new ArgumentNullException( "partialNumber" );
			var nonNumeric = new StringBuilder();
			int iChar=0;
			for( ; iChar<partialNumber.Length; ++iChar )
			{
				char character = partialNumber[iChar];
				if( !IsNumber(character) )
					nonNumeric.Append( character );
				else
					break;
			}
			while( iChar<partialNumber.Length )
			{
				if( IsNumber(partialNumber[iChar]) )
					++iChar;
				else
					break;
			}

			for( ; iChar<partialNumber.Length; ++iChar )
			{
				char character = partialNumber[iChar];
				nonNumeric.Append( character );
			}
			
			return nonNumeric.ToString();
		}
		static bool IsNumber( char ch )
		{
			return	Char.IsDigit(ch) 
					||	ch==CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0]
					||	ch==CultureInfo.InvariantCulture.NumberFormat.NegativeSign.ToCharArray()[0];
		}

		public static decimal? ParseEmptyDecimal( string value )
		{
			return string.IsNullOrEmpty( value ) ? null : (decimal?)decimal.Parse( value, CultureInfo.InvariantCulture );
		}

		public static int? ParseEmptyInt( string value )
		{
			return string.IsNullOrEmpty( value ) ? null : (int?)int.Parse( value, CultureInfo.InvariantCulture );
		}

		public static double? ParseEmptyDouble( string value )
		{
			return string.IsNullOrEmpty( value ) ? null : (double?)double.Parse( value, CultureInfo.InvariantCulture );
		}

		public static decimal ParseDecimal( string value, string errorSuffix=null, params object[] args )
		{
			decimal result;
			if( !decimal.TryParse(value, out result) )
			{
				string parseError = string.Format(CultureInfo.InvariantCulture, "could not parse '{0}' into a decimal.", value==null ? "{null}" : value ); 
				var error = string.IsNullOrEmpty(errorSuffix) ? parseError : string.Format(CultureInfo.InvariantCulture, "{0}  {1}", parseError, string.Format(CultureInfo.CurrentCulture, errorSuffix, args));
				throw new InvalidCastException( error );
			}
			return result;
		}

		public static long ParseLong( string value, string errorSuffix=null, params object[] args )
		{
			long result;
			if( !long.TryParse(value, out result) )
			{
				string parseError = string.Format(CultureInfo.InvariantCulture, "could not parse '{0}' into a long.", value==null ? "{null}" : value ); 
				var error = string.IsNullOrEmpty(errorSuffix) ? parseError : string.Format(CultureInfo.InvariantCulture, "{0}  {1}", parseError, string.Format(CultureInfo.CurrentCulture, errorSuffix, args));
				throw new InvalidCastException( error );
			}
			return result;
		}
		public static ulong ParseULong( string value, string errorSuffix=null, params object[] args )
		{
			ulong result;
			if( !ulong.TryParse(value, out result) )
			{
				string parseError = string.Format(CultureInfo.InvariantCulture, "could not parse '{0}' into a ulong.", value==null ? "{null}" : value ); 
				var error = string.IsNullOrEmpty(errorSuffix) ? parseError : string.Format(CultureInfo.InvariantCulture, "{0}  {1}", parseError, string.Format(CultureInfo.CurrentCulture, errorSuffix, args));
				throw new InvalidCastException( error );
			}
			return result;
		}

	}
}
