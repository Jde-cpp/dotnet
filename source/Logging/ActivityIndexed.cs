using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jde.Logging
{
	public class ActivityIndexed : Activity2
	{
		public ActivityIndexed( string name, int count, string instanceDetails=null ):
			base( name, instanceDetails )
		{
			Count = count;
		}

		public void Step()
		{
			StepStopwatch.Stop();
			var average = new TimeSpan(Stopwatch.ElapsedTicks/++Index);
			base.WriteVerboseCategory( "Step", "'{0}'/'{1}' - {2} average={3} estimate time left={4}", Index, Count, StepStopwatch.Elapsed, average, new TimeSpan(average.Ticks*(Count- Index)) );
			StepStopwatch.Restart();
		}
		public System.Diagnostics.Stopwatch StepStopwatch {get;} = System.Diagnostics.Stopwatch.StartNew();
		public int Count{get;set;}
		public int Index{get;set;}
	}
}
