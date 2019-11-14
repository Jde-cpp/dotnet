using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Jde.Collections
{
	[Serializable, XmlRoot("items")]
	public sealed class SerializableDictionary<TKey,TValue> : Dictionary<TKey,TValue>, IXmlSerializable
	{
		public SerializableDictionary( /*string xmlCollectionName, string xmlItemName, string xmlKeyName, string xmlValueName*/ )
		{
			//XmlCollectionName = xmlCollectionName;
			//XmlItemName = xmlItemName;
			//XmlKeyName = xmlKeyName;
			//XmlValueName = xmlValueName;
		}
		
		SerializableDictionary( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ):
			base( info, context )
		{}
		
		#region IXmlSerializable Members
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}
		void IXmlSerializable.ReadXml( XmlReader reader )
		{
			if( reader==null )
				throw new ArgumentNullException( "reader" );
			if( reader.IsEmptyElement )
				reader.Read();
			else
			{
				reader.Read();
				while( reader.NodeType!=System.Xml.XmlNodeType.EndElement )
				{
					if( reader.NodeType!=System.Xml.XmlNodeType.Element || (!string.IsNullOrEmpty(XmlCollectionName) && reader.LocalName==XmlCollectionName) )
					{
						reader.Read();
						continue;
					}
					//if( string.IsNullOrEmpty(XmlItemName) || reader.LocalName==XmlItemName )
					{
						var pair = Xml.XmlSerialization<Tuple<TKey,TValue>>.Read( reader );
						Add( pair.Item1, pair.Item2 );
					}
					//else
						//reader.Read();
				}
				reader.ReadEndElement();
			}
		}

		void IXmlSerializable.WriteXml( XmlWriter writer )
		{
			if( writer==null )
				throw new ArgumentNullException( "writer" );
			if( !string.IsNullOrEmpty(XmlCollectionName) )
				writer.WriteStartElement( XmlCollectionName );

			var enumerator = GetEnumerator();
			while( enumerator.MoveNext() )
			{
				//writer.WriteStartElement( XmlItemName );
				Xml.XmlSerialization<Tuple<TKey,TValue>>.Write( writer, new Tuple<TKey,TValue>(enumerator.Current.Key, enumerator.Current.Value) );
				//writer.WriteEndElement();
			}

			if( !string.IsNullOrEmpty(XmlCollectionName) )
				writer.WriteEndElement();
		}
		#endregion

		const string XmlItemName = "item";
		const string XmlCollectionName=null;
		//const string XmlKeyName = "key";
		//const string XmlValueName = "value";
	}
}
