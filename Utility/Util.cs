using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Utility
{
    public static class Util
    {
        readonly public static string binPath = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location
            ) + "\\";
        public static string kfcPath = binPath; // Default KFC path.
        readonly public static string toolsPath = binPath + @"tools\";
        readonly public static string cachePath = binPath + @"cache\";

        readonly public static string musicDbPath = Util.kfcPath + "data\\others\\music_db.xml";
        readonly public static string metaDbPath = Util.kfcPath + "data\\others\\meta_usedId.xml";

        readonly public static string musicDbCachePath = cachePath + "music_db.xml";
        readonly public static string metaDbCachePath = cachePath + "meta_usedId.xml";

        public static void ClearCache()
        {
            if (Directory.Exists(Util.cachePath))
                try
                {
                    Directory.Delete(Util.cachePath, true);
                }
                catch (Exception e)
                {
                    ConsoleWrite("*** Exception encountered while clearing cache. Maybe someone is accessing it? ***");
                    ConsoleWrite(e.Message);

                    Console.ReadKey();
                }
            Directory.CreateDirectory(Util.cachePath);
        }

        public static void DbBackup()
        {   
            if (File.Exists(musicDbPath))
            {
                FileInfo musicDbFile = new FileInfo(musicDbPath);
                musicDbFile.CopyTo(musicDbCachePath);
            }
            if (File.Exists(metaDbPath))
            {
                FileInfo metaDbFile = new FileInfo(metaDbPath);
                metaDbFile.CopyTo(metaDbCachePath);
            }
        }

        public static void DbRestore()
        {
            if (File.Exists(musicDbCachePath))
            {
                FileInfo musicDbFile = new FileInfo(musicDbCachePath);
                File.Delete(musicDbPath);
                musicDbFile.CopyTo(musicDbPath);
            }
            if (File.Exists(metaDbCachePath))
            {
                FileInfo metaDbFile = new FileInfo(metaDbCachePath);
                File.Delete(metaDbPath);
                metaDbFile.CopyTo(metaDbPath);
            }
        }

        public static void CopyDirectory(string path, string newPath)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + path);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(newPath, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(newPath, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
            
        }
    

        public static void SetKfcPath(string newPath)
        {
            if (!Directory.Exists(newPath))
                throw new Exception("The path specified does not exist!");

            if (newPath[newPath.Length - 1] != '\\')
                newPath += "\\";

            kfcPath = newPath;
        }

        public static void Execute(string exe, string arg, string directory = "")
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = exe;
            startInfo.Arguments = arg;
            //startInfo.UseShellExecute = false;
            //startInfo.RedirectStandardOutput = true;

            if (directory != "")
                startInfo.WorkingDirectory = directory;

            process.StartInfo = startInfo;
            process.Start();

            //while (!process.StandardOutput.EndOfStream)
            //    Console.WriteLine(process.StandardOutput.ReadLine());

            process.WaitForExit();
        }

        public static void ConsoleWrite(string msg)
        {
            StackTrace st = new StackTrace();
            for (int i = 0; i < st.FrameCount; ++i)
                Console.Write("  ");
            Console.WriteLine(msg);
        }

        public static string IfsToTga(string ifsPath)
        {
            FileInfo currentFile = new FileInfo(ifsPath);
            string cacheFile = cachePath + currentFile.Name;
            currentFile.CopyTo(cacheFile);
            FileInfo cacheFileInfo = new FileInfo(cacheFile);

            string fileName = Path.GetFileNameWithoutExtension(cacheFileInfo.Name);

            string dumpImgFSPath = cacheFile;
            string dumpImgFSOutPath = cachePath + fileName + "_imgfs\\";

            Execute(toolsPath + "dumpImgFS.exe", "\"" + dumpImgFSPath + "\"", cachePath);

            string tex2tgaPath = cachePath + fileName + "_imgfs\\tex\\texturelist.xml";
            string tex2tgaOutPath = cachePath + fileName + "_imgfs_tex\\";

            Execute(toolsPath + "tex2tga.exe", "\"" + tex2tgaPath + "\"", cachePath);

            return tex2tgaOutPath + "tex000\\";
        }

        public static string IfsToTex(string ifsPath)
        {
            FileInfo currentFile = new FileInfo(ifsPath);
            string cacheFile = cachePath + currentFile.Name;
            currentFile.CopyTo(cacheFile);
            FileInfo cacheFileInfo = new FileInfo(cacheFile);

            string fileName = Path.GetFileNameWithoutExtension(cacheFileInfo.Name);

            string dumpImgFSPath = cacheFile;
            string dumpImgFSOutPath = cachePath + fileName + "_imgfs\\";

            Execute(toolsPath + "dumpImgFS.exe", "\"" + dumpImgFSPath + "\"", cachePath);

            return dumpImgFSOutPath;
        }

        public static void TexToIfs(string texPath, string ifsPath)
        {
            string buildImgFSPath = texPath;
            Util.Execute(toolsPath + "buildImgFS.exe", "\"" + buildImgFSPath.Remove(buildImgFSPath.Length - 1) + "\"" + " tex", cachePath);

            string temp = texPath.Remove(texPath.Length - 1) + ".ifs";
            string fileName = Path.GetFileNameWithoutExtension(temp);
            string outFileName = cachePath + fileName + ".ifs";

            FileInfo cacheFile = new FileInfo(outFileName);
            File.Delete(ifsPath);
            cacheFile.MoveTo(ifsPath);
        }

        public static void TgaToTex_Thread(string tgaPath, string texPath)
        {
            Util.Execute(toolsPath + "tga2tex.exe", "\"" + tgaPath + "\"", texPath);
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string IfsPathToTexPath(string ifsPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(ifsPath);
            string dumpImgFSOutPath2 = cachePath + fileName + "_imgfs\\";
            return dumpImgFSOutPath2;
        }
    }
}
