using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Server;
using Rest.Server.Files;
using Rest.Web;
using Server.Datasets;

namespace RestServer.Pages
{
    public class Admin : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile panelFile = FileManager.LoadFile("blocks/panel.html");
            HTMLFile loginFile = FileManager.LoadFile("blocks/login.html");

            if (ctx.Request.Cookies["guid"] != null &&
            Sessions.sessions.ContainsKey(
                (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key)
                    .First()))
            {
                baseFile.AddData(new Dictionary<string, object>()
                {
                    {"account", GetUrl("myaccount")}
                });

                int customerID = (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key).First();

                core_customer customer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(customerID.ToString()))).AddFieldToFilter("is_admin", new Tuple<string, Expression>("eq", new Expression("1"))).Load().ToDataSet<core_customer>().First();

                if (customer != null)
                {
                    Location = "/admin/manage";
                    return;
                }
            }

            loginFile.AddData(new Dictionary<string, object>()
            {
                { "url", "/adminauth" },
                { "redirect", "/admin/manage" }
            });

            panelFile.AddData(new Dictionary<string, object>()
            {
                { "form", loginFile.GetData() },
                { "header", "Beheerders login" }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", panelFile.GetData() }
            });

            LoadBootStrap();
            this.AddCss("styles.css");

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
