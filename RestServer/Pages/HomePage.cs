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
    public class HomePage : Page
    {

        public override void Init(HttpListenerContext ctx = null)
        {
            HTMLFile baseFile = FileManager.LoadFile("base.html");
            HTMLFile homepageFile = FileManager.LoadFile("homepage.html");

            homepageFile.AddData(new Dictionary<string, object>()
            {
                { "car_1", GetImagePath("a6-limousine.png") },
                { "car_2", GetImagePath("bmw.jpg") }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                //{"car_1", new string[] {"hoi", DataSet.ToHTMLForm(Dataset)}}
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", homepageFile.GetData() }
            });

            if (ctx.Request.Cookies["guid"] != null &&
                Sessions.sessions.ContainsKey(
                    (from s in Sessions.sessions where s.Value.Item2 == ctx.Request.Cookies["guid"].Value select s.Key)
                        .First()))
            {
                baseFile.AddData(new Dictionary<string, object>()
                {
                    { "account", GetUrl("myaccount") }
                });   
            }

            LoadBootStrap();
            this.AddCss("styles.css");

            this.response = baseFile.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
