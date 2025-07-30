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

            var scriptUrl = "/JavaScriptInjector/loader.js";
            var scriptTag = $"<script defer src=\"{scriptUrl}\"></script>";

            try
            {
                var content = await File.ReadAllTextAsync(indexPath, cancellationToken);

                var regex = new Regex("<script.*(JavaScriptInjector|JavaScriptInjector/loader.js).*</script>");

                if (regex.IsMatch(content))
                {
                     content = regex.Replace(content, string.Empty).Trim();
                }

                var closingBodyTag = "</body>";
                if (content.Contains(closingBodyTag))
                {
                    content = content.Replace(closingBodyTag, $"{scriptTag}\n{closingBodyTag}");
                    await File.WriteAllTextAsync(indexPath, content, cancellationToken);
                    _logger.LogInformation("Successfully injected the JavaScriptInjector loader script.");
                }
                else
                {
                    _logger.LogWarning("Could not find </body> tag in index.html. Script not injected.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to inject loader script into index.html.");
            }
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[] { new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerStartup } };
        }
    }
}