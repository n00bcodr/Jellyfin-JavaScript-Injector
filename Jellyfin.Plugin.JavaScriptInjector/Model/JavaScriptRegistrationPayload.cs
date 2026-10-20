#nullable enable
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.JavaScriptInjector.Model
{
    /// <summary>
    /// Payload model for registering JavaScript scripts via the plugin interface.
    /// </summary>
    public class JavaScriptRegistrationPayload
    {
        /// <summary>
        /// Unique identifier for the script.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the script.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The JavaScript code to inject.
        /// </summary>
        [JsonPropertyName("script")]
        public string Script { get; set; } = string.Empty;

        /// <summary>
        /// Optional URL to load instead of inlining <see cref="Script"/>.
        /// </summary>
        [JsonPropertyName("src")]
        public string? Src { get; set; }

        /// <summary>
        /// Whether the script is loaded as an ES module (type="module").
        /// </summary>
        [JsonPropertyName("module")]
        public bool Module { get; set; } = false;

        /// <summary>
        /// Whether the script is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether the script requires user authentication to execute.
        /// </summary>
        [JsonPropertyName("requiresAuthentication")]
        public bool RequiresAuthentication { get; set; } = false;

        /// <summary>
        /// The ID of the plugin registering this script.
        /// </summary>
        [JsonPropertyName("pluginId")]
        public string PluginId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the plugin registering this script.
        /// </summary>
        [JsonPropertyName("pluginName")]
        public string PluginName { get; set; } = string.Empty;

        /// <summary>
        /// The version of the plugin registering this script.
        /// </summary>
        [JsonPropertyName("pluginVersion")]
        public string? PluginVersion { get; set; }
    }
}