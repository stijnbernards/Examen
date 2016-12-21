using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rest.Web;
using Rest.Server;
using Rest.Server.Files;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Net.Http.Headers;
using Rest.Database;
using RestServer.Datasets;
using Server.Datasets;
using System.IO;

namespace RestServer.Pages
{
    public class Image : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile adminFile = FileManager.LoadFile("blocks/beheer.html");

            if (ctx.Request.Cookies["guid"] != null &&
            Sessions.sessions.ContainsKey(
                (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key)
                    .First()))
            {
                baseFile.AddData(new Dictionary<string, object>()
                {
                    {"account", GetUrl("myaccount")}
                });
            }
            else
            {
                Location = "/index";
                return;
            }

            int customerID = (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key).First();

            core_customer customer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(customerID.ToString()))).AddFieldToFilter("is_admin", new Tuple<string, Expression>("eq", new Expression("1"))).Load().ToDataSet<core_customer>().First();

            if (customer == null)
            {
                Location = "/index";
                return;
            }

            adminFile.AddData(new Dictionary<string, object>()
            {
                { "username", customer.customer_name }
            });

            if (HTTPMethod == Constants.METHOD_POST)
            {
                adminFile.AddData(new Dictionary<string, object>()
                {
                    { "content", "<h3 class=\"green\">Bestand uploaden gelukt!</h3>" }
                });
            }
            else
            {
                adminFile.AddData(new Dictionary<string, object>()
                {
                    { "content", "<form method=\"post\" action=\"\\image\" enctype=\"multipart/form-data\"><div class=\"form-group\"><input type=\"file\" name=\"file\" /></div><div class=\"form-group\"> <input type=\"submit\" name=\"submit\" /></div></form>" }
                });
            }

            baseFile.AddData(new Dictionary<string, object>()
            {
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", adminFile.GetData() }
            });

            LoadBootStrap();
            this.AddCss("styles.css");

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
