using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Jellyfin.Plugin.JavaScriptInjector.Configuration;

namespace Jellyfin.Plugin.JavaScriptInjector.Controllers
{
    [ApiController]
    [Route("JavaScriptInjector")]
    public class JavaScriptInjectorController : ControllerBase
    {
        /// <summary>
        /// This endpoint provides scripts that do NOT require authentication.
        /// It is accessible to everyone, including users on the login page.
        /// </summary>
        [HttpGet("public.js")]
        [Produces("application/javascript")]
        [AllowAnonymous]
        public ActionResult GetPublicScript()
        {
            var config = Plugin.Instance?.Configuration;
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
        public ActionResult GetPrivateScript()
        {
            var config = Plugin.Instance?.Configuration;
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
            // Filter scripts based on whether they are enabled and match the authentication requirement
            var scriptsToInject = config.CustomJavaScripts
                .Where(s => s.Enabled && s.RequiresAuthentication == requiresAuth);

            foreach (var scriptEntry in scriptsToInject)
            {
                if (!string.IsNullOrWhiteSpace(scriptEntry.Script))
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
}
