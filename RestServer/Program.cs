using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest;
using Rest.Database;
using Rest.Server;
using RestServer.Datasets;
using Server.Datasets;
using Server.Pages;
using System.Reflection;
using Rest.Web;
using RestServer.Pages;

namespace Server
{
    class Program
    {
        public static Rest.RestServer rest;
        public static string DBUrl = "localhost", DBUser = "root", DBPwd = "", DBName = "rentacar", BaseURI = "localhost";
        public static bool Force = false;
        public static bool Delete = false;

        public static Dictionary<string, Tuple<string, Delegate>> Commands = new Dictionary<string, Tuple<string, Delegate>>()
        {
            { "-h", Tuple.Create<string, Delegate>("Help function;", new Action<string>((string v) => { Help(); })) },
            { "-dburl", Tuple.Create<string, Delegate>("Sets the database host url;", new Action<string>((string v) => { DBUrl = v; })) },
            { "-dbuser", Tuple.Create<string, Delegate>("Sets the database username;", new Action<string>((string v) => { DBUser = v; })) },
            { "-dbpwd", Tuple.Create<string, Delegate>("Sets the database password;", new Action<string>((string v) => { DBPwd = v; })) },
            { "-dbname", Tuple.Create<string, Delegate>("Sets the database name;", new Action<string>((string v) => { DBName = v; })) },
            { "-baseuri", Tuple.Create<string, Delegate>("Sets the base URI;", new Action<string>((string v) => { BaseURI = v; })) },
            { "-s", Tuple.Create<string, Delegate>("Starts the server;", new Action<string>((string v) => { Start(); })) },
            { "-rb", Tuple.Create<string, Delegate>("Rebuilds the database(--d to drop the database!);", new Action<string>((string v) => { Force = true; if(v == "--d") Delete = true; })) }
        };

        static void Main(string[] args)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("-") && args.Length > i + 1 && !args[i + 1].Contains("-") || (args.Length > i + 1 && args[i + 1].Contains("--")))
                {
                    results.Add(args[i], args[i + 1]);
                    i++;
                }
                else
                {
                    results.Add(args[i], "");
                } 
            }

            foreach (KeyValuePair<string, string> kv in results)
                if (Commands.ContainsKey(kv.Key))
                    Commands[kv.Key].Item2.DynamicInvoke(kv.Value);

            Console.ReadLine();
        }

        public static void Help()
        {
            Console.WriteLine("Command List:");
            
            foreach (KeyValuePair<string, Tuple<string, Delegate>> kv in Commands)
                Console.WriteLine(string.Format("{0}: {1}", kv.Key, kv.Value.Item1));
        }

        public static void Start()
        {
            DB database = new DB(DBUrl, DBUser, DBPwd, DBName);

            rest = new Rest.RestServer("http://" + BaseURI + "/", "rest", 0, true);

            rest.ConstructDataBase(Assembly.GetExecutingAssembly(), Force, Delete);
            rest.AddRouting<Authentication>("auth");
            rest.AddRouting<Search>("users/search", typeof(core_customer));
            rest.AddRouting<HomePage>("index", typeof(core_customer));
            rest.AddRouting<HomePage>("", typeof(core_customer));
            rest.AddRouting<Register>("register", typeof(core_customer));
            rest.AddRouting<Login>("login", typeof(core_customer));
            rest.AddRouting<Catalog>("catalog", typeof(sales_catalog_car));
            rest.AddRouting<MyAccount>("myaccount", typeof(core_customer));
            rest.AddRouting<Editaccount>("editaccount", typeof(core_customer));
            rest.AddRouting<Invoices>("invoices", typeof(core_customer));
            rest.AddRouting<Contact>("contact", typeof(core_customer));
            rest.AddRouting<Overons>("overons", typeof(core_customer));

            foreach (sales_catalog_car car in DB.GetModel("sales_catalog_car").Select("car_license_plate").Load().ToDataSet<sales_catalog_car>())
            {
                rest.AddRouting<Car>("car/" + car.car_license_plate, typeof(sales_catalog_car));
                rest.AddRouting<Rent>("rent/" + car.car_license_plate, typeof(sales_flat_order));
                rest.AddRouting<Complete>("complete/" + car.car_license_plate, typeof(sales_flat_order));
            }

            rest.AddNoAuthUrl("car");
            rest.AddNoAuthUrl("contact");
            rest.AddNoAuthUrl("overons");
            rest.AddNoAuthUrl("invoices");
            rest.AddNoAuthUrl("rent");
            rest.AddNoAuthUrl("complete");
            rest.AddNoAuthUrl("catalog");
            rest.AddNoAuthUrl("auth");
            rest.AddNoAuthUrl("corecustomer");
            rest.AddNoAuthUrl("editaccount");
            rest.AddNoAuthUrl("index");
            rest.AddNoAuthUrl("myaccount");
            rest.AddNoAuthUrl("");
            rest.AddNoAuthUrl("register");
            rest.AddNoAuthUrl("login");
            rest.ConstructService(Assembly.GetExecutingAssembly());
            rest.Start();
        }
    }
}
