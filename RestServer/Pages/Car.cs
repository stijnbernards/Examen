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

namespace RestServer.Pages
{
    public class Car : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile carFile = FileManager.LoadFile("blocks/detail.html");

            sales_catalog_car car = DB.GetModel("sales_catalog_car").Select("*").AddFieldToFilter("car_license_plate", new Tuple<string, Expression>("eq", new Expression(Url.Split('/')[1]))).Load().ToDataSet<sales_catalog_car>().First();

            sales_order_item[] items = DB.GetModel("sales_order_item").Select("*").AddFieldToFilter("car_id", new Tuple<string, Expression>("eq", new Expression(car.car_id.ToString()))).Load().ToDataSet<sales_order_item>();

            List<string> dates = new List<string>();

            foreach(sales_order_item item in items)
            {
                for (DateTime dt = Convert.ToDateTime(item.hired_from); dt <= Convert.ToDateTime(item.hired_to); dt = dt.AddDays(1))
                {
                    dates.Add(string.Format("{0:dd/MM/yyyy}", dt.Date));
                }
            }

            carFile.AddData(new Dictionary<string, object>()
            {
                { "image", GetImagePath(car.car_image) },
                { "name", car.car_type + " - " + car.car_brand },
                { "description", car.car_description },
                { "price", car.car_day_price.ToString() },
                { "license", car.car_license_plate },
                { "dates",  dates.ToArray() }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", carFile.GetData() }
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
