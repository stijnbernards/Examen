using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Web
{
    public class Head
    {
        public string Authentication { get { return this.authentication; } set { this.authentication = value; } }
        public string Session { get { return this.session; } set { this.session = value; } }

        private string authentication;
        private string session;

        public Head(NameValueCollection head)
        {
            this.authentication = head["Authentication"];
            this.session = head["Session"];
        }
    }
}
