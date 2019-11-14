using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Jde.Collections.Extensions
{
	public static class StringCollectionExtensions
	{
		public static void AddKeyToFront( this StringCollection keys, string key )
		{
			if( keys==null )
				throw new ArgumentNullException( "keys" );
			if( keys.IndexOf(key)!=0 )
			{
				if( keys.Contains(key) )
					keys.Remove( key );
				var newCollection = new StringCollection();
				newCollection.Add( key );
				foreach( var existingKey in keys )
					newCollection.Add( existingKey );
				keys.Clear();
				foreach( var existingKey in newCollection )
					keys.Add( existingKey );
			}
		}
	}
}
