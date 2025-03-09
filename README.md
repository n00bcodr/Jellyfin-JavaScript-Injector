# Jellyfin Custom JavaScript Plugin

This plugin allows you to inject custom JavaScript code into the Jellyfin web UI. It provides a configuration page with a text area where you can enter any JavaScript code, which will then be executed when the Jellyfin web interface loads.

## Install Process

### IMPORTANT: If your Jellyfin server runs on a Docker container without root, then you'll need to perform the following prerequisites

The client script will fail to inject automatically into the jellyfin-web server if there is a difference in permission between the owner of the web files (root, or www-data, etc.) and the executor of the main jellyfin-server. You will see an error in your Jellyfin server logs in this case, like

```bash
[2025-03-09 19:30:41.162 +00:00] [ERR] [1] Jellyfin.Plugin.CustomJavaScript.Plugin: Encountered exception while writing to "/usr/share/jellyfin/web/index.html": "System.UnauthorizedAccessException: Access to the path '/usr/share/jellyfin/web/index.html' is denied.
```

If this happens, you can mitigate the issue by

1. Run `docker cp jellyfin_server_1:/usr/share/jellyfin/web/index.html /your/jellyfin/config/path/index.html`
2. Update your docker-compose file with a `volume` mapping, like `- /your/jellyfin/config/path/index.html:/usr/share/jellyfin/web/index.html`

This way, the plugin will have appropriate permissions to inject javascript into the index.html file.

### Plugin Installation Process

1. In Jellyfin, go to `Dashboard -> Plugins -> Catalog -> Gear Icon (upper left)` add and a repository.
1. Set the Repository name to @johnpc (Custom Javascript)
1. Set the Repository URL to https://raw.githubusercontent.com/johnpc/jellyfin-plugin-custom-javascript/refs/heads/main/manifest.json
1. Click "Save"
1. Go to Catalog and search for Custom Javascript
1. Click on it and install
1. Restart Jellyfin

## Configuration

1. After installing the plugin, go to Dashboard → Plugins
2. Find "Custom JavaScript" in the list and click on it
3. In the text area, enter the JavaScript code you want to inject
4. Click Save
5. Refresh your browser to apply the changes

## Usage Examples

Here are some examples of what you can do with custom JavaScript:

### Hello World

```javascript
console.log("hello from the plugin");
```

### Modify the CSS Based On Username

```javascript
const userNameToShowCustomCss = 'guest'

const customCSS = document.createElement('style');
customCSS.textContent = `
.skinHeader::after {
    content: "NOTICE: This banner CSS was created via custom javascript! You'll only see this when logged in as ${userNameToShowCustomCss}$";
    display: block;
    position: relative;
    background-color: #fbc531;
    color: #192a56;
    left: 50%;
    transform: translateX(-50%);
    width: fit-content;
    text-align: center;
    width: 100%;
    padding: 10px;
}
.homeSectionsContainer {
  margin-top: 50px;
}
`;
const userButton = document.querySelector(".headerUserButton");
if (userButton.title.toLowerCase() === userNameToShowCustomCss) {
    document.head.appendChild(customCSS);
}
```

## Development

### Building from Source

1. Clone the repository:
   ```
   git clone https://github.com/johnpc/jellyfin-plugin-custom-javascript.git
   ```

2. Build the plugin:
   ```
   dotnet build
   ```

3. The compiled dll will be in the `bin/Debug/net6.0` directory

### Project Structure

- `Plugin.cs` - The main plugin class
- `Configuration/PluginConfiguration.cs` - Defines the configuration model
- `Configuration/configPage.html` - The HTML for the configuration page
- `Web/customjavascript.js` - The JavaScript file that injects custom code into Jellyfin

## License

This plugin is licensed under the MIT License.

## Security Note

Be careful when using custom JavaScript as it can potentially introduce security vulnerabilities. Only use code from trusted sources or that you fully understand. The plugin author is not responsible for any issues caused by custom code entered by users.