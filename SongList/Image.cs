using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Utility;

namespace SongList
{
    class Image
    {
        public Image(string path)
        {
            this.path = path;
        }

        public void ToTga(string outputPath, int pixel)
        {
            // Not done yet;
            FileInfo fi = new FileInfo(Util.toolsPath + "dummy" + pixel.ToString() + ".tga");
            fi.CopyTo(outputPath);
        }

        string path;
        string name;
    }
}
