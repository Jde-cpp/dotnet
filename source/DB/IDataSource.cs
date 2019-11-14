using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Jde.DB
{
	public interface IDataSource
	{
		DataSet LoadDataSet( string sql );
		IDataReader ExecuteReader( string sql );
		SqlSyntax Syntax
		{ 
			get; 
//			set;
		}
	}
}
