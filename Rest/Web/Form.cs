using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Web
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Form : Attribute
    {
        public string Method;
        public string Url;

        public Form(string method, string url)
        {
            Method = method;
            Url = url;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Field : Attribute
    {
        public string Type;
        public string Placeholder;

        public Field(string type, string placeholder)
        {
            Type = type;
            Placeholder = placeholder;
        }
    }

    public static class FieldTypes
    {
        public const string TYPE_TEXT = "text";
        public const string TYPE_HIDDEN = "hidden";
        public const string TYPE_PASSWORD = "password";
    }
}
