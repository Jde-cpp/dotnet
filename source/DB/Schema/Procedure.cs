using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
//using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Jde.DB.Schema
{
	[Serializable, XmlType("proc_types")]
	public enum ProcedureType
	{
		[XmlEnum( "none" )]
		None=0,
		[XmlEnum( "select" )]
		Select=1,
		[XmlEnum( "insert" )]
		Insert=2,
		[XmlEnum( "update" )]
		Update=3,
		[XmlEnum( "custom" )]
		Custom=4
	};

	[XmlRoot( "procedure", IsNullable=true, Namespace=DataSchema.XmlNamespace ), Serializable]
	[XmlSchemaProvider("MySchema")]
	public class Procedure : IXmlSerializable
	{
		#region Constructors
		public Procedure(string name)
		{
			Name=name;
		}
		public Procedure() { }
		public Procedure(string name, ProcedureType type)
		{
			Name=name;
			ProcedureType=type;
		}
		#endregion
		#region IXmlSerializable Members
		public const string XmlElementName = "procedure";
		public static XmlQualifiedName MySchema( System.Xml.Schema.XmlSchemaSet schemaSet )
		{
			schemaSet.XmlResolver = new XmlUrlResolver();
			schemaSet.Add( DataSchema.XmlSchema );
			return new XmlQualifiedName( XmlElementName, DataSchema.XmlNamespace );
		}
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}
		void IXmlSerializable.ReadXml( System.Xml.XmlReader reader )
		{
			string location = reader.GetAttribute( "location" );
			if( !string.IsNullOrEmpty(location) )
				Location = DB.Schema.Location.Parse( location );
	
			Name = reader.GetAttribute( "name" );

			string type = reader.GetAttribute( "type" );
			if( !string.IsNullOrEmpty(type) )
				ProcedureType = (ProcedureType)Enum.Parse(typeof(ProcedureType), type, true );

			while( reader.NodeType==XmlNodeType.Attribute )
				reader.MoveToElement();
			if( !reader.IsEmptyElement )
			{
				reader.Read();
				while( reader.NodeType!=XmlNodeType.EndElement )
				{
					if( reader.NodeType!=XmlNodeType.Element )
					{
						reader.Read();
						continue;
					}
#if Unused
					else if( reader.LocalName=="package" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Package) );
						Package = (Package)ser.Deserialize( reader );
					}
#endif
					else if( reader.LocalName=="parameters" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(SchemaParameter) );
						while( reader.NodeType!=XmlNodeType.EndElement )
							Parameters.Add( (SchemaParameter)ser.Deserialize(reader) );
					}
					else if( reader.LocalName=="prefix" )
						Prefix = reader.ReadElementContentAsString();
					else if( reader.LocalName=="table" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Table) );
						Table = (Table)ser.Deserialize( reader );
					}
					else if( reader.LocalName=="text" )
						Text = reader.ReadElementContentAsString();
					else if( reader.LocalName=="sequence" )
					{
						XmlSerializer ser = new XmlSerializer( typeof(Sequence) );
						Sequence = (Sequence)ser.Deserialize( reader );
					}
					else if( reader.LocalName=="suffix" )
						Suffix = reader.ReadElementContentAsString();
					else
						reader.Read();
				}
				reader.ReadEndElement();
			}
			else
				reader.Read();
		}

		void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			if( Location!=Locations.Production )
				writer.WriteAttributeString( "location", Location.ToString() );
			if( !string.IsNullOrEmpty(Name) )
				writer.WriteAttributeString("name", Name );
			if( ProcedureType!=ProcedureType.Insert )
				writer.WriteAttributeString( "type", ProcedureType.ToString() );
#if Unused
			XmlSerializer ser = new XmlSerializer( typeof(Package) );
			if( Package!=null )
				ser.Serialize( writer, Package );
