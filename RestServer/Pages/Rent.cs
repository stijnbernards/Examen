using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestServer.Datasets;
using Rest.Server;
using Rest.Web;
using Rest.Database;
using Server.Datasets;
using Rest.Server.Files;
using System.Net;
using System.Globalization;

namespace RestServer.Pages
{
    public class Rent : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile confirmFile = FileManager.LoadFile("blocks/confirm.html");

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
            else
            {
                this.Location = "/register";
                return;
            }

            sales_catalog_car car = DB.GetModel("sales_catalog_car").Select("*").AddFieldToFilter("car_license_plate", new Tuple<string, Expression>("eq", new Expression(Url.Split('/')[1]))).Load().ToDataSet<sales_catalog_car>().First();

            int customerID = (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key).First();

            core_customer customer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(customerID.ToString()))).Load().ToDataSet<core_customer>().First();

            confirmFile.AddData(new Dictionary<string, object>()
            {
                { "car_name", car.car_type + " - " + car.car_brand },
                { "hired_from", _POST["date_1"] },
                { "hired_to", _POST["date_2"] },
                { "address", "<address><strong>" + customer.customer_name + "</strong>, " + customer.address_street + " " + customer.address_number + " <br>" + customer.address_postal + "</address>" },
                { "price", Convert.ToInt16((DateTime.ParseExact(_POST["date_2"], "dd/MM/yyyy", CultureInfo.InvariantCulture) - DateTime.ParseExact(_POST["date_1"], "dd/MM/yyyy", CultureInfo.InvariantCulture)).TotalDays) * car.car_day_price }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                //{"car_1", new string[] {"hoi", DataSet.ToHTMLForm(Dataset)}}
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", confirmFile.GetData() }
            });

            LoadBootStrap();
            this.AddCss("styles.css");

            this.Location = "";

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
