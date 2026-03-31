# DirectImageSaver

DirectImageSaver is a Windows MVP that lets you hover an image in Chrome or Edge and save it immediately with a configurable trigger. The default trigger is `Shift + Right Click`.

## Architecture Overview

- `DirectImageSaver.App`: .NET 8 WPF tray application that owns settings, logging, sounds, save handling, and the named-pipe server.
- `DirectImageSaver.NativeHost`: .NET 8 console application used as the Native Messaging host. It bridges browser messages to the tray application through `DirectImageSaver.NativeBridge`.
- `DirectImageSaver.Core`: shared contracts and services for settings, logging, naming, downloading, sounds, and pipe/native message transport.
- `extension`: Manifest V3 Chrome/Edge extension with a content script for hovered `<img>` tracking and a service worker for Native Messaging.

## Repository Layout

```text
src/DirectImageSaver.App
src/DirectImageSaver.NativeHost
src/DirectImageSaver.Core
assets/icons
extension
scripts
samples
docs
tests
```

## Installation

1. Build and install the Windows components:

   ```powershell
   cd F:\DirectImageSaver
   .\scripts\install.ps1
   ```

2. Load the unpacked extension in Chrome or Edge:
   - Open `chrome://extensions` or `edge://extensions`
   - In Chrome, you can also open it from `︙` → `拡張機能` → `拡張機能を管理`
   - In Edge, you can also open it from `…` → `拡張機能` → `拡張機能の管理`
   - Enable Developer Mode
   - Click `Load unpacked`
   - Select `%LocalAppData%\DirectImageSaver\current\extension`
   - Confirm the extension name is `DirectImageSaver`
   - Confirm the extension ID is `kblklkfadcpplofmmfkkplglcmomicmm`
   - After Native Messaging registration changes, click `Reload` on the DirectImageSaver extension

3. Confirm that the tray app is running.
4. Hover an image and press `Shift + Right Click`.

## Quick Start From Codex

From Codex or PowerShell, the fastest way to launch the tray app for development is:

```powershell
cd F:\DirectImageSaver
.\start.ps1
```

Useful variants:

```powershell
.\start.ps1 -ShowSettings
.\start.ps1 -SkipBuild
```

There is also a simple wrapper command:

```cmd
start.cmd
```

## Chrome / Edge Extension Loading

- The extension uses a fixed `key`, so the unpacked extension ID stays stable.
- Expected extension ID: `kblklkfadcpplofmmfkkplglcmomicmm`
- Native Messaging host name: `com.directimagesaver.host`
- Use `%LocalAppData%\DirectImageSaver\current\extension` as the only supported unpacked extension source for regular use.

## Native Messaging Host Registration

The install script writes the host manifest to:

- `%LocalAppData%\DirectImageSaver\current\nativehost\com.directimagesaver.host.json`

It then registers that manifest under:

- `HKCU\Software\Google\Chrome\NativeMessagingHosts\com.directimagesaver.host`
- `HKCU\Software\Microsoft\Edge\NativeMessagingHosts\com.directimagesaver.host`

To re-register manually:

```powershell
.\scripts\register-native-host.ps1 -ManifestPath "$env:LOCALAPPDATA\DirectImageSaver\current\nativehost\com.directimagesaver.host.json"
```

## Configuration File

- Path: `%AppData%\DirectImageSaver\config.json`
- Sample: [`samples/config.sample.json`](samples/config.sample.json)

Default settings include:

- save folder: `%USERPROFILE%\Pictures\DirectImageSaver`
- trigger: `ShiftRightClick`
- success sound: on
- error sound: on
- auto start: on

## Changing The Save Folder

- Right-click the tray icon
- Open `Settings`
- Pick a new folder and save

The application writes the updated settings back to `%AppData%\DirectImageSaver\config.json`.

## Supported Scope

- Windows
- Chrome
- Edge
- X (Twitter) Web
- X (Twitter) PWA installed from Chrome/Edge
- DOM `<img>` elements
- Fixed-folder immediate save

## Non-Supported Scope

- CSS `background-image`
- `canvas`-rendered images
- `video` posters
- native Windows apps in general
- the standalone native X/Twitter app
- authenticated or anti-hotlink protected images that reject normal HTTP requests
- automatic image conversion, duplicate detection, hashing, classification, or AI sorting

