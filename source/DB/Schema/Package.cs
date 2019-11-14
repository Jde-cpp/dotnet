using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
#if Unused
	[XmlRoot( "package", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class Package
	{
		#region Constructors
		public Package(string name)
		{
			Name=name;
		}
		public Package() { }
		#endregion
		#region DataTypes
		Collection<UserDataType> _dataTypes;
		[XmlElement("data_types")]
		public Collection<UserDataType> DataTypes
		{
			get{ return _dataTypes;}
			//private set{ _dataTypes=value;}
		}
		#endregion
		#region Name
		string	_name;
		[XmlAttribute("name")]
		public string Name
		{ 
			get{ return _name;}
			set{ _name=value; }
		}
		#endregion
		#region Procedures
		Collection<Procedure> _procedures;
		[XmlElement("procedure")]
		public Collection<Procedure> Procedures
		{
			get{ return _procedures;}
			//private set{ _procedures=value;}
		}
		#endregion
		#region Table
		Table _table;
		public Table Table
		{
			get{return _table;}
			set{_table=value;}
		}
		#endregion
	};
#endif
}
