using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Server;
using System.Reflection;

namespace Rest.Web
{
    public class Page
    {
        public Dictionary<string, string> _POST { get; set; }
        public Dictionary<string, string> _GET { get; set; }
        public Head Headers { get; set; }
        public string ContentType { get { return contentType; } set { contentType = value; } }
        public string HTTPMethod { get; set; }
        public int StatusCode = 200;
        public Type Dataset { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
        public string response = "";
        public virtual void Init(HttpListenerContext ctx = null) { }
        public virtual void Load() { }
        public virtual string Send() { return BuildHead() + response + BuildFooter(); }
        public virtual string Send(string response) { return this.response; }

        public virtual void CleanUp() { Clear(); }

        private string contentType = Constants.CONTENT_JSON;
        private List<string> headLines = new List<string>();

        private string BuildHead()
        {
            if (contentType == Constants.CONTENT_HTML)
            {
                string head = "<html><head><link rel=\"shortcut icon\" type=\"image/x-icon\" href=\"/assets/images/favicon.ico\" /><link rel=\"icon\" type=\"image/x-icon\" href=\"/assets/images/favicon.ico\" /> ";

                foreach (string line in headLines)
                {
                    head += line;
                }

                head += "</head><body id=\"C#-WebServer\">";

                return head;
            }

            return "";
        }

        private string BuildFooter()
        {
            if (contentType == Constants.CONTENT_HTML)
            {
                return "</body></html>";
            }

            return "";
        }

        public void LoadBootStrap()
        {
            AddCss("bootstrap.min.css");
            AddCss("bootstrap-theme.min.css");
            AddJs("jquery-3.1.1.min.js");
            AddJs("bootstrap.min.js");
        }

        public void Clear()
        {
            this.headLines.Clear();
        }

        public void AddCss(string cssPath)
        {
            headLines.Add(string.Format("<link rel=\"stylesheet\" href=\"{0}\">", FileManager.AssetsDir("css") + cssPath));
        }

        public string GetImagePath(string imageName)
        {
            return FileManager.AssetsDir("images") + imageName;
        }

        public string GetUrl(string url)
        {
            return "/" + url;
        }

        public void AddJs(string jsPath)
        {
            headLines.Add(string.Format("<script src=\"{0}\"></script>", FileManager.AssetsDir("js") + jsPath));
        }

        public void AddResponse(string data)
        {
            response += data;
        }

        public void Process()
        {
            string primary_key = "ID";

            foreach (PropertyInfo prop in Dataset.GetProperties())
            {
                if (prop.GetCustomAttribute<PrimaryKey>() != null)
                {
                    primary_key = prop.Name;
                    break;
                }   
            }

            if (this._POST.ContainsKey("_METHOD"))
            {
                this.HTTPMethod = _POST["_METHOD"];
                _POST.Remove("_METHOD");
            }

            if (this._POST.ContainsKey("_REDIRECT"))
            {
                this.Location = _POST["_REDIRECT"];
                _POST.Remove("_REDIRECT");
            }

            Table table;
            int UID = 0;
            int colID = 0;

            if (this._POST != null)
            {
                if (this.HTTPMethod != Constants.METHOD_PUT && this.HTTPMethod != Constants.METHOD_PATCH && HTTPMethod != Constants.METHOD_DELETE)
                {
                    UID = Sessions.sessions.Where(x => x.Value.Item2 == Headers.Session)
                            .Select(x => x.Key)
                            .Cast<int>()
                            .First();

                    if (UID <= 0)
                    {
                        colID = 0;
                        //fix this shit
                        try
                        {
                            if (this.HTTPMethod == Constants.METHOD_GET && this._GET["1"] != "all" &&
                                this._GET["1"] != "info")
                                colID = Convert.ToInt16(this._GET["1"]);
                            else if (this.HTTPMethod != Constants.METHOD_GET)
                                colID = Convert.ToInt16(this._POST["ID"]);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    else
                        colID = UID;
                }

                Model model = DB.GetModel(this.Dataset.Name);

                if (this.HTTPMethod == Constants.METHOD_PATCH)
                {
                    table = model.Select("*").AddFieldToFilter(primary_key, Tuple.Create<string, Expression>("eq", new Expression(_POST["ROW_ID"]))).Load();
                    if (((DataSet[])typeof(Table).GetMethod("ToDataSet").MakeGenericMethod(this.Dataset).Invoke(table, Type.EmptyTypes)).Length > 0)
                    {
                        model = model.Update();
                        foreach (KeyValuePair<string, string> kv in this._POST)
                        {
                            if (kv.Key == "ROW_ID")
                            {
                                continue;
                            }

                            KeyValuePair<string, string> kv2 = new KeyValuePair<string, string>(kv.Key, kv.Value.Replace("+", " "));
                            model = model.AddKeyValueToUpdate(kv2);
                        }

                        model.AddFieldToFilter(primary_key, Tuple.Create<string, Expression>("eq", new Expression(_POST["ROW_ID"]))).Load();

                        this.ContentType = Constants.CONTENT_JSON;
                        this.response = Constants.STATUS_TRUE;
                    }
                }
                else if(this.HTTPMethod == Constants.METHOD_PUT)
                {
                    Model insertModel = model.Insert();

                    foreach (KeyValuePair<string, string> kv in this._POST)
                    {
                        KeyValuePair<string, string> kv2 = new KeyValuePair<string, string>(kv.Key, kv.Value.Replace("+", " "));
                        insertModel.AddDataToInsert(kv2);
                    }

                    insertModel.Load();

                    table = model.Select("*").AddFieldToFilter(primary_key, Tuple.Create<string, Expression>("eq", new Expression("LAST_INSERT_ID()", false))).Load();

                    dynamic[] result = ((dynamic[])typeof(Table).GetMethod("ToDataSet").MakeGenericMethod(this.Dataset).Invoke(table, Type.EmptyTypes));

                    this.ContentType = Constants.CONTENT_JSON;
                    this.response = result.First().ToJSON();
                }
                else if (this.HTTPMethod == Constants.METHOD_DELETE)
                {
                    model.Delete().AddFieldToFilter(primary_key, Tuple.Create<string, Expression>("eq", new Expression(_POST["ROW_ID"]))).Load();

                    this.ContentType = Constants.CONTENT_JSON;
                    this.response = Constants.STATUS_TRUE;
                }
                else if (this.HTTPMethod == Constants.METHOD_GET)
                {
                    model = DB.GetModel(this.Dataset.Name);
                    if (this._GET["1"] == "all")
                    {
                        int[] limit = this._GET.Count >= 3 ? new int[] { Convert.ToInt16(this._GET["2"]), Convert.ToInt16(this._GET["3"]) } : null;


                        model = model.Select("*");
                        if (limit != null)
                            model = model.SetLimit(limit[0], limit[1]);

                        table = model.Load();

                        dynamic[] result = ((dynamic[])typeof(Table).GetMethod("ToDataSet").MakeGenericMethod(this.Dataset).Invoke(table, Type.EmptyTypes));

                        this.ContentType = Constants.CONTENT_JSON;
                        this.response = "[";
                        
                        result.ToList<dynamic>().ForEach(x => this.response += x.ToJSON() + ",");
                        
                        this.response = this.response.Remove(this.response.Length - 1);
                        
                        this.response += "]";

                        if (result.Length <= 0)
                            this.response = Constants.STATUS_FALSE;
                    }
                    else if(int.TryParse(this._GET["1"], out colID))
                    {
                        //colID = Convert.ToInt16(this._GET["1"]);

                        table = model.Select("*").AddFieldToFilter(primary_key, Tuple.Create<string, Expression>("eq", new Expression(colID.ToString()))).Load();
                        dynamic[] result = ((dynamic[])typeof(Table).GetMethod("ToDataSet").MakeGenericMethod(this.Dataset).Invoke(table, Type.EmptyTypes));

                        this.ContentType = Constants.CONTENT_JSON;

                        if (result.Length > 0)
                            this.response = result.First().ToJSON();
                        else
                            this.response = Constants.STATUS_FALSE;
                    }
                    else if (this._GET["1"].Contains("info"))
                    {
                        table = model.Select("COUNT(*) as `records`").Load();

                        Dictionary<string, string> result = table.ToDict();

                        if (result.Count > 0)
                            this.response = result.ToJSON();
                    }
                }
            }
        }
    }
}
