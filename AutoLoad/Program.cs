using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

using SongList;

namespace AutoLoad
{
    static class Program
    {
        //readonly static string kfcPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        readonly static string kfcPath = @"E:\CHIKAN\ks_to_SDVX\Minimal SDVX HH for FX testing\";
        readonly public static string binPath = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location
            ) + "\\";
        readonly public static string toolsPath = binPath + @"tools\";
        readonly public static string cachePath = binPath + @"cache\";

        static void Main(string[] args)
        {
            if (!File.Exists(kfcPath + "soundvoltex.dll"))
                throw new Exception("soundvoltex.dll not found! This should be executed in a KFC directory.");

            if (Directory.Exists(cachePath))
                Directory.Delete(cachePath, true);
            Directory.CreateDirectory(cachePath);

            MetaInfo metaDb = new MetaInfo(kfcPath);

            SongList.SongList songList = new SongList.SongList();

            songList.LoadFromKshSong(kfcPath, metaDb.IdToIfs(), metaDb.IdToVer(), metaDb.TypeAttr());

            songList.Save();
            //songList.SaveTexture();
        }


    }

    class MetaInfo
    {
        Dictionary<int, int> idToIfs;
        Dictionary<int, int> idToVer;
        Dictionary<string, string> typeAttr = new Dictionary<string, string>();

        public MetaInfo(string kfcPath)
        {
            string mataDbPath = kfcPath + "\\data\\others\\meta_usedId.xml";

            idToIfs = new Dictionary<int, int>();
            idToVer = new Dictionary<int, int>();
            typeAttr = new Dictionary<string, string>();

            //==========Input==========

            if (File.Exists(mataDbPath))
            {
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
                // Parse Used Ids

                string dbPath = kfcPath + "\\data\\others\\music_db.xml";
                XElement root = XElement.Load(dbPath);

                List<int> usedId = new List<int>();

                foreach (XElement songXml in root.Elements("music"))
                {
                    usedId.Add(int.Parse(songXml.Attribute("id").Value));
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

                string[] jacketIfsFiles = Directory.GetFiles(kfcPath + "\\data\\graphics\\", "s_jacket*.ifs");
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
            FileInfo currentFile = new FileInfo(ifsPath);
            string cachePath = Program.cachePath + currentFile.Name;
            currentFile.CopyTo(cachePath);
            FileInfo cacheFile = new FileInfo(cachePath);

            string fileName = Path.GetFileNameWithoutExtension(cacheFile.Name);

            List<int> list = new List<int>();

            
            string dumpImgFSPath = cachePath;
            string dumpImgFSOutPath = Program.binPath + fileName + "_imgfs\\";
            string dumpImgFSOutPath2 = Program.cachePath + fileName + "_imgfs\\";

            Execute(Program.toolsPath + "dumpImgFS.exe", dumpImgFSPath);

            DirectoryInfo currentDirectory = new DirectoryInfo(dumpImgFSOutPath);
            currentDirectory.MoveTo(dumpImgFSOutPath2);

            string tex2tgaPath = Program.cachePath + fileName + "_imgfs\\tex\\texturelist.xml";
            string tex2tgaOutPath = Program.binPath + fileName + "_imgfs_tex\\";
            string tex2tgaOutPath2 = Program.cachePath + fileName + "_imgfs_tex\\";

            Execute(Program.toolsPath + "tex2tga.exe", tex2tgaPath);

            DirectoryInfo currentDirectory2 = new DirectoryInfo(tex2tgaOutPath);
            currentDirectory2.MoveTo(tex2tgaOutPath2);

            string tgaPath = tex2tgaOutPath2 + "tex000\\";

            foreach (string file in Directory.GetFiles(tgaPath, "jk_*_*_*.tga"))
            {
                string[] tokens = file.Split('_');
                list.Add(int.Parse(tokens[tokens.Length - 2]));
            }

            return list;
        }

        static void Execute(string exe, string arg)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = exe;
            startInfo.Arguments = arg;

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public Dictionary<int, int> IdToIfs() { return idToIfs; }
        public Dictionary<int, int> IdToVer() { return idToVer; }
        public Dictionary<string, string> TypeAttr() { return typeAttr; }

        //public int IfsId(int id) { return idToIfs[id]; }
        //public bool ContainsId(int id) { return idToIfs.ContainsKey(id); }
    }
}

