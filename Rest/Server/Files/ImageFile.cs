using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Rest.Server.Files
{
    public class ImageFile : FileBase
    {
        private string fileType;
        private Bitmap image;

        public ImageFile(string filePath, string type)
        {
            fileType = type.Replace(".", "");
            image = new Bitmap(FileManager.BaseDir + filePath);
        }

        public ImageFile(ImageFile fb) : base(fb)
        {
            fileType = fb.fileType;
            image = fb.image;
        }

        public string GetContentType()
        {
            string type = fileType;

            switch (fileType)
            {
                case "jpg":
                    type = "jpeg";
                    break;
            }

            return $"image/{type}";
        }

        public MemoryStream GetStream()
        {
            MemoryStream ms = new MemoryStream();

            switch (fileType)
            {
                case "jpg":
                    image.Save(ms, ImageFormat.Jpeg);
                    break;
                case "png":
                    image.Save(ms, ImageFormat.Png);
                    break;
            }

            return ms;
        }
    }
}
