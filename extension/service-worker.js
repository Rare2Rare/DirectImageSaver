const HOST_NAME = "com.directimagesaver.host";
const DEFAULT_TRIGGER_MODE = "ShiftRightClick";
const CONFIG_CACHE_TTL_MS = 30_000;

let cachedConfig = { triggerMode: DEFAULT_TRIGGER_MODE };
let cachedConfigExpiresAt = 0;

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  void handleMessage(message).then(sendResponse);
  return true;
});

async function handleMessage(message) {
  if (!message || typeof message.type !== "string") {
    return { ok: false, errorCode: "InvalidMessage", message: "Message type is missing." };
  }

  if (message.type === "getTriggerMode") {
    const config = await getConfig();
    return {
      ok: true,
      triggerMode: config.triggerMode || DEFAULT_TRIGGER_MODE
    };
  }

  if (message.type === "saveHoveredImage") {
    const response = await sendNativeMessage({
      type: "saveImage",
      payload: message.payload
    });

    if (!response || !response.ok) {
      console.warn("DirectImageSaver: saveImage failed", response);
    }

    return response;
  }

  return {
    ok: false,
    errorCode: "UnsupportedMessage",
    message: `Unsupported extension message: ${message.type}`
  };
}

async function getConfig() {
  if (cachedConfig && cachedConfigExpiresAt > Date.now()) {
    return cachedConfig;
  }

  const response = await sendNativeMessage({ type: "getConfig" });
  if (response && response.ok && response.config) {
    cachedConfig = response.config;
    cachedConfigExpiresAt = Date.now() + CONFIG_CACHE_TTL_MS;
    return cachedConfig;
  }

  console.warn("DirectImageSaver: unable to fetch config from native host", response);
  return cachedConfig;
}

function sendNativeMessage(request) {
  return new Promise((resolve) => {
    chrome.runtime.sendNativeMessage(HOST_NAME, request, (response) => {
      const runtimeError = chrome.runtime.lastError;
      if (runtimeError) {
        console.warn("DirectImageSaver: native host unavailable", runtimeError.message);
        resolve({
          ok: false,
          errorCode: "NativeHostUnavailable",
          message: runtimeError.message
        });
        return;
      }

      resolve(response || {
        ok: false,
        errorCode: "EmptyNativeResponse",
        message: "The native host returned no response."
      });
    });
  });
}
