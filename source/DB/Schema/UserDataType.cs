using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "data_type", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class UserDataType
	{
	   #region Constructors
	   public UserDataType() { }
	   public UserDataType( DataType dataType )
	   {
	      DataType=dataType;
	      Name=dataType.ToString();
	   }
	   #endregion
	   #region Name
	   string  _name=string.Empty;
	   [XmlAttribute("name")]
	   public string Name
	   {
	      get{ return _name;}
	      set{ _name=value; }
	   }
	   #endregion
	   #region Type
	   DataType	_type;
	   [XmlAttribute("type")]
	   public DataType DataType
	   {
	      get{return _type;}
	      set{_type=value;}
	   }
	   #endregion
	};
}
