// Custom JavaScript injector for Jellyfin

// Get the current plugin instance
const CustomJavaScriptPlugin = {
    // Method to inject the custom JavaScript into the page
    injectCustomJavaScript: function () {
        // Get the configuration from the API
        ApiClient.getPluginConfiguration('38f5aa9b-9c3a-4db0-b4c4-a2d6823e7da7').then(function (config) {
            if (config && config.CustomJavaScript && config.CustomJavaScript.trim() !== '') {
                try {
                    // Create a script element
                    const scriptElement = document.createElement('script');
                    scriptElement.type = 'text/javascript';
                    
                    // Add the custom JavaScript content
                    scriptElement.textContent = config.CustomJavaScript;
                    
                    // Append the script element to the document head
                    document.head.appendChild(scriptElement);
                    
                    console.log('Custom JavaScript has been injected');
                } catch (error) {
                    console.error('Error injecting custom JavaScript:', error);
                }
            } else {
                console.log('No custom JavaScript provided in the configuration');
            }
        }).catch(function (error) {
            console.error('Failed to load Custom JavaScript plugin configuration:', error);
        });
    }
};

// Initialize when the document is loaded
document.addEventListener('DOMContentLoaded', function () {
    CustomJavaScriptPlugin.injectCustomJavaScript();
});

// Also inject when dashboard is loaded (for single page app navigation)
Events.on(Emby.Page, 'pageshow', function () {
    CustomJavaScriptPlugin.injectCustomJavaScript();
});