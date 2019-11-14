using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "item", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	public class DataItem
	{
		#region Constructors
		public DataItem( Column column, string value, bool required )
		{
			Column=column;
			StringValue=value;
			Required=required;
		}
		public DataItem(){ }
		#endregion
		#region Column
		Column _column;
		[XmlElement("column")]
		public Column Column
		{
			get{ return _column;}
			set{ _column=value; }
		}
		#endregion
		#region Value
		string _value=string.Empty;
		[XmlAttribute("value")]
		public string StringValue
		{
			get{ return _value; }
			set{ _value = value; }
		}
		public ulong? ULongValue()
		{
			ulong? value = null;
			switch( Column.DataType )
			{
			case DataType.Int8:
			case DataType.Int:
			case DataType.Long:
			case DataType.ULong:
			case DataType.UInt:
				NumberStyles styles = NumberStyles.None;
				string stringValue = StringValue;
				if( stringValue.Length>2 && string.Compare(stringValue.Substring(0, 2), "0x", StringComparison.OrdinalIgnoreCase)==0 )
				{
					styles |= NumberStyles.HexNumber;
					stringValue = stringValue.Substring(2);
				}
				value = string.IsNullOrEmpty( StringValue ) ? null : (ulong?)ulong.Parse( stringValue, styles, CultureInfo.InvariantCulture );
				break;
			default:
				throw new NotSupportedException( string.Format(CultureInfo.InvariantCulture, "can not turn column type '{0}' for column '{1}' into an int", Column.DataType, Column.Name) );
			}
			return value;
		}
		
		public System.Data.Common.DbParameter GetParameter( SqlSyntax syntax )
		{
			System.Data.Common.DbParameter dbParameter = null;
			switch( Column.DataType )
			{
			case DataType.Bit:
				dbParameter = new Jde.DB.Parameter<bool>( syntax, Column.Name, bool.Parse(StringValue) );
				break;
			case DataType.Char:
			case DataType.TChar:
			case DataType.WChar:
				dbParameter = new Jde.DB.Parameter<char>( syntax, Column.Name, StringValue[0] );
				break;
			case DataType.SmallDateTime:
			case DataType.DateTime:
				dbParameter = new Jde.DB.Parameter<DateTime>( syntax, Column.Name, DateTime.Parse(StringValue) );
				break;
			case DataType.Decimal:
			case DataType.Money:
			case DataType.Numeric:
				dbParameter = new Jde.DB.Parameter<decimal>( syntax, Column.Name, decimal.Parse(StringValue) );
				break;
			case DataType.Float:
				dbParameter = new Jde.DB.Parameter<double>( syntax, Column.Name, double.Parse(StringValue) );
				break;
			case DataType.Guid:
				dbParameter = new Jde.DB.Parameter<Guid>( syntax, Column.Name, new Guid(StringValue) );
				break;
			case DataType.Int:
				dbParameter = new Jde.DB.Parameter<int>( syntax, Column.Name, int.Parse(StringValue) );
				break;
			case DataType.Int16:
				dbParameter = new Jde.DB.Parameter<Int16>( syntax, Column.Name, Int16.Parse(StringValue) );
				break;
			case DataType.Int8:
				dbParameter = new Jde.DB.Parameter<byte>( syntax, Column.Name, byte.Parse(StringValue) );
				break;
			case DataType.Long:
				dbParameter = new Jde.DB.Parameter<long>( syntax, Column.Name, long.Parse(StringValue) );
				break;
			case DataType.Blob:
			case DataType.NText:
			case DataType.Text:
			case DataType.Uri:
			case DataType.VarBinary:
			case DataType.VarChar:
			case DataType.VarTChar:
			case DataType.VarWChar:
				dbParameter = new Jde.DB.Parameter<string>( syntax, Column.Name, StringValue );
				break;
			case DataType.SmallFloat:
				dbParameter = new Jde.DB.Parameter<Single>( syntax, Column.Name, Single.Parse(StringValue) );
				break;
			case DataType.TimeSpan:
				dbParameter = new Jde.DB.Parameter<long>( syntax, Column.Name, TimeSpan.Parse(StringValue).Ticks );
				break;
			case DataType.UInt:
				dbParameter = new Jde.DB.Parameter<uint>( syntax, Column.Name, uint.Parse(StringValue) );
				break;
			case DataType.ULong:
				dbParameter = new Jde.DB.Parameter<ulong?>( syntax, Column.Name, ULongValue() );
				break;
			case DataType.Image:
			case DataType.Cursor:
			case DataType.RefCursor:
			default:
				throw Exceptions.ExceptionHelper.NotSupported( "data type '{0}' is not supported.", Column.DataType );
			}
			return dbParameter;
		}
		public string FormatWhere( SqlSyntax sql )
		{
			string value = string.Empty;
			try
			{
				switch( Column.DataType )
				{
				case DataType.TChar:
				case DataType.VarTChar:
				case DataType.VarChar:
				case DataType.Char:
					value = SqlSyntax.FormatWhere( Column.Name, StringValue );
					break;
				case DataType.Int8:
				case DataType.Int:
					NumberStyles styles = NumberStyles.None;
					string stringValue = StringValue;
					if( stringValue.Length>2 && string.Compare(stringValue.Substring(0, 2), "0x", StringComparison.OrdinalIgnoreCase)==0 )
					{
						styles |= NumberStyles.HexNumber;
						stringValue = stringValue.Substring(2);
					}
					value = DB.SqlSyntax.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (int?)int.Parse(stringValue, styles, CultureInfo.InvariantCulture) );
					break;
				case DataType.UInt:
					value = DB.SqlSyntax.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (int?)int.Parse(StringValue, CultureInfo.InvariantCulture) );
					break;
				case DataType.Long:
					value = DB.SqlSyntax.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (long?)long.Parse(StringValue) );
					break;
				case DataType.ULong:
					styles = NumberStyles.None;
					stringValue = StringValue;
					if( stringValue.Length>2 && string.Compare(stringValue.Substring(0, 2), "0x", StringComparison.OrdinalIgnoreCase)==0 )
					{
						styles |= NumberStyles.HexNumber;
						stringValue = stringValue.Substring(2);
					}
					value = SqlSyntax.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (ulong?)ulong.Parse(stringValue, styles, CultureInfo.InvariantCulture) );
					break;
				case DataType.Float:
					value = SqlSyntax.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (double?)double.Parse(StringValue, NumberStyles.Number, CultureInfo.InvariantCulture) );
					break;
				case DataType.DateTime:
					value = sql.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (DateTime?)DateTime.Parse(StringValue, CultureInfo.InvariantCulture) );
					break;
				case DataType.Guid:
					value = sql.FormatWhere( Column.Name, string.IsNullOrEmpty(StringValue) ? null : (Guid?)new Guid(StringValue) );
					break;
				default:
					throw new NotImplementedException( string.Format(CultureInfo.InvariantCulture, "DataType '{0}' is not supported", Column.DataType) );
				}
			}
			catch( FormatException e )
			{
				throw new FormatException( string.Format(CultureInfo.InvariantCulture, "Could not format ('{0}')'{1}'.", Column.DataType, StringValue), e );
			}
			return value;
		}
		public string FormatValue( SqlSyntax sql )
		{
			string value = string.Empty;
			try
			{
				switch( Column.DataType )
				{
				case DataType.TChar:
				case DataType.VarTChar:
				case DataType.VarChar:
				case DataType.Char:
					value = SqlSyntax.Format( StringValue );
					break;
				case DataType.Int8:
				case DataType.Int:
				case DataType.UInt:
					NumberStyles styles = NumberStyles.None;
					string stringValue = StringValue;
					if( stringValue.Length>2 && string.Compare(stringValue.Substring(0, 2), "0x", StringComparison.OrdinalIgnoreCase)==0 )
					{
						styles |= NumberStyles.HexNumber;
						stringValue = stringValue.Substring(2);
					}
					value = SqlSyntax.Format( string.IsNullOrEmpty(stringValue) ? null : (int?)int.Parse(stringValue, styles, CultureInfo.InvariantCulture) );
					break;
				case DataType.Long:
					value = SqlSyntax.Format( string.IsNullOrEmpty(StringValue) ? null : (long?)long.Parse(StringValue, CultureInfo.InvariantCulture) );
					break;
				case DataType.ULong:
					styles = NumberStyles.None;
					stringValue = StringValue;
					if( stringValue.Length>2 && string.Compare(stringValue.Substring(0, 2), "0x", StringComparison.OrdinalIgnoreCase)==0 )
					{
						styles |= NumberStyles.HexNumber;
						stringValue = stringValue.Substring(2);
					}
					value = SqlSyntax.Format( string.IsNullOrEmpty(StringValue) ? null : (ulong?)ulong.Parse(stringValue, styles, CultureInfo.InvariantCulture) );
					break;
				case DataType.DateTime:
					value = sql.Format( string.IsNullOrEmpty(StringValue) ? null : (DateTime?)DateTime.Parse(StringValue, CultureInfo.InvariantCulture) );
					break;
				case DataType.Guid:
					value = sql.Format( string.IsNullOrEmpty(StringValue) ? null : (Guid?)new Guid(StringValue) );
					break;
				case DataType.Float:
					value = SqlSyntax.Format( string.IsNullOrEmpty(StringValue) ? null : (double?)double.Parse(StringValue, CultureInfo.InvariantCulture) );
					break;
				default:
					throw new NotImplementedException( string.Format(CultureInfo.InvariantCulture, "DataType '{0}' is not supported", Column.DataType) );
				}
			}
			catch( FormatException e )
			{
				throw new FormatException( string.Format(CultureInfo.InvariantCulture, "Could not format ('{0}')'{1}'.", Column.DataType, StringValue), e );
			}
			return value;
		}
		public bool TestValue( object value )
		{
			bool equal = false;
			string stringValue = value as string;
			if( stringValue!=null )
				equal = stringValue==StringValue;
			else if( value is int )
				equal = (int)value==int.Parse( StringValue, CultureInfo.InvariantCulture );
			else if( value is uint )
				equal = (uint)value==uint.Parse( StringValue, CultureInfo.InvariantCulture );
			else if( value is long )
				equal = (Int64)value==Int64.Parse( StringValue, CultureInfo.InvariantCulture );
			else if( value is ulong ulongValue )
			{
				equal = ulongValue == (StringValue.StartsWith( "0x", StringComparison.OrdinalIgnoreCase )
					? ulong.Parse(StringValue.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
					: ulong.Parse(StringValue, CultureInfo.InvariantCulture) );
			}
			else if( value is double )
				equal = (double)value==double.Parse( StringValue, CultureInfo.InvariantCulture );
			else if( value is DateTime dateTimeValue )
				equal = dateTimeValue==DateTime.Parse( StringValue, CultureInfo.InvariantCulture );
			else if( value is float floatValue )
				equal = floatValue==double.Parse( StringValue, CultureInfo.InvariantCulture );
			else
				throw new ArgumentException( string.Format(CultureInfo.InvariantCulture, "could not test type '{0}'", value.GetType()) );
			return equal;
		}
		#endregion
		#region Required
		bool _required;
		[XmlAttribute("required")]
		public bool Required
		{
			get{ return _required; }
			set{ _required=value; }
		}
		#endregion
	};
}
