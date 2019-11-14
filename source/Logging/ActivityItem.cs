using System;
using System.Collections.Generic;
using CultureInfo=System.Globalization.CultureInfo;
using System.Linq;

namespace Jde.Logging
{
	public class ActivityItem
	{
		public ActivityItem( ActivityItemTypes type )
		{
			Type = type;
		}
		public ActivityItem( ActivityItemTypes type, int count ):
			this( type )
		{
			Count = count;
		}
		public ActivityItem( Exception e ):
			this( ActivityItemTypes.Error )
		{
			if( e==null )
				throw new ArgumentNullException( "e" );
			Descriptions = new LinkedList<string>();
			Descriptions.AddLast( e.ToString() );
		}
		//public ActivityItem( string value, params object[] args )
		//{
		//   Description = string.Format( CultureInfo.CurrentCulture, value, args );
		//}
		[System.Runtime.Serialization.DataMember]
		public int Count{get;set;}
		[System.Runtime.Serialization.DataMember]
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods" )]
		public ActivityItemTypes Type{get;set;}
		
		LinkedList<string> _descriptions;
		[System.Runtime.Serialization.DataMember]
		public LinkedList<string> Descriptions
		{
			get{ return _descriptions;}
			private set{ _descriptions = value; }
		}
		
		public void Add( ActivityItem addItem )
		{
			if( addItem==null )
				throw new ArgumentNullException( "addItem" );
			Count+=addItem.Count;
			if( addItem.Descriptions!=null )
			{
				if( Descriptions==null )
					Descriptions = new LinkedList<string>();
				foreach( var description in addItem.Descriptions )
					Descriptions.AddLast( description );
			}
		}
	}
	[Flags, Serializable]
	public enum ActivityItemTypes
	{
		None=0,
		Retrieve=0x2,
		Insert=0x4,
		Update=0x8,
		Delete=0x10,
		Error=0x20,
		NotFound=0x40,
		Multiple=0x80,
		Upload=0x100
	}
}
