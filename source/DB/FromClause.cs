using System;
using CultureInfo=System.Globalization.CultureInfo;
using System.Collections.Generic;
using System.Linq;

namespace Jde.DB
{
	public sealed class FromClause
	{
		public FromClause( string firstTable, string secondTable, string secondJoinColumn )
		{
			FirstTable = firstTable;
			SecondTable = secondTable;
			SecondJoinColumn = secondJoinColumn;
		}
		public override string ToString()
		{
			return string.Format( CultureInfo.InvariantCulture, "{0} inner join {1} on {2}={3}", FirstTable, SecondTable, "id", SecondJoinColumn );
		}
		public string FirstTable{get;set;}
		public string SecondTable{get;set;}
		public string SecondJoinColumn{get;set;}
	}
}
