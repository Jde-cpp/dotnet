using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jde.Logging
{
	[Serializable]
	public class Activity2 : IDisposable
	{
		public Activity2()
		{
			Id = Guid.NewGuid();
			Parent = Current;
			Current=this;
			if( Parent!=null )
				Parent.Children.AddLast( Current );

		}
		public Activity2( string name, string instanceDetails=null )
		{
			Name = name;
			InstanceDetails = instanceDetails;
			Id = Guid.NewGuid();
			WriteVerboseCategory( "Starting", InstanceDetails );
		}
		
		public virtual void Dispose()
		{
			Stopwatch.Stop();
			Current = Parent;
			WriteVerboseCategory( "Ending", Result==null ? "{0} - {2}" : "{0} - {1} - {2}", InstanceDetails, Result, Stopwatch.Elapsed );
		}
		public void Write(LogException e)
		{
			if( e.TraceEventType>System.Diagnostics.TraceEventType.Verbose )
				Exceptions.AddLast( e );
			//Jde.Log.Default.Write(e);
		}
		public void WriteError(Exception e){ Exceptions.AddLast( e ); Jde.Log.Default.WriteError(e); }
		public void WriteError(Exception e, string message, params object[] args ){ Exceptions.AddLast( e ); Jde.Log.Default.WriteError(e, message, args); }
		public void Warning( string msg, params object[] args ){ Jde.Log.Default.WriteWarning( msg, args ); }
		public static void WriteWarning( string msg, params object[] args ){ if( Current!=null )Current.Warning( msg, args ); else Jde.Log.Default.WriteWarning( msg, args ); }

		public void WriteVerbose( string msg, params object[] args )
		{
			if( Log.ShouldLog(Name, System.Diagnostics.SourceLevels.Verbose) )
				Write( ()=>Log.WriteVerbose( Name, msg ?? string.Empty, args ) );
		}
		public void WriteVerboseCategory( string subCategory, string msg, params object[] args )
		{
			var category = string.Format("{0}-{1}", Name,subCategory);
			if( Log.ShouldLog(category, System.Diagnostics.SourceLevels.Verbose) )
				Write( ()=>Jde.Logging.Log.WriteVerbose( category, msg ?? string.Empty, args) );
		}
		public static void Write( Action action )
		{
			if( action==null )
				throw new ArgumentNullException("action");
			if( !Async )
				action();
			else
			{
				LogEntries.Enqueue( action );
				if( Thread==null )
				{
					lock( ThreadSync )
					{
						if( Thread==null )
						{
							Thread = new System.Threading.Thread(ExecuteThread);
							Thread.Start();
						}
					}
				}
				if( LogEntries.Count>20 )
				{
					while( LogEntries.Count>0 )
						System.Threading.Thread.Sleep( 1 );
				}
			}
		}
		public static void ExecuteThread()
		{
			var index = 0;
			while( true )
			{
				Action action;
				bool success = LogEntries.TryDequeue( out action );
				if( success )
				{
					action();
					index = 0;
				}
				else
				{
					if( ++index > 10 )
					{
						lock( ThreadSync )
						{
							if( LogEntries.Count==0 )
								Thread = null;
							break;
						}
					}
					else
						System.Threading.Thread.Sleep( 1 );
				}
			}
		}
		#region Id
		[System.Runtime.Serialization.DataMember]
		public Guid Id {get;set;}
		#endregion
		static bool Async {get;set;}=true;
		#region Children
		Lazy<LinkedList<Activity2>> _children = new Lazy<LinkedList<Activity2>>( ()=>new LinkedList<Activity2>() );
		[System.Runtime.Serialization.DataMember]
		public LinkedList<Activity2> Children{get {return _children.Value;} }
		#endregion
		#region Current
		[System.ThreadStatic]
		static Activity2 _current;
		public static Activity2 Current {get {return _current; } private set{_current=value; } }
		#endregion
		#region Exceptions
		[System.Runtime.Serialization.DataMember]
		public LinkedList<Exception> Exceptions {get;} = new LinkedList<Exception>();
		#endregion
		#region InstanceDetails
		[System.Runtime.Serialization.DataMember]
		public string InstanceDetails {get;set;}
		#endregion
		#region Name
		[System.Runtime.Serialization.DataMember]
		public string Name{get;set;}
		#endregion
		static System.Collections.Concurrent.ConcurrentQueue<Action> LogEntries {get; }= new System.Collections.Concurrent.ConcurrentQueue<Action>();
		static System.Threading.Thread _thread;
		static System.Threading.Thread Thread {get {return _thread; } set {_thread=value; } }
		static object ThreadSync {get;} = new object();
		#region Parent
		//[System.ThreadStatic]
		Activity2 _parent;
		public Activity2 Parent{get {return _parent; } set{_parent=value; } }
		#endregion
		public object Result {get;set;}
		protected System.Diagnostics.Stopwatch Stopwatch {get; }= System.Diagnostics.Stopwatch.StartNew();
	}
}
