import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const templatesRoot = join(dirname(fileURLToPath(import.meta.url)), "..", "..", "templates", "blazor");

function executeThemeBoot(path, initiallyDark) {
  const attributes = new Map();
  const storage = new Map([["ui-mode", "system"]]);
  const listeners = [];
  const media = {
    matches: initiallyDark,
    addEventListener(type, listener) {
      if (type === "change") {
        listeners.push(listener);
      }
    },
    change(matches) {
      this.matches = matches;
      for (const listener of listeners) {
        listener({ matches });
      }
    },
  };
  const window = {
    localStorage: {
      getItem: (key) => storage.get(key) ?? null,
      setItem: (key, value) => storage.set(key, value),
    },
    matchMedia: () => media,
  };
  const document = {
    documentElement: {
      setAttribute: (name, value) => attributes.set(name, value),
    },
  };

  vm.runInNewContext(readFileSync(path, "utf8"), { document, window });
  return { attributes, media, window };
}

for (const [host, project] of [
  ["wasm", "CleanArchBlazorWasm"],
  ["server", "CleanArchBlazorServer"],
]) {
  test(`${host} resolves System mode in the DOM and follows OS changes`, () => {
    const path = join(templatesRoot, host, "src", `${project}.Web`, "wwwroot", "theme-boot.js");
    const boot = executeThemeBoot(path, false);

    assert.equal(boot.attributes.get("data-ui-mode"), "light");
    assert.equal(boot.window.dornTheme.getSnapshot().mode, "system");

    boot.media.change(true);

    assert.equal(boot.attributes.get("data-ui-mode"), "dark");
    assert.equal(boot.window.dornTheme.getSnapshot().mode, "system");
  });
}
