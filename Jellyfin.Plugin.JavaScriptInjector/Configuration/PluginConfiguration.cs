using MediaBrowser.Model.Plugins;
using System.Collections.Generic;

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
        }

        /// <summary>
        /// Gets or sets the custom JavaScripts.
        /// </summary>
        public List<CustomJavaScriptEntry> CustomJavaScripts { get; set; }
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
    }
}