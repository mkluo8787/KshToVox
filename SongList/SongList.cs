using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

using IfsParse;
using Utility;

namespace SongList
{
	public class SongList
	{
        readonly public static string binPath = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location
            ) + "\\";
        readonly public static string toolsPath = binPath + @"tools\";
        readonly public static string cachePath = binPath + @"cache\";

        private readonly static int listSize = 1061;
		public readonly static string[] DIFS = { "novice", "advanced", "exhaust", "infinite" };

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

				Song song = new Song(data, chartData, kfcPath, true);
				songs[id] = song;

                // Caching imformation
                if (data["custom"] == "1")
                {
                    kshPathToId[data["kshFolder"]] = id;

                    lastModifiedFolder[id] = data["lastModFolder"];
                    //lastModifiedChart[id] = data["lastModChart"];
                    //lastModifiedPic[id] = data["lastModPic"];
                    //lastModifiedSound[id] = data["lastModSound"];
                }
            }

			stream.Close();

			loaded = true;
		}

        public void LoadFromKshSong(string kfcPath, Dictionary<int, int> idToIfs, Dictionary<int, int> idToVer, Dictionary<string, string> typeAttr, bool firstLoad)
        {
            Clear();

            this.kfcPath = kfcPath;
            this.typeAttr = typeAttr;
            this.idToIfs = idToIfs;

            string kshFolderPath = kfcPath + "KshSongs\\";
            DirectoryInfo kshFolder = new DirectoryInfo(kshFolderPath);

            if (!firstLoad)
            {
                Load(kfcPath);

                // Check for oldid existance

                for (int id = 0; id < listSize; ++id)
                {
                    if (songs[id].Data("custom") == "1")
                    {
                        DirectoryInfo di = new DirectoryInfo(songs[id].Data("kshFolder"));
                        DirectoryInfo[] dis = kshFolder.GetDirectories();

                        bool hit = false;
                        foreach (DirectoryInfo existDi in dis)
                            if (existDi.FullName == di.FullName)
                                hit = true;

                        if (!hit) // Ksh Folder removed!
                        {
                            DeleteId(id);
                            lastModifiedFolder.Remove(id);
                        }
                    }
                }
            }

            foreach (DirectoryInfo kshSong in kshFolder.GetDirectories())
            {
                if (kshPathToId.ContainsKey(kshSong.FullName)) // cache hit!
                {
                    int oldId = kshPathToId[kshSong.FullName];
                    if (Directory.GetLastWriteTime(kshSong.FullName).ToString() ==
                        lastModifiedFolder[oldId])
                    {
                        Util.ConsoleWrite("Song: " + songs[kshPathToId[kshSong.FullName]].Data("title_name") + " Ksh data is unchanged. Will be skipped while saving.");
                        continue; // Skips the entire loading
                    }
                }

                if (kshSong.GetFiles("*.ksh").Length == 0)
                    //throw new Exception("Invalid folder in KshSongs!");
                    continue;

                int newId = -1;
                foreach (KeyValuePair<int, int> idPair in idToIfs)
                    if ((songs[idPair.Key].IsDummy()) && (idPair.Key >= 256))
                        newId = idPair.Key;
                            

                if (newId == -1) throw new Exception("Song DB Full!");

                AddKshSong(kshSong.FullName, newId, idToVer[newId]);

                Util.ConsoleWrite("Song: " + songs[newId].Data("title_name") + " Ksh data loaded.");
            }

            loaded = true;
        }

        private void Clear()
        {
            for (int i = 0; i < listSize; ++i)
                songs[i] = new Song();
        }

		public int AddKshSong(string path, int startId = 0, int ver = 4)
		{
            int newId = 0;
            if (startId == 0)
            {
                foreach (int id in Enumerable.Range(Math.Max(256, startId), listSize))
                {
                    if (songs[id].IsDummy())
                    {
                        newId = id;
                        break;
                    }
                }
                if (newId == 0) throw new Exception("Song list is full!");
            }
            else
                newId = startId;

            songs[newId] = new Song(newId.ToString(), path, ver);

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
                if (!IsUnmoddedCustom(id))
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

        // Texture Save (Replacement)

        public void SaveTexture()
        {
            // s_jackets

            List<int> targetIfs = new List<int>();

            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;
                if (IsUnmoddedCustom(id)) continue;

                if (!targetIfs.Contains(idToIfs[id]))
                    targetIfs.Add(idToIfs[id]);
            }

            Dictionary<int, string> texPaths = new Dictionary<int, string>();

            foreach (int ifsId in targetIfs)
            {
                // DO: ifs -> tex

                string ifsPath = Util.kfcPath + "data\\graphics\\s_jacket" + ifsId.ToString().PadLeft(2, '0') + ".ifs";

                texPaths[ifsId] = Util.IfsToTex(ifsPath);
            }

            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;
                if (IsUnmoddedCustom(id)) continue;

                string texPath = texPaths[idToIfs[id]];

                song.ImageToTex("jk_" + song.BaseNameNum() + "_1.tga", texPath + "tex\\", 202);
            }

            foreach (int ifsId in targetIfs)
            {
                // DO: tex -> ifs

                string texPath = texPaths[ifsId];

                string ifsPath = Util.kfcPath + "data\\graphics\\s_jacket" + ifsId.ToString().PadLeft(2, '0') + ".ifs";

                Util.TexToIfs(texPath, ifsPath);
            }

            // jk_b and jk_s
            
            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;
                if (IsUnmoddedCustom(id)) continue;

                string[] sufs = {"_b", "_s"};
                foreach (string suf in sufs)
                {
                    string ifsPath = Util.kfcPath + "data\\graphics\\jk\\jk_" + song.BaseNameNum() + "_1" + suf + ".ifs";

                    string texPath = Util.IfsToTex(ifsPath);

                    string tgaName = "jk_" + song.BaseNameNum() + "_1" + suf + ".tga";

                    if (suf == "_b")
                        song.ImageToTex(tgaName, texPath + "tex\\", 452);
                    else if (suf == "_s")
                        song.ImageToTex(tgaName, texPath + "tex\\", 74);
                    else
                        throw new Exception();

                    Util.TexToIfs(texPath, ifsPath);
                }
            }
            
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

        private bool IsUnmoddedCustom(int id)
        {
            if (!lastModifiedFolder.ContainsKey(id)) return false;

            if (songs[id].Data("custom") == "1")
            {
                string path = songs[id].Data("kshFolder");
                return Directory.GetLastWriteTime(path).ToString() ==
                        lastModifiedFolder[id];
            }
            else
                return false;
        }

        public bool Loaded() { return loaded; }

        // Datas

        //private List<Song> songs = new List<Song>(listSize);
        private Song[] songs;
		private string kfcPath;
		private bool loaded = false;

        private Dictionary<string, string> typeAttr = new Dictionary<string, string>();
        private Dictionary<int, int> idToIfs;

        private Dictionary<string, int> kshPathToId = new Dictionary<string, int>();
        private Dictionary<int, string> lastModifiedFolder = new Dictionary<int, string>();
    }
}
