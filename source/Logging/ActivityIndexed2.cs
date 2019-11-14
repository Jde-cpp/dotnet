using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jde.Logging
{
	public class ActivityIndexed2 : Activity2
	{
		public ActivityIndexed2( int count, Action<Guid,string,int> start, Action<Guid,string,object,int,int,TimeSpan,TimeSpan,TimeSpan> stepLog, Action<Guid,string,object,TimeSpan> end, string details=null ):
			base(null, details)
		{
			Count = count;
			StepLog = stepLog;
			End = end;
			start(Id, InstanceDetails, Count);
		}
	
		public override void Dispose()
		{
			Stopwatch.Stop();
			End( Id, InstanceDetails, Result, Stopwatch.Elapsed );
		}

		public void Step()
		{
			StepStopwatch.Stop();
			var average = new TimeSpan( Stopwatch.ElapsedTicks/++Index );
			StepLog( Id, InstanceDetails, Result, Index, Count, average, StepStopwatch.Elapsed, new TimeSpan(average.Ticks*(Count- Index)) );
			StepStopwatch.Restart();
		}
		public System.Diagnostics.Stopwatch StepStopwatch {get;} = System.Diagnostics.Stopwatch.StartNew();
		//protected System.Diagnostics.Stopwatch Stopwatch {get;}= System.Diagnostics.Stopwatch.StartNew();
		public int Count{get;set;}
		public int Index{get;set;}
		//public object Result{get;set;}

		Action<Guid,string,object,int,int,TimeSpan,TimeSpan,TimeSpan> StepLog{get;set;}
		Action<Guid,string,object,TimeSpan> End {get;set; }
	}
}
