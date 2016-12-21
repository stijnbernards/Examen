using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Server.Datasets;
using Rest.Web;
using Rest.Database;
using Rest.Server;

namespace Server.Pages
{
    class AdminAuth : Page
    {
        private Tuple<string, string> auth;
        private string result;

        public override void Init(HttpListenerContext ctx)
        {
            this.ContentType = "application/json";
            string guid;

            this.auth = Tuple.Create<string, string>(_POST["customer_email"], _POST["customer_password"]);

            Model model = DB.GetModel("core_customer");
            core_customer[] user = model.Select("*").AddFieldToFilter("customer_email", Tuple.Create<string, Expression>("eq", new Expression(this.auth.Item1))).AddFieldToFilter("customer_password", Tuple.Create<string, Expression>("eq", new Expression(this.auth.Item2))).AddFieldToFilter("is_admin", Tuple.Create<string, Expression>("eq", new Expression("1"))).Load().ToDataSet<core_customer>();
            if (user.Length > 0)
            {
                guid = Guid.NewGuid().ToString();
                if (Sessions.sessions.ContainsKey(user.First().customer_id))
                {
                    this.result = "{ \"status\": " + "\"User already logged in!\"" + "}";
                }
                else
                {
                    lock (Sessions.sessions)
                    {
                        Sessions.sessions.Add(user[0].customer_id, Tuple.Create<string, string, DateTime>(ctx.Request.RemoteEndPoint.Address.ToString(), guid, DateTime.Now));
                    }

                    string cookieDate = DateTime.UtcNow.AddMinutes(60d).ToString("dddd, dd-MM-yyyy hh:mm:ss GMT");

                    ctx.Response.Headers.Add("Set-Cookie", $"guid={guid}; expires={cookieDate}; path=/");

                    this.result = user[0].ToJSON(new List<string>() { "\"key\" : \"" + guid + "\"" });

                    if (this._POST.ContainsKey("_REDIRECT"))
                    {
                        this.Location = _POST["_REDIRECT"];
                        _POST.Remove("_REDIRECT");
                    }
                }
            }
            else
            {
                this.result = Constants.STATUS_FALSE;
                this.Location = "/admin";
            }
        }

        public override string Send()
        {
            return this.result;
        }
    }
}
