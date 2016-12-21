using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Web
{
    public struct Cache
    {
        public string Response { get; set; }
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public long ContentLength64 { get; set; }
        public int StatusCode { get; set; }
    }
}
