using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace IfsParse
{
    public class Ifs
    {
        public enum IfsParseType
        {
            Image,
            Chart
        }

        private List<FileIndex> fileList;
        private FileStream ifsFile;

        public Ifs(string ifsPath, IfsParseType type)
        {
            ifsFile = new FileStream(ifsPath, FileMode.Open);
            fileList = new List<FileIndex>();
            switch (type)
            {
                case (IfsParseType.Image):
                    {
                        throw new Exception("Not Supporting Image Ifs!");                        
                        break;
                    }
                case (IfsParseType.Chart):
                    {
                        fileList = ParseIfsChart(ifsFile);
                        break;
                    }
            }
        }

        public void Close()
        {
            ifsFile.Close();
        }

        static List<FileIndex> ParseIfsChart(Stream stream)
        {
            //Parse for file name
            BinaryReader br = new BinaryReader(stream);
            
            stream.Position = 0x4D;

            List<string> fileNames = new List<string>();

            long pos = stream.Position;
            long endPos = 0;

            while (true)
            {
                int i = stream.ReadByte();
                if (i == 0xFE)
                {
                    endPos = stream.Position;
                    stream.Position = pos - 1;

                    string s = new string(br.ReadChars(Convert.ToInt32(endPos - pos)));
                    s = s.Replace("__","_");
                    s = s.Replace("_E", ".");
                    s = s.Replace("_D", "-");
                    if (s.ElementAt<char>(0) == '_')
                        s = s.Remove(0, 1);
                    fileNames.Add(s);

                    stream.Position = endPos;
                    if (stream.ReadByte() == 0xFE) break;

                    stream.Position = endPos + 3;
                    pos = stream.Position;
                }
            }

            //Parse for file
            stream.Seek(16, SeekOrigin.Begin);
            var fIndex = ReadInt(stream);
            stream.Seek(40, SeekOrigin.Begin);
            var fHeader = ReadInt(stream);

            if (fHeader % 4 != 0)
            {
                throw new ArgumentException("fHeader%4 != 0");
            }

            stream.Seek(fHeader + 72, SeekOrigin.Begin);

            var packet = new byte[4];
            var zeroPadArray = new byte[] { 0, 0, 0, 0 };
            var separator = new byte[] { 0, 0, 0, 0 };
            var sepInit = false;
            var zeroPad = false;
            var entryNumber = 0;

            var fileMappings = new List<FileIndex>();

            while (stream.Position < fIndex)
            {
                stream.Read(packet, 0, 4);

                /*
                if (stream.Position >= fIndex) break;

                if (!sepInit || ByteArrayEqual(separator, zeroPadArray))
                {
                    if (!ByteArrayEqual(packet, zeroPadArray))
                    {
                        packet.CopyTo(separator, 0);
                        sepInit = true;
                        continue;
                    }
                }
                else
                {
                    if (separator[0] == packet[0]) continue;

                    if (ByteArrayEqual(packet, zeroPadArray))
                    {
                        if (zeroPad) continue;
                        zeroPad = true;
                    }
                }
                */
                var index = ReadInt(packet);

                if (stream.Position >= fIndex) break;

                var size = ReadInt(stream);
                stream.Position += 4;
                if (size > 0)
                    fileMappings.Add(new FileIndex(fileNames[entryNumber], stream, fIndex + index, size, entryNumber++));
            }

            return fileMappings;
        }
    
        public void Cache(string cachePath)
        {
            foreach (FileIndex file in fileList)
            {
                FileStream fs = new FileStream(cachePath + file.FileName(), FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(System.Text.Encoding.ASCII.GetString(file.Read()));
                sw.Close();
            }
        }

        private static int ReadInt(Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            return ReadInt(bytes);
        }

        private static int ReadInt(byte[] bytes)
        {
            var r = 0;
            for (var i = 0; i < 4; ++i)
            {
                r = (r << 8) + bytes[i];
            }

            return r;
        }

        private static bool ByteArrayEqual(byte[] a, byte[] b)
        {
            var aLen = a.Length;
            if (aLen != b.Length)
            {
                return false;
            }
            for (var i = 0; i < aLen; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
