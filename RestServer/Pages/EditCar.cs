using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rest.Web;
using Rest.Server;
using Rest.Server.Files;
using Rest.Database;
using RestServer.Datasets;
using Server.Datasets;

namespace RestServer.Pages
{
    public class EditCar : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile adminFile = FileManager.LoadFile("blocks/beheer.html");

            sales_catalog_car car = DB.GetModel("sales_catalog_car").Select("*").AddFieldToFilter("car_license_plate", new Tuple<string, Expression>("eq", new Expression(Url.Split('/')[3]))).Load().ToDataSet<sales_catalog_car>().First();

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

            adminFile.AddData(new Dictionary<string, object>()
            {
                { "content", DataSet.ToHTMLForm(typeof(sales_catalog_car), car, car.car_id, "PATCH", "http://localhost/admin/cars") }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", adminFile.GetData() }
            });

            if (ctx.Request.Cookies["guid"] != null &&
                Sessions.sessions.ContainsKey(
                    (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key)
                        .First()))
            {
                baseFile.AddData(new Dictionary<string, object>()
                {
                    { "account", GetUrl("myaccount") }
                });
            }

            LoadBootStrap();
            this.AddCss("styles.css");
            this.AddCss("datepicker.min.css");
            this.AddJs("datepicker.min.js");
            this.AddJs("custom.js");

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
