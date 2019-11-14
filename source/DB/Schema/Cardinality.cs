using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	public enum Cardinality
	{
		/// <summary>0:0 person:parking space</summary>
		[XmlEnum("link")]
      Link,
		/// <summary>1:0 person:programmer</summary>
		[XmlEnum("sub_type")]
		SubType,
		/// <summary>1:1 person:dna pattern</summary>
		[XmlEnum("physical_segment")]
		PhysicalSegment,
		/// <summary>0:M person:phone#</summary>
		[XmlEnum("possession")]
		Possession,
		/// <summary>1:M user:user group</summary>
		[XmlEnum("child")]
		Child,
		/// <summary>1:M user:user group</summary>
		[XmlEnum("characteristic")]
		Characteristic,
		/// <summary>1:M on both sides citizenship:person</summary>
		[XmlEnum("paradox")]
		Paradox,
		/// <summary>M:M people:Employers with no link</summary>
		[XmlEnum("association")]
		Association
	}

	public static class CardinalityExtensions
	{
		public static bool SingularForeignKey( this Cardinality cardinality )
		{
			return cardinality==Cardinality.Link || cardinality==Cardinality.SubType;
		}
		public static string ToString( this Cardinality cardinality )
		{
			string result = null;
			switch( cardinality )
			{
			case Cardinality.Association:
				result = "association";
				break;
			case Cardinality.Characteristic:
				result = "characteristic";
				break;
			case Cardinality.Child:
				result = "child";
				break;
			case Cardinality.Link:
				result = "link";
				break;
			case Cardinality.Paradox:
				result = "paradox";
				break;
			case Cardinality.PhysicalSegment:
				result = "physical_segment";
				break;
			case Cardinality.Possession:
				result = "possession";
				break;
			case Cardinality.SubType:
				result = "sub_type";
				break;
			default:
				throw new InvalidCastException( string.Format(CultureInfo.InvariantCulture, "Could not parse '{0}'.", cardinality) );
			}
			return result;
		}

		public static Cardinality Parse( string name )
		{
			Cardinality? cardinality = null;
			if( name=="link" )
				cardinality = Cardinality.Link;
			else if( name == "sub_type" )
				cardinality = Cardinality.SubType;
			else if( name == "physical_segment" )
				cardinality = Cardinality.PhysicalSegment;
			else if( name == "possession" )
				cardinality = Cardinality.Possession;
			else if( name == "child" )
				cardinality = Cardinality.Child;
			else if( name == "characteristic" )
				cardinality = Cardinality.Characteristic;
			else if( name == "paradox" )
				cardinality = Cardinality.Paradox;
			else if( name == "association" )
				cardinality = Cardinality.Association;
			else
				throw new InvalidCastException( string.Format(CultureInfo.InvariantCulture, "Could not parse '{0}'.", name) );

			return cardinality.Value;
		}
	}
}