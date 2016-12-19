using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Rest.Web;
using Rest.Database;
using Rest.Server;

namespace Server.Pages
{
    class Search : Page
    {

        public override void Init(System.Net.HttpListenerContext ctx = null)
        {
            string searchQ = "";

            this.ContentType = Constants.CONTENT_JSON;

            if (this._POST.ContainsKey("search"))
                searchQ = this._POST["search"];

            Table table = DB.QueryString(Dataset.Name, searchQ).Load();
            dynamic[] result = (dynamic[])typeof(Table).GetMethod("ToDataSet").MakeGenericMethod(this.Dataset).Invoke(table, Type.EmptyTypes);
            this.response = "[";
            result.ToList<dynamic>().ForEach(x => { response += x.ToJSON() + ","; });
            this.response = this.response.Substring(0, this.response.Length - 1);
            this.response += "]";
        }
    }
}
