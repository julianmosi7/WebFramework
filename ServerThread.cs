using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using WebFramework.Helpers;

namespace WebFramework
{
  public class ServerThread
  {

    public static Assembly ExecutingAssembly { get; set; }
    public static string Namespace { get; set; }


    private readonly TcpClient connection;

    public ServerThread(TcpClient connection) => this.connection = connection;

    public void Start() => new Thread(Run).Start();

    #region --------------------------------------------------------- framework
    public void Run()
    {
      Console.WriteLine();
      Console.WriteLine("____________________________________________ Client connected");
      var outStream = connection.GetStream();
      var inStream = new StreamReader(connection.GetStream());
      HandleRequest(inStream, outStream);
      connection.Close();
      inStream.Dispose();
      Console.WriteLine("____________________________________________ Client disconnected");
    }

    private static void HandleRequest(StreamReader inStream, Stream outStream)
    {
      try
      {
        string request = ReadRequest(inStream);                   //1 - ReadRequest
        string mime = MimeMapper.GetMimeType(request);            //2 - detect MIME
        string responseContent = PrepareResponse(request);        //3 - PrepareResponse
        int code = responseContent != null ? StatusCode.HTTP_OK : StatusCode.NOT_FOUND;
        outStream.WriteResponse(responseContent, mime, code);     //4 - WriteResponse
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc.ToString());
        outStream.WriteErrorResponse(StatusCode.HTTP_INTERNAL_SERVER_ERROR, exc.Message);
      }
    }

    private static string ReadRequest(StreamReader inStream)
    {
      var linesIn = inStream.ReadAllLinesFromStream();
      linesIn.ForEach(x => x.PrintWithColor("-->", ConsoleColor.Green, ConsoleColor.Black, ExtensionMethods.INOUT_PAD));
      if (!linesIn.Any()) return null;
      var request = linesIn.First();
      return request;
    }

    public static string PrepareResponse(string request)
    {
      string url = request.Split(' ')[1];
      if (url == "/") url = "/index.html";
      string filename = url.Substring(1);
      if (filename.Contains(".")) return PrepareStaticFile(filename);
      return PrepareRoute(filename);
    }

    #endregion

    private static string PrepareStaticFile(string filename)
    {
      Console.WriteLine($"PrepareStaticFile {filename}");
      string wwwRoot = @"./wwwroot"; //or $@"..\..\wwwroot"
      string path = $@"{wwwRoot}/{filename}";
      if (!File.Exists(path)) return null;
      return File.ReadAllText(path);
    }

    private static string PrepareRoute(string route)
    {
      Console.WriteLine($"PrepareRoute {route}");
      var items = route.Split("/");
      string controller = $"{items[0]}Controller";
      string action = items.Length > 1 ? items[1] : "Index";
      string typeName = $"WebServerSimple.Controllers.{controller}";

      var type = Assembly.GetEntryAssembly().GetType(typeName);
      var instance = Activator.CreateInstance(type);

      string queryString = null;
      if ( action.Contains("?"))
      {
                string[] parts = action.Split("?");
                queryString = parts[1];
                parts = queryString.Split("&");
                string f = parts[0];
                string l = parts[1];
                string[] first = f.Split("=");
                string[] last = l.Split("=");
                string firstname = first[1];
                string lastname = last[1];
                return type.GetMethod(action.Split('?')[0]).Invoke(instance, new object[] { firstname, lastname }) as string;
      }
            return type.GetMethod(action)?.Invoke(instance, null) as string;
    }
  }
}
