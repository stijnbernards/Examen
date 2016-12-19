using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Rest.Web;
using Rest.Server;
using System.IO;
using System.Text.RegularExpressions;
using Rest.Server.Files;

namespace Rest.Server
{
    class Router
    {
        private Dictionary<string, List<Page>> Routing = new Dictionary<string, List<Page>>();
        private List<string> NoAuth = new List<string>();

        public void AddNoAuthUrl(string url)
        {
            NoAuth.Add(url);
        }

        public void Add(string url, Page route)
        {
            if (Routing.ContainsKey(url))
            {
                this.Routing[url].Add(route);
            }
            else
            {
                this.Routing.Add(url, new List<Page>() { route });
            }
        }

        public void Call(string url, HttpListenerContext ctx)
        {

            if (url == "favicon.ico")
                return;

            Dictionary<string, string> get = new Dictionary<string, string>();
            Dictionary<string, string> post = new Dictionary<string, string>();
            Encoding utf = new UTF8Encoding(false);
            Head head = new Head(ctx.Request.Headers);
            bool allow = false;

            string response = null;
            string guid = null;
            Console.WriteLine(url);
            
            url = url.ToLower();

            List<string> urlsp = url.Split('/').ToList<string>();

            if (ctx.Request.HttpMethod == Constants.METHOD_GET)
            {
                url = string.Join("/", urlsp);
                //url = urlsp.First();
                //for (int i = 1; i < urlsp.Count; i++)
                //    get.Add(i.ToString(), urlsp[i]);
            }
            else
            {
                url = string.Join("/", urlsp);
            }

            if (url.Contains(".css"))
            {
                response = FileManager.LoadCSSFile(url);
                ctx.Response.ContentEncoding = utf;
                ctx.Response.ContentType = Constants.CONTENT_CSS;
                ctx.Response.ContentLength64 = utf.GetByteCount(response);
                ctx.Response.StatusCode = Constants.StatusCodes.OK;

                ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                ctx.Response.Close();

                return;
            }
            else if (url.Contains(".js"))
            {
                response = FileManager.LoadCSSFile(url);
                ctx.Response.ContentEncoding = utf;
                ctx.Response.ContentType = Constants.CONTENT_JAVASCRIPT;
                ctx.Response.ContentLength64 = utf.GetByteCount(response);
                ctx.Response.StatusCode = Constants.StatusCodes.OK;

                ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                ctx.Response.Close();

                return;
            }
            else if (url.Contains(".png") || url.Contains(".jpg"))
            {
                ImageFile image = FileManager.LoadImagefile(url);

                ctx.Response.ContentType = image.GetContentType();
                ctx.Response.ContentLength64 = image.GetStream().Length;

                image.GetStream().WriteTo(ctx.Response.OutputStream);

                ctx.Response.OutputStream.Flush();
                ctx.Response.OutputStream.Close();
                ctx.Response.Close();

                return;
            }

            NoAuth.ForEach(x => { if (!url.Contains(x)) guid = head.Session; else allow = true; });
            if (guid == "user" || guid == "admin") allow = true;

            if (allow || Sessions.sessions.Any(i => i.Value.Item2 == guid && i.Value.Item1 == ctx.Request.RemoteEndPoint.Address.ToString()))
            {
                try
                {
                    if (!allow)
                    {
                        Tuple<string, string, DateTime> sess = Sessions.sessions[(from s in Sessions.sessions where s.Value.Item2 == guid select s.Key).First<int>()];
                        Tuple<string, string, DateTime> newSess = Tuple.Create<string, string, DateTime>(sess.Item1, sess.Item2, DateTime.Now);
                        Sessions.sessions[(from s in Sessions.sessions where s.Value.Item2 == guid select s.Key).First<int>()] = newSess;
                    }
                    
                    //Routing.ToList<KeyValuePair<string, List<Page>>>().ForEach(x => Console.WriteLine(x.Key));

                    foreach (Page page in Routing[url])
                    {
                        using (StreamReader reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                        {
                            string postR = reader.ReadToEnd();
                            Console.WriteLine(postR);
                            if (postR.Length > 0)
                                postR.Split('&').ToList<string>().ForEach(x => { var spl = x.Split('='); post.Add(spl[0], Uri.UnescapeDataString(spl[1])); });
                        }

                        page._POST = post;
                        page._GET = get;
                        
                        page.HTTPMethod = ctx.Request.HttpMethod;
                        page.Headers = head;
                        page.Init(ctx);
                        response += page.Send();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
#if DEBUG
                    Console.WriteLine("Route doesn't exist!");
                    response = Constants.STATUS_FALSE;
                    ctx.Response.ContentEncoding = utf;
                    ctx.Response.ContentType = Constants.CONTENT_JSON;
                    ctx.Response.ContentLength64 = utf.GetByteCount(response);
                    //ctx.Response.StatusCode = Routing[url][0].StatusCode;

                    ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                    ctx.Response.Close();
                    return;
#endif
                    if (Routing.ContainsKey("404"))
                    {
                        Routing["404"][0].Init();
                        response = Routing["404"][0].Send();
                    }
                }
                ctx.Response.ContentEncoding = utf;
                ctx.Response.ContentType = Routing[url][0].ContentType;
                ctx.Response.ContentLength64 = utf.GetByteCount(response);
                //ctx.Response.StatusCode = Routing[url][0].StatusCode;

                ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                ctx.Response.Close();
            }
            else
            {
                response = Constants.STATUS_UNAUTHORIZED;
                ctx.Response.ContentEncoding = utf;
                Console.WriteLine(url);
                ctx.Response.ContentType = Constants.CONTENT_JSON;
                ctx.Response.ContentLength64 = utf.GetByteCount(response);
                ctx.Response.StatusCode = Constants.StatusCodes.UNAUTHORIZED;

                ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                ctx.Response.Close();
            }
        }
    }
}
