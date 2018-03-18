using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Security.Cryptography;

namespace GenerateUpdateXML
{
    static public class GenerateUpdateXML
    {
        private static String xml_dir = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..");
        private static String xml_path = Path.Combine(xml_dir, @"KshToVox_Update.xml");
        private static String exe_path = Path.Combine(xml_dir, @"KshToVox.window\bin\Release\KshToVox.window.exe");
        private static String exe_name = Path.GetFileName(exe_path);
        private static String exe_name_no_ext = Path.GetFileNameWithoutExtension(exe_path);
        private static String exe_URL = @"https://github.com/MKLUO/KshToVox/raw/master/KshToVox.window/bin/Release/KshToVox.window.exe";
        private static String exe_MD5 = GetMd5(exe_path);
        private static String exe_ver = FileVersionInfo.GetVersionInfo(exe_path).ProductVersion;

        static XmlDocument doc;
        static XmlNode update;

        static public void Generate()
        {
            doc = new XmlDocument();
            XmlNode root;

            root = doc.CreateElement("sharpUpdate");
            doc.AppendChild(root);

            update = doc.CreateElement("update");
            XmlAttribute attribute = doc.CreateAttribute("appID");
            attribute.Value = exe_name_no_ext;
            update.Attributes.Append(attribute);
            root.AppendChild(update);

            appendNode("version", exe_ver);
            appendNode("url", exe_URL);
            appendNode("fileName", exe_name);
            appendNode("md5", exe_MD5);
            appendNode("launchArgs", "");

            appendNode("description", "");
            doc.Save(xml_path);

            Environment.Exit(1);
        }

        static String GetMd5(String path)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            FileStream stream = File.Open(path, FileMode.Open);
            md5.ComputeHash(stream);
            stream.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5.Hash.Length; i++)
            {
                sb.Append(md5.Hash[i].ToString("x2"));
            }

            return sb.ToString().ToUpper();
        }

        static void appendNode(String nodeName, String value)
        {
            XmlNode version = doc.CreateElement(nodeName);
            version.InnerText = value;
            update.AppendChild(version);
        }
    }
}
