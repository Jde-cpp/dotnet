using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "item", IsNullable=false, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class EnumItem
	{
		#region Object Members
		public override string ToString()
		{
			return ToString( false );
		}
		public string ToString( bool hex )
		{
			return string.Format( hex ? "{0}=0x{1}" : "{0}={1}", Name, Value.ToString( hex ? "x" : string.Empty, CultureInfo.CurrentUICulture) );
		}
		#endregion
		#region Description
		string _description=string.Empty;
		[XmlAttribute( "description" )]
		public string Description
		{
			get { return _description; }
			set { _description=value; }
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute( "name" )]
		public string Name
		{
			get { return _name; }
			set { _name=value; }
		}
		#endregion
		#region Value
		long _value=long.MinValue;
		[XmlIgnore]
		public long Value
		{
			get { return _value; }
			set { _value=value; }
		}
		[XmlAttribute( "value" )]
		public string StringValue
		{
			get { return _value.ToString(CultureInfo.InvariantCulture); }
			set
			{
				NumberStyles styles = NumberStyles.None;
				string stringValue = value;
				if( stringValue.Length>2 && string.Compare(value.Substring(0, 2), "0x", StringComparison.OrdinalIgnoreCase)==0 )
				{
					styles |= NumberStyles.HexNumber;
					stringValue = stringValue.Substring(2);
				}
				try
				{
					_value = int.Parse( stringValue, styles, CultureInfo.InvariantCulture );
				}
				catch( Exception e )
				{
					throw new InvalidCastException( string.Format(CultureInfo.InvariantCulture, "Could not parse '{0}'.", stringValue), e );
				}
			}
		}
		#endregion
		#region Aliases
		Collection<Alias> _aliases = new Collection<Alias>();
		[XmlElement("alias")]
		public Collection<Alias> Aliases
		{
			get{ return _aliases;}
			set{ _aliases = value; }
		}
		#endregion
	}
}
