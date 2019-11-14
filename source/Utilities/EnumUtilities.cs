using System;
using System.Collections.Generic;
using CultureInfo = System.Globalization.CultureInfo;
using System.Linq;
using System.Text;

namespace Jde.Utilities
{
	using Extensions;

	public static class EnumUtilities
	{
		public static string ToPipeDelimited( Type enumType, Enum enumeration )
		{
			if( Convert.ToInt64(enumeration, CultureInfo.InvariantCulture)==0 )
				return string.Empty;

			var values = Enum.GetValues( enumType );
			var names = new LinkedList<string>();
			var enumValue = Convert.ToUInt64( enumeration, CultureInfo.InvariantCulture );
			foreach( object value in values )
			{
				var longObject = Convert.ToUInt64( value, CultureInfo.InvariantCulture );
				if( longObject==0 && enumValue==longObject )
				{
					names.AddLast( Enum.GetName(enumType,value) );
					break;
				}
				if( longObject!=0 && (enumValue & longObject)==longObject )
					names.AddLast( Enum.GetName(enumType, value) );
			}

			return names.ToDelimitedList( "|" );
		}
	}
}
