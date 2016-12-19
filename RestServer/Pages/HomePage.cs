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
            HTMLFile file = FileManager.LoadFile("homepage.html");

            file.AddData(new Dictionary<string, object>()
            {
                //{"car_1", new string[] {"hoi", DataSet.ToHTMLForm(Dataset)}}
                { "car_1", GetImagePath("a6-limousine.png") },
                { "car_2", GetImagePath("bmw.jpg") }
            });

            this.AddCss("styles.css");
            LoadBootStrap();

            this.response = file.GetData();
            this.ContentType = Constants.CONTENT_HTML;
        }
    }
}