#endif
			XmlSerializer ser = new XmlSerializer( typeof(SchemaParameter) );
			foreach( SchemaParameter item in Parameters )
				ser.Serialize( writer, item );
			if( !string.IsNullOrEmpty(Prefix) )
				writer.WriteElementString( "prefix", Prefix );
			if( Table!=null )
			{
				ser = new XmlSerializer( typeof(Table) );
				ser.Serialize( writer, Table );
			}
			if( !string.IsNullOrEmpty(Text) )
				writer.WriteElementString( "text", Text );
			if( Sequence!=null )
			{
				ser = new XmlSerializer( typeof(Sequence) );
				ser.Serialize( writer, Sequence );
			}
			if( !string.IsNullOrEmpty(Suffix) )
				writer.WriteElementString( "suffix", Suffix );
		}
		#endregion
		#region FillValues
		void GetFormatedString( XmlNode node, ref int cTab, ref StringBuilder text )
		{
			bool paragraph = node.Name=="p";
			bool tab = node.Name=="blockquote";
			if( tab )
				++cTab;
			if( !string.IsNullOrEmpty(node.InnerText) )
			{
				for( int iTab=0; iTab<cTab; ++iTab )
					text.Append("\t");
				text.Append( node.InnerText );
			}
			foreach( XmlNode childNode in node.ChildNodes )
				GetFormatedString( childNode, ref cTab, ref text );

			if( tab )
				--cTab;
			if( paragraph )
				text.Append("\n");
		}

		public void FillValues()
		{
/*			if( Prefix.Length>0 )
			{
				StringBuilder text = new StringBuilder();
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(Prefix);
				int cTab = 0;
				foreach( XmlNode node in doc.ChildNodes )
					GetFormatedString(node, ref cTab, ref text);
				Prefix = text.ToString();
			}
			if( Suffix.Length>0 )
			{
				StringBuilder text = new StringBuilder();
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(Suffix);
				int cTab = 0;
				foreach( XmlNode node in doc.ChildNodes )
					GetFormatedString(node, ref cTab, ref text);
				Suffix = text.ToString();
			}
*/
			Text = Text.Replace(@"\n", "\n");
			Text = Text.Replace(@"\t", "\t");
			Prefix = Prefix.Replace(@"\n", "\n");
			Prefix = Prefix.Replace(@"\t", "\t");
			Suffix = Suffix.Replace(@"\n", "\n");
			Suffix = Suffix.Replace(@"\t", "\t");
		}
		#endregion
#if Unused
		#region GetBody
		public static string GetBody( SqlType sql, Package package, bool unicode )
		{
			StringBuilder statements = new StringBuilder();
			if( sql.SupportsPackages )
			{
				foreach( Procedure proc in package.Procedures )
				{
					Dictionary<string, string> createStatement = proc.GetCreateStatements(sql, unicode);
					foreach( string statement in createStatement.Values )
						statements.Append(statement);
				}
			}
			return statements.ToString();
		}
		#endregion
#endif
		#region GetCreateStatements
		public Dictionary<string, string> GetRecreateStatements( SqlSyntax sql, bool unicode )
		{
			return GetCreateStatements( sql, unicode, true );
		}

		public Dictionary<string, string> GetCreateStatements( SqlSyntax sql, bool unicode, bool recreate=false )
		{
			Dictionary<string, string> statements = new Dictionary<string, string>();
			switch( ProcedureType )
			{
			case ProcedureType.Select:
				statements = GetSelectStatements( sql, unicode/*, recreate*/ );
				break;
			case ProcedureType.Insert:
				statements = GetInsertStatements(sql, unicode, recreate);
				break;
			case ProcedureType.Update:
				statements = GetUpdateStatements( /*sql, unicode, recreate*/ );
				break;
			case ProcedureType.Custom:
				statements = GetCustomStatements( /*sql, unicode, recreate*/ );
				break;
			default:
				throw new ArgumentException(string.Format( CultureInfo.InvariantCulture, "undefined type '{0}'.", ProcedureType) );
			}
			return statements;
		}
#if Unused
		public static string GetCreateStatement( SqlType sql, Package package, bool unicode )
		{
			StringBuilder statement = new StringBuilder();
			if( sql.SupportsPackages )
			{
				statement.AppendFormat( CultureInfo.InvariantCulture, "CREATE OR REPLACE PACKAGE {0} AS\n", package.Name );
				foreach( UserDataType dataType in package.DataTypes )
					statement.AppendFormat("TYPE {0} IS {1};\n", dataType.Name, sql.GetTypeString(dataType.DataType, unicode));

				foreach( Procedure proc in package.Procedures )
				{
					for( int iVersion =0; iVersion<proc.VersionCount(); ++iVersion )
						statement.AppendFormat("{0};", proc.GetPrototype(sql,unicode, iVersion));
				}
				statement.AppendFormat("END {0};", package.Name);
			}
			return statement.ToString();
		}
