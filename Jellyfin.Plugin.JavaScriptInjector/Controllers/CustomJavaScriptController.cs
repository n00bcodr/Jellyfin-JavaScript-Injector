using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Jellyfin.Plugin.JavaScriptInjector.Controllers;

[ApiController]
[Route("JavaScriptInjector")]
[AllowAnonymous] // Allow access to the script without authentication
public class JavaScriptInjectorController : ControllerBase
{
    [HttpGet("loader.js")]
    [Produces("application/javascript")]
    public ActionResult GetLoaderScript()
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return Content("/* Plugin configuration not loaded. */", "application/javascript");
        }

        var scriptBuilder = new StringBuilder();
        scriptBuilder.AppendLine("/* Custom JavaScript from Jellyfin Plugin */");

        foreach (var scriptEntry in config.CustomJavaScripts)
        {
            if (scriptEntry.Enabled && !string.IsNullOrWhiteSpace(scriptEntry.Script))
            {
                scriptBuilder.AppendLine($"/* Script: {scriptEntry.Name} */");
                scriptBuilder.AppendLine("(function() { try {");
                scriptBuilder.AppendLine(scriptEntry.Script);
                scriptBuilder.AppendLine("} catch (e) { console.error('Error in Injected JavaScript [\"" + scriptEntry.Name + "\"]:', e); } })();");
            }
        }

        return Content(scriptBuilder.ToString(), "application/javascript");
    }
}