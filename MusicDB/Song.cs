using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace MusicDB
{
	class Song
	{
		// Methods

		// From K-Shoot/KFC
		public Song() { }

		// Song Attributes
		private string title;
		private string artist;
		private string ascii;

		private int version;

		// Charts
		private Dictionary<int, Chart> charts;

		// Music
		private Stream music;
	}
}
