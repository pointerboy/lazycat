using System;
using LightWebServer;

namespace WebConsole
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Server.SpawnServer(8000, "");
            Console.ReadKey();
        }
    }
}