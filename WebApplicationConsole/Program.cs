using System;

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
            server.RenderWebTemplate(@"C:\Users\Luka\RiderProjects\LightWebServer\WebApplicationConsole\Web\test.html");
        }

        private static void IndexPage()
        {
            string hello_world = "Hello_World!";
            server.RenderWebTemplate(@"C:\Users\Luka\RiderProjects\LightWebServer\WebApplicationConsole\Web\index.html", hello_world);
        }
    }
}