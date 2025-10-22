using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.JavaScriptInjector.Configuration;
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

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _appPaths = applicationPaths;
            _logger = logger;
        }

        public static Plugin? Instance { get; private set; }

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

            var startComment = "<!-- BEGIN JavaScript Injector Plugin -->";
            var endComment = "<!-- END JavaScript Injector Plugin -->";
            // Public scripts are loaded immediately for all users (including on the login page).
            var publicScriptTag = "<script defer src=\"../JavaScriptInjector/public.js\"></script>";
            // This inline script waits for the user to be authenticated and then fetches the private scripts.
            // It uses the official ApiClient.fetch method, which automatically includes authentication headers.
            var privateScriptLoader = @"
            <script>
                (function() {
                    'use strict';
                    const fetchPrivateScripts = () => {
                        // Check if the API client is fully initialized and a user is logged in.
                        if (window.ApiClient && typeof window.ApiClient.getCurrentUserId === 'function' && window.ApiClient.getCurrentUserId() && window.ApiClient.serverInfo) {
                            // Once authenticated, stop checking.
                            clearInterval(authInterval);
                            // Use the built-in ApiClient.fetch to make an authenticated request for the private scripts.
                            ApiClient.fetch({
                                url: ApiClient.getUrl('JavaScriptInjector/private.js'),
                                type: 'GET',
                                dataType: 'text'
                            }).then(scriptText => {
                                if (scriptText && scriptText.trim().length > 0) {
                                    const scriptElement = document.createElement('script');
                                    scriptElement.textContent = scriptText;
                                    document.head.appendChild(scriptElement);
                                    console.log('JavaScript Injector: Private scripts loaded successfully.');
                                }
                            }).catch(err => {
                                console.error('JavaScript Injector: Failed to load private scripts.', err);
                            });
                        }
                    };
                    // Set an interval to check for authentication status every 300 milliseconds.
                    const authInterval = setInterval(fetchPrivateScripts, 300);
                })();
            </script>";
            // The full block to be injected, wrapped in comments.
            var injectionBlock = $@"{startComment}
            <!-- Injected into index.html -->
            {publicScriptTag}
            {privateScriptLoader}
            {endComment}";

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
                var newRegex = new Regex($"{startComment}[\\s\\S]*?{endComment}", RegexOptions.Multiline);
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
                var startComment = "<!-- BEGIN JavaScript Injector Plugin -->";
                var endComment = "<!-- END JavaScript Injector Plugin -->";
                var newRegex = new Regex($"{startComment}[\\s\\S]*?{endComment}\\s*", RegexOptions.Multiline);
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
