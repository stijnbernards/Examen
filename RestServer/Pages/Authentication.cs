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
    class Authentication : Page
    {
        private Tuple<string, string> auth;
        private string result;

        public override void Init(HttpListenerContext ctx)
        {
            this.ContentType = "application/json"; 
            List<string> authstr;
            string guid;

            authstr = Encoding.UTF8.GetString(Convert.FromBase64String(Headers.Authentication)).Split(':').ToList<string>();
            this.auth = Tuple.Create<string, string>(authstr[0], authstr[1]);

            Model model = DB.GetModel("users");
            core_customer[] user = model.Select("*").AddFieldToFilter("customer_name", Tuple.Create<string, Expression>("eq", new Expression(this.auth.Item1))).AddFieldToFilter("customer_password", Tuple.Create<string, Expression>("eq", new Expression(this.auth.Item2))).Load().ToDataSet<core_customer>();
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

                    this.result = user[0].ToJSON(new List<string>() { "\"key\" : \"" + guid + "\"" });
                }
            }
            else
            {
                this.result = Constants.STATUS_FALSE;
            }
        }

        public override string Send()
        {
            return this.result;
        }
    }
}
