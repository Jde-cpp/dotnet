using System;
using System.Collections.Generic;
using CultureInfo=System.Globalization.CultureInfo;
using System.Linq;
using System.Text;

namespace Jde.Utilities
{
	public static class DateHelper
	{
		public static DateTime? ParseEmpty( string date, string format )
		{
			return string.IsNullOrEmpty(date) ? null : (DateTime?)DateTime.ParseExact( date, format, CultureInfo.InvariantCulture );
		}
	}
}
