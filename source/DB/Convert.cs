using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jde.DB
{
	public static class DBConvert
	{
		public static int? ToInt( object value )
		{
			return Convert.IsDBNull(value) ? null : (int?)Convert.ToInt32( value );
		}

		public static decimal? ToDecimal( object value )
		{
			return Convert.IsDBNull(value) ? null : (decimal?)Convert.ToDecimal( value );
		}
	}
}
