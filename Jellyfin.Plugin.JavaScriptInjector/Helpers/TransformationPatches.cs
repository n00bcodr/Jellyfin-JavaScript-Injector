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

            var startComment = "<!-- BEGIN JavaScript Injector Plugin -->";
            var endComment = "<!-- END JavaScript Injector Plugin -->";
            // Public scripts are loaded immediately for all users (including on the login page).
            var publicScriptTag = "<script defer src=\"JavaScriptInjector/public.js\"></script>";
            // This inline script waits for the user to be authenticated and then fetches the private scripts.
            // It uses the official ApiClient.fetch method, which automatically includes authentication headers.
            var privateScriptLoader = @"
            <script>
                (function() {
                    'use strict';
                    const fetchPrivateScripts = () => {
                        // Check if the API client is fully initialized and a user is logged in.
                        if (window.ApiClient && typeof window.ApiClient.getCurrentUserId === 'function' && window.ApiClient.getCurrentUserId() && window.ApiClient.serverInfo) {
                            // Once authenticated, stop checking.
                            clearInterval(authInterval);
                            // Use the built-in ApiClient.fetch to make an authenticated request for the private scripts.
                            ApiClient.fetch({
                                url: ApiClient.getUrl('JavaScriptInjector/private.js'),
                                type: 'GET',
                                dataType: 'text'
                            }).then(scriptText => {
                                if (scriptText && scriptText.trim().length > 0) {
                                    const scriptElement = document.createElement('script');
                                    scriptElement.textContent = scriptText;
                                    document.head.appendChild(scriptElement);
                                    console.log('JavaScript Injector: Private scripts loaded successfully.');
                                }
                            }).catch(err => {
                                console.error('JavaScript Injector: Failed to load private scripts.', err);
                            });
                        }
                    };
                    // Set an interval to check for authentication status every 300 milliseconds.
                    const authInterval = setInterval(fetchPrivateScripts, 300);
                })();
            </script>";
            // The full block to be injected, wrapped in comments.
            var injectionBlock = $@"{startComment}
            <!-- Injected using file-transformation -->
            {publicScriptTag}
            {privateScriptLoader}
            {endComment}";

            if (content.Contents.Contains("</body>"))
            {
                return content.Contents.Replace("</body>", $"{injectionBlock}</body>");
            }

            return content.Contents;
        }
    }
}
