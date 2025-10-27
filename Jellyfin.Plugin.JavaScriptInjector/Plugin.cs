using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.JavaScriptInjector.Configuration;
using Jellyfin.Plugin.JavaScriptInjector.Helpers;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JavaScriptInjector
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly IApplicationPaths _appPaths;
        private readonly ILogger<Plugin> _logger;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger, IServiceProvider serviceProvider)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _appPaths = applicationPaths;
            _logger = logger;
            ServiceProvider = serviceProvider;
        }

        public static Plugin? Instance { get; private set; }

        public IServiceProvider ServiceProvider { get; }

        public override string Name => "JavaScript Injector";

        public override Guid Id => Guid.Parse("f5a34f7b-2e8a-4e6a-a722-3a216a81b374");

        public string IndexHtmlPath => Path.Combine(_appPaths.WebPath, "index.html");

        public void InjectScript()
        {
            var indexPath = IndexHtmlPath;
            if (!File.Exists(indexPath))
            {
                _logger.LogError("Could not find index.html at path: {Path}", indexPath);
                return;
            }

            var injectionBlock = JavascriptHelper.BuildInjectionBlock();

            try
            {
                var content = File.ReadAllText(indexPath);

                if (content.Contains(injectionBlock))
                {
                    _logger.LogInformation("JavaScript Injector script is already correctly injected. No changes needed.");
                    return;
                }
                // --- Removal Logic ---
                // This logic is designed to be idempotent and handle upgrades gracefully.

                // 1. Remove any blocks from this new, comment-wrapped version.
                var newRegex = new Regex($"{JavascriptHelper.StartComment}[\\s\\S]*?{JavascriptHelper.EndComment}", RegexOptions.Multiline);
                content = newRegex.Replace(content, string.Empty);

                var closingBodyTag = "</body>";
                if (content.Contains(closingBodyTag))
                {
                    content = content.Replace(closingBodyTag, $"{injectionBlock}\n{closingBodyTag}");
                    File.WriteAllText(indexPath, content);
                    _logger.LogInformation("Successfully injected the JavaScriptInjector script block.");
                }
                else
                {
                    _logger.LogWarning("Could not find </body> tag in index.html. Scripts not injected.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to inject script block into index.html.");
            }
        }

        public override void OnUninstalling()
        {
            try
            {
                var indexPath = IndexHtmlPath;
                if (!File.Exists(indexPath))
                {
                    _logger.LogError("Could not find index.html at path: {Path}", indexPath);
                    return;
                }

                var content = File.ReadAllText(indexPath);
                var newRegex = new Regex($"{JavascriptHelper.StartComment}[\\s\\S]*?{JavascriptHelper.EndComment}\\s*", RegexOptions.Multiline);
                if (newRegex.IsMatch(content))
                {
                    content = newRegex.Replace(content, string.Empty);
                    File.WriteAllText(indexPath, content);
                    _logger.LogInformation("Successfully removed the JavaScript Injector script from index.html during uninstall.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to remove script from index.html during uninstall.");
            }

            base.OnUninstalling();
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    DisplayName = "JS Injector",
                    EnableInMainMenu = true,
                    EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html",
                }
            };
        }
    }
}
