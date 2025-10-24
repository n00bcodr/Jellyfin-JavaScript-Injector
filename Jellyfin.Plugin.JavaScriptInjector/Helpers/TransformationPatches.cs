using Jellyfin.Plugin.JavaScriptInjector.Model;

namespace Jellyfin.Plugin.JavaScriptInjector.Helpers
{
    public static class TransformationPatches
    {
        public static string IndexHtml(PatchRequestPayload content)
        {
            if (string.IsNullOrEmpty(content.Contents))
            {
                return content.Contents ?? string.Empty;
            }

            var injectionBlock = JavascriptHelper.BuildInjectionBlock();

            if (content.Contents.Contains("</body>"))
            {
                return content.Contents.Replace("</body>", $"{injectionBlock}</body>");
            }

            return content.Contents;
        }
    }
}
