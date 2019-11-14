using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jde.DB.Schema
{
	public enum Serialization
	{
		Attribute=0,
		Element=1,
		Text=2,
	}
	namespace Extensions
	{
		public static class SerializationExtensions
		{
			public static Serialization Parse( string value )
			{
				var serialization = Serialization.Attribute;
				if( value=="attribute" )
					serialization = Serialization.Attribute;
				else if( value=="element" )
					serialization = Serialization.Element;
				else if( value=="text" )
					serialization = Serialization.Text;
				else
					throw new System.ComponentModel.InvalidEnumArgumentException( value );
				
				return serialization;
			}
			public static string ToString( this Serialization serialization )
			{
				string text = null;
				switch( serialization )
				{
				case Serialization.Attribute:
					text = "attribute";
					break;
				case Serialization.Element:
					text = "element";
					break;
				case Serialization.Text:
					text = "text";
					break;
				default:
					throw new System.ComponentModel.InvalidEnumArgumentException( serialization.ToString() );
				}
				return text;
			}
		}
	}
}
