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
    public class MyAccount : Page
    {

        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile accountFile = FileManager.LoadFile("blocks/myaccount.html");

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

            core_customer customer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(customerID.ToString()))).Load().ToDataSet<core_customer>().First();

            accountFile.AddData(new Dictionary<string, object>()
            {
                {"username", customer.customer_name}
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                {"register", GetUrl("register")},
                {"login", GetUrl("login")},
                {"content", accountFile.GetData()}
            });

            LoadBootStrap();
            this.AddCss("styles.css");

            this.Location = "";

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}