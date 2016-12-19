using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.Server.Files;

namespace Rest.Server
{
    public class FileManager
    {
        public static string BaseDir = @"C:\htdocs\";

        private static readonly Dictionary<string, HTMLFile> files = new Dictionary<string, HTMLFile>();
        private static readonly Dictionary<string, CSSFile> cssFiles = new Dictionary<string, CSSFile>();
        private static readonly Dictionary<string, ImageFile> imageFiles = new Dictionary<string, ImageFile>();

        public static HTMLFile LoadFile(string fileName)
        {
            if (RestServer.Developer)
            {
                return new HTMLFile(fileName);
            }

            if (files.ContainsKey(fileName))
            {
                return new HTMLFile(files[fileName]);
            }
            else
            {
                files.Add(fileName, new HTMLFile(fileName));

                return new HTMLFile(files[fileName]);
            }
        }

        public static string LoadCSSFile(string fileName)
        {
            if (RestServer.Developer)
            {
                return new CSSFile(fileName).data;
            }

            if (cssFiles.ContainsKey(fileName))
            {
                return new CSSFile(cssFiles[fileName]).data;
            }
            else
            {
                cssFiles.Add(fileName, new CSSFile(fileName));

                return new CSSFile(cssFiles[fileName]).data;
            }
        }

        public static ImageFile LoadImagefile(string fileName)
        {
            if (RestServer.Developer)
            {
                return new ImageFile(fileName, Path.GetExtension(fileName));;
            }

            if (imageFiles.ContainsKey(fileName))
            {
                return new ImageFile(imageFiles[fileName]);;
            }
            else
            {
                imageFiles.Add(fileName, new ImageFile(fileName, Path.GetExtension(fileName)));

                return new ImageFile(imageFiles[fileName]);
            }
        }

        public static string AssetsDir(string subDir)
        {
            return @"http://localhost\assets\" + subDir + @"\";
        }
    }
}
