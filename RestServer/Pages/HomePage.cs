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
            HTMLFile panelFile = FileManager.LoadFile("blocks/panel.html");

            homepageFile.AddData(new Dictionary<string, object>()
            {
                { "car_1", GetImagePath("a6-limousine.png") },
                { "car_2", GetImagePath("bmw.jpg") }
            });

            panelFile.AddData(new Dictionary<string, object>()
            {
                { "form", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus consequat quam quis tellus mattis, ac malesuada felis laoreet. Curabitur velit lorem, ullamcorper non commodo ac, tristique id ipsum. Vestibulum posuere est vitae arcu auctor, eu dapibus felis aliquet. Nullam pellentesque turpis in dui scelerisque condimentum. Donec rhoncus eu eros vel eleifend. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Sed tincidunt elit a sagittis pharetra.<br> Vestibulum et tempor arcu, nec hendrerit tortor. In hac habitasse platea dictumst. Pellentesque et mi id diam finibus volutpat at in sapien. Quisque gravida neque sed lacus congue volutpat.<br> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam tempus magna sed nisl dictum, vel semper lectus euismod. Integer in enim a quam feugiat imperdiet sed nec urna. Sed accumsan vulputate lectus, a tincidunt nisl dapibus sed.<br> <br> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus consequat quam quis tellus mattis, ac malesuada felis laoreet. Curabitur velit lorem, ullamcorper non commodo ac, tristique id ipsum. Vestibulum posuere est vitae arcu auctor, eu dapibus felis aliquet. Nullam pellentesque turpis in dui scelerisque condimentum. Donec rhoncus eu eros vel eleifend. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Sed tincidunt elit a sagittis pharetra.<br> Vestibulum et tempor arcu, nec hendrerit tortor. In hac habitasse platea dictumst. Pellentesque et mi id diam finibus volutpat at in sapien. Quisque gravida neque sed lacus congue volutpat.<br> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam tempus magna sed nisl dictum, vel semper lectus euismod. Integer in enim a quam feugiat imperdiet sed nec urna. Sed accumsan vulputate lectus, a tincidunt nisl dapibus sed." }
            });

            baseFile.AddData(new Dictionary<string, object>()
            {
                //{"car_1", new string[] {"hoi", DataSet.ToHTMLForm(Dataset)}}
                { "register", GetUrl("register") },
                { "login", GetUrl("login") },
                { "content", homepageFile.GetData() + panelFile.GetData() }
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
