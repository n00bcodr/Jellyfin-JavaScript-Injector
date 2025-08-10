using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JavaScriptInjector.ScheduledTasks
{
    public class InjectScriptTask : IScheduledTask
    {
        private readonly ILogger<InjectScriptTask> _logger;
        private readonly IApplicationPaths _appPaths;

        public InjectScriptTask(ILoggerFactory loggerFactory, IApplicationPaths appPaths)
        {
            _logger = loggerFactory.CreateLogger<InjectScriptTask>();
            _appPaths = appPaths;
        }

        public string Name => "Inject JavaScript";
        public string Key => "InjectJavaScript";
        public string Description => "Injects JavaScript into the web client on boot";
        public string Category => "Application";

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var indexPath = Path.Combine(_appPaths.WebPath, "index.html");
            if (!File.Exists(indexPath))
            {
                _logger.LogError("Could not find index.html at path: {Path}", indexPath);
                return;
            }

            // Define new, safe delimiters for the injection block.
            var startComment = "<!-- BEGIN JavaScript Injector Plugin -->";
            var endComment = "<!-- END JavaScript Injector Plugin -->";

            // Public scripts are loaded immediately for all users (including on the login page).
            var publicScriptTag = "<script defer src=\"/JavaScriptInjector/public.js\"></script>";

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
            // The full block to be injected, now wrapped in comments.
            var injectionBlock = $@"{startComment}
            {publicScriptTag}
            {privateScriptLoader}
            {endComment}";

            try
            {
                var content = await File.ReadAllTextAsync(indexPath, cancellationToken);

                // --- Removal Logic ---
                // This logic is designed to be idempotent and handle upgrades gracefully.

                // 1. Remove any blocks from this new, comment-wrapped version.
                var newRegex = new Regex($"{startComment}[\\s\\S]*?{endComment}", RegexOptions.Multiline);
                content = newRegex.Replace(content, string.Empty);

                // 2. Remove any blocks from the previous. This regex is now much more specific
                // and avoid consuming other plugins' scripts. (v1.1.0.0)
                var oldRegex = new Regex("(<script defer src=\"/JavaScriptInjector/public.js\"></script>\\s*<script>[\\s\\S]*?clearInterval\\(authInterval\\);[\\s\\S]*?</script>)");
                content = oldRegex.Replace(content, string.Empty);

                // 3. Remove the original single loader.js tag for upgrades from very old versions. (v1.0.0.0)
                var oldestRegex = new Regex("<script.*JavaScriptInjector/loader.js.*</script>");
                content = oldestRegex.Replace(content, string.Empty);

                // After all removal operations, trim whitespace from the file content.
                content = content.Trim();

                // --- Injection Logic ---
                var closingBodyTag = "</body>";
                if (content.Contains(closingBodyTag))
                {
                    // Inject the new script block before the closing body tag.
                    content = content.Replace(closingBodyTag, $"{injectionBlock}\n{closingBodyTag}");
                    await File.WriteAllTextAsync(indexPath, content, cancellationToken);
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

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[] { new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerStartup } };
        }
    }
}
