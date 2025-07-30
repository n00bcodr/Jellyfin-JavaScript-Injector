# Jellyfin Plugin - JavaScript Injector

The JavaScript Injector plugin for Jellyfin allows you to inject multiple, independent JavaScript snippets into the Jellyfin web UI. It provides a powerful and easy-to-use configuration page to manage all your custom scripts from one place.

<p align="center">
  <img src="https://img.shields.io/badge/Jellyfin%20Version-10.10.x-AA5CC3?logo=jellyfin&logoColor=00A4DC&labelColor=black" alt="Jellyfin Version">
  <br/><br/>
    <img alt="Logo" src="icon.png" width="80%"  />
<br>
</p>

## ✨ Features
--------

-   **Multiple Scripts**: Add as many custom JavaScript snippets as you want.

-   **Organized UI**: Each script is managed in its own collapsible section, keeping your configuration clean and easy to navigate.

-   **Enable/Disable on the Fly**: Toggle individual scripts on or off without having to delete the code.

-   **Immediate Injection**: The plugin injects a loader script into the Jellyfin web UI upon server startup. Your custom scripts are loaded dynamically, and changes take effect after a simple browser refresh.



## ⚙️ Installation

------------

1.  In Jellyfin, go to **Dashboard** > **Plugins** > **Catalog** > ⚙️
2.  Click **➕** and give the repository a name (e.g., "JavaScript Injector Repo").
3.  Set the **Repository URL** to: `https://raw.githubusercontent.com/n00bcodr/jellyfin-javascript-injector/main/manifest.json`
4.  Click **Save**.
5.  Go to the **Catalog** tab, find **JavaScript Injector** in the list, and click **Install**.
6.  **Restart** your Jellyfin server to complete the installation.


## 🔧 Configuration
-------------

1.  After installing, navigate to **Dashboard** > **Plugins** > **JavaScript Injector** in the list **--OR--** click on "JS Injector" in the dashboard sidebar

2.  Click **Add Script** to create a new entry.
3.  Give your script a descriptive **name**.
4.  Enter your code in the **JavaScript Code** text area.
5.  Use the **Enabled** checkbox to control whether the script is active.
6.  Click **Save**.
7.  **Refresh your browser** to see the changes take effect.


## ⌨️ Usage Examples
--------------

### Example 1: Simple Browser Alert Message

A great way to test if the plugin is working.

```js
(function() {
    'use strict';

    const toast= `
        alert('Yay!, Javascript injection worked!');
    `;

    const scriptElem = document.createElement('script');
    scriptElem.textContent = toast;
    document.head.appendChild(scriptElem);
})();


```

### Example 2: Add a Custom Banner

This script adds a banner to the top of the page for a specific user.

```js
// Change this to the username you want to target
(function () {
    const targetUsername = 'admin';

    const flashingBannerCSS = `
    @keyframes flashBanner {
        0% { background-color: #ffeb3b; color: black; }
        50% { background-color: #ff2111; color: white; }
        100% { background-color: #ffeb3b; color: black; }
    }
    .skinHeader::before {
        content: "⚠️ NOTICE: Special Banner for ${targetUsername} ⚠️";
        display: block;
        width: 100%;
        text-align: center;
        font-weight: bold;
        font-size: 1.2rem;
        padding: 0px;
        animation: flashBanner 1s infinite;
        position: relative;
        z-index: 9999;
    }
    `;

    function tryInjectBanner() {
        const userButton = document.querySelector(".headerUserButton");
        if (userButton && userButton.title.toLowerCase() === targetUsername.toLowerCase()) {
            const styleElem = document.createElement('style');
            styleElem.innerText = flashingBannerCSS;
            document.head.appendChild(styleElem);
            return true;
        }
        return false;
    }
    const interval = setInterval(() => {
        if (tryInjectBanner()) clearInterval(interval);
    }, 300);
})();

```

## 🙏🏻Credits
This plugin is a fork of and builds upon the original work of [johnpc](https://github.com/johnpc/jellyfin-plugin-custom-javascript). Thanks to the original author for creating the foundation for this project.

## 🗒️ Note
-------------

Be careful when using any custom JavaScript, as it can potentially introduce security vulnerabilities or break the Jellyfin UI. Only use code from trusted sources or code that you have written and fully understand.