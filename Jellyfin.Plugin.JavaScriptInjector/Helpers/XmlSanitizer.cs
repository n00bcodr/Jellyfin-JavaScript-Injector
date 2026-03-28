using System.Text;
using System.Xml;

namespace Jellyfin.Plugin.JavaScriptInjector.Helpers
{
    /// <summary>
    /// Removes characters that cannot be persisted safely in XML configuration files.
    /// </summary>
    public static class XmlSanitizer
    {
        public static string Sanitize(string? input, string fallback = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return fallback;
            }

            StringBuilder? builder = null;

            for (var index = 0; index < input.Length; index++)
            {
                var current = input[index];
                if (XmlConvert.IsXmlChar(current))
                {
                    if (builder != null)
                    {
                        builder.Append(current);
                    }

                    continue;
                }

                builder ??= new StringBuilder(input.Length);
                if (builder.Length == 0 && index > 0)
                {
                    builder.Append(input, 0, index);
                }
            }

            return builder?.ToString() ?? input;
        }
    }
}