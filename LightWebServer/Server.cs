using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helpers;

namespace LightWebServer
{
   public static class Server
   {
      private static HttpListener _listener;
      public static int maxSimultaneousConnections = 25;
      private static Semaphore _semaphore = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

      /*
       * TODO: Implement a new way of scanning addresses that can be accessed
       */
      private static List<IPAddress> GetLocalHostIPs()
      {
         IPHostEntry localHost;

         localHost = Dns.GetHostEntry(Dns.GetHostName());
         List<IPAddress> returnValue =
            localHost.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

         return returnValue;
      }

      private static HttpListener SpawnListener(List<IPAddress> localhosts)
      {
         HttpListener listener = new HttpListener();
         listener.Prefixes.Add("http://localhost/");
         
         localhosts.ForEach(ip =>
         {
            Console.WriteLine("Listening on: " + "http://" + ip.ToString() + "/");
            listener.Prefixes.Add("http://" + ip.ToString() + "/");
         });

         return listener;
      }

      private static void Start(HttpListener listener)
      {
         listener.Start();

         Task.Run(() => RunServer(listener));
      }

      private static void RunServer(HttpListener listener)
      {
         while (true)
         {
            _semaphore.WaitOne();

            StartConnectionListener(listener);
         }
      }

      private static async void StartConnectionListener(HttpListener listener)
      {
         HttpListenerContext context = await listener.GetContextAsync();
         _semaphore.Release();

         Log(context.Request);
         
         string response = "Hello World!";
         byte[] encoded = Encoding.UTF8.GetBytes(response);
         context.Response.ContentLength64 = encoded.Length;
         context.Response.OutputStream.Write(encoded, 0, encoded.Length);
         context.Response.OutputStream.Close();
      }

      public static void Log(HttpListenerRequest request)
      {
         // TODO: Work a possibly better logging system

         string log = String.Format("REQUEST: {0} {1}/{2}", request.RemoteEndPoint,
            request.HttpMethod, request.Url.AbsoluteUri.RightOf('/', 3));
         
         Console.WriteLine(log);
      }

      public static void SpawnServer(int port)
      {
         HttpListener listener = new HttpListener();
         listener.Prefixes.Add("http://localhost:8000/");

         Start(listener);
      }
   }
}