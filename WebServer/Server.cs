using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Scriban;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;

namespace WebServer
{
    public class WebServer
    {
        private Thread _thread;
        private HttpListener _httpListener;
        private int _runningPort;
        
        private string _rootDirectory;


        private HttpListenerContext _dirtyContext;

        private static readonly IDictionary<string, string> MimeTypeMappings =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {".asf", "video/x-ms-asf"},
                {".asx", "video/x-ms-asf"},
                {".avi", "video/x-msvideo"},
                {".bin", "application/octet-stream"},
                {".cco", "application/x-cocoa"},
                {".crt", "application/x-x509-ca-cert"},
                {".css", "text/css"},
                {".deb", "application/octet-stream"},
                {".der", "application/x-x509-ca-cert"},
                {".dll", "application/octet-stream"},
                {".dmg", "application/octet-stream"},
                {".ear", "application/java-archive"},
                {".eot", "application/octet-stream"},
                {".exe", "application/octet-stream"},
                {".flv", "video/x-flv"},
                {".gif", "image/gif"},
                {".hqx", "application/mac-binhex40"},
                {".htc", "text/x-component"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".ico", "image/x-icon"},
                {".img", "application/octet-stream"},
                {".iso", "application/octet-stream"},
                {".jar", "application/java-archive"},
                {".jardiff", "application/x-java-archive-diff"},
                {".jng", "image/x-jng"},
                {".jnlp", "application/x-java-jnlp-file"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".js", "application/x-javascript"},
                {".mml", "text/mathml"},
                {".mng", "video/x-mng"},
                {".mov", "video/quicktime"},
                {".mp3", "audio/mpeg"},
                {".mpeg", "video/mpeg"},
                {".mpg", "video/mpeg"},
                {".msi", "application/octet-stream"},
                {".msm", "application/octet-stream"},
                {".msp", "application/octet-stream"},
                {".pdb", "application/x-pilot"},
                {".pdf", "application/pdf"},
                {".pem", "application/x-x509-ca-cert"},
                {".pl", "application/x-perl"},
                {".pm", "application/x-perl"},
                {".png", "image/png"},
                {".prc", "application/x-pilot"},
                {".ra", "audio/x-realaudio"},
                {".rar", "application/x-rar-compressed"},
                {".rpm", "application/x-redhat-package-manager"},
                {".rss", "text/xml"},
                {".run", "application/x-makeself"},
                {".sea", "application/x-sea"},
                {".shtml", "text/html"},
                {".sit", "application/x-stuffit"},
                {".swf", "application/x-shockwave-flash"},
                {".tcl", "application/x-tcl"},
                {".tk", "application/x-tcl"},
                {".txt", "text/plain"},
                {".war", "application/java-archive"},
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
                {".xml", "text/xml"},
                {".xpi", "application/x-xpinstall"},
                {".zip", "application/zip"},
            };

        private static IDictionary<string, Action> Routes = new Dictionary<string, Action>(StringComparer.InvariantCultureIgnoreCase);

        public int GetEmptyPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }

        private void Stop()
        {
            _thread.Abort();
            _httpListener.Stop();
        }

        private void ListenForConnections()
        {
            _httpListener = new HttpListener();
            string prefix = String.Format("http://*:{0}/", _runningPort);
            
            _httpListener.Prefixes.Add(prefix);
            _httpListener.Start();

            while (true)
            {
                try
                {
                    var context = _httpListener.GetContext();
                    ProcessWebRequest(context);
                }
                catch (HttpListenerException exception)
                {
                    Console.WriteLine("ERROR: HTTP Listener raised an exception!");
                    Console.WriteLine(exception.Message);
                }
            }
        }

        public void AddWebRoute(string path, Action action)
        {
            Routes.Add(path, action);
        }

        private string ParseWebFile(string template, ScriptObject arguments)
        {
            // TODO: Figure out a new way for passing data
            
            string workingTemplate = File.ReadAllText(template);
            var scribanTemplate = Template.Parse(workingTemplate);

            var context = new TemplateContext(arguments);
            var result = scribanTemplate.Render(context);
            
            return result;
        }
        
        public void RenderWebTemplate(string templateName, ScriptObject arguments)
        {
            var filename = templateName;
            string returnFile = ParseWebFile(filename, arguments);
            
            if (File.Exists(filename))
            {
                try
                {
                    byte[] returnBytes = Encoding.ASCII.GetBytes(returnFile);
                    Stream input = new MemoryStream(returnBytes);

                    //Adding permanent http response headers
                    string mime;
                    _dirtyContext.Response.ContentType = MimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime)
                        ? mime
                        : "application/octet-stream";
                    _dirtyContext.Response.ContentLength64 = input.Length;
                    
                    _dirtyContext.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    _dirtyContext.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));

                    var buffer = new byte[1024 * 16];
                    int nbytes;
                    
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        _dirtyContext.Response.OutputStream.Write(buffer, 0, nbytes);
                    
                    input.Close();

                    _dirtyContext.Response.StatusCode = (int) HttpStatusCode.OK;
                    _dirtyContext.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    _dirtyContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }
            }

            _dirtyContext.Response.OutputStream.Close();
        }
        private void ProcessWebRequest(HttpListenerContext context)
        {
            string routePath = context.Request.Url.AbsolutePath;
            bool pageExists = false;
            
            foreach (var routeElement in Routes)
            {
                if (routeElement.Key == routePath.TrimStart('/'))
                {
                    _dirtyContext = context;
                    
                    var routeAction = routeElement.Value;
                    routeAction.Invoke();
                    
                    pageExists = true;
                }
            }

            if (!pageExists)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
            }
            
            string request = String.Format("Request: {0} {1}", context.Request.Url.AbsolutePath,
                context.Response.StatusCode);
            
            Console.WriteLine(request);
            context.Response.Close();

        }

        public void Start(string path, int port)
        {
            _rootDirectory = path;
            _runningPort = port;
            
            _thread = new Thread(ListenForConnections);

            _thread.IsBackground = true;
            _thread.Start();
            
            Console.WriteLine("SERVER: Started on http://localhost:" + _runningPort);
        }
    }
}