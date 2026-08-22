// Owned JS module (design C7), consumed starting with DropdownMenu/Select (PR6). Positions a
// floating element relative to its anchor: flips to the opposite side when the preferred side
// would overflow the viewport, clamps the cross axis so the floating element never overflows,
// and re-positions on scroll/resize until disposed.

const observers = new Map();

function measure(anchorEl, floatingEl, side, align, offset, collisionPadding) {
  const anchorRect = anchorEl.getBoundingClientRect();
  const floatingRect = floatingEl.getBoundingClientRect();
  const viewportWidth = window.innerWidth;
  const viewportHeight = window.innerHeight;

  let resolvedSide = side;
  const fitsBelow =
    anchorRect.bottom + offset + floatingRect.height <= viewportHeight - collisionPadding;
  const fitsAbove = anchorRect.top - offset - floatingRect.height >= collisionPadding;

  if (side === "bottom" && !fitsBelow && fitsAbove) {
    resolvedSide = "top";
  } else if (side === "top" && !fitsAbove && fitsBelow) {
    resolvedSide = "bottom";
  }

  const top =
    resolvedSide === "top"
      ? anchorRect.top - offset - floatingRect.height
      : anchorRect.bottom + offset;

  let left =
    align === "end"
      ? anchorRect.right - floatingRect.width
      : align === "center"
        ? anchorRect.left + anchorRect.width / 2 - floatingRect.width / 2
        : anchorRect.left;

  left = Math.min(
    Math.max(left, collisionPadding),
    viewportWidth - floatingRect.width - collisionPadding,
  );

  return { top, left };
}

export function position(anchorEl, floatingEl, side, align, offset, collisionPadding) {
  function reposition() {
    const next = measure(anchorEl, floatingEl, side, align, offset, collisionPadding);
    floatingEl.style.position = "fixed";
    floatingEl.style.top = `${next.top}px`;
    floatingEl.style.left = `${next.left}px`;
  }

  reposition();

  window.addEventListener("scroll", reposition, true);
  window.addEventListener("resize", reposition);
  observers.set(floatingEl, reposition);
}

export function dispose(floatingEl) {
  const reposition = observers.get(floatingEl);
  if (!reposition) {
    return;
  }

  window.removeEventListener("scroll", reposition, true);
  window.removeEventListener("resize", reposition);
  observers.delete(floatingEl);
}

export function show(floatingEl) {
  floatingEl.showPopover();
}

export function hide(floatingEl) {
  if (floatingEl.matches(":popover-open")) {
    floatingEl.hidePopover();
  }
}
