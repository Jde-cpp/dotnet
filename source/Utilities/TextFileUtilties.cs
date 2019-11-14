using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jde.Utilities
{
	public class TextFileUtilties
	{
		/// <summary>Read a column of doubles.</summary>
		/// <param name="path">Path of the file.</param>
		/// <param name="column">column to extract.</param>
		/// <param name="header">if there is a file header.</param>
		/// <returns>doubles in the column.</returns>
		/// <exception cref="System.IO.IOException">path includes an incorrect or invalid syntax for file name, directory name, or volume label.</exception>
		///<exception cref="System.FormatException">value in column does not represent a number in a valid format.</exception>
		public static LinkedList<double> ReadDoubles( string path, int column, bool header )
		{
			var values = new LinkedList<double>();
			using( var sr = new System.IO.StreamReader(path) )
         {
				if( header )
					sr.ReadLine();
				for( var line = sr.ReadLine(); line!=null; line = sr.ReadLine() )
				{
					int iChar=0;
					for( int iColumn = 0; iColumn<column; ++iColumn )
					{
						while( iChar<line.Length && !char.IsWhiteSpace(line[iChar]) )
							++iChar;
						while( iChar<line.Length && char.IsWhiteSpace(line[iChar]) )
							++iChar;
					}

					var valueString = new StringBuilder();
					while( iChar<line.Length && !char.IsWhiteSpace(line[iChar]) )
						valueString.Append( line[iChar++] );

					values.AddLast( double.Parse(valueString.ToString(), System.Globalization.NumberStyles.Number) );
				}						
			}
			return values;
		}
	}
}
