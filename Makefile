export VERSION := 0.0.0.1
export GITHUB_REPO := johnpc/jellyfin-plugin-custom-javascript
export FILE := custom-javascript-${VERSION}.zip

build:
	dotnet build

zip:
	zip "${FILE}" Jellyfin.Plugin.CustomJavaScript/bin/Debug/net6.0/Jellyfin.Plugin.CustomJavascript.dll

csum:
	md5sum "${FILE} ""

create-tag:
	git tag ${VERSION}
	git push origin ${VERSION}

create-gh-release:
	gh release create ${VERSION} "${FILE}" --generate-notes --verify-tag

update-version:
	node scripts/update-version.js

update-manifest:
	node scripts/validate-and-update-manifest.js

push-manifest:
	git commit -m 'new release' manifest.json
	git push origin main

release: update-version build zip create-tag create-gh-release update-manifest push-manifest