using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jde.Utilities
{
	public static class TimeSpanHelper
	{
		public static TimeSpan Parse( string timeSpan )
		{
			TimeSpan? value = null;
			int? count = Number.TryParsePrefix( timeSpan, true );
			if( count!=null )
				value = Parse( (int)count, StringHelper.TryParseSuffix(timeSpan) );
			else
				throw new InvalidOperationException( string.Format(CultureInfo.InvariantCulture, "Could not parse '{0}'.", timeSpan) );
			
			return (TimeSpan)value;
		}

		public static TimeSpan Parse( int count, string timePart )
		{
			TimeSpan? value = null;
			if( string.Compare(timePart,"days", StringComparison.OrdinalIgnoreCase)==0 || string.Compare(timePart,"day", StringComparison.OrdinalIgnoreCase)==0 )
				value = TimeSpan.FromDays( count );
			else if( string.Compare(timePart,"hours", StringComparison.OrdinalIgnoreCase)==0 || string.Compare(timePart,"hour", StringComparison.OrdinalIgnoreCase)==0 )
				value = TimeSpan.FromHours( count );
			else if( string.Compare(timePart,"Milliseconds", StringComparison.OrdinalIgnoreCase)==0 || string.Compare(timePart,"Millisecond", StringComparison.OrdinalIgnoreCase)==0 )
				value = TimeSpan.FromMilliseconds( count );
			else if( string.Compare(timePart,"Minutes", StringComparison.OrdinalIgnoreCase)==0 || string.Compare(timePart,"Minute", StringComparison.OrdinalIgnoreCase)==0 )
				value = TimeSpan.FromMinutes( count );
            else if (string.Compare(timePart, "Months", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(timePart, "Month", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if( count>12 || count<0 )
                    throw new InvalidOperationException( string.Format(CultureInfo.CurrentUICulture, "'{0}' is invalid for months", count) );
                value = TimeSpan.FromDays( count * 30 );
            }
            else if (string.Compare(timePart, "Seconds", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(timePart, "Second", StringComparison.OrdinalIgnoreCase) == 0)
                value = TimeSpan.FromSeconds(count);
            else if (string.Compare(timePart, "Ticks", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(timePart, "Tick", StringComparison.OrdinalIgnoreCase) == 0)
                value = TimeSpan.FromTicks(count);
            else if (string.Compare(timePart, "weeks", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(timePart, "week", StringComparison.OrdinalIgnoreCase) == 0)
                value = TimeSpan.FromDays(count * 7);
            else if (string.Compare(timePart, "Years", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(timePart, "Year", StringComparison.OrdinalIgnoreCase) == 0)
                value = TimeSpan.FromDays(count * 365);
            else
                throw new InvalidCastException( string.Format(CultureInfo.InvariantCulture, "Could not parse '{0}'.", timePart) );
			
			return (TimeSpan)value;
		}
	}
}
