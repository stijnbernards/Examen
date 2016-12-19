using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace Rest.Database
{
    public class DB
    {
        public static MySqlConnection connection;

        public static string Database;

        private string server;
        private string uid;
        private string password;

        /// <summary>
        /// Set all variable and initialize the connection.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="uid"></param>
        /// <param name="password"></param>
        public DB(string server, string uid, string password, string database)
        {
            string connectionString;

            DB.Database = database;
            this.server = server;
            this.uid = uid;
            this.password = password;

            connectionString = "SERVER=" + server + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";Convert Zero Datetime=True;";
            DB.connection = new MySqlConnection(connectionString);
            OpenConnection();
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        private bool OpenConnection(){
            try
            {
                DB.connection.Open();
                Console.WriteLine("Database connection opened!");
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        /// <returns></returns>
        private bool CloseConnection()
        {
            try
            {
                DB.connection.Close();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        public static Model GetModel(string model){
            return new Model(model);
        }

        public static Model QueryString(string modelnm, string query)
        {
            Model model = new Model(modelnm);
            model = model.Select("*");

            query.Split('&')
                .ToList<string>()
                .ForEach(x => 
                {
                    string type = x.Contains("%") ? "LIKE" : "eq";
                    List<string> qp = new List<string>();

                    x.Split('=')
                        .ToList<string>()
                        .ForEach(y => 
                        {
                            qp.Add(y);
                        });

                    model = model.AddFieldToFilter(qp[0], Tuple.Create<string, Expression>(type, new Expression(qp[1])));
                });

            return model;
        }
    }
}
