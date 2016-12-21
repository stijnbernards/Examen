using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Server;
using Rest.Server.Files;
using Rest.Web;
using RestServer.Datasets;
using Server.Datasets;
using Invoicer.Models;
using Invoicer.Services;

namespace RestServer.Pages
{
    public class Complete : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            if (ctx.Request.Cookies["guid"] != null &&
                Sessions.sessions.ContainsKey(
                    (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key)
                        .First()))
            {

            }

            sales_catalog_car car = DB.GetModel("sales_catalog_car").Select("*").AddFieldToFilter("car_license_plate", new Tuple<string, Expression>("eq", new Expression(Url.Split('/')[1]))).Load().ToDataSet<sales_catalog_car>().First();

            int customerID = (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key).First();

            core_customer customer = DB.GetModel("core_customer").Select("*").AddFieldToFilter("customer_id", new Tuple<string, Expression>("eq", new Expression(customerID.ToString()))).Load().ToDataSet<core_customer>().First();

            Model insertModel = DB.GetModel("sales_flat_order").Insert();

            insertModel
                .AddDataToInsert(new KeyValuePair<string, string>("customer_id", customer.customer_id.ToString()))
                .AddDataToInsert(new KeyValuePair<string, string>("treated_by", "PIET"));

            insertModel.Load();

            sales_flat_order order = DB.GetModel("sales_flat_order").Select("*").AddFieldToFilter("order_id", Tuple.Create<string, Expression>("eq", new Expression("LAST_INSERT_ID()", false))).Load().ToDataSet<sales_flat_order>().First();

            DB.GetModel("sales_flat_order")
                .Update()
                .AddKeyValueToUpdate(new KeyValuePair<string, string>("invoice_link",
                    "pdf/1000" + order.order_id + ".pdf"))
                .AddFieldToFilter("order_id",
                    Tuple.Create<string, Expression>("eq", new Expression(order.order_id.ToString())))
                .Load();

            DB.GetModel("sales_order_item").Insert()
                .AddDataToInsert(new KeyValuePair<string, string>("order_id", order.order_id.ToString()))
                .AddDataToInsert(new KeyValuePair<string, string>("car_id", car.car_id.ToString()))
                .AddDataToInsert(new KeyValuePair<string, string>("hired_from", _POST["date_1"]))
                .AddDataToInsert(new KeyValuePair<string, string>("hired_to", _POST["date_2"]))
                .Load();

            int days =
                Convert.ToInt16(
                (DateTime.ParseExact(_POST["date_2"], "yyyy/MM/dd", CultureInfo.InvariantCulture) -
                 DateTime.ParseExact(_POST["date_1"], "yyyy/MM/dd", CultureInfo.InvariantCulture)).TotalDays);

            new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, "€")
                .TextColor("#0000CC")
                .BackColor("#87ceeb")
                .Title("Factuur")
                .BillingDate(DateTime.Now)
                .Reference("1000" + order.order_id)
                .Company(Address.Make("VAN", new string[] {"Rent-a-Car", "Almere"}))
                .Client(Address.Make("FACTUUR ADRES",
                    new string[]
                    {
                        customer.customer_name, customer.address_street + " " + customer.address_number,
                        customer.address_city, customer.address_postal
                    }))
                .Items(new List<ItemRow>
                {
                    ItemRow.Make(car.car_type + " - " + car.car_brand, "van: " + _POST["date_1"] + " tot: " + _POST["date_2"],
                        (decimal) days, days * decimal.Multiply(car.car_day_price, (decimal) 0.25), (decimal)car.car_day_price,
                        decimal.Multiply(days, car.car_day_price)),
                })
                .Totals(new List<TotalRow>
                {
                    TotalRow.Make("Subtotaal", days*decimal.Multiply(car.car_day_price, (decimal) 0.75)),
                    TotalRow.Make("BTW @ 25%", days*decimal.Multiply(car.car_day_price, (decimal) 0.25)),
                    TotalRow.Make("Totaal", decimal.Multiply(days, car.car_day_price), true),
                })
                .Details(new List<DetailRow>
                {
                    DetailRow.Make("BETALINGS INFORMATIE",
                        "U kunt achteraf betalen bij de medewerker die uw order afhandeld.", "",
                        "Als u nog verdere vragen heeft kunt u mailen naar info@rent-a-car.com", "",
                        "Bedankt voor uw bestelling",
                        "U kunt de auto ophalen op: " + _POST["date_1"],
                        "En de auto dient op zijn laatst om 17:00 op: " + _POST["date_2"] + " terug gebracht te worden.")
                })
                .Footer("http://www.rent-a-car.com")
                .Save(FileManager.BaseDir + "pdf/1000" + order.order_id + ".pdf");

            string path = FileManager.BaseDir + "pdf/1000" + order.order_id + ".pdf";

            Location = "/pdf/1000" + order.order_id + ".pdf";

            this.response = "";
            this.ContentType = Constants.CONTENT_PDF;
        }
    }
}
