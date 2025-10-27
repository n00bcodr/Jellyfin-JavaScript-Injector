using Jellyfin.Plugin.JavaScriptInjector.Configuration;
using Jellyfin.Plugin.JavaScriptInjector.Model;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JavaScriptInjector.Services
{
    /// <summary>
    /// Implementation of JavaScript registration service for managing scripts from other plugins.
    /// </summary>
    public class JavaScriptRegistrationService : IJavaScriptRegistrationService
    {
        private readonly ILogger<JavaScriptRegistrationService> _logger;
        private static readonly int MaxChars = 1024 * 1024 / 2; // ~1MB worth of UTF-16 chars

        public JavaScriptRegistrationService(ILogger<JavaScriptRegistrationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a new JavaScript script or updates an existing one.
        /// </summary>
        /// <param name="payload">The script registration payload.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool RegisterScript(JavaScriptRegistrationPayload payload)
        {
            try
            {
                var validation = ValidatePayload(payload);
                if (!validation.IsValid)
                {
                    _logger.LogError("Script registration failed validation: {ErrorMessage}", validation.ErrorMessage);
                    return false;
                }

                if (Plugin.Instance == null)
                {
                    _logger.LogError("Plugin instance is not available");
                    return false;
                }

                var config = Plugin.Instance.Configuration;
                if (config == null)
                {
                    _logger.LogError("Plugin configuration is not available");
                    return false;
                }

                // Check if a script with this ID already exists
                var existingScript = config.PluginJavaScripts.FirstOrDefault(s => s.Id == payload.Id);

                if (existingScript != null)
                {
                    // Update existing script
                    _logger.LogInformation("Updating existing script {ScriptId} from plugin {PluginName}", payload.Id, payload.PluginName);
                    UpdateScriptEntry(ref existingScript, payload);
                }
                else
                {
                    // Add new script
                    _logger.LogInformation("Registering new script {ScriptId} from plugin {PluginName}", payload.Id, payload.PluginName);
                    var newScript = CreateScriptEntry(payload);
                    config.PluginJavaScripts.Add(newScript);
                }

                // Save configuration asynchronously
                Plugin.Instance.SaveConfiguration();

                _logger.LogDebug("Successfully registered/updated script {ScriptId}", payload.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register script {ScriptId} from plugin {PluginName}", payload.Id, payload.PluginName);
                return false;
            }
        }


        /// <summary>
        /// Unregisters a JavaScript script by its ID.
        /// </summary>
        /// <param name="scriptId">The unique ID of the script to unregister.</param>
        /// <returns>True if the script was found and removed, false otherwise.</returns>
        public bool UnregisterScript(string scriptId)
        {
            if (string.IsNullOrWhiteSpace(scriptId))
            {
                _logger.LogWarning("Cannot unregister script: scriptId is null or empty");
                return false;
            }

            try
            {
                if (Plugin.Instance == null)
                {
                    _logger.LogError("Plugin instance is not available");
                    return false;
                }

                var config = Plugin.Instance.Configuration;
                if (config == null)
                {
                    _logger.LogError("Plugin configuration is not available");
                    return false;
                }

                var scriptToRemove = config.PluginJavaScripts.FirstOrDefault(s => s.Id == scriptId);
                if (scriptToRemove != null)
                {
                    config.PluginJavaScripts.Remove(scriptToRemove);
                    Plugin.Instance.SaveConfiguration();

                    _logger.LogInformation("Successfully unregistered script {ScriptId}", scriptId);
                    return true;
                }

                _logger.LogWarning("Script {ScriptId} not found for unregistration", scriptId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister script {ScriptId}", scriptId);
                return false;
            }
        }

        /// <summary>
        /// Unregisters all JavaScript scripts from a specific plugin.
        /// </summary>
        /// <param name="pluginId">The ID of the plugin whose scripts should be removed.</param>
        /// <returns>The number of scripts that were removed.</returns>
        public int UnregisterAllScriptsFromPlugin(string pluginId)
        {
            if (string.IsNullOrWhiteSpace(pluginId))
            {
                _logger.LogWarning("Cannot unregister scripts: pluginId is null or empty");
                return 0;
            }

            try
            {
                if (Plugin.Instance == null)
                {
                    _logger.LogError("Plugin instance is not available");
                    return 0;
                }

                var config = Plugin.Instance.Configuration;
                if (config == null)
                {
                    _logger.LogError("Plugin configuration is not available");
                    return 0;
                }

                var scriptsToRemove = config.PluginJavaScripts.Where(s => s.PluginId == pluginId).ToList();
                var removedCount = scriptsToRemove.Count;

                if (removedCount > 0)
                {
                    foreach (var script in scriptsToRemove)
                    {
                        config.PluginJavaScripts.Remove(script);
                    }

                    Plugin.Instance.SaveConfiguration();

                    _logger.LogInformation("Successfully unregistered {Count} scripts from plugin {PluginId}", removedCount, pluginId);
                }
                else
                {
                    _logger.LogDebug("No scripts found for plugin {PluginId}", pluginId);
                }

                return removedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister scripts from plugin {PluginId}", pluginId);
                return 0;
            }
        }

        public (bool IsValid, string ErrorMessage) ValidatePayload(JavaScriptRegistrationPayload payload)
        {
            if (payload == null)
            {
                return (false, "Payload cannot be null");
            }

            if (string.IsNullOrWhiteSpace(payload.Id))
            {
                return (false, "Script ID is required");
            }

            if (string.IsNullOrWhiteSpace(payload.Name))
            {
                return (false, "Script name is required");
            }

            if (string.IsNullOrWhiteSpace(payload.Script))
            {
                return (false, "Script content is required");
            }

            if (string.IsNullOrWhiteSpace(payload.PluginId))
            {
                return (false, "Plugin ID is required");
            }

            if (string.IsNullOrWhiteSpace(payload.PluginName))
            {
                return (false, "Plugin name is required");
            }

            // Additional validation rules
            if (payload.Id.Length > 100)
            {
                return (false, "Script ID cannot exceed 100 characters");
            }

            if (payload.Name.Length > 200)
            {
                return (false, "Script name cannot exceed 200 characters");
            }

            if (payload.Script.Length > MaxChars)
            {
                return (false, "Script content cannot exceed 1MB");
            }

            return (true, string.Empty);
        }

        private static void UpdateScriptEntry(ref PluginJavaScriptEntry existingScript, JavaScriptRegistrationPayload payload)
        {
            existingScript.Name = payload.Name;
            existingScript.Script = payload.Script;
            existingScript.Enabled = payload.Enabled;
            existingScript.RequiresAuthentication = payload.RequiresAuthentication;
            existingScript.PluginId = payload.PluginId;
            existingScript.PluginName = payload.PluginName;
            existingScript.PluginVersion = payload.PluginVersion ?? string.Empty;
        }

        private static PluginJavaScriptEntry CreateScriptEntry(JavaScriptRegistrationPayload payload)
        {
            return new PluginJavaScriptEntry
            {
                Id = payload.Id,
                Name = payload.Name,
                Script = payload.Script,
                Enabled = payload.Enabled,
                RequiresAuthentication = payload.RequiresAuthentication,
                PluginId = payload.PluginId,
                PluginName = payload.PluginName,
                PluginVersion = payload.PluginVersion ?? string.Empty
            };
        }
    }
}