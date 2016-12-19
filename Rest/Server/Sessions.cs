using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Server
{
    public static class Sessions
    {
        public static Dictionary<int, Tuple<string, string, DateTime>> sessions = new Dictionary<int, Tuple<string, string, DateTime>>();
    }
}
