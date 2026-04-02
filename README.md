# DirectImageSaver

DirectImageSaver is a Windows tray app plus a Chrome/Edge extension for immediate save of hovered media. This release supports DOM images and direct-link videos exposed by `<video>` or `<source>` with normal `http/https` URLs.

## Build And Distribution

Create the distributable artifacts:

```powershell
cd F:\DirectImageSaver
.\scripts\publish.ps1
```

Outputs:

- `artifacts\publish\` as the unpacked distribution root
- `artifacts\DirectImageSaver.zip` as the ZIP distribution

The ZIP and unpacked distribution include only:

- `app`
- `nativehost`
- `extension`
- `scripts\install.ps1`
- `scripts\uninstall.ps1`
- `scripts\register-native-host.ps1`
- `README.md`

## Installation

### From repository

```powershell
cd F:\DirectImageSaver
.\scripts\install.ps1
```

### From distribution ZIP

1. Extract `DirectImageSaver.zip`
2. Open PowerShell in the extracted folder
3. Run:

   ```powershell
   .\scripts\install.ps1
   ```

The install script supports both repository mode and extracted distribution mode.

## Chrome / Edge Extension Loading

1. Open `chrome://extensions` or `edge://extensions`
2. In Chrome from the menu: `3-dot menu > Extensions > Manage Extensions`
3. In Edge from the menu: `3-dot menu > Extensions > Manage Extensions`
4. Enable Developer Mode
5. Click `Load unpacked`
6. Select `%LocalAppData%\DirectImageSaver\current\extension`
7. Confirm the extension name is `DirectImageSaver`
8. Confirm the extension ID is `kblklkfadcpplofmmfkkplglcmomicmm`
9. After reinstalling or re-registering Native Messaging, click `Reload`
10. Refresh the target tab before testing save triggers

## Native Messaging Host Registration

The install script writes the host manifest to:

- `%LocalAppData%\DirectImageSaver\current\nativehost\com.directimagesaver.host.json`

It registers that manifest under:

- `HKCU\Software\Google\Chrome\NativeMessagingHosts\com.directimagesaver.host`
- `HKCU\Software\Microsoft\Edge\NativeMessagingHosts\com.directimagesaver.host`

Manual re-registration:

```powershell
.\scripts\register-native-host.ps1 -ManifestPath "$env:LOCALAPPDATA\DirectImageSaver\current\nativehost\com.directimagesaver.host.json"
```

## Configuration File

- Path: `%AppData%\DirectImageSaver\config.json`

Default settings:

- save folder: `%USERPROFILE%\Pictures\DirectImageSaver`
- trigger: `ShiftRightClick`
- success sound: on
- error sound: on
- direct-link video save: on
- auto start: on

## Changing The Save Folder

- Right-click the tray icon
- Open `Settings`
- Select a new folder
- Save

## Supported Scope

- Windows
- Chrome
- Edge
- X (Twitter) Web
- X (Twitter) PWA installed from Chrome/Edge
- DOM `<img>` elements
- DOM `<video>` / `<source>` elements with normal `http/https` media URLs
- Fixed-folder immediate save

## Non-Supported Scope

- CSS `background-image`
- `canvas` rendered images
- `video` posters
- `blob:` or MediaSource-backed playback
- HLS / MSE playback and `m3u8` streams
- native Windows apps in general
- the standalone native X/Twitter app
- authenticated or anti-hotlink protected images or videos that reject normal HTTP requests
- automatic conversion, duplicate detection, hashing, classification, or AI sorting

## Known Limitations

- `Shift + Right Click` can conflict with site handlers or other extensions. Switch `triggerMode` to `CtrlRightClick`, `AltRightClick`, or `CtrlShiftS` if needed.
- `CtrlRightClick`, `ShiftRightClick`, and `AltRightClick` are detected on `mousedown`, so trigger changes may require extension reload plus page reload.
- Direct-link video saving supports only `http/https` URLs exposed by `<video>` / `<source>`.
- `blob:` URLs, MediaSource playback, HLS, and MSE are out of scope in this release.
- Some protected image or video URLs reject `HttpClient` even with `User-Agent`, `Referer`, and `Accept` headers.
- The standalone native X/Twitter application is not supported.
- PWA behavior depends on the browser continuing to allow extensions inside installed PWAs.

## Sounds

- Success: Windows standard `Asterisk`
- Failure: Windows standard `Hand`

## Logs

- Directory: `%AppData%\DirectImageSaver\logs\`
- Files:
  - `directimagesaver-app-YYYYMMDD.log`
  - `directimagesaver-nativehost-YYYYMMDD.log`

Check `nativehost` first when the extension cannot talk to Windows. Check `app` when the request reached the tray app and the save itself failed.

## Troubleshooting

If the normal browser context menu opens and no save happens:

1. Open `chrome://extensions`
2. Confirm `DirectImageSaver` is listed
3. Confirm the ID is `kblklkfadcpplofmmfkkplglcmomicmm`
4. Confirm it was loaded from `%LocalAppData%\DirectImageSaver\current\extension`
5. Click `Reload`
6. Refresh the page
7. Try again on a plain public `<img>` or direct-link `<video>`

Interpretation:

- `DirectImageSaver` is missing: the extension is not installed in that profile
- no new `directimagesaver-nativehost-*.log`: the browser never reached Native Messaging
- new `nativehost` log but no new `app` log: the native host could not reach the tray app
- new `app` log: the save pipeline is running, so the remaining issue is download or file save handling

## Setup Scripts

- `scripts/publish.ps1`: publish binaries, collect distribution files, and create `artifacts\DirectImageSaver.zip`
- `scripts/install.ps1`: install from either repository output or extracted distribution package
- `scripts/uninstall.ps1`: remove Native Messaging registration, startup registration, and installed binaries

## Testing

### Automated

```powershell
dotnet test .\DirectImageSaver.sln
```

### Manual checklist

1. Save `jpg`, `png`, `gif`, `webp`, and `avif`
2. Save an image selected from `srcset`
3. Save a `<video src="...mp4">`
4. Save a `<video><source src="...webm"></video>`
5. Save multiple files in the same second and confirm `_01`, `_02`, `_03`
6. Save images on X Web in Chrome
7. Save images on X Web in Edge
8. Save images in the Chrome/Edge-installed X PWA
9. Confirm failure logging when:
   - the URL is broken
   - the save folder is missing
   - Native Messaging is not registered
   - `Shift + Right Click` is blocked by the page
   - the video URL is `blob:` or otherwise unsupported
10. Confirm `Ctrl + Right Click` suppresses the browser context menu and triggers save when `triggerMode` is `CtrlRightClick`
