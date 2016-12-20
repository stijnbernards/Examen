using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rest.Web;
using Rest.Server.Files;
using Rest.Server;
using Rest.Database;
using RestServer.Datasets;

namespace RestServer.Pages
{
    public class Catalog : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile catalogFile = FileManager.LoadFile("blocks/catalog.html");

            sales_catalog_car[] cars = DB.GetModel("sales_catalog_car").Select("*").Load().ToDataSet<sales_catalog_car>();

            List<string> carsData = new List<string>();

            for(int i = 0; i < cars.Length; i += 2)
            {
                HTMLFile carFile = FileManager.LoadFile("blocks/carsmall.html");
                carFile.AddData(new Dictionary<string, object>()
                {
                    { "car_1_image", GetImagePath(cars[i].car_image) },
                    { "car_1_name", cars[i].car_type + " - " + cars[i].car_brand },
                    { "car_1_link", GetUrl(cars[i].car_license_plate) },
                    { "car_1_desc", cars[i].car_description },
                    { "car_1_price", cars[i].car_day_price },
                    { "car_2_image", GetImagePath(cars[i+1].car_image) },
                    { "car_2_name", cars[i+1].car_type + " - " + cars[i+1].car_brand },
                    { "car_2_link", GetUrl(cars[i+1].car_license_plate) },
                    { "car_2_desc", cars[i+1].car_description },
                    { "car_2_price", cars[i+1].car_day_price }
                });

                carsData.Add(carFile.GetData());
            }

            catalogFile.AddData(new Dictionary<string, object>()
            {
                { "products", carsData.ToArray() },
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", catalogFile.GetData() }
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

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
