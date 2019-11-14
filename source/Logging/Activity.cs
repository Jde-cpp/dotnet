using System;
using System.Collections.Generic;
using CultureInfo=System.Globalization.CultureInfo;
using System.Linq;

namespace Jde.Logging
{
	using Collections.Extensions;
	[Serializable]
	public class Activity //: System.Xml.Serialization.IXmlSerializable
	{
		#region Constructors
		public Activity()
		{
		}

		public Activity( string name ):
			this()
		{
			Name = name;
		}
		public Activity( string name, string instanceDetails ):
			this(name)
		{
			InstanceDetails= instanceDetails;
		}

/*		public Activity( string name, System.ServiceModel.FaultException e ):
			this( name )
		{
			Exception = e;
		}
		*/
		public Activity( string name, int count ):
			this( name )
		{
			Total=count;
		}

		public Activity( string name, ActivityItemTypes type, int count ):
			this( name )
		{
			this[type].Count=count;
		}

		public Activity( string name, ActivityItemTypes type, string value ):
			this( name )
		{
			this[type].Descriptions.AddLast( value );
		}

		//public Activity( string name, ActivityItemTypes type, string description ):
		//	this( name )
		//{
		//	this.ActivityDescription = description;
		//}

		public Activity( string name, /*ActivityItemTypes type, string description,*/ Activity subActivity ):
			this( name )
		{
			Add( subActivity );
		}
		#endregion
		#region ToString
		public override string ToString()
		{
			return string.Format( CultureInfo.InvariantCulture, "{0}-{1}", Name, ActivityDescription );
		}
		#endregion
		#region IXmlSerializable
		//System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
		//{
		//   return null;
		//}

		//void System.Xml.Serialization.IXmlSerializable.ReadXml( System.Xml.XmlReader reader )
		//{
		//   Name = reader.GetAttribute( "name" );
		//   var attributeValue = reader.GetAttribute("activity_type");
		//   if( !string.IsNullOrEmpty(attributeValue) )
		//   {
		//      var value = ActivityTypes.None;
		//      if( ActivityTypes.TryParse(attributeValue, out value) )
		//         ActivityType = value;
		//   }
		//   ActivityDescription = reader.GetAttribute( "activity_description" );
		//   attributeValue = reader.GetAttribute("total");
		//   if( !string.IsNullOrEmpty(attributeValue) )
		//   {
		//      var value = 0;
		//      if( int.TryParse( attributeValue, out value) )
		//         Total = value;
		//   }
		//   Xml.XmlSerialization.ReadElements( reader, ReadElement );
		//}
		
		//bool ReadElement( System.Xml.XmlReader reader )
		//{
		//   if( reader.LocalName=="exception" )
		//   {
				

		//}

		//void System.Xml.Serialization.IXmlSerializable.WriteXml( System.Xml.XmlWriter writer )
		//{
		//   throw new NotImplementedException();
		//}
		#endregion
		#region Add
		public void Add( ActivityItemTypes type, int count )
		{
			this[type].Count+=count;
			Total+=count;
		}
		public void Add( Activity activity )
		{
			var existing = SubActivities.FirstOrDefault( sub => sub.Name==activity.Name );
			if( existing==null )
				SubActivities.AddLast( activity );
			else
			{
				foreach( var sub in activity.ActivityItems )
					existing[sub.Type].Add( sub );
			}
		}
		public void Add( Exception e )
		{
			Exceptions.AddLast( e );
		}
		#endregion
		public ActivityItem this[ActivityItemTypes index]
		{
			get
			{ 
				var activityItem = ActivityItems.FirstOrDefault( item =>item.Type==index );
				if( activityItem==null )
					activityItem = new ActivityItem( index );
				
				return activityItem;
			}
			set
			{  
				var item = this[index];
				if( value==null )
					throw new ArgumentNullException( "value" );

				item.Count = value.Count;
			}
		}
		//#region WriteEntry
		//public void WriteEntry( string value, params object[] args )
		//{
		//   Description = string.Format( CultureInfo.CurrentCulture, value, args );
		//}
		//public void WriteEntry( string value )
		//{
		//   Description = value;
		//}
		//#endregion
		#region Exceptions
		[System.Runtime.Serialization.DataMember]
		public LinkedList<Exception> Exceptions {get;} = new LinkedList<Exception>();
		#endregion
		#region Id
		[System.Runtime.Serialization.DataMember]
		public Guid Id {get;set;}
		#endregion
		#region Name
		[System.Runtime.Serialization.DataMember]
		public string Name{get;set;}
		#endregion
		[System.Runtime.Serialization.DataMember]
		public ActivityTypes ActivityType{get;set;}
		[System.Runtime.Serialization.DataMember]
		public string ActivityDescription{get;set;}
		#region InstanceDetails
		[System.Runtime.Serialization.DataMember]
		public string InstanceDetails {get;set;}
		#endregion
		[System.Runtime.Serialization.DataMember]
		public int Total{get;set;}
		[System.Runtime.Serialization.DataMember]
		/*public System.ServiceModel.FaultException Exception{get;set;}*/
		#region ActivityItems
		LinkedList<ActivityItem> _activityItems = new LinkedList<ActivityItem>();
		[System.Runtime.Serialization.DataMember]
		public LinkedList<ActivityItem> ActivityItems
		{
			get{ return _activityItems;}
			//private set { _activityItems = value; }
		}
		#endregion

		#region SubActivites
		LinkedList<Activity> _subActivites = new LinkedList<Activity>();
		[System.Runtime.Serialization.DataMember]
		public LinkedList<Activity> SubActivities
		{
			get{ return _subActivites;}
			//private set { _subActivites = value; }
		}
		#endregion
	}
	[Flags, Serializable]
	public enum ActivityTypes
	{
		None=0x0,
		NoChanges=0x1
	}
}
