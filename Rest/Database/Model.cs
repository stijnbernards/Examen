using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Rest.Database
{
    public class Model
    {
        private string name;
        private string type;
        private string from;

        private int[] limit = null;

        private List<string> filters = new List<string>();
        private Dictionary<string, string> insertData = new Dictionary<string, string>();
        private Dictionary<string, string> updateData = new Dictionary<string, string>();

        public Model(string name)
        {
            this.name = name;
        }

        public Model SetLimit(int l1, int l2)
        {
            this.limit = new int[2];
            this.limit[0] = l1;
            this.limit[1] = l2;
            return this;
        }

        public Model AddFieldToFilter(string column, Tuple<string, Expression> expr)
        {
            string operand;
            switch (expr.Item1)
            {
                case "lt":
                    operand = "<";
                    break;
                case "gt":
                    operand = ">";
                    break;
                case "eq":
                    operand = "=";
                    break;
                case "<":
                    operand = expr.Item1;
                    break;
                case ">":
                    operand = expr.Item1;
                    break;
                case "=":
                    operand = expr.Item1;
                    break;
                case "LIKE":
                    operand = expr.Item1;
                    break;
                default:
                    throw new System.Exception("Invalid operand supplied");
            }
            filters.Add("`" + column + "` " + operand + " " + expr.Item2.ToString());
            return this;
        }

        public Table Load()
        {
            Table table = new Table();
            table.Query = BuildQuery();
            table.Execute();

            return table;
        }

        public Model Select(string columns)
        {
            Model model = new Model(this.name);
            model.type = "SELECT";
            model.from = columns;
            return model;
        }

        public Model Delete()
        {
            Model model = new Model(this.name);
            model.type = "DELETE";
            return model;
        }

        public Model Insert()
        {
            Model model = new Model(this.name);
            model.type = "INSERT";
            return model;
        }

        public Model Update()
        {
            Model model = new Model(this.name);
            model.type = "UPDATE";
            return model;
        }

        public Model AddDataSetToInsert(DataSet set)
        {
            object propertyValue;
            PropertyInfo[] props = set.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                propertyValue = prop.GetValue(set);
                try
                {
                    if (propertyValue != null)
                    {
                        insertData.Add(prop.Name, propertyValue.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            return this;
        }

        public Model AddDataToInsert(KeyValuePair<string, string> kvp)
        {
            insertData.Add(kvp.Key, kvp.Value);
            return this;
        }

        public Model AddKeyValueToUpdate(KeyValuePair<string, string> keyValue)
        {
            this.updateData.Add(keyValue.Key, keyValue.Value);
            return this;
        }

        public Model AddKeyValueToUpdate(Dictionary<string, string> dict)
        {
            dict.ToList().ForEach(x => { this.updateData.Add(x.Key, x.Value); });
            return this;
        }

        private string BuildQuery()
        {
            string query;

            switch (type)
            {
                case "SELECT":
                    query = "SELECT " + this.from + " FROM `"+ this.name +"`";
                    break;
                case "DELETE":
                    query = "DELETE FROM `" + this.name + "`";
                    break;
                case "INSERT":
                    List<string> data = new List<string>();
                    List<string> column = new List<string>();

                    query = "INSERT INTO `" + this.name + "` (";
                    foreach (KeyValuePair<string, string> pair in insertData)
                    {
                        if (pair.Value != null)
                        {
                            column.Add("`" + pair.Key + "`");
                            data.Add("'" + pair.Value + "'");
                        }
                    }
                    query += string.Join(", ", column.ToArray()) + ") VALUES (" + string.Join(", ", data.ToArray()) + ")"; 
                    break;
                case "UPDATE":
                    query = "UPDATE `" + this.name + "` SET ";
                    query += string.Join(", ", updateData.Select(x => "`" + x.Key + "` = '" + x.Value + "'").ToList<string>());
                    break;
                default:
                    throw new Exception("Invalid query type!");
            }

            if (filters.Count > 0)
            {
                int iter = 0;
                query += " WHERE ";
                foreach(string filter in filters)
                {
                    if (iter == 0)
                    {
                        query += filter;
                    }
                    else
                    {
                        query += " AND " + filter;
                    }
                    iter++;
                }
            }

            if (this.limit != null && this.limit.Length > 1)
            {
                query += " LIMIT " + this.limit[0] + ", " + this.limit[1];
            }

            return query;
        }
    }
}
