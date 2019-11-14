using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "sequence", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class Sequence : ICloneable
	{
		#region Constructors
		public Sequence(string name)
		{
			_name=name;
		}
		public Sequence() { }
		public Sequence( Table table )
		{
			Name=table.Name+"_seq";
		}
		#endregion
		#region CreateStatement
		public string CreateStatement
		{
			get{ return string.Format( CultureInfo.InvariantCulture, "create sequence {0} increment by {1} start with {2}", Name, Increment, Start); }
		}
		#endregion
		#region DropStatement
		public string DropStatement
		{
			get{ return string.Format( CultureInfo.InvariantCulture, "drop sequence {0}", Name ); }
		}
		#endregion
		#region DbLocation
		Locations  _dbLocation = Locations.ProductionArc;
		public Locations DBLocation
		{
			get{ return _dbLocation;}
			set{ _dbLocation = value;}
		}
		#endregion
		#region Increment
		int _increment=1;
		[XmlAttribute( "increment" )]
		public int Increment
		{
			get { return _increment; }
			set { _increment=value; }
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute("name")]
		public string Name
		{
			get
			{
				if( string.IsNullOrEmpty(_name) )
					_name = GetStdName();
				return _name; 
			}
			set{_name=value;}
		}
		string GetStdName()
		{
			if( Table==null )
				throw new InvalidOperationException( "Table has not been initiated" );

			string baseName = Table.BaseName();
			return baseName.ToUpper( CultureInfo.InvariantCulture );
		}
		#endregion
		#region Location
		Locations _location = Locations.Production;
		[XmlAttribute("location")]
		public Locations Location
		{
			get{ return _location;}
			set{ _location = value; }
		}
		#endregion
		#region Start
		int _start=1001;
		[XmlAttribute("start")]
		public int Start
		{
			get{ return _start;}
			set{ _start=value; }
		}
		#endregion
		#region Table
		Table _table;
		[XmlIgnore]
		public Table Table
		{
			get{return _table;}
			set{_table=value;}
		}
		#endregion
		#region ICloneable Members
		public object Clone()
		{
			return new Sequence(Name);
		}
		#endregion
	}
}
