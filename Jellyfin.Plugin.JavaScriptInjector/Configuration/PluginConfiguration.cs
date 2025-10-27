using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JavaScriptInjector.Configuration
{
    /// <summary>
    /// Configuration class for the JavaScript Injector plugin.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            CustomJavaScripts = new List<CustomJavaScriptEntry>();
            PluginJavaScripts = new List<PluginJavaScriptEntry>();
        }

        /// <summary>
        /// Gets or sets the custom JavaScripts.
        /// </summary>
        public List<CustomJavaScriptEntry> CustomJavaScripts { get; set; }

        /// <summary>
        /// Gets or sets the JavaScripts registered by other plugins.
        /// </summary>
        public List<PluginJavaScriptEntry> PluginJavaScripts { get; set; }
    }

    /// <summary>
    /// Represents a single custom javascript entry.
    /// </summary>
    public class CustomJavaScriptEntry
    {
        /// <summary>
        /// Gets or sets the name of the script.
        /// </summary>
        public string Name { get; set; } = "My Custom Script";

        /// <summary>
        /// Gets or sets the script content.
        /// </summary>
        public string Script { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this script is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this script requires authentication to be loaded.
        /// </summary>
        public bool RequiresAuthentication { get; set; } = false;
    }

    /// <summary>
    /// Represents a JavaScript entry registered by another plugin.
    /// </summary>
    public class PluginJavaScriptEntry : CustomJavaScriptEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier for this script entry.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the plugin that registered this script.
        /// </summary>
        public string PluginId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the plugin that registered this script.
        /// </summary>
        public string PluginName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the plugin that registered this script.
        /// </summary>
        public string PluginVersion { get; set; } = string.Empty;
    }
}