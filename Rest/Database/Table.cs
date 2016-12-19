using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Reflection;

namespace Rest.Database
{
    public class Table
    {
        public string Query { get { return this.query; } set { this.query = value; } }

        private string query;

        private static MySqlDataReader data;

        public void Execute()
        {
            if (Table.data != null && !Table.data.IsClosed)
                Table.data.Close();

            MySqlCommand cmd;
            try
            {
#if DEBUG
                Console.WriteLine(this.query);
#endif
                cmd = new MySqlCommand(this.query, DB.connection);
                Table.data = cmd.ExecuteReader();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public T[] ToDataSet<T>() where T : DataSet, new()
        {
            List<T> retval = new List<T>();
            try
            {
                while (Table.data.Read())
                {
                    T dataSet = new T();
                    for (int i = 0; i < Table.data.FieldCount; i++)
                    {
                        typeof(T).GetProperty(Table.data.GetName(i)).SetValue(dataSet, Table.data[i], null);
                    }
                    retval.Add(dataSet);
                }
                Table.data.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return retval.ToArray();
        }

        public Dictionary<string, string> ToDict()
        {
            Dictionary<string, string> retval = new Dictionary<string, string>();
            try
            {
                while (Table.data.Read())
                {
                    for (int i = 0; i < Table.data.FieldCount; i++)
                    {
                        retval.Add(Table.data.GetName(i), Table.data[i].ToString());
                    }
                }
                Table.data.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return retval;
        }

        public static void GenerateTable(Type dataset)
        {
            string Primary = "";
            string key = "";
            string unique = "";
            Table table = new Table();
            table.Query = "CREATE TABLE `" + dataset.Name + "` (";

            foreach (PropertyInfo prop in dataset.GetProperties())
            {
                Column colInf = prop.GetCustomAttribute<Column>(true);

                if (colInf == null)
                {
                    continue;
                }

                table.Query += "`" + prop.Name + "` " + colInf.DataType + " " + colInf.AllowNull + " " + colInf.Default + ", ";
                if (prop.GetCustomAttributes(typeof(PrimaryKey), true).Length > 0)
                    Primary = "PRIMARY KEY (`" + prop.Name + "`)";
                if (prop.GetCustomAttributes(typeof(Key), true).Length > 0)
                    key += ", INDEX `" + prop.Name + "` (`" + prop.Name + "`)";
                if (prop.GetCustomAttributes(typeof(UniqueKey), true).Length > 0)
                    unique += ", UNIQUE INDEX `" + prop.Name + "` (`" + prop.Name + "`)";
            }

            table.Query += Primary + key + unique + ");";
            table.Execute();
        }

        public static void GenerateFK(Type dataset)
        {
            Table table = new Table();
            foreach (PropertyInfo prop in dataset.GetProperties())
            {
                if (prop.GetCustomAttributes(typeof(ForeignKey), true).Length > 0)
                {
                    foreach (object attr in prop.GetCustomAttributes(typeof(ForeignKey), true))
                    {
                        ForeignKey key = (ForeignKey)attr;
                        table.Query = "ALTER TABLE `" + dataset.Name + "` ADD CONSTRAINT `" + key.Name + "` FOREIGN KEY (`" + prop.Name + "`) REFERENCES " + key.Reference;
                        table.Execute();
                    }
                }
            }
        }
    }
}
