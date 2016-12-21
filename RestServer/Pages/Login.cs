using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Server;
using Rest.Server.Files;
using Rest.Web;

namespace RestServer.Pages
{
    public class Login : Page
    {
        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile panelFile = FileManager.LoadFile("blocks/panel.html");
            HTMLFile loginFile = FileManager.LoadFile("blocks/login.html");

            loginFile.AddData(new Dictionary<string, object>()
            {
                { "url", "/auth" },
                { "redirect", "/index" }
            });

            panelFile.AddData(new Dictionary<string, object>()
            {
                { "form", loginFile.GetData() },
                { "header", "Login" }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                //{"car_1", new string[] {"hoi", DataSet.ToHTMLForm(Dataset)}}
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", panelFile.GetData() }
            });

            LoadBootStrap();
            this.AddCss("styles.css");

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
