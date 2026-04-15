(() => {
  const DEFAULT_TRIGGER_MODE = "ShiftRightClick";
  const CONFIG_REFRESH_TTL_MS = 30_000;
  const CONTEXT_MENU_SUPPRESSION_MS = 1_500;
  const SAVE_POPUP_DURATION_MS = 1_800;
  const SAVE_POPUP_CONTAINER_ID = "direct-image-saver-popup";

  let hoveredMedia = null;
  let triggerMode = DEFAULT_TRIGGER_MODE;
  let enableVideoSave = true;
  let lastKnownGoodConfig = {
    triggerMode: DEFAULT_TRIGGER_MODE,
    enableVideoSave: true
  };
  let lastConfigRefreshAt = 0;
  let configRefreshPromise = null;
  let suppressContextMenuUntil = 0;
  let popupHideTimeoutId = 0;

  void refreshRuntimeConfig();

  document.addEventListener("mousemove", onMouseMove, { capture: true, passive: true });
  document.addEventListener("mousedown", onMouseDown, true);
  document.addEventListener("contextmenu", onContextMenu, true);
  document.addEventListener("keydown", onKeyDown, true);
  document.addEventListener("visibilitychange", () => {
    if (!document.hidden) {
      void refreshRuntimeConfig();
    }
  });

  function onMouseMove(event) {
    const candidate = resolveHoveredMedia(event.target);
    if (!areSameHoveredMedia(candidate, hoveredMedia)) {
      hoveredMedia = candidate;
    }
  }

  function onMouseDown(event) {
    if (event.button !== 2) {
      return;
    }

    void ensureRuntimeConfigFresh();

    const targetMedia = resolveHoveredMedia(event.target) || hoveredMedia;
    if (!targetMedia || !isAllowedMediaType(targetMedia.type)) {
      return;
    }

    if (!matchesMouseDownTrigger(event, getActiveTriggerMode())) {
      return;
    }

    suppressContextMenuUntil = Date.now() + CONTEXT_MENU_SUPPRESSION_MS;
    event.preventDefault();
    event.stopPropagation();
    void triggerSave(targetMedia);
  }

  function onContextMenu(event) {
    if (Date.now() > suppressContextMenuUntil) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
  }

  function onKeyDown(event) {
    void ensureRuntimeConfigFresh();

    if (getActiveTriggerMode() !== "CtrlShiftS" || !hoveredMedia || event.repeat) {
      return;
    }

    if (!isAllowedMediaType(hoveredMedia.type)) {
      return;
    }

    if (event.ctrlKey && event.shiftKey && event.key.toLowerCase() === "s") {
      event.preventDefault();
      event.stopPropagation();
      void triggerSave(hoveredMedia);
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
    return triggerMode || lastKnownGoodConfig.triggerMode || DEFAULT_TRIGGER_MODE;
  }

  function isAllowedMediaType(type) {
    return type === "image" || (type === "video" && enableVideoSave);
  }

  function ensureRuntimeConfigFresh() {
    if (lastConfigRefreshAt + CONFIG_REFRESH_TTL_MS > Date.now()) {
      return configRefreshPromise;
    }

    return refreshRuntimeConfig();
  }

  async function refreshRuntimeConfig() {
    if (configRefreshPromise) {
      return configRefreshPromise;
    }

    configRefreshPromise = refreshRuntimeConfigInternal();
    try {
      await configRefreshPromise;
    } finally {
      configRefreshPromise = null;
    }
  }

  async function refreshRuntimeConfigInternal() {
    try {
      const response = await sendRuntimeMessage({ type: "getRuntimeConfig" });
      if (response && response.ok) {
        triggerMode = response.triggerMode || DEFAULT_TRIGGER_MODE;
        enableVideoSave = response.enableVideoSave !== false;
        lastKnownGoodConfig = {
          triggerMode,
          enableVideoSave
        };
        lastConfigRefreshAt = Date.now();
      }
    } catch (error) {
      console.warn(`DirectImageSaver: failed to fetch runtime config. ${formatError(error)}`);
      triggerMode = lastKnownGoodConfig.triggerMode;
      enableVideoSave = lastKnownGoodConfig.enableVideoSave;
    }
  }

  async function triggerSave(hoveredEntry) {
    const payload = buildPayload(hoveredEntry);
    if (!payload) {
      console.info(`DirectImageSaver: skipped unsupported hovered target. ${describeHoveredEntry(hoveredEntry)}`);
      return;
    }

    try {
      const response = await sendRuntimeMessage({
        type: "saveHoveredMedia",
        payload
      });

      if (response && response.ok) {
        showSavePopup(response);
      } else {
        console.warn(`DirectImageSaver: save failed. ${formatNativeResponse(response)}`);
      }
    } catch (error) {
      console.warn(`DirectImageSaver: failed to send save request. ${formatError(error)}`);
    }
  }

  function buildPayload(hoveredEntry) {
    if (!hoveredEntry || !hoveredEntry.element) {
      return null;
    }

    if (hoveredEntry.type === "image") {
      return buildImagePayload(hoveredEntry.element);
    }

    if (hoveredEntry.type === "video") {
      return buildVideoPayload(hoveredEntry.element);
    }

    return null;
  }

  function buildImagePayload(image) {
    const mediaUrl = resolveImageUrl(image);
    if (!mediaUrl) {
      return null;
    }

    return {
      mediaType: "Image",
      mediaUrl,
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

  function buildVideoPayload(video) {
    const mediaUrl = resolveVideoUrl(video);
    if (!mediaUrl || mediaUrl.startsWith("blob:")) {
      return null;
    }

    const duration = Number.isFinite(video.duration) && video.duration > 0 ? video.duration : undefined;

    return {
      mediaType: "Video",
      mediaUrl,
      pageUrl: window.location.href,
      pageTitle: document.title || "",
      host: window.location.hostname || "",
      userAgent: navigator.userAgent,
      referrer: window.location.href || document.referrer || undefined,
      timestamp: new Date().toISOString(),
      durationSeconds: duration,
      videoWidth: video.videoWidth || undefined,
      videoHeight: video.videoHeight || undefined
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

  function resolveVideoUrl(video) {
    if (typeof video.currentSrc === "string" && video.currentSrc.trim()) {
      return video.currentSrc.trim();
    }

    if (typeof video.src === "string" && video.src.trim()) {
      return video.src.trim();
    }

    const source = Array.from(video.querySelectorAll("source"))
      .map((element) => element.getAttribute("src"))
      .find((value) => typeof value === "string" && value.trim());

    return source ? source.trim() : null;
  }

  function resolveHoveredMedia(target) {
    if (!(target instanceof Element)) {
      return null;
    }

    const mediaElement = target.closest("img,video");
    if (mediaElement instanceof HTMLImageElement) {
      return { type: "image", element: mediaElement };
    }

    if (mediaElement instanceof HTMLVideoElement) {
      return { type: "video", element: mediaElement };
    }

    return null;
  }

  function areSameHoveredMedia(left, right) {
    return !!left && !!right && left.type === right.type && left.element === right.element;
  }

  function describeHoveredEntry(hoveredEntry) {
    if (!hoveredEntry || !hoveredEntry.type) {
      return "No hovered media was available.";
    }

    if (hoveredEntry.type === "video") {
      return "Video saving only supports direct http/https URLs. blob: and MediaSource targets are skipped.";
    }

    return "No saveable media URL was found on the hovered element.";
  }

  function formatNativeResponse(response) {
    if (!response || typeof response !== "object") {
      return "No response details were returned.";
    }

    const errorCode = typeof response.errorCode === "string" && response.errorCode
      ? response.errorCode
      : "UnknownError";
    const message = typeof response.message === "string" && response.message
      ? response.message
      : "No error message was returned.";

    return `${errorCode}: ${message}`;
  }

  function formatError(error) {
    if (error instanceof Error) {
      return error.message;
    }

    return typeof error === "string" ? error : "Unknown error";
  }

  function showSavePopup(response) {
    const popup = ensureSavePopupElement();
    const titleElement = popup.querySelector("[data-role='title']");
    const fileNameElement = popup.querySelector("[data-role='file-name']");
    const fileName = getPopupFileName(response);

    if (titleElement) {
      titleElement.textContent = "保存しました";
    }

    if (fileNameElement) {
      fileNameElement.textContent = fileName || "";
      fileNameElement.style.display = fileName ? "block" : "none";
    }

    popup.dataset.state = "visible";

    if (popupHideTimeoutId) {
      window.clearTimeout(popupHideTimeoutId);
    }

    popupHideTimeoutId = window.setTimeout(() => {
      popup.dataset.state = "hidden";
      popupHideTimeoutId = 0;
    }, SAVE_POPUP_DURATION_MS);
  }

  function getPopupFileName(response) {
    if (!response || typeof response !== "object") {
      return "";
    }

    if (typeof response.fileName === "string" && response.fileName.trim()) {
      return response.fileName.trim();
    }

    if (typeof response.savedPath === "string" && response.savedPath.trim()) {
      const parts = response.savedPath.trim().split(/[\\/]/);
      return parts[parts.length - 1] || "";
    }

    return "";
  }

  function ensureSavePopupElement() {
    const existing = document.getElementById(SAVE_POPUP_CONTAINER_ID);
    if (existing) {
      return existing;
    }

    const popup = document.createElement("div");
    popup.id = SAVE_POPUP_CONTAINER_ID;
    popup.dataset.state = "hidden";
    popup.setAttribute("aria-live", "polite");
    popup.style.position = "fixed";
    popup.style.right = "18px";
    popup.style.bottom = "18px";
    popup.style.zIndex = "2147483647";
    popup.style.pointerEvents = "none";
    popup.style.minWidth = "220px";
    popup.style.maxWidth = "320px";
    popup.style.padding = "10px 12px";
    popup.style.borderRadius = "12px";
    popup.style.background = "rgba(16, 18, 24, 0.78)";
    popup.style.backdropFilter = "blur(10px)";
    popup.style.boxShadow = "0 10px 30px rgba(0, 0, 0, 0.22)";
    popup.style.color = "#f5f7fb";
    popup.style.fontFamily = "'Segoe UI', 'Yu Gothic UI', sans-serif";
    popup.style.fontSize = "12px";
    popup.style.lineHeight = "1.4";
    popup.style.opacity = "0";
    popup.style.transform = "translateY(8px)";
    popup.style.transition = "opacity 160ms ease, transform 160ms ease";
    popup.style.border = "1px solid rgba(255, 255, 255, 0.08)";

    const title = document.createElement("div");
    title.dataset.role = "title";
    title.style.fontWeight = "600";
    title.style.letterSpacing = "0.01em";
    title.textContent = "保存しました";

    const fileName = document.createElement("div");
    fileName.dataset.role = "file-name";
    fileName.style.marginTop = "3px";
    fileName.style.opacity = "0.88";
    fileName.style.wordBreak = "break-word";

    popup.appendChild(title);
    popup.appendChild(fileName);

    const observer = new MutationObserver(() => {
      const isVisible = popup.dataset.state === "visible";
      popup.style.opacity = isVisible ? "1" : "0";
      popup.style.transform = isVisible ? "translateY(0)" : "translateY(8px)";
    });
    observer.observe(popup, { attributes: true, attributeFilter: ["data-state"] });

    const root = document.documentElement || document.body;
    root.appendChild(popup);
    return popup;
  }

  function sendRuntimeMessage(message) {
    return browser.runtime.sendMessage(message);
  }
})();
