using System.Reflection;
using System.Text;
using Jellyfin.Plugin.JavaScriptInjector.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.JavaScriptInjector.Controllers
{
    [ApiController]
    [Route("JavaScriptInjector")]
    public class JavaScriptInjectorController : ControllerBase
    {
        // Allowlist of vendored config-page assets (CodeMirror 5, for the code editor
        // on the config page). Keyed by the filename requested in the URL so an
        // arbitrary path can never be used to read other embedded resources.
        private static readonly Dictionary<string, string> VendorAssets = new()
        {
            ["codemirror.min.js"] = "Configuration.vendor.codemirror.codemirror.min.js",
            ["codemirror.min.css"] = "Configuration.vendor.codemirror.codemirror.min.css",
            ["javascript.min.js"] = "Configuration.vendor.codemirror.javascript.min.js",
            ["matchbrackets.min.js"] = "Configuration.vendor.codemirror.matchbrackets.min.js",
            ["closebrackets.min.js"] = "Configuration.vendor.codemirror.closebrackets.min.js",
            ["active-line.min.js"] = "Configuration.vendor.codemirror.active-line.min.js",
            ["material-darker.min.css"] = "Configuration.vendor.codemirror.material-darker.min.css",
            ["lint.min.js"] = "Configuration.vendor.codemirror.lint.min.js",
            ["lint.min.css"] = "Configuration.vendor.codemirror.lint.min.css",
            ["javascript-lint.min.js"] = "Configuration.vendor.codemirror.javascript-lint.min.js",
            ["jshint.min.js"] = "Configuration.vendor.codemirror.jshint.min.js",
        };

        /// <summary>
        /// Serves vendored, config-page-only static assets (currently CodeMirror 5,
        /// for the script code editor) from embedded resources, so the config page
        /// doesn't depend on a CDN. Not used by the injected client scripts.
        /// </summary>
        [HttpGet("vendor/{fileName}")]
        [AllowAnonymous]
        public ActionResult GetVendorAsset(string fileName)
        {
            if (!VendorAssets.TryGetValue(fileName, out var resourceSuffix))
            {
                return NotFound();
            }

            var assembly = Assembly.GetExecutingAssembly();
            // Embedded resource logical names are rooted at the assembly's RootNamespace
            // ("Jellyfin.Plugin.JavaScriptInjector"), not this controller's own namespace.
            var resourceName = $"Jellyfin.Plugin.JavaScriptInjector.{resourceSuffix}";
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return NotFound();
            }

            var contentType = fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                ? "text/css"
                : "application/javascript";

            Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            return new FileStreamResult(stream, contentType);
        }

        /// <summary>
        /// This endpoint provides scripts that do NOT require authentication.
        /// It is accessible to everyone, including users on the login page.
        /// </summary>
        [HttpGet("public.js")]
        [Produces("application/javascript")]
        [AllowAnonymous]
        public ActionResult GetPublicScript([FromQuery] string? v = null)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return Content("/* Plugin configuration not loaded. */", "application/javascript");
            }
            // Generate script content for public (non-authenticated) scripts
            return GenerateScript(config, false);
        }

        /// <summary>
        /// This endpoint provides scripts that DO require authentication.
        /// The [Authorize] attribute ensures that only logged-in users can access it.
        /// </summary>
        [HttpGet("private.js")]
        [Produces("application/javascript")]
        [Authorize]
        public ActionResult GetPrivateScript([FromQuery] string? v = null)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return Content("/* Plugin configuration not loaded. */", "application/javascript");
            }
            // Generate script content for private (authenticated) scripts
            return GenerateScript(config, true);
        }

        /// <summary>
        /// Helper method to generate the JavaScript content based on the authentication requirement.
        /// </summary>
        /// <param name="config">The plugin configuration.</param>
        /// <param name="requiresAuth">A boolean indicating whether to filter for scripts that require authentication.</param>
        /// <returns>An ActionResult containing the JavaScript code.</returns>
        private ActionResult GenerateScript(PluginConfiguration config, bool requiresAuth)
        {
            if (config == null)
            {
                return Content("/* Plugin configuration not loaded. */", "application/javascript");
            }

            var scriptBuilder = new StringBuilder();

            // Filter user scripts based on whether they are enabled and match the authentication requirement
            var userScriptsToInject = config.CustomJavaScripts
                .Where(s => s.Enabled && s.RequiresAuthentication == requiresAuth);

            foreach (var scriptEntry in userScriptsToInject)
            {
                if (!string.IsNullOrWhiteSpace(scriptEntry.Script))
                {
                    scriptBuilder.AppendLine($"/* User Script: {scriptEntry.Name} */");
                    scriptBuilder.AppendLine("(function() { try {");
                    scriptBuilder.AppendLine(scriptEntry.Script);
                    scriptBuilder.AppendLine("} catch (e) { console.error('Error in Injected JavaScript [\"" + scriptEntry.Name + "\"]:', e); } })();");
                }
            }

            // Filter plugin scripts based on whether they are enabled and match the authentication requirement
            var pluginScriptsToInject = config.PluginJavaScripts
                .Where(s => s.Enabled && s.RequiresAuthentication == requiresAuth);

            foreach (var scriptEntry in pluginScriptsToInject)
            {
                if (!string.IsNullOrWhiteSpace(scriptEntry.Script))
                {
                    scriptBuilder.AppendLine($"/* Plugin Script: {scriptEntry.Name} (from {scriptEntry.PluginName}) */");
                    scriptBuilder.AppendLine("(function() { try {");
                    scriptBuilder.AppendLine(scriptEntry.Script);
                    scriptBuilder.AppendLine("} catch (e) { console.error('Error in Plugin JavaScript [\"" + scriptEntry.Name + "\" from \"" + scriptEntry.PluginName + "\"]:', e); } })();");
                }
            }

            return Content(scriptBuilder.ToString(), "application/javascript");
        }
    }
}
