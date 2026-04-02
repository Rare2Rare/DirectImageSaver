const HOST_NAME = "com.directimagesaver.host";
const DEFAULT_TRIGGER_MODE = "ShiftRightClick";
const CONFIG_CACHE_TTL_MS = 30_000;

let cachedConfig = {
  triggerMode: DEFAULT_TRIGGER_MODE,
  enableVideoSave: true
};
let cachedConfigExpiresAt = 0;

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  void handleMessage(message).then(sendResponse);
  return true;
});

async function handleMessage(message) {
  if (!message || typeof message.type !== "string") {
    return { ok: false, errorCode: "InvalidMessage", message: "Message type is missing." };
  }

  // Keep getTriggerMode for older tabs until every active content script is refreshed.
  if (message.type === "getRuntimeConfig" || message.type === "getTriggerMode") {
    const config = await getConfig();
    return {
      ok: true,
      triggerMode: config.triggerMode || DEFAULT_TRIGGER_MODE,
      enableVideoSave: config.enableVideoSave !== false
    };
  }

  // Keep saveHoveredImage for older tabs until every active content script is refreshed.
  if (message.type === "saveHoveredMedia" || message.type === "saveHoveredImage") {
    const response = await sendNativeMessage({
      type: "saveMedia",
      payload: normalizeSavePayload(message.payload, message.type)
    });

    if (!response || !response.ok) {
      console.warn(`DirectImageSaver: saveMedia failed. ${formatNativeResponse(response)}`);
    }

    return response;
  }

  return {
    ok: false,
    errorCode: "UnsupportedMessage",
    message: `Unsupported extension message: ${message.type}`
  };
}

function normalizeSavePayload(payload, messageType) {
  const source = payload && typeof payload === "object" ? payload : {};
  const mediaType = source.mediaType || (messageType === "saveHoveredImage" ? "Image" : undefined);

  return {
    ...source,
    ...(mediaType ? { mediaType } : {})
  };
}

async function getConfig() {
  if (cachedConfig && cachedConfigExpiresAt > Date.now()) {
    return cachedConfig;
  }

  const response = await sendNativeMessage({ type: "getConfig" });
  if (response && response.ok && response.config) {
    cachedConfig = {
      ...cachedConfig,
      ...response.config
    };
    cachedConfigExpiresAt = Date.now() + CONFIG_CACHE_TTL_MS;
    return cachedConfig;
  }

  console.warn(`DirectImageSaver: unable to fetch config from native host. ${formatNativeResponse(response)}`);
  return cachedConfig;
}

function sendNativeMessage(request) {
  return new Promise((resolve) => {
    chrome.runtime.sendNativeMessage(HOST_NAME, request, (response) => {
      const runtimeError = chrome.runtime.lastError;
      if (runtimeError) {
        console.warn(`DirectImageSaver: native host unavailable. ${runtimeError.message}`);
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

function formatNativeResponse(response) {
  if (!response || typeof response !== "object") {
    return "No response details were returned.";
  }

  const errorCode = typeof response.errorCode === "string" && response.errorCode
    ? response.errorCode
    : response.ok
      ? "Success"
      : "UnknownError";
  const message = typeof response.message === "string" && response.message
    ? response.message
    : response.ok
      ? "Request completed."
      : "No error message was returned.";

  return `${errorCode}: ${message}`;
}
