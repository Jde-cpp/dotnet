using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB
{
	public enum DataType
	{
		[XmlEnum("none")]
		None,
		[XmlEnum("int16")]
		Int16,
		[XmlEnum("int")]
		Int,
		[XmlEnum( "uint" )]
		UInt,
		[XmlEnum( "float4" )]
		SmallFloat,
		[XmlEnum("float")]
		Float,
		[XmlEnum("bit")]
		Bit,
		[XmlEnum("decimal")]
		Decimal,
		[XmlEnum("int8")]
		Int8,
		[XmlEnum("long")]
		Long,
		[XmlEnum( "ulong" )]
		ULong,
		[XmlEnum("guid")]
		Guid,
		[XmlEnum("binary")]
		Binary,
		[XmlEnum("vbinary")]
		VarBinary,
		[XmlEnum("vtchar")]
		VarTChar,
		[XmlEnum("vwchar")]
		VarWChar,
		[XmlEnum("numeric")]
		Numeric,
		[XmlEnum("date_time")]
		DateTime,
		[XmlEnum("cursor")]
		Cursor,
		[XmlEnum("tchar")]
		TChar,
		[XmlEnum("vchar")]
		VarChar,
		[XmlEnum("ref_cursor")]
		RefCursor,
		[XmlEnum("small_date_time")]
		SmallDateTime,
		[XmlEnum("wchar")]
		WChar,
		[XmlEnum("ntext")]
		NText,
		[XmlEnum("text")]
		Text,
		[XmlEnum("image")]
		Image,
		[XmlEnum("blob")]
		Blob,
		[XmlEnum("money")]
		Money,
		[XmlEnum("char")]
		Char,
		[XmlEnum( "time_span" )]
		TimeSpan,
		[XmlEnum( "uri" )]
		Uri,
		[XmlEnum("uint16")]
		UInt16,
		[XmlEnum("uint8")]
		UInt8,
	};

	public static class DataTypes
	{
		public static bool HasLength( DataType type )
		{
			return type == DataType.VarChar || type == DataType.TChar || type == DataType.VarWChar || type == DataType.VarTChar || type == DataType.WChar || type == DataType.Char || type==DataType.Uri || type==DataType.Binary;
		}
		/// <summary>The datatype is effected by the unicode setting.</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool UnicodeEffected( DataType type )
		{
			return type==DataType.TChar || type== DataType.VarTChar;
		}
		
		public static DataType Parse( string dataTypeName )
		{
			DataType dataType = DataType.None;
			if( dataTypeName=="none" )
				dataType = DataType.None;
			else if( dataTypeName=="int16")
				dataType = DataType.Int16;
			else if (dataTypeName == "uint8")
				dataType = DataType.UInt8;
			else if (dataTypeName == "uint16")
				dataType = DataType.UInt16;
			else if ( dataTypeName=="int")
				dataType = DataType.Int;
			else if( dataTypeName=="uint" )
				dataType = DataType.UInt;
			else if( dataTypeName=="float4" )
				dataType = DataType.SmallFloat;
			else if( dataTypeName=="float")
				dataType = DataType.Float;
			else if( dataTypeName=="bit")
				dataType = DataType.Bit;
			else if( dataTypeName=="decimal")
				dataType = DataType.Decimal;
			else if( dataTypeName=="int8")
				dataType = DataType.Int8;
			else if( dataTypeName=="long")
				dataType = DataType.Long;
			else if( dataTypeName=="ulong" )
				dataType = DataType.ULong;
			else if( dataTypeName=="guid")
				dataType = DataType.Guid;
			else if( dataTypeName=="vbinary")
				dataType = DataType.VarBinary;
			else if( dataTypeName=="vtchar")
				dataType = DataType.VarTChar;
			else if( dataTypeName=="vwchar")
				dataType = DataType.VarWChar;
			else if( dataTypeName=="numeric")
				dataType = DataType.Numeric;
			else if( dataTypeName=="date_time")
				dataType = DataType.DateTime;
			else if( dataTypeName=="cursor")
				dataType = DataType.Cursor;
			else if( dataTypeName=="tchar")
				dataType = DataType.TChar;
			else if( dataTypeName=="vchar")
				dataType = DataType.VarChar;
			else if( dataTypeName=="ref_cursor")
				dataType = DataType.RefCursor;
			else if( dataTypeName=="small_date_time")
				dataType = DataType.SmallDateTime;
			else if( dataTypeName=="wchar")
				dataType = DataType.WChar;
			else if( dataTypeName=="ntext")
				dataType = DataType.NText;
			else if( dataTypeName=="text")
				dataType = DataType.Text;
			else if( dataTypeName=="image")
				dataType = DataType.Image;
			else if( dataTypeName=="blob")
				dataType = DataType.Blob;
			else if( dataTypeName=="money")
				dataType = DataType.Money;
			else if( dataTypeName=="char")
				dataType = DataType.Char;
			else if( dataTypeName=="time_span" )
				dataType = DataType.TimeSpan;
			else if( dataTypeName=="uri" )
				dataType = DataType.Uri;
			else
				throw new NotImplementedException( string.Format(CultureInfo.InvariantCulture, "could not parse type '{0}'.", dataTypeName) );

			return dataType;
		}
	};
}