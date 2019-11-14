using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Jde.DB.Schema
{
	[XmlRoot( "schema", IsNullable=true, Namespace=DataSchema.XmlNamespace ), System.Serializable]
	public class Database
	{
		#region Constructors
		public Database( string name )
		{
			Name=name;
		}
		#endregion
		#region Load
		public static List<Schema.Database> Load( DataSource ds )
		{
		   DataSet dataSet = ds.LoadDataSet( ds.Syntax.GetDatabaseNameQuery() );
		   List<Schema.Database> dbs = new List<Schema.Database>();
		   foreach( DataRow row in dataSet.Tables[0].Rows )
		      dbs.Add( new Schema.Database( (string)row["name"] ) );
		   return dbs;
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute( "id" )]
		public string Name
		{
			get { return _name; }
			set { _name=value; }
		}
		#endregion
	}
}