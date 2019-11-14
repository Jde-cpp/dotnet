using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jde.Utilities
{
	public static class TimeZoneInfoHelper
	{

		public static TimeZoneInfo EasternStandardTime
		{
			get{ return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); }
		}
		public static TimeZoneInfo CentralStandardTime
		{
			get{ return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); }
		}
	}
}
