using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using Utility;

namespace SongList
{
    class FshImage
    {
        public FshImage(string path)
        {
            this.path = path;
        }

        public void ToTga(string outputPath, int pixel)
        {
            FileInfo fi = new FileInfo(path);
            if (!File.Exists(path)) throw new Exception();

            string fName = Path.GetFileNameWithoutExtension(path);
            string dName = fi.DirectoryName;
            string tgacache = dName + "\\" + fName + ".tga" + pixel.ToString();
            if (File.Exists(tgacache))
            {
                FileInfo cacheFi = new FileInfo(tgacache);
                cacheFi.MoveTo(outputPath);
                return;
            }

            string name = Util.cachePath + Util.RandomString(20);

            // Image to Png (from bmp or jpg)

            //if ((fi.Extension == ".bmp") || (fi.Extension == ".jpg"))
            //{
            Bitmap bmp = ResizeImage(new Bitmap(path), pixel, pixel);
            bmp.Save(name + ".png", ImageFormat.Png);
            //}

            // Png to Tga

            FileInfo fio = new FileInfo(outputPath);

            Util.Execute("tools\\png2tga.exe", "-i \"" + name + ".png" + "\" -o \"" + Util.cachePath + "\"");
            FileInfo fiOri = new FileInfo(name + ".tga");
            fiOri.CopyTo(tgacache);
            fiOri.MoveTo(outputPath);

            // dummy
            /*
            FileInfo fi = new FileInfo(Util.toolsPath + "dummy" + pixel.ToString() + ".tga");
            fi.CopyTo(outputPath);
            */
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public string Name()
        {
            FileInfo fi = new FileInfo(path);
            return fi.Name;
        }

        string path;
        string name;
    }
}
