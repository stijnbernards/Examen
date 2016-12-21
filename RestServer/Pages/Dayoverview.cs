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
    public class Dayoverview : Page
    {

        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile beheerFile = FileManager.LoadFile("blocks/beheer.html");
            HTMLFile carsFile = FileManager.LoadFile("blocks/day_cars.html");

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

            string date = null;

            if(!_POST.ContainsKey("date"))
            {
                date = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
            }
            else
            {
                date = _POST["date"];
            }

            int customerID = (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key).First();

            core_customer customer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(customerID.ToString()))).AddFieldToFilter("is_admin", new Tuple<string, Expression>("eq", new Expression("1"))).Load().ToDataSet<core_customer>().First();

            if(customer == null)
            {
                Location = "/index";
                return;
            }

            sales_order_item[] items = DB.GetModel("sales_order_item").Select("*").AddFieldToFilter("hired_from", new Tuple<string, Expression>("eq", new Expression(date))).Load().ToDataSet<sales_order_item>();

            List<string> cars = new List<string>();

            foreach (sales_order_item item in items)
            {
                HTMLFile carLine = FileManager.LoadFile("blocks/day_cars_line.html");

                sales_catalog_car car = DB.GetModel("sales_catalog_car").Select("*").AddFieldToFilter("car_id", new Tuple<string, Expression>("eq", new Expression(item.car_id.ToString()))).Load().ToDataSet<sales_catalog_car>().First();
                core_customer itemCustomer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(DB.GetModel("sales_flat_order").Select("*").AddFieldToFilter("order_id", new Tuple<string, Expression>("eq", new Expression(item.order_id.ToString()))).Load().ToDataSet<sales_flat_order>().First().customer_id.ToString()))).Load().ToDataSet<core_customer>().First();

                carLine.AddData(new Dictionary<string, object>()
                {
                    { "license_plate", car.car_license_plate },
                    { "brand", car.car_brand + " - " + car.car_type},
                    { "customer", itemCustomer.customer_name }
                });

                cars.Add(carLine.GetData());
            }

            carsFile.AddData(new Dictionary<string, object>()
            {
                {"cars", cars.ToArray()},
                { "date", date }
            });

            beheerFile.AddData(new Dictionary<string, object>()
            {
                { "username", customer.customer_name },
                { "content", carsFile.GetData() }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                {"register", GetUrl("register")},
                {"login", GetUrl("login")},
                {"content", beheerFile.GetData()}
            });

            LoadBootStrap();
            this.AddCss("styles.css");
            this.AddCss("datepicker.min.css");
            this.AddJs("datepicker.min.js");

            this.Location = "";

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}