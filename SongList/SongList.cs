using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Xml.Linq;

using IfsParse;
using Utility;

namespace SongList
{
	public class SongList
	{
        private readonly static int listSize = 1061;
		public readonly static string[] DIFS = { "novice", "advanced", "exhaust", "infinite" };

        private Object songsLock = new Object();

        public SongList()
		{
            songs = new Song[listSize];
            songsIdOccupied = new bool[listSize];
            for (int id = 0; id < listSize; ++id)
            {
                songs[id] = new Song();
                songsIdOccupied[id] = false;
            }
            Directory.CreateDirectory(Util.cachePath);
		}

		~SongList()
		{
			//Directory.Delete(cachePath, true);
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
                songsIdOccupied[id] = true;

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

        public void LoadFromKshSongs(bool forceReload, bool forceMeta)
        {
            Clear();

            if (forceMeta)
                File.Delete(Util.kfcPath + "data\\others\\meta_usedId.xml");

            MetaInfo metaDb = new MetaInfo();

            this.typeAttr = metaDb.TypeAttr();
            this.idToIfs = metaDb.IdToIfs();
            this.idToVer = metaDb.IdToVer();

            string kshFolderPath = Util.kfcPath + "KshSongs\\";
            DirectoryInfo kshFolder = new DirectoryInfo(kshFolderPath);

            if (!(metaDb.FirstLoad() || forceReload))
            {
                Load(Util.kfcPath);

                // Check for oldid existance

                for (int id = 0; id < listSize; ++id)
                {
                    if (songs[id].Data("custom") == "1")
                    {
                        DirectoryInfo di = new DirectoryInfo(songs[id].Data("kshFolder"));
                        DirectoryInfo[] dis = kshFolder.GetDirectories();
                        Dictionary<string, bool> disb = new Dictionary<string, bool>();

                        foreach (DirectoryInfo existDi in dis)
                            disb.Add(existDi.FullName, true);

                        if (!disb.ContainsKey(di.FullName)) // Ksh Folder removed!
                        {
                            DeleteId(id);
                            lastModifiedFolder.Remove(id);
                            kshPathToId.Remove(di.FullName);
                            songsIdOccupied[id] = false;
                        }
                    }
                }
            }

            List<string> kshPaths = new List<string>();
            foreach (DirectoryInfo kshSong in kshFolder.GetDirectories())
                kshPaths.Add(kshSong.FullName);
            
            LoadKshSongs(kshPaths.ToArray());
        }

        public void LoadKshSongs(string[] paths)
        {
            //List<Task> tasks = new List<Task>();
            Dictionary<string, Task> tasks = new Dictionary<string, Task>();

            foreach (string kshPath in paths)
            {
                if (kshPathToId.ContainsKey(kshPath)) // cache hit!
                {
                    int oldId = kshPathToId[kshPath];
                    //Util.ConsoleWrite("Old: " + songs[kshPathToId[kshSong.FullName]].Data("title_name"));
                    continue; // Skips the entire loading
                }

                DirectoryInfo kshSong = new DirectoryInfo(kshPath);

                if (kshSong.GetFiles("*.ksh").Length == 0)
                    //throw new Exception("Invalid folder in KshSongs!");
                    continue;

                int newId = -1;
                foreach (KeyValuePair<int, int> idPair in idToIfs)
                    if ((!songsIdOccupied[idPair.Key]) && (idPair.Key >= 256))
                    {
                        songsIdOccupied[idPair.Key] = true;
                        newId = idPair.Key;
                        break;
                    }

                            

                if (newId == -1) throw new Exception("Song DB Full!");

                Util.ConsoleWrite("New: " + kshSong.Name);

                Task task = Task.Run(() => AddKshSong_Task(kshSong.FullName, newId, idToVer[newId]));
                tasks.Add(kshSong.Name, task);
            }
            
            //foreach (Task task in tasks)
            foreach (KeyValuePair<string, Task> task in tasks)
                try
                {
                    task.Value.Wait();
                }
                catch (AggregateException ae)
                {
                    foreach (var e in ae.InnerExceptions)
                    {
                        Util.ConsoleWrite("*** Exception encountered while loading new song " + task.Key + " ***");
                        Util.ConsoleWrite(e.Message);
                    }
                }
        }

        private void Clear()
        {
            for (int i = 0; i < listSize; ++i)
                songs[i] = new Song();
        }

		public void AddKshSong_Task(string path, int newId, int ver = 4)
		{
            Song newSong = new Song(newId.ToString(), path, ver);

            lock (songsLock)
            {
                songs[newId] = newSong;
            }
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
            // Write .vox and 2dx
            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;

                try
                {
                    if (!IsUnmoddedCustom(id))
                        song.Save(kfcPath);
                }
                catch (Exception e)
                {
                    Util.ConsoleWrite("*** Exception encountered while saving song " + song.Data("title_name") + " ***");
                    Util.ConsoleWrite(e.Message);

                    songs[id] = new Song();
                }
            }

            // Write to db

            string dbPath = kfcPath + "\\data\\others\\music_db.xml";

            XElement root = new XElement("mdb");
            XDocument xmlFile = new XDocument(
                new XDeclaration("1.0", "shift-jis", "yes"),
                root
            );

            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;

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

            List<Task> tasks = new List<Task>();

            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;
                if (IsUnmoddedCustom(id)) continue;

                string texPath = texPaths[idToIfs[id]];

                tasks.Add(song.ImageToTex("jk_" + song.BaseNameNum() + "_1.tga", texPath + "tex\\", 202));
            }

            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;
                if (IsUnmoddedCustom(id)) continue;

