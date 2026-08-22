(function () {
  "use strict";

  // Boot-default theme (design B6). The `Theme` template symbol replaces the sentinel word
  // below (not the surrounding quotes: Template Engine's `replaces` substitutes the raw
  // parameter value verbatim, so quoting the search pattern itself would emit an unquoted,
  // syntactically broken literal). The sentinel is unique across the whole template tree, so
  // a --theme choice cannot drift or rewrite an unrelated occurrence.
  var DEFAULT_THEME = "__DORN_DEFAULT_THEME__";

  var THEME_STORAGE_KEY = "ui-theme";
  var MODE_STORAGE_KEY = "ui-mode";

  var root = document.documentElement;
  var media = window.matchMedia("(prefers-color-scheme: dark)");

  function readStoredTheme() {
    try {
      return window.localStorage.getItem(THEME_STORAGE_KEY) || DEFAULT_THEME;
    } catch (error) {
      return DEFAULT_THEME;
    }
  }

  function readStoredMode() {
    try {
      return window.localStorage.getItem(MODE_STORAGE_KEY) || "system";
    } catch (error) {
      return "system";
    }
  }

  function resolveMode(mode) {
    if (mode === "light" || mode === "dark") {
      return mode;
    }
    return media.matches ? "dark" : "light";
  }

  function applyMode(mode) {
    root.setAttribute("data-ui-mode", resolveMode(mode));
  }

  // Runs synchronously, before Blazor WASM has even started downloading (design B5): WASM
  // starts after first paint, so applying the theme from C# would flash the wrong theme on
  // every load. This classic (non-module) script is the only thing that can run early enough.
  root.setAttribute("data-ui-theme", readStoredTheme());
  applyMode(readStoredMode());

  // Only the "system" preference needs a live subscription: "light"/"dark" are explicit user
  // choices that must not move just because the OS setting changed underneath them.
  media.addEventListener("change", function () {
    if (readStoredMode() === "system") {
      applyMode("system");
    }
  });

  window.dornTheme = {
    getSnapshot: function () {
      return {
        theme: readStoredTheme(),
        mode: readStoredMode(),
        systemPrefersDark: media.matches,
      };
    },
    setTheme: function (theme) {
      try {
        window.localStorage.setItem(THEME_STORAGE_KEY, theme);
      } catch (error) {
        // Storage unavailable (e.g. private browsing) — the attribute still applies for
        // this page load, it just will not survive a refresh.
      }
      root.setAttribute("data-ui-theme", theme);
    },
    setMode: function (mode) {
      try {
        window.localStorage.setItem(MODE_STORAGE_KEY, mode);
      } catch (error) {
        // See setTheme above.
      }
      applyMode(mode);
    },
  };
})();
