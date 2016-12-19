using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using System.Diagnostics;
using System.Reflection.Emit;
using Rest.Server;
using Rest.Web;
using Rest.Database;

namespace Rest
{
    public class RestServer
    {
        #region members

        public static bool Developer = false;

        private string BaseUrl = null;
        private string Suffix = null;
        private string Port = null;
        private int urlSplit = 0;
        public MemoryStream MsPHP = new MemoryStream();
        
        private HttpListener Server = new HttpListener();
        private Router router = new Router();


        #endregion

        public RestServer(string baseurl, string suffix, int port = 0, bool developer = false)
        {
            Developer = developer;
            this.BaseUrl = baseurl;
            
            if(port != 0)
                this.Port = string.Format(":{0}/", port);
            this.Suffix = suffix;
            this.urlSplit = BaseUrl.Split('/').Count();

            if (!Directory.Exists(FileManager.BaseDir))
            {
                Directory.CreateDirectory(FileManager.BaseDir);
            }

            lock (Sessions.sessions)
            {
                Sessions.sessions.Add(0, Tuple.Create<string, string, DateTime>("::1", "admin", DateTime.Now.AddYears(100)));
                Sessions.sessions.Add(-1, Tuple.Create<string, string, DateTime>("::1", "user", DateTime.Now.AddYears(100)));
            }
        }

        public void Start()
        {
            Console.WriteLine("Done!");
            this.Server.Prefixes.Add(string.Format("{0}{1}", BaseUrl, Port));
            this.Server.Start();

            new Thread(
                () => {
                    Console.WriteLine("Started server!");
                    while (true)
                    {

                        HttpListenerContext ctx = this.Server.GetContext();
                        ThreadPool.QueueUserWorkItem((_) => ProcessRequest(ctx));
                    }                        
                }
            ).Start();

            new Thread(
                () =>
                {
                    Console.WriteLine("Started tick handler!");
                    while (true)
                    {
                        this.TickHandler();
                        Thread.Sleep(1000);
                    }
                }
            ).Start();
        }

        public void AddNoAuthUrl(string url)
        {
            this.router.AddNoAuthUrl(url);
        }

        private void TickHandler()
        {
            foreach (KeyValuePair<int, Tuple<string, string, DateTime>> session in Sessions.sessions)
            {
                if (session.Value.Item3 <= DateTime.Now.AddMinutes(-10))
                {
                    lock (Sessions.sessions)
                    {
                        Console.WriteLine(session.Value.Item1);
                        Sessions.sessions.Remove(session.Key);
                        break;
                    }
                }
            }
        }

        private void ProcessRequest(HttpListenerContext ctx)
        {
            List<string> splitUrl = ctx.Request.Url.ToString().Split('/').ToList<string>();
#if DEBUG
            Console.WriteLine("Requested url: " + ctx.Request.Url + " HTTP Method: " + ctx.Request.HttpMethod);
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            for (int i = 1; i < urlSplit; i++)
                splitUrl.RemoveAt(0);

            router.Call(string.Join("/", splitUrl), ctx);
#if DEBUG
            sw.Stop();
            double ticks = sw.ElapsedTicks;
            double ns = (ticks / Stopwatch.Frequency) * 1000000000;
            Console.WriteLine("Request handled in: " + ns + " Nano seconds");
#endif
        }

        public void AddRouting<T>(string url) where T : Page, new()
        {
            Console.WriteLine("Added route: " + url.ToLower());
            this.router.Add(url.ToLower(), new T());
        }

        public void AddRouting(string url, Page route)
        {
            url = url.Replace("_", "");
            Console.WriteLine("Added route: " + url.ToLower());
            this.router.Add(url.ToLower(), route);
        }

        public void AddRouting<T>(string url, Type ds) where T : Page, new()
        {
            Console.WriteLine("Added route: " + url.ToLower());
            this.router.Add(url.ToLower(), new T() { Dataset = ds });
        }

        public void ConstructService(Assembly assembly)
        {
            IEnumerable<Type> generate = GetGenerateClasses(assembly);
            foreach (Type type in generate)
            {
                BuildClass(type);
            }
        }

        private IEnumerable<Type> GetGenerateClasses(Assembly assembly)
        {
            foreach(Type type in assembly.GetTypes())
                if(type.GetCustomAttributes(typeof(AutoGenerate), true).Length > 0)
                    yield return type;
        }

        private bool BuildClass(Type type)
        {
            Type[] types = { typeof(HttpListenerContext) };
            TypeBuilder tb = GetTypeBuilder(type.Name);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            MethodBuilder mB = tb.DefineMethod("Init", MethodAttributes.Public | MethodAttributes.Virtual, null, types);
            ILGenerator il = mB.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(Page).GetMethod("Process", Type.EmptyTypes));
            il.Emit(OpCodes.Ret);

            tb.DefineMethodOverride(mB, typeof(Page).GetMethod("Init"));

            try
            {
                Type tc = tb.CreateType();
                Page p = (Page)Activator.CreateInstance(tc);
                p.Dataset = type;
                this.AddRouting(type.Name, p);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        private TypeBuilder GetTypeBuilder(string name)
        {
            string typeSignature = name;
            AssemblyName aN = new AssemblyName(typeSignature);
            AssemblyBuilder aB = AppDomain.CurrentDomain.DefineDynamicAssembly(aN, AssemblyBuilderAccess.Run);
            ModuleBuilder mB = aB.DefineDynamicModule("MainModule");
            TypeBuilder tB = mB.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                typeof(Page));

            return tB;
        }


        public void ConstructDataBase(Assembly assembly, bool force, bool delete)
        {
            try
            {
                DB.connection.ChangeDatabase(DB.Database);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Unknown database"))
                {
                    force = true;
                }
            }

            if (force)
            {
                if (delete) new Table() { Query = "DROP DATABASE `" + DB.Database + "`;" }.Execute();
                new Table() { Query = "CREATE DATABASE " + DB.Database }.Execute();
                DB.connection.ChangeDatabase(DB.Database);
                assembly.GetTypes().ToList<Type>().Where(x => x.GetCustomAttributes(typeof(DataBaseEntry), true).Length > 0).ToList<Type>().ForEach(x => Table.GenerateTable(x));
                assembly.GetTypes().ToList<Type>().Where(x => x.GetCustomAttributes(typeof(DataBaseEntry), true).Length > 0).ToList<Type>().ForEach(x => Table.GenerateFK(x));
            }
        }
    }
}