## Known Limitations

- `Shift + Right Click` can conflict with site handlers or other extensions. Switch `triggerMode` in the config to `CtrlRightClick`, `AltRightClick`, or `CtrlShiftS` if needed.
- `CtrlRightClick`, `ShiftRightClick`, and `AltRightClick` are detected on `mousedown`, so after changing `triggerMode` or re-registering Native Messaging you should reload the extension and refresh the target tab.
- Some protected image URLs reject `HttpClient` requests even with `User-Agent`, `Referer`, and `Accept` headers.
- Trigger changes made in `config.json` while a page is already open may require reloading that page before the content script picks them up.
- The standalone native X/Twitter application is not supported in this MVP.
- PWA behavior depends on the browser continuing to allow extensions inside installed PWAs.

## Sounds

- Success: Windows standard `Asterisk`
- Failure: Windows standard `Hand`

## Logs

- Directory: `%AppData%\DirectImageSaver\logs\`
- Files:
  - `directimagesaver-app-YYYYMMDD.log`
  - `directimagesaver-nativehost-YYYYMMDD.log`
- Check `nativehost` first when the extension cannot talk to Windows, then `app` for actual save failures.
- If neither log file updates after you try to save, Chrome is most likely not running the DirectImageSaver extension in the profile you are testing.
- Spec: [`docs/log-spec.md`](docs/log-spec.md)

## Troubleshooting

If `Shift + Right Click` or `Ctrl + Right Click` only opens the normal browser context menu and no log file changes:

1. Open `chrome://extensions`.
   Chrome menu path: `︙` → `拡張機能` → `拡張機能を管理`
2. Confirm `DirectImageSaver` is listed.
3. Confirm its ID is `kblklkfadcpplofmmfkkplglcmomicmm`.
4. Confirm it was loaded from `%LocalAppData%\DirectImageSaver\current\extension`.
5. Click `Reload`.
6. Refresh the page you are testing.
7. Try again on a plain public `<img>` element.

Interpretation:

- `DirectImageSaver` is not listed: the extension is not installed in that Chrome profile yet.
- `directimagesaver-nativehost-*.log` does not update: the browser never reached Native Messaging.
- `directimagesaver-nativehost-*.log` updates but `directimagesaver-app-*.log` does not: the request reached the native host but not the tray app.
- `directimagesaver-app-*.log` updates: the save pipeline is running and the remaining issue is in download or file save handling.

## Setup Scripts

- `scripts/generate-icons.ps1`: regenerate the master icon deliverables (`SVG`, `ICO`, extension `PNG`s, and preview sheet)
- `scripts/publish.ps1`: publish WPF app, native host, and copy the extension into `artifacts/publish`
- `scripts/install.ps1`: publish, copy files into `%LocalAppData%\DirectImageSaver\current`, register Native Messaging, configure startup, and launch the tray app
- `scripts/uninstall.ps1`: remove Native Messaging registration, startup registration, and installed binaries

## Testing / Validation

### Automated

Run the unit tests:

```powershell
dotnet test .\DirectImageSaver.sln
```

### Manual verification checklist

0. After reinstalling or re-registering Native Messaging, reload the DirectImageSaver extension and refresh the target tab.
1. Save a `jpg`, `png`, `gif`, `webp`, and `avif`.
2. Save an image selected from `srcset`.
3. Save multiple images within the same second and confirm the `_01`, `_02`, `_03` sequence.
4. Save images on X Web in Chrome.
5. Save images on X Web in Edge.
6. Save images in the Chrome/Edge-installed X PWA.
7. Confirm failure logging when:
   - the URL is broken
   - the save folder is missing
   - Native Messaging is not registered
   - `Shift + Right Click` is blocked by the page
   - the `nativehost` log captures connection failures before the app log is reached
8. Confirm `Ctrl + Right Click` on Chrome suppresses the browser context menu and triggers save when `triggerMode` is `CtrlRightClick`.

## Future Extension Candidates

- optional toast notifications
- richer UI for trigger editing
- retry and better error hints for hotlink failures
- support for additional DOM source types beyond `<img>`
