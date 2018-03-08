using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

using NDesk.Options;

using SongList;
using Utility;

namespace AutoLoad
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Arguments

            bool skipTextures = false;
            bool forceRaload = false;

            OptionSet p = new OptionSet() {
                { "p|path=", "The {PATH} of KFC directory.",
                   v => Util.setKfcPath(v) },
                { "t|texture",  "Do the texture replacement (which takes a long time).",
                   v => skipTextures = v != null },
                { "f|force-reload",  "Force reload meta DB and all songs.",
                   v => forceRaload = v != null }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("AutoLoad: ");
                Console.WriteLine(e.Message);
                return;
            }

            // Welcome message

            Util.ConsoleWrite(@"
  _  __    _  _________      __       
 | |/ /   | ||__   __\ \    / /       
 | ' / ___| |__ | | __\ \  / /____  __
 |  < / __| '_ \| |/ _ \ \/ / _ \ \/ /
 | . \\__ \ | | | | (_) \  / (_) >  <  March 2018. Alpha version
 |_|\_\___/_| |_|_|\___/ \/ \___/_/\_\ Author: Iced & MKLUO

");

            // Check if kfc dll exists.
            if (!File.Exists(Util.kfcPath + "soundvoltex.dll"))
            {
                Console.WriteLine("soundvoltex.dll not found! Please choose a valid KFC path.");
                return;
            }

            Util.ClearCache();

            // DB backup (for later restore)
            Util.DbBackup();

            if (forceRaload)
                File.Delete(Util.kfcPath + "data\\others\\meta_usedId.xml");

            Util.ClearCache();

            MetaInfo metaDb = new MetaInfo();

            SongList.SongList songList = new SongList.SongList();

            Util.ConsoleWrite("Loading from KshSongs...");

            try
            {
                songList.LoadFromKshSong(Util.kfcPath,
                                            metaDb.IdToIfs(),
                                            metaDb.IdToVer(),
                                            metaDb.TypeAttr(),
                                            metaDb.FirstLoad());
            }
            catch (Exception e)
            {
                Util.ConsoleWrite("*** Exception encountered while loading from KshSongs. ***");
                Util.ConsoleWrite(e.Message);

                Util.DbRestore();

                return;
            }

            Util.ConsoleWrite("Saving song...");

            try
            {
                // The KFC data could be corrupted here even if the exceptions are caught,
                // so the music_db and metaDb should be removed before abort.
                songList.Save();
            }
            catch (Exception e)
            {
                Util.ConsoleWrite("*** Fetal: Exception encountered while saving ***");
                Util.ConsoleWrite(e.Message);

                File.Delete(Util.kfcPath + "\\data\\others\\music_db.xml");
                File.Delete(Util.kfcPath + "\\data\\others\\meta_usedId.xml");

                Util.ConsoleWrite(@"*** Please force reload with '--f' ***");

                return;
            }

            Util.ClearCache();

            if (!skipTextures)
            {
                Util.ConsoleWrite("Saving texture... (This should took a while)");

                try
                {
                    // Chart data should be fine regardless of the result of SaveTexture.
                    // No need to erase the DBs in exceptions.
                    songList.SaveTexture();
                }
                catch (Exception e)
                {
                    Util.ConsoleWrite("*** Exception encountered while saving texture ***");
                    Util.ConsoleWrite(e.Message);
                    Util.ConsoleWrite("The charts will still be saved. (Without the custom jackets)");
                }
            }

            Util.ConsoleWrite("\nLoading Done. Press any key to proceed...");
            Console.ReadKey();
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
            string mataDbPath = Util.kfcPath + "data\\others\\meta_usedId.xml";

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
                Util.ConsoleWrite("Parsing from original KFC data...");

                firstLoad = true;

                // Parse Used Ids

                string dbPath = Util.kfcPath + "\\data\\others\\music_db.xml";
                string dbOriPath = Util.kfcPath + "\\data\\others\\music_db_original.xml";
               
                if (!File.Exists(dbOriPath))
                {
                    FileInfo fi = new FileInfo(dbPath);
                    fi.MoveTo(dbOriPath);
                }

                XElement root = XElement.Load(dbOriPath);

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

