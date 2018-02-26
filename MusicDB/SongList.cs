using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDB
{
	class SongList
	{
		// From KFC
		public void Load() { }

		// To KFC
		public void Save() { }

		public void AddSong(Song newSong, int index = 0) { }
		public void RemoveSong(int index) { }

		private Dictionary<int, Song> songs;
	}
}
