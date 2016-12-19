using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Web
{
    public static class Extension
    {
        public static string ToJSON(this Dictionary<string, string> dictionary)
        {
            List<string> json = new List<string>();

            foreach (KeyValuePair<string, string> kv in dictionary)
            {
                json.Add('"' + kv.Key.ToString() + '"' + " : " + '"' + kv.Value + '"');
            }

            return "{" + string.Join(", ", json) + "}";
        }
    }
}
