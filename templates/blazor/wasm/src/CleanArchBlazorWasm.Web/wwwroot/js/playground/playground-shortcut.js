let listening = false;

export function activate() {
  if (listening) {
    return;
  }

  document.addEventListener("keydown", onKeyDown, true);
  listening = true;
}

export function deactivate() {
  if (!listening) {
    return;
  }

  document.removeEventListener("keydown", onKeyDown, true);
  listening = false;
}

function onKeyDown(event) {
  if (event.key !== "/" || event.ctrlKey || event.metaKey || event.altKey) {
    return;
  }

  if (isEditable(document.activeElement)) {
    return;
  }

  const target = [...document.querySelectorAll("[data-playground-search]")].find(
    (el) => el.offsetParent !== null || el.closest("dialog[open]"),
  );
  if (!target) {
    return;
  }

  event.preventDefault();
  target.focus();
  target.select?.();
}

function isEditable(el) {
  if (!el) {
    return false;
  }

  const tag = el.tagName;
  return tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT" || el.isContentEditable;
}
