(() => {
  const DEFAULT_TRIGGER_MODE = "ShiftRightClick";
  const CONFIG_REFRESH_TTL_MS = 30_000;
  const CONTEXT_MENU_SUPPRESSION_MS = 1_500;

  let hoveredImage = null;
  let triggerMode = DEFAULT_TRIGGER_MODE;
  let lastKnownGoodTriggerMode = DEFAULT_TRIGGER_MODE;
  let lastTriggerModeRefreshAt = 0;
  let triggerModeRefreshPromise = null;
  let suppressContextMenuUntil = 0;

  void refreshTriggerMode();

  document.addEventListener("mousemove", onMouseMove, { capture: true, passive: true });
  document.addEventListener("mousedown", onMouseDown, true);
  document.addEventListener("contextmenu", onContextMenu, true);
  document.addEventListener("keydown", onKeyDown, true);
  document.addEventListener("visibilitychange", () => {
    if (!document.hidden) {
      void refreshTriggerMode();
    }
  });

  function onMouseMove(event) {
    const candidate = resolveImageElement(event.target);
    if (candidate !== hoveredImage) {
      hoveredImage = candidate;
    }
  }

  function onMouseDown(event) {
    if (event.button !== 2) {
      return;
    }

    void ensureTriggerModeFresh();

    const image = resolveImageElement(event.target) || hoveredImage;
    if (!image) {
      return;
    }

    if (!matchesMouseDownTrigger(event, getActiveTriggerMode())) {
      return;
    }

    suppressContextMenuUntil = Date.now() + CONTEXT_MENU_SUPPRESSION_MS;
    event.preventDefault();
    event.stopPropagation();
    void triggerSave(image);
  }

  function onContextMenu(event) {
    if (Date.now() > suppressContextMenuUntil) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
  }

  function onKeyDown(event) {
    void ensureTriggerModeFresh();

    if (getActiveTriggerMode() !== "CtrlShiftS" || !hoveredImage || event.repeat) {
      return;
    }

    if (event.ctrlKey && event.shiftKey && event.key.toLowerCase() === "s") {
      event.preventDefault();
      event.stopPropagation();
      void triggerSave(hoveredImage);
    }
  }

  function matchesMouseDownTrigger(event, activeTriggerMode) {
    if (activeTriggerMode === "ShiftRightClick") {
      return event.button === 2 && event.shiftKey;
    }

    if (activeTriggerMode === "CtrlRightClick") {
      return event.button === 2 && event.ctrlKey;
    }

    if (activeTriggerMode === "AltRightClick") {
      return event.button === 2 && event.altKey;
    }

    return false;
  }

  function getActiveTriggerMode() {
    return triggerMode || lastKnownGoodTriggerMode || DEFAULT_TRIGGER_MODE;
  }

  function ensureTriggerModeFresh() {
    if (lastTriggerModeRefreshAt + CONFIG_REFRESH_TTL_MS > Date.now()) {
      return triggerModeRefreshPromise;
    }

    return refreshTriggerMode();
  }

  async function refreshTriggerMode() {
    if (triggerModeRefreshPromise) {
      return triggerModeRefreshPromise;
    }

    triggerModeRefreshPromise = refreshTriggerModeInternal();
    try {
      await triggerModeRefreshPromise;
    } finally {
      triggerModeRefreshPromise = null;
    }
  }

  async function refreshTriggerModeInternal() {
    try {
      const response = await sendRuntimeMessage({ type: "getTriggerMode" });
      if (response && response.triggerMode) {
        triggerMode = response.triggerMode;
        lastKnownGoodTriggerMode = response.triggerMode;
        lastTriggerModeRefreshAt = Date.now();
      }
    } catch (error) {
      console.warn("DirectImageSaver: failed to fetch trigger mode", error);
      triggerMode = lastKnownGoodTriggerMode;
    }
  }

  async function triggerSave(image) {
    const payload = buildPayload(image);
    if (!payload) {
      console.warn("DirectImageSaver: no image payload could be built.");
      return;
    }

    try {
      const response = await sendRuntimeMessage({
        type: "saveHoveredImage",
        payload
      });

      if (!response || !response.ok) {
        console.warn("DirectImageSaver: save failed", response);
      }
    } catch (error) {
      console.warn("DirectImageSaver: failed to send save request", error);
    }
  }

  function buildPayload(image) {
    const imageUrl = resolveImageUrl(image);
    if (!imageUrl) {
      return null;
    }

    return {
      imageUrl,
      pageUrl: window.location.href,
      pageTitle: document.title || "",
      host: window.location.hostname || "",
      alt: image.alt || "",
      naturalWidth: image.naturalWidth || undefined,
      naturalHeight: image.naturalHeight || undefined,
      userAgent: navigator.userAgent,
      referrer: window.location.href || document.referrer || undefined,
      timestamp: new Date().toISOString()
    };
  }

  function resolveImageUrl(image) {
    if (typeof image.currentSrc === "string" && image.currentSrc.trim()) {
      return image.currentSrc.trim();
    }

    if (typeof image.src === "string" && image.src.trim()) {
      return image.src.trim();
    }

    const srcset = image.getAttribute("srcset");
    if (!srcset) {
      return null;
    }

    const candidate = srcset
      .split(",")
      .map((entry) => entry.trim().split(/\s+/)[0])
      .find(Boolean);

    return candidate || null;
  }

  function resolveImageElement(target) {
    if (!(target instanceof Element)) {
      return null;
    }

    const match = target.closest("img");
    return match instanceof HTMLImageElement ? match : null;
  }

  function sendRuntimeMessage(message) {
    return new Promise((resolve, reject) => {
      chrome.runtime.sendMessage(message, (response) => {
        const runtimeError = chrome.runtime.lastError;
        if (runtimeError) {
          reject(new Error(runtimeError.message));
          return;
        }

        resolve(response);
      });
    });
  }
})();
