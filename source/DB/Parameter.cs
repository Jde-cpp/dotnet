using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Jde.DB
{
	public class Parameter<T> : DbParameter
	{
		#region Constructors
		public Parameter( SqlSyntax syntax, string name )
		{
			ParameterName = syntax.BuildParameterName( name );
			Direction = ParameterDirection.Output;
		}

		public Parameter( SqlSyntax syntax, string name, T value, bool isStoredProc=false )
		{
			TypedValue = value;
			ParameterName = syntax.BuildParameterName( name, isStoredProc );
			Direction = ParameterDirection.Input;
		}
		#endregion
		public override string ToString()
		{
			return ParameterName+"="+(Value==null ? "null" : Value.ToString());
		}
		#region DbType
		DbType? _dbType;
		/// <summary>Gets or sets the System.Data.DbType of the parameter.</summary>
		/// <remarks>One of the System.Data.DbType values. The default is System.Data.DbType.String.</remarks>
		/// <exception cref="ArgumentException">The property is not set to a valid System.Data.DbType.</exception>
		[RefreshProperties(RefreshProperties.All)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override DbType DbType
		{ 
			get
			{
				if( _dbType==null )
				{
					Type type = typeof(T);
					if( type==typeof(string) || type==typeof(Uri) )
						_dbType = IsUnicode ? DbType.String : DbType.AnsiString;
					else if( type==typeof(int) || type==typeof(Nullable<int>) || type==typeof(uint) || type==typeof(Nullable<uint>) )
						_dbType = DbType.Int32;
					else if( type==typeof(Int16) || type==typeof(Nullable<Int16>) || type==typeof(UInt16) || type==typeof(Nullable<UInt16>) )
						_dbType = DbType.Int32;
					else if( type==typeof(IDataParameter) )
						_dbType = ((IDataParameter)TypedValue).DbType;
					else if( type==typeof(DateTime) || type==typeof(Nullable<DateTime>) )
						_dbType = DbType.DateTime;
					else if( type==typeof(decimal) || type==typeof(Nullable<decimal>) )
						_dbType = DbType.Decimal;
					else if( type==typeof(long) || type==typeof(Nullable<long>) || type==typeof(TimeSpan) || type==typeof(TimeSpan?) )
						_dbType = DbType.Int64;
					else if( type==typeof(double) || type==typeof(Nullable<double>) )
						_dbType = DbType.Double;
					else if( type==typeof(ulong) || type==typeof(Nullable<ulong>) )
						_dbType = DbType.Int64;
					else if( type==typeof(bool) || type==typeof(Nullable<bool>) )
						_dbType = DbType.Boolean;
					else if( type==typeof(Guid) || type==typeof(Nullable<Guid>) )
						_dbType = DbType.Guid;
					else if( type==typeof(DBNull) )
						_dbType = DbType.AnsiString;
					else
						throw new InvalidCastException( string.Format(CultureInfo.CurrentUICulture, Properties.Resources.IsNotSupported1, type) );
				}
				return (DbType)_dbType;
			} 
			set{ _dbType = value; }
		}
		#endregion
		#region Direction
		//
		// Summary:
		//     Gets or sets a value that indicates whether the parameter is input-only,
		//     output-only, bidirectional, or a stored procedure return value parameter.
		//
		// Returns:
		//     One of the System.Data.ParameterDirection values. The default is Input.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     The property is not set to one of the valid System.Data.ParameterDirection
		//     values.
		[RefreshProperties(RefreshProperties.All)]
		public override ParameterDirection Direction { get; set; }
		#endregion
		//
		// Summary:
		//     Gets or sets a value that indicates whether the parameter accepts null values.
		//
		// Returns:
		//     true if null values are accepted; otherwise false. The default is false.
		[Browsable(false)]
		[DesignOnly(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool IsNullable { get; set; }
		//
		// Summary:
		//     Gets or sets the name of the System.Data.Common.DbParameter.
		//
		// Returns:
		//     The name of the System.Data.Common.DbParameter. The default is an empty string
		//     ("").
		[DefaultValue("")]
		public override string ParameterName { get; set; }
		//
		// Summary:
		//     Gets or sets the maximum size, in bytes, of the data within the column.
		//
		// Returns:
		//     The maximum size, in bytes, of the data within the column. The default value
		//     is inferred from the parameter value.
		public override int Size { get; set; }
		//
		// Summary:
		//     Gets or sets the name of the source column mapped to the System.Data.DataSet
		//     and used for loading or returning the System.Data.Common.DbParameter.Value.
		//
		// Returns:
		//     The name of the source column mapped to the System.Data.DataSet. The default
		//     is an empty string.
		[DefaultValue("")]
		public override string SourceColumn { get; set; }
		//
		// Summary:
		//     Sets or gets a value which indicates whether the source column is nullable.
		//     This allows System.Data.Common.DbCommandBuilder to correctly generate Update
		//     statements for nullable columns.
		//
		// Returns:
		//     true if the source column is nullable; false if it is not.
		[RefreshProperties(RefreshProperties.All)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DefaultValue(false)]
		public override bool SourceColumnNullMapping { get; set; }
		//
		// Summary:
		//     Gets or sets the System.Data.DataRowVersion to use when you load System.Data.Common.DbParameter.Value.
		//
		// Returns:
		//     One of the System.Data.DataRowVersion values. The default is Current.
		//
		// Exceptions:
		//   System.ArgumentException:
		//     The property is not set to one of the System.Data.DataRowVersion values.
		public override DataRowVersion SourceVersion { get; set; }
		//
		// Summary:
		//     Gets or sets the value of the parameter.
		//
		// Returns:
		//     An System.Object that is the value of the parameter. The default value is
		//     null.
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue("")]
		public override object Value 
		{ 
			get
			{ 
				object value = TypedValue;
				if( TypedValue!=null )
				{
					var param = value as IDataParameter;
					if( param!=null )
						value = param.Value;
					else
					{
						TimeSpan? timeSpan = value as TimeSpan?;
						if( timeSpan!=null )
							value = timeSpan.Value.Ticks;
						else
						{
							var uri = value as Uri;
							if( uri!=null )
								value = uri.ToString();
						}
					}
				}
				return value;
			}
			set{ TypedValue = (T)value; }
		}

		// Summary:
		//     Resets the DbType property to its original settings.
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override void ResetDbType()
		{
			DbType=DbType.String;
		}

		#region TypedValue
		//
		// Summary:
		//     Gets or sets the value of the parameter.
		//
		// Returns:
		//     An System.Object that is the value of the parameter. The default value is
		//     null.
		[RefreshProperties(RefreshProperties.All)]
		[DefaultValue("")]
		public T TypedValue{ get; protected set; }
		#endregion
		#region IsUnicode
		public static bool DefaultIsUnicode{ get; set; }
		bool? _isUnicode;
		public bool IsUnicode
		{
			get{ return _isUnicode==null ? DefaultIsUnicode : _isUnicode.Value;}
			set{ _isUnicode = value; }
		}
		#endregion
	}
}