#endif
		#endregion
		#region GetCustomStatements
		static Dictionary<string, string> GetCustomStatements( /*SqlType sql, bool unicode, bool recreate*/ )
		{
		   throw new InvalidOperationException("need to implement.");
		}
		#endregion
		public string DropStatement{ get=>string.Format( CultureInfo.InvariantCulture, "drop procedure {0}", Name ); }
		#region GetInsertStatements
		Dictionary<string, string> GetInsertStatements( SqlSyntax sql, bool unicode, bool recreate )
		{
			if( Table==null )
				return new Dictionary<string, string>();//???
			string prefix = recreate ? "alter" : "create";
			//if( !string.IsNullOrEmpty(sql.AltDelimiter) )
				//prefix = string.Format( "delimiter {0}\n{1}", sql.AltDelimiter, prefix );
			StringBuilder statement = new StringBuilder( prefix+" procedure "+GetPrototype(sql, unicode, 0));
			statement.Append(sql.ProcedurePrefix);
			statement.Append(Prefix);
			if( Table.Sequence()!=null && !sql.SupportsIdentity )
				statement.AppendFormat("\tselect {0}.nextval INTO {1} from dual;\n", Table.Sequence().Name, Table.SurrogateKeys()[0].Name);

			statement.AppendFormat("\tinsert into {0}(", Table.Name);
			bool first=true;
			foreach( var column in Table.SortedColumns().Values )
			{
				if( !column.RealColumn || (column.Sequence != null && sql.SupportsIdentity) )
					continue;
				if(first)
					first=false;
				else
					statement.Append(",");

				statement.Append(column.Name);
			}
			statement.Append(")\n\t\tvalues(");
			first=true;
			foreach( var column in Table.SortedColumns().Values )
			{
				if( !column.RealColumn || (column.Sequence != null && sql.SupportsIdentity) )
					continue;
				if( first )
					first=false;
				else
					statement.Append(",");
				statement.Append( sql.ProcedureParameterPrefix+column.Name );
			}
			statement.Append(");\n");

			if( Table.Sequence()!=null && sql.SupportsIdentity )
				statement.AppendFormat( sql.LastInsertFormat, '_'+Table.SurrogateKeys()[0].Name );
				//statement.AppendFormat("\tselect @{0}=@@identity", Table.SurrogateKeys()[0].Name);

			statement.Append(Suffix);
			statement.Append( sql.ProcedureSuffix);


			Dictionary<string, string> statements=new Dictionary<string, string>();
			statements.Add(Name, statement.ToString());
			return statements;
		}
		#endregion
		#region GetPrototype
		public string GetPrototype( SqlSyntax sql, bool unicode, int version )
		{
			StringBuilder prototype= new StringBuilder(string.Format(CultureInfo.InvariantCulture, "{0}(", Name));
			SortedList<int, SchemaParameter> parameters = SequencedParameters();

			int iParam=0;
			foreach( SchemaParameter param in parameters.Values )
			{
				if( iParam>0 )
					prototype.Append(",");
				string direction = param.Direction==ParameterDirection.Output ? " out " : string.Empty;
				if( sql.ProcPrefixDirection==null )
					prototype.Append(direction);
				prototype.AppendFormat("{0}{1} ",
					sql.ProcedureParameterPrefix+param.Name,
					param.Max==1 || version==0 ? string.Empty : (iParam+1).ToString(CultureInfo.InvariantCulture));
				if( sql.ProcPrefixDirection==true )
					prototype.Append(direction);
				prototype.Append( sql.GetTypeString(param.UserDataType.DataType, unicode).ToLower(CultureInfo.InvariantCulture) );
				if( sql.SpecifyParameterLength && DataTypes.HasLength(param.UserDataType.DataType) )
					prototype.AppendFormat( "({0})", param.Column.MaxLength );
				if( sql.ProcPrefixDirection==false )
					prototype.Append(direction);

				++iParam;
			}
			prototype.Append(")\n");
			return prototype.ToString();
		}
		#endregion
		#region GetSelectStatements
		Dictionary<string, string> GetSelectStatements( SqlSyntax sql, bool unicode )
		{
			throw new NotImplementedException();
#if Unused
			Dictionary<string, string> statements=new Dictionary<string, string>();

			StringBuilder statement = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "CREATE PACKAGE BODY {0} AS", Package.Name));

			int cVersions = VersionCount();
			for( int iVersion=0; iVersion<cVersions; ++iVersion )
			{
				StringBuilder version = new StringBuilder(GetPrototype(sql,unicode, iVersion));
				version.Append(sql.ProcedurePrefix);

				foreach( Parameter param in Parameters )
				{
					if( param.Direction!=ParameterDirection.Output )
						continue;

					version.Append("select *\n");
					version.AppendFormat("from {0}\nwhere\t", param.Table.Name);

					int iInParam=0;
					SortedList<int, Parameter> paramaters = SequencedParameters();
					foreach( Parameter inParam in paramaters.Values )
					{
						if( inParam.Direction!=ParameterDirection.Input )
							continue;
						if( iInParam++>0 )
							version.Append("and\t");
						if( iVersion==0 || inParam.Max==1 )
							version.AppendFormat("{0}={1}\n", inParam.Column.Name, sql.ProcedureParameterPrefix+inParam);
						else
						{
							version.Append("{0} in (");
							for( int iParamNum = 0; iParamNum<iVersion && iParamNum < inParam.Max; ++iParamNum )
							{
								if( iParamNum > 0 )
									version.Append(",");
								version.AppendFormat("{0}{1}", sql.ProcedureParameterPrefix+inParam, (iParamNum+1).ToString(CultureInfo.InvariantCulture));
							}
							version.Append(")\n");
						}
					}
					StringBuilder name = new StringBuilder(Name);
					if( iVersion!=0 )
						name.AppendFormat("_{0}", iVersion.ToString(CultureInfo.InvariantCulture));

					statement.AppendFormat("\tend {0}", name);
					if( sql.SupportsPackages )
						version.Append(statement);
					else
						statements.Add(name.ToString(), statement.ToString());
				}
			}//for each version.

			if( sql.SupportsPackages )
				statements.Add(Name, statement.ToString());
			return statements;
