using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

using IfsParse;

namespace SongList
{
	public class SongList
	{
        private readonly static int listSize = 2048;
		public readonly static string[] DIFS = { "novice", "advanced", "exhaust", "infinite" };
		public readonly static string cachePath = System.IO.Path.GetDirectoryName(
			System.Reflection.Assembly.GetExecutingAssembly().Location
			) + "\\cache\\";

		public SongList()
		{
            songs = new Song[listSize];
            for (int id = 0; id < listSize; ++id)
                songs[id] = new Song();
            Directory.CreateDirectory(cachePath);
		}

		~SongList()
		{
			Directory.Delete(cachePath, true);
		}

		// From KFC
		public void Load(string kfcPath_)
		{
			Clear();

			kfcPath = kfcPath_;

            // Parse Ifs Files (Charts, Jackets) into Cache
            // Jackets are not parsed for now, will be included in further version
            //Ifs vox01 = new Ifs(kfcPath + "\\data\\others\\vox_ifs\\vox_02.ifs", Ifs.IfsParseType.Chart);
            //vox01.Cache(cachePath);
            //vox01.Close();

            // DB backup?
            string dbPath = kfcPath + "\\data\\others\\music_db.xml";
			if (!File.Exists(dbPath)) throw new FileNotFoundException();
			FileStream stream = new FileStream(dbPath, FileMode.Open);

			XElement root = XElement.Load(stream);
			foreach (XElement songXml in root.Elements("music"))
			{
				// Basic Attributes
				Dictionary<string, string> data = new Dictionary<string, string>();

				int id = int.Parse(songXml.Attribute("id").Value);

                foreach (XElement xe in songXml.Element("info").Elements())
                {
                    data[xe.Name.LocalName] = xe.Value;
                    if (xe.Attribute("__type") != null)
                        typeAttr[xe.Name.LocalName] = xe.Attribute("__type").Value;
                }

                

                // Difficulties. 0 = dummy.
                Dictionary<string, Dictionary<string, string>> chartData = new Dictionary<string, Dictionary<string, string>>();

                foreach (string dif in DIFS)
                {
                    chartData[dif] = new Dictionary<string, string>();
                    foreach (XElement xe in songXml.Element("difficulty").Element(dif).Elements())
                    { 
                        chartData[dif][xe.Name.LocalName] = xe.Value;
                        if (xe.Attribute("__type") != null)
                            typeAttr[xe.Name.LocalName] = xe.Attribute("__type").Value;
                    }

                    // Data Manipulation
                    chartData[dif]["price"] = "-1";
                    chartData[dif]["limited"] = "3";
                }

				Song song = new Song(data, chartData, kfcPath, false);
				songs[id] = song;
			}

			stream.Close();

			loaded = true;
		}

        private void Clear()
        {
            for (int i = 0; i < listSize; ++i)
                songs[i] = new Song();
        }

		public int AddKshSong(string path, int startId = 0)
		{
			int newId = 0;
            foreach (int id in Enumerable.Range(Math.Max(256, startId), 1024))
            {
                if (songs[id].IsDummy())
                {
                    newId = id;
                    break;
                }
            }
			if (newId == 0) throw new Exception("Song list is full!");

			songs[newId] = new Song(newId.ToString(), path);

			return newId;
		}

		public bool DeleteId(int id)
		{
            if (songs[id].IsDummy()) return false;
            else
            {
                songs[id] = new Song();
                return true;
            }
		}


		// To KFC
		public void Save()
		{
			string dbPath = kfcPath + "\\data\\others\\music_db.xml";

			XElement root = new XElement("mdb");
            XDocument xmlFile = new XDocument(
                new XDeclaration("1.0", "shift-jis", "yes"),
                root
            );

            for (int id = 0; id < listSize; ++id)
            {
				Song song	= songs[id];

                if (song.IsDummy()) continue;

                // Write .vox and 2dx
                song.Save(kfcPath);

                // Write to db
                XElement music = new XElement("music", new XAttribute("id", id));

				XElement info = new XElement("info");

                foreach (KeyValuePair<string, string> d in song.Dict())
                    if (typeAttr.ContainsKey(d.Key))
                        info.Add(new XElement(d.Key, new XAttribute("__type", typeAttr[d.Key]), d.Value));
                    else
                        info.Add(new XElement(d.Key, d.Value));

                XElement difficulty = new XElement("difficulty");
				
				foreach(KeyValuePair<string, Dictionary<string, string>> chartInfo in song.ChartDict())
                { 
                    XElement difTag = new XElement(chartInfo.Key);
                    foreach (KeyValuePair<string, string> d in chartInfo.Value)
                        if (typeAttr.ContainsKey(d.Key))
                            difTag.Add(new XElement(d.Key, new XAttribute("__type", typeAttr[d.Key]), d.Value));
                        else
                            difTag.Add(new XElement(d.Key, d.Value));

                    difficulty.Add(difTag);
				}

				music.Add(info);
				music.Add(difficulty);

				root.Add(music);
			}

            xmlFile.Save(dbPath);
		}

		// Utils

		public Song Song(int id) {
            if ((id < 1) || (id > listSize - 1)) return new Song();
            return songs[id];
        }

		public List<KeyValuePair<int, Song>> List()
		{
			List<KeyValuePair<int, Song>> list = new List<KeyValuePair<int, Song>>();
			foreach (int id in Enumerable.Range(1, listSize - 1))
				list.Add(new KeyValuePair<int, Song>(id, songs[id]));
			return list;
		}

        public static string GetCachePath() { return cachePath; }

		public bool Loaded() { return loaded; }

        // Datas

        //private List<Song> songs = new List<Song>(listSize);
        private Song[] songs;
		private string kfcPath;
		private bool loaded = false;

        private static Dictionary<string, string> typeAttr = new Dictionary<string, string>();
	}
}
