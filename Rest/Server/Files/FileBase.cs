using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.Web;

namespace Rest.Server.Files
{
    public class FileBase
    {
        public string data;

        public FileBase()
        {
            
        }

        public FileBase(string filePath)
        {
            if (File.Exists(FileManager.BaseDir + filePath))
            {
                data = File.ReadAllText(FileManager.BaseDir + filePath);
            }
            else
            {
                data = Constants.STATUS_FALSE;
            }
        }

        public FileBase(FileBase fb)
        {
            data = fb.data;
        }
    }
}
