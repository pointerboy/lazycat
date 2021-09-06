using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Scriban.Runtime;

namespace WebApplicationConsole
{
    internal class Program
    {
        public static WebServer.WebServer server = new WebServer.WebServer();
        
        public static void Main(string[] args)
        {
            server.AddWebRoute("Index", IndexPage);
            server.AddWebRoute("Test", Test);
            server.Start(@"C:\Users\Luka\RiderProjects\LightWebServer\WebApplicationConsole\Web", 8084);

            Console.ReadKey();
        }

        private static void Test()
        {
            ScriptObject obj = new ScriptObject();
            
            obj.Add("variable", "World");
            server.RenderWebTemplate(@"C:\Users\Luka\RiderProjects\LightWebServer\WebApplicationConsole\Web\test.html", 
                obj);
        }

        private static void IndexPage()
        {
        }
    }
}