#endif
		}
		#endregion
		#region GetUpdateStatements
		static Dictionary<string, string> GetUpdateStatements( /*SqlType sql, bool unicode, bool recreate*/ )
		{
			throw new InvalidOperationException("need to implement.");
		}
		#endregion
		public string GetAlterStatement( SqlSyntax syntax )=>syntax.CanAlterProcedures ? Text.Replace("create procedure", "alter procedure" ) : $"{DropStatement};\n{Text}";
		#region GetWords
		public static Collection<string> GetWords( string statement )
		{
			return GetWords( statement, string.Empty );
		}

		public static Collection<string> GetWords( string statement, string firstWord )
		{
			var words = new Collection<string>();
			var word = new StringBuilder();
			bool foundFirst = string.IsNullOrEmpty(firstWord);
			for( int iChar=0; iChar<statement.Length; ++iChar )
			{
				char ch = statement[iChar];
				bool delimiter = ch==',' || ch=='(' || ch==')' || ch==';';
				if( !Char.IsWhiteSpace(ch) && !delimiter )
					word.Append( ch );
				else if( word.Length>0 || delimiter )
				{
					if( !foundFirst )
						foundFirst = word.ToString() == firstWord;

					if( foundFirst && word.Length>0 )
						words.Add( word.ToString() );
					word.Length=0;
					if( delimiter )
						words.Add( new string(ch,1) );
				}
			}
			if( word.Length>0 )
				words.Add( word.ToString() );
			return words;		
		}
		#endregion
		#region operators
		public static bool operator==( Procedure first, Procedure second )
		{
			if( ReferenceEquals(first, second) )
				return true;
			if( Type.ReferenceEquals(first, null) || Type.ReferenceEquals(second, null) || first.Name!=second.Name )
				return false;
			if( string.IsNullOrEmpty(first.Text) || string.IsNullOrEmpty(second.Text) )
				return true;

			Collection<string> aWords = GetWords( first.Text );
			Collection<string> bWords = GetWords( second.Text );
			var equal = aWords.Count==bWords.Count;
			int iWord = 0;
			for( ; iWord< aWords.Count && equal; ++iWord )
			{
				//bool insensitive = true;//string.Compare( aWords[iWord], "create", StringComparison.OrdinalIgnoreCase )==0;
				StringComparison comparison = StringComparison.OrdinalIgnoreCase;
				var aWord = aWords[iWord];
				var bWord = bWords[iWord];
				equal = string.Compare( aWord, bWord, comparison )==0;
				if( !equal )
				{
					if( aWord.StartsWith("`") && aWord.EndsWith("`") && aWord.Length>2 )
						equal = string.Compare( aWord.Substring(1,aWord.Length-2), bWord, comparison )==0;
					else if( bWord.StartsWith("`") && bWord.EndsWith("`") && bWord.Length>2 )
						equal = string.Compare( aWord, bWord.Substring(1,bWord.Length-2), comparison )==0;
				}
			}
			return equal;
		}
		public static bool operator!=( Procedure first, Procedure second )
		{
			return !(first == second);
		}
		public override bool Equals( object obj )
		{
			Procedure proc = obj as Procedure;
			return proc!=null && this==proc;
		}
		public override int GetHashCode()
		{
			return Text.GetHashCode() ^ Name.GetHashCode();
		}
		#endregion
		#region ToString()
		public override string ToString()
		{
			return Name;
		}
		#endregion
		#region Location
		Locations _location=Locations.Production;
		[XmlAttribute("location")]
		public Locations Location
		{
			get{ return _location; }
			set{ _location=value;}
		}
		#endregion
		#region Name
		string _name=string.Empty;
		[XmlAttribute("name")]
		public string Name
		{ 
			get{ return _name;}
			set
			{ 
//				if( value.Length>30 )
//					throw new ArgumentException( string.Format("Name must be 30 characters or less.  '{0}' is '{1}' characters.", value, value.Length) );
				_name=value; 
			
			}
		}
		#endregion
		#region Package