                string[] sufs = { "_b", "_s" };
                foreach (string suf in sufs)
                {
                    string ifsPath = Util.kfcPath + "data\\graphics\\jk\\jk_" + song.BaseNameNum() + "_1" + suf + ".ifs";

                    string texPath = Util.IfsToTex(ifsPath);

                    string tgaName = "jk_" + song.BaseNameNum() + "_1" + suf + ".tga";

                    if (suf == "_b")
                        tasks.Add(song.ImageToTex(tgaName, texPath + "tex\\", 452));
                    else if (suf == "_s")
                        tasks.Add(song.ImageToTex(tgaName, texPath + "tex\\", 74));
                    else
                        throw new Exception();
                }
            }

            // Tex conversion threads sync.
            foreach (Task task in tasks)
                task.Wait();

            for (int id = 0; id < listSize; ++id)
            {
                Song song = songs[id];
                if (song.IsDummy()) continue;
                if (IsUnmoddedCustom(id)) continue;

                string[] sufs = { "_b", "_s" };
                foreach (string suf in sufs)
                {
                    string ifsPath = Util.kfcPath + "data\\graphics\\jk\\jk_" + song.BaseNameNum() + "_1" + suf + ".ifs";
                    string texPath = Util.IfsPathToTexPath(ifsPath);

                    Util.TexToIfs(texPath, ifsPath);
                }
            }

            foreach (int ifsId in targetIfs)
            {
                // DO: tex -> ifs

                string texPath = texPaths[ifsId];

                string ifsPath = Util.kfcPath + "data\\graphics\\s_jacket" + ifsId.ToString().PadLeft(2, '0') + ".ifs";

                Util.TexToIfs(texPath, ifsPath);
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

        public static string GetCachePath() { return Util.cachePath; }

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

        private volatile Song[] songs;
        private bool[] songsIdOccupied;
        private string kfcPath;
		private bool loaded = false;

        private Dictionary<string, string> typeAttr = new Dictionary<string, string>();
        private Dictionary<int, int> idToIfs;
        private Dictionary<int, int> idToVer;

        private Dictionary<string, int> kshPathToId = new Dictionary<string, int>();
        private Dictionary<int, string> lastModifiedFolder = new Dictionary<int, string>();
    }
}
