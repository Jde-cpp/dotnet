using System;
using System.Collections.Generic;
using CultureInfo=System.Globalization.CultureInfo;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace Jde.Xml
{
	/// <summary>Utility class for .net xml serialization.</summary>
	/// <typeparam name="T">Type to serialize.</typeparam>
	public static class XmlSerialization<T>
	{
		private static XmlSerializer _factory;
		private static XmlSerializer Factory
		{
			get
			{ 
				if( _factory==null )
					Factory = new XmlSerializer( typeof(T) );
				return _factory; 
			}
			set{ _factory = value; }
		}

		/// <summary>Reads a class from a file.</summary>
		/// <param name="filePath">Path of the file to read.</param>
		/// <returns>Class serialized from the file.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T ReadFromFile( string filePath )
		{
			using( XmlReader inputStream = XmlReader.Create(filePath) )
				return Read( inputStream );
		}

		/// <summary>Serializes a class to a string.</summary>
		/// <param name="data">Class to serialize.</param>
		/// <returns>string serialization of the class.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
		public static string Write( T data )
		{
			using( var stringWriter = new StringWriter(CultureInfo.InvariantCulture) )
			{
				using( var writer = XmlWriter.Create(stringWriter) )
					Write( writer, data );
				return stringWriter.ToString();
			}
		}

		/// <summary>Serializes a class to a string.</summary>
		/// <param name="data">Class to serialize.</param>
		/// <param name="namespaces">Namespaces referenced by the class.</param>
		/// <returns>string serialization of the class.</returns>
		public static string Write( T data, XmlSerializerNamespaces namespaces )
		{
			using( var stringWriter = new System.IO.StringWriter( CultureInfo.InvariantCulture ) )
			{
				using( var writer = XmlWriter.Create(stringWriter) )
					Write( writer, data, namespaces );
				return stringWriter.ToString();
			}
		}


#if !SILVERLIGHT
		/// <summary>Saves a clss to a file.</summary>
		/// <param name="filePath">Path of the file to save.</param>
		/// <param name="data">Class to save.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static void SaveToFile( string filePath, T data )
		{
			SaveToFile( filePath, data, null );
		}

		/// <summary>Saves a clss to a file.</summary>
		/// <param name="filePath">Path of the file to save.</param>
		/// <param name="data">Class to save.</param>
		/// <param name="namespaces">Namespaces referenced by the class.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static void SaveToFile( string filePath, T data, XmlSerializerNamespaces namespaces )
		{
			using( XmlWriter writer = XmlWriter.Create(filePath) )
				Write( writer, data, namespaces );
		}
#endif
		/// <summary>Writes a class to an XmlWriter.</summary>
		/// <param name="filePath">XmlWriter to write the class to.</param>
		/// <param name="data">Class to save.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static void Write( XmlWriter writer,  T data )
		{
			Write( writer, data, null );
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static void Write( XmlWriter outputStream,  T data, XmlSerializerNamespaces namespaces )
		{
			Factory.Serialize( outputStream, data, namespaces );
		}


		/// <summary>Reads a class from an XmlReader.</summary>
		/// <param name="inputStream">XmlReader to read the class from.</param>
		/// <returns>The class read from the XmlReader.</returns>
		/// <exception cref="InvalidOperationException">Failed to read.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T Read( System.Xml.XmlReader inputStream )
		{
			T rVal = (T)Factory.Deserialize(inputStream);
			return rVal;
		}

		/// <summary>Reads a class from an TextReader.</summary>
		/// <param name="inputStream">TextReader to read the class from.</param>
		/// <returns>The class read from the XmlReader.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T Read( System.IO.TextReader inputStream )
		{
			T rVal = (T)Factory.Deserialize(inputStream);
			return rVal;
		}

		/// <summary>Reads a class from an TextReader.</summary>
		/// <param name="inputStream">TextReader to read the class from.</param>
		/// <returns>The class read from the XmlReader.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T Read( Stream inputStream )
		{
			//namespace
			T rVal = (T)Factory.Deserialize(inputStream);
			return rVal;
		}

		/// <summary>Reads a class from an XmlNode.</summary>
		/// <param name="inputStream">XmlNode to read the class from.</param>
		/// <returns>The class read from the XmlNode.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T Read( XmlNode node )
		{
			using( var readerNode = new XmlNodeReader(node) )
			{
				T rVal = (T)Factory.Deserialize( readerNode );
				return rVal;
			}
		}

		/// <summary>Reads a class from an XmlNode.</summary>
		/// <param name="inputStream">XmlNode to read the class from.</param>
		/// <returns>The class read from the XmlNode.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T Read( string xml )
		{
			return Read( xml, true );
		}

		/// <summary>Reads a class from a string.</summary>
		/// <param name="xml">string to read the class from.</param>
		/// <param name="prohibitDtd">Whether to prohibit document type definition (DTD) processing.</param>
		/// <returns>The class read from the string.</returns>
		/// <exception cref="InvalidOperationException">Failed to read.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes" )]
		public static T Read( string xml, bool prohibitDtd )
		{
			var settings = new XmlReaderSettings();
			settings.DtdProcessing = prohibitDtd ? DtdProcessing.Prohibit : DtdProcessing.Ignore;
			//settings.ProhibitDtd = prohibitDtd;
			
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader( xml );
				using( var reader = XmlReader.Create(stringReader, settings) )
				{
					stringReader = null;
					return Read( reader );
				}
			}
			finally
			{
				if( stringReader!=null )
					stringReader.Dispose();
			}
		}

		/// <summary>Reads a class from a string.</summary>
		/// <param name="xml">string to read the class from.</param>
		/// <param name="prohibitDtd">Whether to prohibit document type definition (DTD) processing.</param>
		/// <returns>The class read from the string.</returns>
		public static LinkedList<T> ReadParentChildren( System.Xml.XmlReader reader, string childElementName )
		{
			if( reader==null )
				throw new ArgumentNullException( "reader" );
			var items = new LinkedList<T>();
			reader.ReadStartElement();
			while( reader.NodeType!=XmlNodeType.EndElement )
			{
				if( reader.NodeType==XmlNodeType.Element && reader.Name==childElementName )
					items.AddLast( Xml.XmlSerialization<T>.Read(reader) );
				else
					reader.Read();
			}
			reader.ReadEndElement();

			return items;
		}
	}

	public static class XmlSerialization
	{
		/// <summary>Delegate to read the current element.</summary>
		/// <param name="reader">XmlReader used to read the element.</param>
		/// <returns>true if the element was able to be read.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public delegate bool ReadElement( System.Xml.XmlReader reader );
		/// <summary>Reads elements.</summary>
		/// <param name="reader">XmlReader to read from.</param>
		/// <param name="readElement">delegate to pass the reading of individule elements.</param>
		public static void ReadElements( System.Xml.XmlReader reader, ReadElement readElement )
		{
			if( reader==null )
				throw new ArgumentNullException( "reader" );
			if( readElement==null )
				throw new ArgumentNullException( "readElement" );
			if( reader.IsEmptyElement )
			   reader.Read();
			else
			{
			   reader.Read();
			   while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
			   {
			      if( reader.NodeType!=System.Xml.XmlNodeType.Element || !readElement(reader) )
			         reader.Read();
			   }
			   reader.ReadEndElement();
			}
		}

		public static void ReadToEnd( System.Xml.XmlReader reader )
		{
			if( reader==null )
				throw new ArgumentNullException( "reader" );
			if( reader.IsEmptyElement )
				reader.Read();
			else
			{
				reader.Read();
				while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
					reader.Read();
				reader.ReadEndElement();
			}
		}
	}

	namespace Extensions
	{
		public static class XmlSerializationExtensions
		{
			public static void WriteParentChildren<T>( this System.Xml.XmlWriter writer, string containerName, IEnumerable<T> items )
			{
				if( writer==null )
					throw new ArgumentNullException( "writer" );
				if( items==null )
					throw new ArgumentNullException( "items" );

				bool writeContainingElement = !string.IsNullOrEmpty( containerName );
				if( writeContainingElement )
					writer.WriteStartElement( containerName );
		
				foreach( T item in items )
					Xml.XmlSerialization<T>.Write( writer, item );

				if( writeContainingElement )
					writer.WriteEndElement();
			}

			public static T? ReadEnum<T>( this System.Xml.XmlReader reader ) where T :struct
			{
				var type = typeof( T );
				var attriute = type.CustomAttributes.FirstOrDefault( attribute=>attribute.AttributeType==typeof(System.Xml.Serialization.XmlRootAttribute) );
				var nameAttribute =  attriute.ConstructorArguments.First();
				var attributeName = nameAttribute.Value as string;

				var namespaceAttriute = attriute.NamedArguments.FirstOrDefault( attribute=>attribute.MemberName=="Namespace" );
				string @namespace = namespaceAttriute==null ? null : namespaceAttriute.TypedValue.Value as string;

				var enumValue = string.IsNullOrEmpty(@namespace) ? reader.GetAttribute( attributeName ) : reader.GetAttribute( attributeName, @namespace );

				//var members = Enum.GetValues( type );

				var members = type.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static );
				T? value = null;
				foreach( var member in members )
				{
					//var memberInfo = member.GetType().Ge;
					var enumValueAttriute = member.CustomAttributes.FirstOrDefault( attribute=>attribute.AttributeType==typeof(System.Xml.Serialization.XmlEnumAttribute) );
					if( enumValueAttriute==null )
						continue;
					var enumValueAttriuteName =  enumValueAttriute.ConstructorArguments.First();
					var enumValueAttriuteNameValue = enumValueAttriuteName.Value as string;
					if( enumValueAttriuteNameValue==enumValue )
					{
						value = (T?)member.GetValue( null );
						break;
					}
				}
				return value;
			}
			
			public static int? ReadInt( this System.Xml.XmlReader reader, string attributeName )
			{
				string stringValue = reader.GetAttribute( attributeName );
				int value;
				return !string.IsNullOrEmpty(stringValue) && int.TryParse(stringValue, out value ) ? (int?)value : null;
			}

			public static DateTime? ReadDateTime( this System.Xml.XmlReader reader, string attributeName )
			{
				string stringValue = reader.GetAttribute( attributeName );
				DateTime value;
				return !string.IsNullOrEmpty(stringValue) && DateTime.TryParse(stringValue, out value ) ? (DateTime?)value : null;
			}

			public static TimeSpan? ReadTimeSpan( this System.Xml.XmlReader reader, string attributeName )
			{
				string stringValue = reader.GetAttribute( attributeName );
				TimeSpan value;
				return !string.IsNullOrEmpty(stringValue) && TimeSpan.TryParse(stringValue, out value ) ? (TimeSpan?)value : null;
			}

			public static double? ReadDouble( this System.Xml.XmlReader reader, string attributeName, string @namespace )
			{
				string stringValue = string.IsNullOrEmpty(@namespace) ? reader.GetAttribute( attributeName ) : reader.GetAttribute( attributeName, @namespace );
				double value;
				return !string.IsNullOrEmpty(stringValue) && double.TryParse(stringValue, out value ) ? (double?)value : null;
			}

		}
	}

}
