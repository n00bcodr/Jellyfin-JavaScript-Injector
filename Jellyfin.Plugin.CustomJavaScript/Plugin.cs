using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.CustomJavaScript.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.CustomJavaScript
{
    /// <summary>
    /// The main plugin class.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
        /// <param name="configurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger, IServerConfigurationManager configurationManager)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;

            // Inject custom JavaScript into index.html
            if (!string.IsNullOrWhiteSpace(applicationPaths.WebPath))
            {
                var indexFile = Path.Combine(applicationPaths.WebPath, "index.html");
                if (File.Exists(indexFile))
                {
                    string indexContents = File.ReadAllText(indexFile);
                    string basePath = "";

                    // Get base path from network config
                    try
                    {
                        var networkConfig = configurationManager.GetConfiguration("network");
                        var configType = networkConfig.GetType();
                        var basePathField = configType.GetProperty("BaseUrl");
                        var confBasePath = basePathField?.GetValue(networkConfig)?.ToString()?.Trim('/');

                        if (!string.IsNullOrEmpty(confBasePath)) basePath = "/" + confBasePath.ToString();
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Unable to get base path from config, using '/': {e}", e);
                    }

                    // Don't run if script already exists
                    string scriptReplace = "<script plugin=\"CustomJavaScript\".*?</script>";
                    string script = Configuration.CustomJavaScript;
                    string scriptElement = string.Format("<script plugin=\"CustomJavaScript\" defer=\"defer\">{0}</script>", script);

                    if (!indexContents.Contains(scriptElement))
                    {
                        logger.LogInformation("Attempting to inject CustomJavaScript client script code in {indexFile}", indexFile);
                        logger.LogInformation(scriptElement);

                        // Replace old scripts
                        indexContents = Regex.Replace(indexContents, scriptReplace, "", RegexOptions.Singleline);

                        // Insert script last in body
                        int bodyClosing = indexContents.LastIndexOf("</body>");
                        if (bodyClosing != -1)
                        {
                            indexContents = indexContents.Insert(bodyClosing, scriptElement);

                            try
                            {
                                File.WriteAllText(indexFile, indexContents);
                                logger.LogInformation("Finished injecting CustomJavaScript script code in {indexFile}", indexFile);
                            }
                            catch (Exception e)
                            {
                                logger.LogError("Encountered exception while writing to {indexFile}: {e}", indexFile, e);
                            }
                        }
                        else
                        {
                            logger.LogInformation("Could not find closing body tag in {indexFile}", indexFile);
                        }
                    }
                    else
                    {
                        logger.LogInformation("Found client script already injected in {indexFile}", indexFile);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin Instance { get; private set; }

        /// <inheritdoc />
        public override string Name => "Custom JavaScript";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("38f5aa9b-9c3a-4db0-b4c4-a2d6823e7da7");

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html"
                },
                new PluginPageInfo
                {
                    Name = "CustomJavaScriptJs",
                    EmbeddedResourcePath = $"{GetType().Namespace}.Web.customjavascript.js"
                }
            };
        }
    }
}