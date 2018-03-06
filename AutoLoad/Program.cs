using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

using SongList;
using Utility;

namespace AutoLoad
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists(Util.kfcPath + "soundvoltex.dll"))
                throw new Exception("soundvoltex.dll not found! This should be executed within a KFC directory.");

            ClearCache();

            MetaInfo metaDb = new MetaInfo();

            SongList.SongList songList = new SongList.SongList();

            Util.ConsoleWrite("Loading form KshSongs...");

            songList.LoadFromKshSong(   Util.kfcPath, 
                                        metaDb.IdToIfs(), 
                                        metaDb.IdToVer(), 
                                        metaDb.TypeAttr(),
                                        metaDb.FirstLoad());

            Util.ConsoleWrite("Saving song...");

            songList.Save();

            ClearCache();

            Util.ConsoleWrite("Saving texture...");

            songList.SaveTexture();

            Util.ConsoleWrite("\nLoading Done. Press any key to proceed...");
            Console.ReadKey();
        }

        static void ClearCache()
        {
            if (Directory.Exists(Util.cachePath))
                Directory.Delete(Util.cachePath, true);
            Directory.CreateDirectory(Util.cachePath);

            DirectoryInfo di = new DirectoryInfo(Util.binPath);
            foreach (FileInfo fi in di.GetFiles())
                if (!fi.Name.Contains("."))
                    File.Delete(fi.FullName);
        }
    }

    class MetaInfo
    {
        Dictionary<int, int> idToIfs;
        Dictionary<int, int> idToVer;
        Dictionary<string, string> typeAttr = new Dictionary<string, string>();

        bool firstLoad;

        public MetaInfo()
        {
            string mataDbPath = Util.kfcPath + "\\data\\others\\meta_usedId.xml";

            idToIfs = new Dictionary<int, int>();
            idToVer = new Dictionary<int, int>();
            typeAttr = new Dictionary<string, string>();

            //==========Input==========

            if (File.Exists(mataDbPath))
            {
                firstLoad = false;

                XElement inXml = XElement.Load(mataDbPath);

                foreach (XElement usedId in inXml.Elements("usedId"))
                {
                    idToIfs[int.Parse(usedId.Element("id").Value)] =
                        int.Parse(usedId.Element("ifs").Value);
                    idToVer[int.Parse(usedId.Element("id").Value)] =
                        int.Parse(usedId.Element("ver").Value);
                }

                foreach (XElement type in inXml.Element("typeAttr").Elements())
                    typeAttr[type.Name.LocalName] = type.Value;
            }
            else
            {
                Util.ConsoleWrite("Meta DB not found. Parsing from original KFC data...");

                firstLoad = true;

                // Parse Used Ids

                string dbPath = Util.kfcPath + "\\data\\others\\music_db.xml";
                XElement root = XElement.Load(dbPath);

                List<int> usedId = new List<int>();

                foreach (XElement songXml in root.Elements("music"))
                {
                    int id = int.Parse(songXml.Attribute("id").Value);
                    if (id == 840) continue;
                    usedId.Add(id);
                    idToVer[int.Parse(songXml.Attribute("id").Value)] =
                        int.Parse(songXml.Element("info").Element("version").Value);
                }

                // Parse for tag types

                foreach (XElement xe in root.Elements("music").First<XElement>().Element("info").Elements())
                    if (xe.Attribute("__type") != null)
                        typeAttr[xe.Name.LocalName] = xe.Attribute("__type").Value;

                foreach (XElement xe in root.Elements("music").First<XElement>().Element("difficulty").Element("novice").Elements())
                    if (xe.Attribute("__type") != null)
                        typeAttr[xe.Name.LocalName] = xe.Attribute("__type").Value;

                // Parse jacket ifs Ids

                string[] jacketIfsFiles = Directory.GetFiles(Util.kfcPath + "data\\graphics\\", "s_jacket*.ifs");
                foreach (string s in jacketIfsFiles)
                {
                    List<int> idList = ParseJacketIfsToIds(s);
                    int ifsId = int.Parse(s.Substring(s.Length - 6, 2));
                    foreach (int id in idList)
                        idToIfs[id] = ifsId;
                }

                //==========Output==========


                XElement outXml = new XElement("usedIds");
                foreach (int id in usedId)
                {
                    XElement item = new XElement("usedId");
                    item.Add(new XElement("id", id));
                    item.Add(new XElement("ifs", idToIfs[id]));
                    item.Add(new XElement("ver", idToVer[id]));
                    outXml.Add(item);
                }
                XElement types = new XElement("typeAttr");
                foreach (KeyValuePair<string, string> type in typeAttr)
                { 
                    XElement item = new XElement(type.Key, type.Value);
                    types.Add(item);
                }  
                outXml.Add(types);

                outXml.Save(mataDbPath);
            } 
        }

        static List<int> ParseJacketIfsToIds(string ifsPath)
        {

            string tgaPath = Util.IfsToTga(ifsPath);

            List<int> list = new List<int>();

            foreach (string file in Directory.GetFiles(tgaPath, "jk_*_*_*.tga"))
            {
                string[] tokens = file.Split('_');
                if ((tokens[tokens.Length - 1] == "1.tga") &&
                    (tokens[tokens.Length - 2] != "0840"))
                    list.Add(int.Parse(tokens[tokens.Length - 2]));
            }

            return list;
        }

        public Dictionary<int, int> IdToIfs() { return idToIfs; }
        public Dictionary<int, int> IdToVer() { return idToVer; }
        public Dictionary<string, string> TypeAttr() { return typeAttr; }

        public bool FirstLoad() { return firstLoad;  }

        //public int IfsId(int id) { return idToIfs[id]; }
        //public bool ContainsId(int id) { return idToIfs.ContainsKey(id); }
    }
}

