using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Rest.Web;

namespace Rest.Database
{ 
    public class DataSet
    {
        public string ToJSON(List<string> extra = null)
        {
            List<string> objs = new List<string>();
            PropertyInfo[] props = this.GetType().GetProperties();

            if (extra != null)
                extra.ForEach(x => objs.Add(x));

            foreach (PropertyInfo prop in props)
                if(prop.GetCustomAttributes(typeof(Hidden), false).Length == 0)
                    objs.Add('"' + prop.Name + '"' + " : " + '"' + prop.GetValue(this) + '"');

            return "{" + string.Join(", ", objs) + "}";
        }

        public static string ToHTMLForm(Type datasetType)
        {
            Form form = datasetType.GetCustomAttributes(typeof(Form), true).FirstOrDefault() as Form;

            if (form == null)
            {
                return "";
            }

            string formResult = $"<form method=\"post\" action=\"{form.Url}\">";

            PropertyInfo[] props = datasetType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetCustomAttributes(typeof(Field), false).Length == 0)
                {
                    continue;
                }

                Field field = prop.GetCustomAttributes(typeof(Field), false).FirstOrDefault() as Field;

                if (field == null)
                {
                    continue;
                }

                formResult += $"<div class=\"form-group\">";
                formResult += $"<label for=\"{prop.Name}\">{field.Placeholder}</label>";
                formResult += $"<input class=\"form-control\" id=\"{prop.Name}\" name=\"{prop.Name}\" type=\"{field.Type}\" placeholder=\"{field.Placeholder}\"/>";
                formResult += $"</div>";
            }

            formResult += $"<input type=\"hidden\" name=\"_METHOD\" value=\"{form.Method}\"/>";
            formResult += $"<input type=\"submit\" value=\"submit\" class=\"btn btn-default\"/>";

            formResult += "</form>";

            return formResult;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Hidden : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class AutoGenerate : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class EditAble : Attribute
    {
        public string HTTPMethod { get; set; }

        public EditAble(string HTTPMethod)
        {
            this.HTTPMethod = HTTPMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Column : Attribute
    {
        public string DataType { get; set; }
        public string Default { get; set; }
        public string AllowNull { get; set; }

        public Column(string dataType, string defaultv = "DEFAULT '0'", string allowNull = "NULL")
        {
            this.DataType = dataType;
            this.Default = defaultv;
            this.AllowNull = allowNull;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class DataBaseEntry : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ForeignKey : Attribute
    {
        public string Name { get; set; }
        public string Reference { get; set; }

        public ForeignKey(string name, Type refTable, string refCol)
        {
            this.Name = name;
            this.Reference = "`" + refTable.Name + "` (`" + refCol + "`)"; ;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Key : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueKey : Attribute { }
}
