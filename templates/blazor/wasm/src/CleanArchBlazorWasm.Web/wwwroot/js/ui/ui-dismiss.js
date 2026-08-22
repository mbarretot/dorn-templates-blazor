const activations = new Map();
let isListening = false;

export function activate(id, containerEl, dotNetRef) {
  activations.delete(id);
  activations.set(id, { containerEl, dotNetRef });

  if (isListening) {
    return;
  }

  document.addEventListener("pointerdown", onPointerDown, true);
  document.addEventListener("keydown", onKeyDown, true);
  isListening = true;
}

export function deactivate(id) {
  activations.delete(id);

  if (activations.size !== 0 || !isListening) {
    return;
  }

  document.removeEventListener("pointerdown", onPointerDown, true);
  document.removeEventListener("keydown", onKeyDown, true);
  isListening = false;
}

function onPointerDown(event) {
  const activation = getTopActivation();
  if (activation && !activation.containerEl.contains(event.target)) {
    requestDismiss(activation);
  }
}

function onKeyDown(event) {
  const activation = getTopActivation();
  if (activation && event.key === "Escape") {
    event.preventDefault();
    event.stopPropagation();
    requestDismiss(activation);
  }
}

function getTopActivation() {
  let activation;
  for (const candidate of activations.values()) {
    activation = candidate;
  }

  return activation;
}

function requestDismiss(activation) {
  activation.dotNetRef.invokeMethodAsync("RequestDismissAsync");
}
