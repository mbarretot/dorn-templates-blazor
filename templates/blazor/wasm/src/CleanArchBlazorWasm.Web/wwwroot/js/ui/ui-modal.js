// Owned JS module (design C7): everything the native <dialog> element does NOT already provide.
// showModal() itself gives the top layer, ::backdrop, and document inertness for free — nothing
// outside the dialog stays focusable while it is open, so Tab/Shift+Tab cannot escape it, and
// dialog.close() restores focus to the element that had it before showModal() was called. This
// module adds only ref-counted body scroll lock, explicit initial-focus placement, and routing
// of the native `cancel` event (Escape) and backdrop clicks through the single
// [JSInvokable] RequestDismissAsync callback, so every dismissal source shares the exact same
// C# close path (design: "OnOpenChange fires ONCE").

let scrollLockCount = 0;
let previousBodyOverflow = null;

function lockScroll() {
  scrollLockCount += 1;
  if (scrollLockCount === 1) {
    previousBodyOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
  }
}

function unlockScroll() {
  if (scrollLockCount === 0) {
    return;
  }
  scrollLockCount -= 1;
  if (scrollLockCount === 0) {
    document.body.style.overflow = previousBodyOverflow || "";
    previousBodyOverflow = null;
  }
}

function placeInitialFocus(dialogEl, initialFocusSelector) {
  let target = initialFocusSelector ? dialogEl.querySelector(initialFocusSelector) : null;

  if (!target) {
    target = dialogEl.querySelector(
      "[autofocus], button, [href], input, select, textarea, [tabindex]:not([tabindex='-1'])",
    );
  }

  (target || dialogEl).focus();
}

function isOutsideDialogContent(dialogEl, event) {
  if (event.target !== dialogEl) {
    return false;
  }

  const rect = dialogEl.getBoundingClientRect();
  return (
    event.clientX < rect.left ||
    event.clientX > rect.right ||
    event.clientY < rect.top ||
    event.clientY > rect.bottom
  );
}

export function open(dialogEl, dotNetRef, initialFocusSelector) {
  function requestDismiss() {
    dotNetRef.invokeMethodAsync("RequestDismissAsync");
  }

  function onCancel(event) {
    // Prevent the platform's default auto-close so Escape flows through the same C# state
    // transition as every other dismissal, instead of racing it.
    event.preventDefault();
    requestDismiss();
  }

  function onBackdropClick(event) {
    if (isOutsideDialogContent(dialogEl, event)) {
      requestDismiss();
    }
  }

  dialogEl.addEventListener("cancel", onCancel);
  dialogEl.addEventListener("click", onBackdropClick);
  dialogEl.__dornDialogHandlers = { onCancel, onBackdropClick };

  dialogEl.showModal();
  lockScroll();
  placeInitialFocus(dialogEl, initialFocusSelector);
}

export function close(dialogEl) {
  const handlers = dialogEl.__dornDialogHandlers;
  if (handlers) {
    dialogEl.removeEventListener("cancel", handlers.onCancel);
    dialogEl.removeEventListener("click", handlers.onBackdropClick);
    delete dialogEl.__dornDialogHandlers;
  }

  if (dialogEl.open) {
    dialogEl.close();
  }

  unlockScroll();
}
