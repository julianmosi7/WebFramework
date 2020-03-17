using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFramework.Helpers
{
  public static class ExtensionMethods
  {
    public const int INOUT_PAD = 3;

    public static void PrintWithColor(this string contents, string header, ConsoleColor colBack, ConsoleColor colFore, int padLeft = 0)
    {
      var bgCol = Console.BackgroundColor;
      var foreCol = Console.ForegroundColor;
      Console.BackgroundColor = colBack;
      Console.ForegroundColor = colFore;
      if (padLeft > 0) header = header.PadLeft(padLeft, ' ');
      Console.Write(header);
      Console.BackgroundColor = bgCol;
      Console.ForegroundColor = foreCol;
      if (contents != null) Console.WriteLine($" {contents}");
    }

    public static List<string> ReadAllLinesFromStream(this StreamReader inStream)
    {
      var linesIn = new List<string>();
      while (true)
      {
        string line = inStream.ReadLine();
        if (line == null || line.Trim().Length == 0) break;
        linesIn.Add(line);
      }
      return linesIn;
    }

    public static int Write2Stream(this Stream outStream, string s, bool logToConsole = false, ConsoleColor foreColor = ConsoleColor.Cyan)
    {
      try
      {
        var bytesToSend = Encoding.ASCII.GetBytes($"{s}\r\n");
        outStream.Write(bytesToSend, 0, bytesToSend.Length);
        if (!logToConsole) return bytesToSend.Length;
        PrintWithColor(s, "<--", foreColor, ConsoleColor.Black, INOUT_PAD);
        return bytesToSend.Length;
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return 0;
      }
    }

    public static void WriteResponse(this Stream outStream, string responseContent, string mime, int code)
    {
      Console.WriteLine("Preparing response");
      if (responseContent == null) responseContent = "<html><body><h1>Die angeforderte Seite existiert nicht</h1></body></html>";
      Write2Stream(outStream, $"HTTP/1.1 {code} {StatusCode.StatusDescription(code)}", true, code == StatusCode.HTTP_OK ? ConsoleColor.Cyan : ConsoleColor.Red);
      Write2Stream(outStream, $"Date: {DateTime.Now:ddd, dd MMM yyyy hh:mm:ss} GMT", true);
      Write2Stream(outStream, "Server: WebServerSimple 1.0", true);
      Write2Stream(outStream, $"Content-Length: {responseContent.Length}", true);
      Write2Stream(outStream, $"Content-Type: {mime}", true);
      Write2Stream(outStream, "");
      int nrBytes = Write2Stream(outStream, responseContent);
      string bodyHead = new string(responseContent.ToString().Replace(Environment.NewLine, "").Take(40).ToArray());
      $"{bodyHead}...".PrintWithColor("<--", ConsoleColor.Cyan, ConsoleColor.Black, INOUT_PAD);
    }

    public static void WriteErrorResponse(this Stream outStream, int responseCode, string desc)
    {
      "".PrintWithColor($"> *** {responseCode}: {desc} ***", ConsoleColor.Red, ConsoleColor.White);
      var sb = new StringBuilder();
      sb.Append("<html><head><title>WebServerSimple-Error</title></head<body>");
      sb.Append($"<h1>HTTP/1.1 {responseCode}</h1>");
      sb.Append($"<h3>{desc}</h3>");
      sb.Append("</body></html>");
      WriteResponse(outStream, sb.ToString(), "text/html", responseCode);
    }

  }
}
