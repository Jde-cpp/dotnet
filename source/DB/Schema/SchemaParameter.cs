using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	public enum ParameterDirection
	{
		[XmlEnum("none")]
		None=0,
		[XmlEnum( "output" )]
		Output=1,
		[XmlEnum( "input" )]
		Input=2
	};

	[XmlRoot( "parameter", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class SchemaParameter
	{
		#region Constructors
		public SchemaParameter() { }
		public SchemaParameter( int sequence, Column column )
		{
			Column=column;
			Direction = column.Sequence!=null ? ParameterDirection.Output : ParameterDirection.Input;
			Sequence=sequence;
			UserDataType = new UserDataType(column.DataType);
		}
		#endregion
		#region Column
		Column _column;
		[XmlElement( "column" )]
		public Column Column
		{
			get { return _column; }
			set { _column=value; }
		}
		#endregion
		#region Direction
		ParameterDirection _direction;
		[XmlAttribute( "direction" )]
		public ParameterDirection Direction
		{
			get { return _direction; }
			set { _direction=value; }
		}
		#endregion
		#region Max
		int _max=1;
		[XmlAttribute( "max" )]
		public int Max
		{
			get { return _max; }
			set { _max=value; }
		}
		#endregion
		#region Min
		int _min=1;
		public int Min
		{
			get { return _min; }
			set { _min=value; }
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute("name")]
		public string Name
		{
			get{return string.IsNullOrEmpty(_name) && Column!=null ? Column.Name : _name;}
			set{_name=value;}
		}
		#endregion
#if Unused
		#region Package
		Package _package;
		public Package Package
		{
			get { return _package; }
			set { _package=value; }
		}
		#endregion
#endif
		#region Sequence
		int _sequence;
		[XmlAttribute( "sequence" )]
		public int Sequence
		{
			get { return _sequence; }
			set { _sequence=value; }
		}
		#endregion
		#region Table
		Table _table;
		[XmlElement( "table" )]
		public Table Table
		{
			get { return _table; }
			set { _table=value; }
		}
		#endregion
		#region UserDataType
		UserDataType _userDataType;
		[XmlElement( "user_type" )]
		public UserDataType UserDataType
		{
		   get { return _userDataType; }
		   set { _userDataType=value; }
		}
		#endregion
	};
}
