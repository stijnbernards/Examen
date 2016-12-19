using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Server.Files
{
    public class CSSFile : FileBase
    {
        public CSSFile(string filePath) : base(filePath)
        {
        }

        public CSSFile(FileBase fb) : base(fb)
        {
        }
    }
}
