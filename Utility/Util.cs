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
        //readonly public static string kfcPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        readonly public static string kfcPath = @"E:\CHIKAN\ks_to_SDVX\Minimal SDVX HH for FX testing\";
        readonly public static string binPath = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location
            ) + "\\";
        readonly public static string toolsPath = binPath + @"tools\";
        readonly public static string cachePath = binPath + @"cache\";

        public static void Execute(string exe, string arg)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = exe;
            startInfo.Arguments = arg;
            //startInfo.UseShellExecute = false;
            //startInfo.RedirectStandardOutput = true;

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
            string dumpImgFSOutPath = binPath + fileName + "_imgfs\\";
            string dumpImgFSOutPath2 = cachePath + fileName + "_imgfs\\";

            Util.Execute(toolsPath + "dumpImgFS.exe", "\"" + dumpImgFSPath + "\"");

            DirectoryInfo currentDirectory = new DirectoryInfo(dumpImgFSOutPath);
            currentDirectory.MoveTo(dumpImgFSOutPath2);

            string tex2tgaPath = Util.cachePath + fileName + "_imgfs\\tex\\texturelist.xml";
            string tex2tgaOutPath = Util.binPath + fileName + "_imgfs_tex\\";
            string tex2tgaOutPath2 = Util.cachePath + fileName + "_imgfs_tex\\";

            Util.Execute(Util.toolsPath + "tex2tga.exe", "\"" + tex2tgaPath + "\"");

            DirectoryInfo currentDirectory2 = new DirectoryInfo(tex2tgaOutPath);
            currentDirectory2.MoveTo(tex2tgaOutPath2);

            return tex2tgaOutPath2 + "tex000\\";
        }

        public static string IfsToTex(string ifsPath)
        {
            FileInfo currentFile = new FileInfo(ifsPath);
            string cacheFile = cachePath + currentFile.Name;
            currentFile.CopyTo(cacheFile);
            FileInfo cacheFileInfo = new FileInfo(cacheFile);

            string fileName = Path.GetFileNameWithoutExtension(cacheFileInfo.Name);

            string dumpImgFSPath = cacheFile;
            string dumpImgFSOutPath = binPath + fileName + "_imgfs\\";
            string dumpImgFSOutPath2 = cachePath + fileName + "_imgfs\\";

            Util.Execute(toolsPath + "dumpImgFS.exe", "\"" + dumpImgFSPath + "\"");

            DirectoryInfo currentDirectory = new DirectoryInfo(dumpImgFSOutPath);
            currentDirectory.MoveTo(dumpImgFSOutPath2);

            return dumpImgFSOutPath2;
        }

        public static void TexToIfs(string texPath, string ifsPath)
        {
            string buildImgFSPath = texPath;
            Util.Execute(toolsPath + "buildImgFS.exe", "\"" + buildImgFSPath.Remove(buildImgFSPath.Length - 1) + "\"" + " tex");

            string temp = texPath.Remove(texPath.Length - 1) + ".ifs";
            string fileName = Path.GetFileNameWithoutExtension(temp);
            string outFileName = binPath + fileName + ".ifs";

            FileInfo cacheFile = new FileInfo(outFileName);
            File.Delete(ifsPath);
            cacheFile.MoveTo(ifsPath);
        }

        public static void TgaToTex(string tgaPath, string texPath)
        {
            DirectoryInfo di = new DirectoryInfo(binPath);
            foreach (FileInfo fi in di.GetFiles())
                if (!fi.Name.Contains("."))
                    throw new Exception("File with no extension within bin path!");

            Util.Execute(toolsPath + "tga2tex.exe", "\"" + tgaPath + "\"");
            string fName = "";
            foreach (FileInfo fi in di.GetFiles())
                if (!fi.Name.Contains("."))
                    fName = fi.FullName;

            FileInfo hashFile = new FileInfo(fName);
            File.Delete(texPath + hashFile.Name);
            hashFile.MoveTo(texPath + hashFile.Name);               
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
