using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[Flags]
	public enum Locations
	{
		[XmlEnum("none")]
		None = 0x0,
		[XmlEnum("production")]
		Production = 0x1,
		[XmlEnum("archive")]
		Archive = 0x2, 
		[XmlEnum("production_archive")]
		ProductionArc = 0x3
	}

	public static class Location
	{
		public static Locations Parse( string locationName )
		{
			return (Locations)Enum.Parse( typeof(Locations), locationName, true );
		}

		public static Locations Read( System.Xml.XmlReader reader, Locations dflt )
		{
			string location = reader.GetAttribute( "location" );
			return string.IsNullOrEmpty(location) ? dflt : Parse( location );
		}

		public static void Write( System.Xml.XmlWriter writer, Locations locations )
		{
			writer.WriteAttributeString( "location", locations.ToString() );
		}
		
	}
}
