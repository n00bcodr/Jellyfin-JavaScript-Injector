using Jellyfin.Plugin.JavaScriptInjector.Helpers;
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

        /// <summary>
        /// Gets or sets a value indicating whether the request-time script injection
        /// middleware (<see cref="Services.ScriptInjectionStartupFilter"/>) is disabled.
        /// When disabled, the plugin falls back to registering with the File
        /// Transformation plugin (if installed) or writing directly to index.html.
        /// Off by default -- the middleware is the primary injection path.
        /// </summary>
        public bool DisableScriptInjectionMiddleware { get; set; }
    }

    /// <summary>
    /// Represents a single custom javascript entry.
    /// </summary>
    public class CustomJavaScriptEntry
    {
        private string _name = "My Custom Script";
        private string _script = string.Empty;

        /// <summary>
        /// Gets or sets the name of the script.
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = XmlSanitizer.Sanitize(value, "My Custom Script");
        }

        /// <summary>
        /// Gets or sets the script content.
        /// </summary>
        public string Script
        {
            get => _script;
            set => _script = XmlSanitizer.Sanitize(value);
        }

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
        private string _id = string.Empty;
        private string _pluginId = string.Empty;
        private string _pluginName = string.Empty;
        private string _pluginVersion = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for this script entry.
        /// </summary>
        public string Id
        {
            get => _id;
            set => _id = XmlSanitizer.Sanitize(value);
        }

        /// <summary>
        /// Gets or sets the ID of the plugin that registered this script.
        /// </summary>
        public string PluginId
        {
            get => _pluginId;
            set => _pluginId = XmlSanitizer.Sanitize(value);
        }

        /// <summary>
        /// Gets or sets the name of the plugin that registered this script.
        /// </summary>
        public string PluginName
        {
            get => _pluginName;
            set => _pluginName = XmlSanitizer.Sanitize(value);
        }

        /// <summary>
        /// Gets or sets the version of the plugin that registered this script.
        /// </summary>
        public string PluginVersion
        {
            get => _pluginVersion;
            set => _pluginVersion = XmlSanitizer.Sanitize(value);
        }
    }
}