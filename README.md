<img src="https://i.ibb.co/PmBHKdC/lazy-cat-cartoon-cat-sketch-isolated-white-background-lazy-cat-cartoon-cat-sketch-isolated-white-bac.png" alt="lazy-cat-cartoon-cat-sketch-isolated-white-background-lazy-cat-cartoon-cat-sketch-isolated-white-bac" border="0">

# Lazycat
Lazycat is C# .NET library that contains a small HTTP web server with a microframework powered by Scriban - fast, powerful, safe and lightweight scripting language. It does not require having IIS server installed or any of the ASP.NET libraries in order to run.

# Notes
As of right now, it is only a hobby, therefore, the project is not meant to be served in a production environment!

# Getting Started

The first parameter in the Start method is used for root directory of the web application. 

```csharp
 public static WebServer server = new WebServer();

 public static void Main(string[] args) {
   server.Start("/Web", 8084);

   Console.ReadKey();
 }
```

This should print out a message ```SERVER: Started on``` followed by the port that you have assigned.

# Web Routing

Add a web route using the ```AddWebRoute``` that takes in route path (e.g "index", "faq", ...) and a method that will be executed once a request reaches that path.
```csharp
 server.AddWebRoute("Index", IndexPage);
 
 // ... 
  private static void IndexPage() {
  ScriptObject pageParams = new ScriptObject();

  pageParams.Add("variable", "World");
  server.RenderWebTemplate("index.html", pageParams);
 }
```

```html
<!DOCTYPE html>
<html>
<head>
    <title>Title</title>
</head>
<body>
    Hello {{variable}}!
</body>
</html>
```

Rendering engine lazycat uses is Scriban. If you wish to learn more about how the language works, check out: https://github.com/scriban/scriban/blob/master/doc/language.md

```html
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product.name }}</h2>
           Price: {{ product.price }}
           {{ product.description | string.truncate 15 }}
    </li>
  {{ end }}
</ul>
```
