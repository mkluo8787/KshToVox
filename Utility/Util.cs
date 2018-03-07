﻿using System;
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

        public static void ClearCache()
        {
            if (Directory.Exists(Util.cachePath))
                Directory.Delete(Util.cachePath, true);
            Directory.CreateDirectory(Util.cachePath);
        }

        public static void DbBackup()
        {
            FileInfo musicDbFile = new FileInfo(Util.kfcPath + "\\data\\others\\music_db.xml");
            FileInfo metaDbFile = new FileInfo(Util.kfcPath + "\\data\\others\\meta_usedId.xml");
            musicDbFile.CopyTo(Util.cachePath + "music_db.xml");
            metaDbFile.CopyTo(Util.cachePath + "meta_usedId.xml");
        }

        public static void DbRestore()
        {
            FileInfo musicDbFile = new FileInfo(Util.cachePath + "music_db.xml");
            FileInfo metaDbFile = new FileInfo(Util.cachePath + "meta_usedId.xml");
            File.Delete(Util.kfcPath + "\\data\\others\\music_db.xml");
            File.Delete(Util.kfcPath + "\\data\\others\\meta_usedId.xml");
            musicDbFile.CopyTo(Util.kfcPath + "\\data\\others\\music_db.xml");
            metaDbFile.CopyTo(Util.kfcPath + "\\data\\others\\meta_usedId.xml");
        }

        public static void setKfcPath(string newPath)
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
