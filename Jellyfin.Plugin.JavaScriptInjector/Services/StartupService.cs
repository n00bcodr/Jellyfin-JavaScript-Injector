#nullable enable
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.JavaScriptInjector.Helpers;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.JavaScriptInjector.Services
{
    /// <summary>
    /// Startup cleanup and legacy-fallback injection path.
    ///
    /// Script injection is normally handled entirely by
    /// <see cref="ScriptInjectionStartupFilter"/> at request time, which needs no
    /// scheduled task at all. This task always cleans up any on-disk script block
    /// left by earlier plugin versions (which used to write directly to
    /// index.html), and only re-registers with File Transformation / falls back to
    /// writing index.html itself when the middleware is disabled via
    /// DisableScriptInjectionMiddleware.
    /// </summary>
    public class StartupService : IScheduledTask
    {
        private readonly ILogger<StartupService> _logger;
        private readonly IApplicationPaths _appPaths;

        public string Name => "JavaScript Injector Startup";
        public string Key => "JavaScriptInjectorStartup";
        public string Description => "Cleans up legacy on-disk script injection and, if the request-time middleware is disabled, falls back to File Transformation / direct index.html injection.";
        public string Category => "Startup Services";

        public StartupService(ILogger<StartupService> logger, IApplicationPaths appPaths)
        {
            _logger = logger;
            _appPaths = appPaths;
        }

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                CleanupOldScript();

                var config = Plugin.Instance?.Configuration;
                if (config != null && config.DisableScriptInjectionMiddleware)
                {
                    RegisterFileTransformation();
                }
            }, cancellationToken);
        }

        private void CleanupOldScript()
        {
            try
            {
                var indexPath = Path.Combine(_appPaths.WebPath, "index.html");
                if (!File.Exists(indexPath))
                {
                    _logger.LogWarning("Could not find index.html at path: {Path}. Unable to perform cleanup.", indexPath);
                    return;
                }

                var content = File.ReadAllText(indexPath);
                var startComment = Regex.Escape("<!-- BEGIN JavaScript Injector Plugin -->");
                var endComment = Regex.Escape("<!-- END JavaScript Injector Plugin -->");

                var cleanupRegex = new Regex($"{startComment}[\\s\\S]*?{endComment}\\s*", RegexOptions.Multiline);

                if (cleanupRegex.IsMatch(content))
                {
                    _logger.LogInformation("Found old JavaScript Injector script block in index.html. Removing it now.");
                    content = cleanupRegex.Replace(content, string.Empty);
                    File.WriteAllText(indexPath, content);
                    _logger.LogInformation("Successfully removed old script block.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of old script from index.html.");
            }
        }

        private void RegisterFileTransformation()
        {
            Assembly? fileTransformationAssembly =
                AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x =>
                    x.FullName?.Contains(".FileTransformation") ?? false);

            if (fileTransformationAssembly != null)
            {
                Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");

                if (pluginInterfaceType != null)
                {
                    var payload = new JObject
                    {
                        { "id", Plugin.Instance?.Id.ToString() },
                        { "fileNamePattern", "index.html" },
                        { "callbackAssembly", GetType().Assembly.FullName },
                        { "callbackClass", typeof(TransformationPatches).FullName },
                        { "callbackMethod", nameof(TransformationPatches.IndexHtml) }
                    };

                    pluginInterfaceType.GetMethod("RegisterTransformation")?.Invoke(null, new object?[] { payload });
                    _logger.LogInformation("Successfully registered JavaScript Injector with the File Transformation plugin.");
                }
                else
                {
                    _logger.LogWarning("Could not find PluginInterface in FileTransformation assembly. Using fallback injection method.");
                    if (Plugin.Instance != null)
                    {
                        Plugin.Instance.InjectScript();
                    }
                }
            }
            else
            {
                _logger.LogWarning("File Transformation plugin not found. Using fallback injection method.");
                if (Plugin.Instance != null)
                {
                    Plugin.Instance.InjectScript();
                }
            }
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo()
            {
                Type = TaskTriggerInfoType.StartupTrigger
            };
        }
    }
}
