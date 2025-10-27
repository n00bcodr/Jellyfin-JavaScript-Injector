using Jellyfin.Plugin.JavaScriptInjector.Model;

namespace Jellyfin.Plugin.JavaScriptInjector.Services
{
    /// <summary>
    /// Service interface for managing JavaScript script registrations from other plugins.
    /// </summary>
    public interface IJavaScriptRegistrationService
    {
        /// <summary>
        /// Registers a new JavaScript script or updates an existing one.
        /// </summary>
        /// <param name="payload">The script registration payload.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        bool RegisterScript(JavaScriptRegistrationPayload payload);

        /// <summary>
        /// Unregisters a JavaScript script by its ID.
        /// </summary>
        /// <param name="scriptId">The unique ID of the script to unregister.</param>
        /// <returns>True if the script was found and removed, false otherwise.</returns>
        bool UnregisterScript(string scriptId);

        /// <summary>
        /// Unregisters all JavaScript scripts from a specific plugin.
        /// </summary>
        /// <param name="pluginId">The ID of the plugin whose scripts should be removed.</param>
        /// <returns>The number of scripts that were removed.</returns>
        int UnregisterAllScriptsFromPlugin(string pluginId);

        /// <summary>
        /// Validates a script registration payload.
        /// </summary>
        /// <param name="payload">The payload to validate.</param>
        /// <returns>A validation result with any error messages.</returns>
        (bool IsValid, string ErrorMessage) ValidatePayload(JavaScriptRegistrationPayload payload);
    }
}