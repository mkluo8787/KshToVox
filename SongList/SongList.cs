using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

namespace SongList
{
	public class SongList
	{
		public SongList() { }

		// From KFC
		public void Load(string kfcPath)
		{
			songs.Clear();

			// DB backup!
			string dbPath = kfcPath + "\\data\\others\\music_db.xml";
			if (!File.Exists(dbPath)) throw new FileNotFoundException();
			FileStream stream = new FileStream(dbPath, FileMode.Open);

			XElement root = XElement.Load(stream);
			foreach (XElement songXml in root.Elements("music"))
			{
				// Basic Attributes
				Dictionary<string, string> data = new Dictionary<string, string>();

				int id = int.Parse(songXml.Attribute("id").Value);
				data["label"]	= songXml.Element("info").Element("label").Value;
				data["title"]	= songXml.Element("info").Element("title_name").Value;
				data["artist"]	= songXml.Element("info").Element("artist_name").Value;
				data["ascii"]	= songXml.Element("info").Element("ascii").Value;
				data["version"]	= songXml.Element("info").Element("version").Value;
				data["inf_ver"]	= songXml.Element("info").Element("inf_ver").Value;

				// Difficulties. 0 = dummy.
				Dictionary<string, int> difficulty = new Dictionary<string, int>();

				difficulty["novice"]	= int.Parse(songXml.Element("difficulty").Element("novice")		.Element("difnum").Value);
				difficulty["advanced"]	= int.Parse(songXml.Element("difficulty").Element("advanced")	.Element("difnum").Value);
				difficulty["exhaust"]	= int.Parse(songXml.Element("difficulty").Element("exhaust")	.Element("difnum").Value);
				difficulty["infinite"]	= int.Parse(songXml.Element("difficulty").Element("infinite")	.Element("difnum").Value);

				Song song = new Song(data, difficulty, kfcPath);
				songs[id] = song;
			}

			stream.Close();

			loaded = true;
		}

		public void AddSong(Song song)
		{
			int newId = 0;
			foreach (int id in Enumerable.Range(1, 1024))
				if (!songs.ContainsKey(id))
				{
					newId = id;
					break;
				}
			if (newId == 0) throw new Exception("Song list is full!");

			songs.Add(newId, song);
		}

		public void DeleteId(int id)
		{
			songs.Remove(id);
		}


		// To KFC
		public void Save() { }

		// Utilities

		public Song Song(int id) { return songs[id]; }

		public List<KeyValuePair<int, Song>> List()
		{
			return songs.ToList<KeyValuePair<int, Song>>();
		}

		public bool Loaded() { return loaded; }

		private Dictionary<int, Song> songs = new Dictionary<int, Song>();
		private bool loaded = false;
	}
}
