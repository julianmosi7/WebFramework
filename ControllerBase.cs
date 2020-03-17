using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WebFramework
{
  public abstract class ControllerBase
  {
    private string Root => $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/Views";
    private string InjectToLayout(string view)
    {
      string path = $@"{Root}/Shared/_Layout.html";
      Console.WriteLine($"Reading view {path}");
      string layout = File.ReadAllText(path);
      string html = layout.Replace("@RenderBody()", view);
      return html;
    }

    public string View()
    {
      ExtractCallerInfo(out string viewName, out string controllerName);
      return View(viewName, controllerName);
    }

    public string View(object model)
    {
      ExtractCallerInfo(out string viewName, out string controllerName);
      return View(viewName, controllerName, model);
    }

    private static void ExtractCallerInfo(out string viewName, out string controllerName)
    {
      var callingMethod = new StackTrace().GetFrame(2).GetMethod(); //2, because the "real" method is second on the stack
      viewName = callingMethod.Name;
      controllerName = callingMethod.ReflectedType.Name.Replace("Controller", "");
    }

    public string View(string viewName, string controllerName)
    {
      Console.WriteLine($"ControllerBase::View {viewName}");
      string path = $@"{Root}/{controllerName}/{viewName}.html";
      Console.WriteLine($"Reading view {path}");
      string view = File.ReadAllText(path);
      return InjectToLayout(view);
    }

    public string View(string viewName, string controllerName, object model)
    {
      Console.WriteLine($"ControllerBase::View {viewName} with model {model.GetType().Name}");
      string view = View(viewName, controllerName);

      var placeholders = GetPlaceholders(view);
      Dictionary<string, object> modelValues = ModelAsDict(model);
      view = ReplaceInView(view, placeholders, modelValues);

      return view;
    }

    private static string ReplaceInView(string view, List<string> placeholders, Dictionary<string, object> modelValues)
    {
      placeholders.ForEach(x =>
      {
        Console.WriteLine($"Checking hook {x}");
        string keyLower = x.ToLower();
        if (modelValues.ContainsKey(keyLower))
        {
          string val = modelValues[keyLower].ToString();
          if (val.Length > 20) val = $"{val.Substring(0, 20)}...";
          Console.WriteLine($"  replacing '{x}' with '{val}'");
          view = view.Replace($"@{{{x}}}", modelValues[keyLower].ToString());
        }
        else Console.WriteLine($"  --- placeholder '{x}' not found");
      });
      return view;
    }

    private static Dictionary<string, object> ModelAsDict(object model)
    {
      return model.GetType()
         .GetProperties()
         .Where(x => x.CanRead)
         .Select(x => new
         {
           Hook = x.Name.ToLower(),
           Value = x.GetValue(model)
         })
         .ToDictionary(x => x.Hook.ToLower(), x => x.Value);
    }

    private List<string> GetPlaceholders(string view)
    {
      var regex = new Regex(@"@{(?<name>\w+)}");
      return regex.Matches(view)
        .Select(x => x.Groups["name"].Value)
        .ToList();
    }
  }
}
