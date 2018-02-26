using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SongList
{
	public class SongList
	{
		public SongList() { }

		// From KFC
		public void Load(Stream stream)
		{
			test_string.Clear();
			StreamReader reader = new StreamReader(stream);
			while (!reader.EndOfStream)
				test_string.Add(reader.ReadLine());
		}

		//test
		public List<string> get_string() { return test_string; }
		private List<string> test_string = new List<string>();

		// To KFC
		public void Save() { }

		public void AddSong() { }
		public void RemoveSong(int index) { }

		private Dictionary<int, Song> songs = new Dictionary<int, Song>();
	}
}
