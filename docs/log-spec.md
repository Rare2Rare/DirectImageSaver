# Log Specification

Logs are written to `%AppData%\DirectImageSaver\logs\`.

Files:

- `directimagesaver-app-YYYYMMDD.log`
- `directimagesaver-nativehost-YYYYMMDD.log`

Use `directimagesaver-nativehost-*.log` first when the browser extension appears disconnected. Use `directimagesaver-app-*.log` when the request reached the tray application and the save itself failed.

Each entry includes:

- `timestamp`
- `action`
- `requestType`
- `pageUrl`
- `imageUrl`
- `savePath`
- `contentType`
- `result`
- `errorMessage`
- `stackTrace` when an exception is present

App log behavior:

- save requests write an entry as soon as they reach the save handler
- successful saves use `result=Success`
- failure entries use `result=<SaveErrorCode>`

Native host log behavior:

- writes an entry when a native request is received
- records pipe connection failures, tray auto-launch attempts, and final response status

Interpretation:

- no new `nativehost` log entry after a save attempt usually means the browser extension was not installed, not reloaded, or not running in the Chrome profile being tested
- new `nativehost` entries without new `app` entries usually mean the native host could not reach the tray application
