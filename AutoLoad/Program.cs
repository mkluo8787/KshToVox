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

            bool doTextures = false;
            bool forceReload = false;
            bool forceMeta = false;

            OptionSet p = new OptionSet() {
                { "p|path=", "The {PATH} of KFC directory.",
                   v => Util.SetKfcPath(v) },
                { "t|texture",  "Do the texture replacement (which takes a long time).",
                   v => doTextures = v != null },
                { "f|force-reload",  "Force reload all songs.",
                   v => forceReload = v != null },
                { "fm|force-meta",  "Force reload meta DB and all songs.",
                   v => forceMeta = v != null }
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
 |_|\_\___/_| |_|_|\___/ \/ \___/_/\_\ Author: NTUMG

");

            // Check if kfc dll exists.
            if (!File.Exists(Util.kfcPath + "soundvoltex.dll"))
            {
                Console.WriteLine("soundvoltex.dll not found! Please choose a valid KFC path.");
                Console.ReadKey();
                return;
            }

            // Check if folders exist.
            if (!Directory.Exists(Util.kfcPath + "KshSongs\\"))
                Directory.CreateDirectory(Util.kfcPath + "KshSongs\\");

            if (!Directory.Exists(Util.kfcPath + "data\\others\\vox\\"))
                Directory.CreateDirectory(Util.kfcPath + "data\\others\\vox\\");

            Util.ClearCache();

            // DB backup (for later restore)
            Util.DbBackup();

            SongList.SongList songList = new SongList.SongList();

            Util.ConsoleWrite("Loading from KshSongs...");

            try
            {
                songList.LoadFromKshSongs(  forceReload,
                                            forceMeta);
            }
            catch (Exception e)
            {
                Util.ConsoleWrite("*** Exception encountered while loading from KshSongs. ***");
                Util.ConsoleWrite(e.Message);

                Util.DbRestore();

                Console.ReadKey();

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
                
                Util.ConsoleWrite("*** Fatal: Exception encountered while saving ***");
                Util.ConsoleWrite(e.Message);

                File.Delete(Util.kfcPath + "\\data\\others\\music_db.xml");
                File.Delete(Util.kfcPath + "\\data\\others\\meta_usedId.xml");

                Util.ConsoleWrite(@"*** Please force reload with '--f' ***");

                Console.ReadKey();

                return;
            }

            Util.ClearCache();

            if (doTextures)
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

            Util.ConsoleWrite(@"
/////////////////////////////////////////////////
///                                           ///
/// Loading Done! Press any key to proceed... ///
///                                           ///
/////////////////////////////////////////////////
");
            Console.ReadKey();
        }
    }

}

