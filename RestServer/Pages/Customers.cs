﻿using System;
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
    public class Customers : Page
    {

        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile beheerFile = FileManager.LoadFile("blocks/beheer.html");
            HTMLFile customersFile = FileManager.LoadFile("blocks/customers.html");

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

            core_customer[] customers = DB.GetModel("core_customer").Select("*").Load().ToDataSet<core_customer>();

            List<string> customersText = new List<string>();

            foreach (core_customer customerItem in customers)
            {
                HTMLFile customerline = FileManager.LoadFile("blocks/customer_line.html");

                customerline.AddData(new Dictionary<string, object>()
                {
                    { "customer_name", customerItem.customer_name },
                    { "customer_email", customerItem.customer_email },
                    { "customer_address", customerItem.address_city + " " + customerItem.address_street + " " + customerItem.address_number + " " + customerItem.address_postal},
                    { "id", customerItem.customer_id }
                });

                customersText.Add(customerline.GetData());
            }

            customersFile.AddData(new Dictionary<string, object>()
            {
                { "customers", customersText.ToArray() }
            });

            beheerFile.AddData(new Dictionary<string, object>()
            {
                { "username", customer.customer_name },
                { "content",  customersFile.GetData() }
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