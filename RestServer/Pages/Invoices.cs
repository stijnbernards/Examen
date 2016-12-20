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
using RestServer.Datasets;
using Server.Datasets;

namespace RestServer.Pages
{
    public class Invoices : Page
    {

        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile myaccountFile = FileManager.LoadFile("blocks/myaccount.html");
            HTMLFile invoicesFile = FileManager.LoadFile("blocks/invoices.html");

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

            sales_flat_order[] orders = DB.GetModel("sales_flat_order")
                .Select("*")
                .AddFieldToFilter("customer_id",
                    new Tuple<string, Expression>("eq", new Expression(customerID.ToString())))
                .Load()
                .ToDataSet<sales_flat_order>();

            List<string> invoices = new List<string>();

            foreach (sales_flat_order order in orders)
            {
                HTMLFile invoiceLine = FileManager.LoadFile("blocks/invoice_line.html");

                invoiceLine.AddData(new Dictionary<string, object>()
                {
                    { "invoice_no", order.order_id },
                    { "invoice_link", order.invoice_link }
                });

                invoices.Add(invoiceLine.GetData());
            }

            invoicesFile.AddData(new Dictionary<string, object>()
            {
                {"invoices", invoices.ToArray()}
            });

            myaccountFile.AddData(new Dictionary<string, object>()
            {
                { "username", customer.customer_name },
                { "content", invoicesFile.GetData() }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                {"register", GetUrl("register")},
                {"login", GetUrl("login")},
                {"content", myaccountFile.GetData()}
            });

            LoadBootStrap();
            this.AddCss("styles.css");

            this.Location = "";

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}