#if Unused
		Package _package;
		[XmlElement( "package" )]
		public Package Package
		{
			get { return _package; }
			set { _package=value; }
		}
#endif
		#endregion
		#region Parameters
		Collection<SchemaParameter> _parameters=new Collection<SchemaParameter>();
		[XmlElement("parameters")]
		public Collection<SchemaParameter> Parameters
		{
			get{ return _parameters;}
			//private set{ _parameters=value; }
		}
		public SortedList<int, SchemaParameter> SequencedParameters()
		{
			SortedList<int, SchemaParameter> parameters = new SortedList<int, SchemaParameter>();
			foreach( SchemaParameter param in Parameters )
				parameters.Add( param.Sequence, param );
			return parameters;
		}
		public int VersionCount()
		{
			int cVersions=1;
			foreach( SchemaParameter param in Parameters )
				cVersions *= param.Max;
			return cVersions;
		}
		#endregion
		#region Prefix
		string _prefix=string.Empty;
		[XmlElement("prefix")]
		public string Prefix
		{
			get { return _prefix; }
			set { _prefix=value; }
		}
		#endregion
		#region Table
		Table _table;
		[XmlElement("table")]
		public Table Table
		{
			get{ return _table;}
			set{ _table=value; }
		}
		#endregion
		#region Text
		string _text=string.Empty;
		[XmlElement( "text" )]
		public string Text
		{
			get { return _text; }
			set { _text=value; }
		}
		public string LoadText( DB.Database db )
		{
			if( string.IsNullOrEmpty(_text) )
				_text = db.Syntax.LoadProcText( db, Name );
			return _text;
		}
		#endregion
		#region Types
		ProcedureType _type=ProcedureType.Select;
		[XmlAttribute( "type" )]
		public ProcedureType ProcedureType
		{
			get { return _type; }
			set { _type=value; }
		}
		#endregion
		#region Sequence
		Sequence _sequence;
		[XmlElement("sequence")]
		public Sequence Sequence
		{
			get{ return _sequence;}
			set{ _sequence=value;}
		}
		#endregion
		#region Suffix
		string _suffix=string.Empty;
		[XmlElement("suffix")]
		public string Suffix
		{
			get { return _suffix; }
			set { _suffix=value; }
		}
		#endregion
	};

}
