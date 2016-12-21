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
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Rest.Server
{
    class Router
    {
        private Dictionary<string, List<Page>> Routing = new Dictionary<string, List<Page>>();
        private List<string> NoAuth = new List<string>();
        private Dictionary<string, Cache> Caching = new Dictionary<string, Cache>();

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

            url = url.ToLower();

            Dictionary<string, string> get = new Dictionary<string, string>();
            Dictionary<string, string> post = new Dictionary<string, string>();
            Encoding utf = new UTF8Encoding(false);
            Head head = new Head(ctx.Request.Headers);
            bool allow = false;
            string totalPost = "";

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

                if (!RestServer.Developer)
                {
                    ctx.Response.AddHeader("Cache-Control", "max-age=86400");
                }

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

                if (!RestServer.Developer)
                {
                    ctx.Response.AddHeader("Cache-Control", "private, max-age=86400");
                }

                ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                ctx.Response.Close();

                return;
            }
            else if (url.Contains(".png") || url.Contains(".jpg") || url.Contains(".ico"))
            {
                ImageFile image = FileManager.LoadImagefile(url);

                ctx.Response.ContentType = image.GetContentType();
                ctx.Response.ContentLength64 = image.GetStream().Length;

                if (!RestServer.Developer)
                {
                    ctx.Response.AddHeader("Cache-Control", "max-age=86400");
                }

                image.GetStream().WriteTo(ctx.Response.OutputStream);

                ctx.Response.OutputStream.Flush();
                ctx.Response.OutputStream.Close();
                ctx.Response.Close();

                return;
            }
            else if (url.Contains(".pdf"))
            {

                FileStream fs = File.OpenRead(FileManager.BaseDir + url);
                
                ctx.Response.ContentType = Constants.CONTENT_PDF;
                ctx.Response.ContentLength64 = fs.Length;
                ctx.Response.StatusCode = Constants.StatusCodes.OK;

                fs.CopyTo(ctx.Response.OutputStream);

                if (!RestServer.Developer)
                {
                    ctx.Response.AddHeader("Cache-Control", "max-age=86400");
                }

                ctx.Response.OutputStream.Flush();
                ctx.Response.OutputStream.Close();
                ctx.Response.Close();

                return;
            }

            NoAuth.ForEach(x => { if (!url.Contains(x)) guid = head.Session; else allow = true; });

            if (ctx.Request.Cookies["guid"] != null)
            {
                guid = ctx.Request.Cookies["guid"].Value;
            }

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
                    Console.WriteLine("test");
                    foreach (Page page in Routing[url])
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        using (Stream input = ctx.Request.InputStream)
                        {
                            byte[] buffer = new byte[32 * 1024]; // 32K buffer for example
                            int bytesRead;
                            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, bytesRead);
                            }
                        }
                        memoryStream.Position = 0;

                        using (StreamReader reader = new StreamReader(memoryStream, ctx.Request.ContentEncoding))
                        {
                            
                            string postR = reader.ReadToEnd();

                            totalPost += postR;

                            memoryStream.Position = 0;

                            if (postR.Contains("Content-Disposition"))
                            {
                                SaveFile(memoryStream, ctx.Request.ContentType);
                            }
                            else
                            {
                                if (postR.Length > 0)
                                    postR.Split('&').ToList<string>().ForEach(x => { var spl = x.Split('='); post.Add(spl[0], Uri.UnescapeDataString(spl[1])); });
                            }
                        }

                        using (SHA1Managed sha1 = new SHA1Managed())
                        {
                            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url + totalPost));
                            StringBuilder sb = new StringBuilder(hash.Length * 2);

                            foreach (byte b in hash)
                            {
                                sb.Append(b.ToString("X2"));
                            }

                            if (Caching.ContainsKey(sb.ToString()))
                            {
                                Cache cache = Caching[sb.ToString()];

                                ctx.Response.ContentEncoding = cache.ContentEncoding;
                                ctx.Response.ContentType = cache.ContentType;
                                ctx.Response.ContentLength64 = cache.ContentLength64;

                                ctx.Response.OutputStream.Write(utf.GetBytes(cache.Response), 0, utf.GetByteCount(cache.Response));
                                ctx.Response.OutputStream.Flush();
                                ctx.Response.OutputStream.Close();
                                ctx.Response.Close();
                                return;
                            }
                        }

                        page._POST = post;
                        page._GET = get;
                        page.Url = url;

                        page.HTTPMethod = ctx.Request.HttpMethod;
                        page.Headers = head;
                        page.Init(ctx);
                        response += page.Send();
                        page.CleanUp();

                        if (page.Location != null)
                        {
                            ctx.Response.Redirect(page.Location);
                        }
                        break;
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

                //if (!RestServer.Developer)
                //{
                //    ctx.Response.AddHeader("Cache-Control", "max-age=86400");
                //}

                ctx.Response.OutputStream.Write(utf.GetBytes(response), 0, utf.GetByteCount(response));
                ctx.Response.OutputStream.Flush();
                ctx.Response.OutputStream.Close();
                ctx.Response.Close();

                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url + totalPost));
                    StringBuilder sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("X2"));
                    }

                    Cache cache = new Cache();
                    cache.ContentEncoding = utf;
                    cache.ContentType = Routing[url][0].ContentType;
                    cache.ContentLength64 = utf.GetByteCount(response);
                    cache.Response = response;

                    Caching.Add(sb.ToString(), cache);
                }
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

        public static async void SaveFile(Stream data, string contentType)
        {
            StreamContent streamContent = new StreamContent(data);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            MultipartMemoryStreamProvider provider = await streamContent.ReadAsMultipartAsync();

            foreach (HttpContent httpContent in provider.Contents)
            {
                string fileName = httpContent.Headers.ContentDisposition.FileName;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                using (Stream fileContents = await httpContent.ReadAsStreamAsync())
                {
                    using (Stream filestream = File.Create(FileManager.BaseDir + "assets/images/" + fileName.Replace("\"", "")))
                    {
                        fileContents.CopyTo(filestream);
                        filestream.Close();
                    }
                }
            }
        }
    }
}
