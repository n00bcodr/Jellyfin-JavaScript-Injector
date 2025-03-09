using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.CustomJavaScript.Configuration
{
    /// <summary>
    /// Configuration class for the Custom JavaScript plugin.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            CustomJavaScript = string.Empty;
        }

        /// <summary>
        /// Gets or sets the custom JavaScript code to be injected.
        /// </summary>
        public string CustomJavaScript { get; set; }
    }
}