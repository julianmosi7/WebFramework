using System.Linq;

namespace WebFramework.Helpers
{
  class MimeMapper
  {
    public static string GetMimeType(string request)
    {
      //full list see: https://www.sitepoint.com/mime-types-complete-list/
      string filename = request.Split(' ')[1];
      if (filename == "/") filename = "/index.html";
      if (!filename.Contains(".")) return "text/html";
      string extension = filename.Split('.').Last();
      if (extension == "html") return "text/html";
      if (extension == "htm") return " text/html";
      if (extension == "txt") return "text/plain";
      if (extension == "gif") return "image/gif";
      if (extension == "jpg") return "image/jpeg";
      if (extension == "jpeg") return "image/jpeg";
      if (extension == "png") return "image/png";
      if (extension == "js") return "application/javascript";
      if (extension == "json") return "application/json";
      if (extension == "css") return "text/css";
      return "text/html";
    }
  }
}
