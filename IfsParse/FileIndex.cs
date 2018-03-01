using System.IO;

namespace IfsParse
{
    internal class FileIndex {
        private readonly Stream _stream;

        private readonly int _index;
        internal readonly int Size;
        internal readonly int EntryNumber;

        private string fName;

        internal FileIndex(string fName, Stream stream, int index, int size, int entryNumber)
        {
            _stream = stream;
            EntryNumber = entryNumber;
            Size = size;
            _index = index;

            this.fName = fName;
        }

        internal string FileName() { return fName; }

        internal byte[] Read()
        {
            _stream.Seek(_index, SeekOrigin.Begin);
            var r = new byte[Size];
            _stream.Read(r, 0, Size);
            return r;
        }
    }
}
