using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jde.Facade
{
	public static class Culture
	{
		public static int CompatibleLcid( int lcid )
		{
			return CultureInfo.GetCultureInfo(lcid).LCID;
		}
	}
}
