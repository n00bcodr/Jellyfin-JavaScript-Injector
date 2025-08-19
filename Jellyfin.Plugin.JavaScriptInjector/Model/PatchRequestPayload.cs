#nullable enable
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.JavaScriptInjector.Model
{
    public class PatchRequestPayload
    {
        [JsonPropertyName("contents")]
        public string? Contents { get; set; }
    }
}