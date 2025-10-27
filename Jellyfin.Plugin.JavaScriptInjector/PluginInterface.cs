#nullable enable
using Jellyfin.Plugin.JavaScriptInjector.Model;
using Jellyfin.Plugin.JavaScriptInjector.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.JavaScriptInjector
{
    /// <summary>
    /// Public interface for other plugins to register JavaScript scripts programmatically.
    /// This interface follows the same pattern as the File Transformation plugin for consistency.
    /// </summary>
    public static class PluginInterface
    {
        /// <summary>
        /// Registers a JavaScript script to be injected into the Jellyfin web UI.
        /// This method is designed to be called by other plugins via reflection,
        /// following the same pattern as the File Transformation plugin.
        /// </summary>
        /// <param name="payload">A JObject containing the script registration details.</param>
        /// <remarks>
        /// Expected JObject structure:
        /// {
        ///   "id": "unique-script-id",
        ///   "name": "Script Name",
        ///   "script": "JavaScript code here",
        ///   "enabled": true,
        ///   "requiresAuthentication": false,
        ///   "pluginId": "plugin-guid",
        ///   "pluginName": "Plugin Name",
        ///   "pluginVersion": "1.0.0"
        /// }
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the plugin instance or services are not available.</exception>
        /// <exception cref="ArgumentException">Thrown when required payload fields are missing or invalid.</exception>
        public static bool RegisterScript(JObject payload)
        {
            if (Plugin.Instance == null)
            {
                throw new InvalidOperationException("JavaScript Injector plugin instance is not available.");
            }

            ILogger? logger = null;

            try
            {
                // Get services from DI container
                logger = Plugin.Instance.ServiceProvider?.GetService<ILogger<Plugin>>();
                var registrationService = Plugin.Instance.ServiceProvider?.GetService<IJavaScriptRegistrationService>();

                if (registrationService == null)
                {
                    logger?.LogError("JavaScript registration service is not available ");
                    return false;
                }

                // Convert JObject to strongly typed payload
                JavaScriptRegistrationPayload? castedPayload = payload.ToObject<JavaScriptRegistrationPayload>();

                if (castedPayload == null)
                {
                    logger?.LogError("Failed to convert payload to JavaScriptRegistrationPayload");
                    return false;
                }

                // Validate the payload
                var (isValid, errorMessage) = ValidatePayload(castedPayload);
                if (!isValid)
                {
                    logger?.LogError("Failed to register JavaScript script. Error: {ErrorMessage}", errorMessage);
                    return false;
                }

                // Register the script using the service
                registrationService.RegisterScript(castedPayload);

                logger?.LogInformation(
                    "Successfully registered JavaScript script '{ScriptName}' (ID: {ScriptId}) from plugin '{PluginName}' (ID: {PluginId})",
                    castedPayload.Name,
                    castedPayload.Id,
                    castedPayload.PluginName,
                    castedPayload.PluginId);

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Failed to register JavaScript script. Error: {ErrorMessage}",
                    ex.Message);
                throw; // Re-throw for the calling plugin to handle
            }
        }

        /// <summary>
        /// Unregisters a JavaScript script by its ID.
        /// </summary>
        /// <param name="scriptId">The unique ID of the script to unregister.</param>
        /// <returns>True if the script was found and removed, false otherwise.</returns>
        public static bool UnregisterScript(string scriptId)
        {
            if (Plugin.Instance == null || string.IsNullOrWhiteSpace(scriptId))
            {
                return false;
            }

            ILogger? logger = null;

            try
            {
                logger = Plugin.Instance.ServiceProvider?.GetService<ILogger<Plugin>>();
                var registrationService = Plugin.Instance.ServiceProvider?.GetService<IJavaScriptRegistrationService>();

                if (registrationService == null)
                {
                    logger?.LogWarning("JavaScript registration service is not available for unregistering script {ScriptId}", scriptId);
                    return false;
                }

                bool result = registrationService.UnregisterScript(scriptId);

                if (result)
                {
                    logger?.LogInformation("Successfully unregistered JavaScript script with ID: {ScriptId}", scriptId);
                }
                else
                {
                    logger?.LogWarning("Script with ID {ScriptId} was not found or could not be removed", scriptId);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred while unregistering script {ScriptId}: {ErrorMessage}", scriptId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Unregisters all JavaScript scripts from a specific plugin.
        /// </summary>
        /// <param name="pluginId">The ID of the plugin whose scripts should be removed.</param>
        /// <returns>The number of scripts that were removed.</returns>
        public static int UnregisterAllScriptsFromPlugin(string pluginId)
        {
            if (Plugin.Instance == null || string.IsNullOrWhiteSpace(pluginId))
            {
                return 0;
            }

            ILogger? logger = null;

            try
            {
                logger = Plugin.Instance.ServiceProvider?.GetService<ILogger<Plugin>>();
                var registrationService = Plugin.Instance.ServiceProvider?.GetService<IJavaScriptRegistrationService>();

                if (registrationService == null)
                {
                    logger?.LogError("JavaScript registration service is not available for unregistering scripts from plugin {PluginId}", pluginId);
                    return 0;
                }

                int removedCount = registrationService.UnregisterAllScriptsFromPlugin(pluginId);

                logger?.LogInformation("Unregistered {RemovedCount} JavaScript scripts from plugin {PluginId}", removedCount, pluginId);

                return removedCount;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred while unregistering scripts from plugin {PluginId}: {ErrorMessage}", pluginId, ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Validates the script registration payload.
        /// </summary>
        /// <param name="payload">The payload to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private static (bool IsValid, string ErrorMessage) ValidatePayload(JavaScriptRegistrationPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.Id))
            {
                return (false, "Script ID is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(payload.Name))
            {
                return (false, "Script name is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(payload.Script))
            {
                return (false, "Script content is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(payload.PluginId))
            {
                return (false, "Plugin ID is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(payload.PluginName))
            {
                return (false, "Plugin name is required and cannot be empty.");
            }
            return (true, string.Empty);
        }
    }
}