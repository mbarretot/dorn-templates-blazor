(function () {
  "use strict";

  var MODE_STORAGE_KEY = "ui-mode";

  var media = window.matchMedia("(prefers-color-scheme: dark)");

  function readStoredMode() {
    try {
      return window.localStorage.getItem(MODE_STORAGE_KEY) || "system";
    } catch (error) {
      return "system";
    }
  }

  // Runs synchronously, before Blazor has even started (server: before first paint of the
  // static root document; WASM: before download starts) so MainLayout can resolve the initial
  // MudThemeProvider IsDarkMode value before its first paint-relevant render.
  window.dornTheme = {
    getSnapshot: function () {
      return {
        theme: "",
        mode: readStoredMode(),
        systemPrefersDark: media.matches,
      };
    },
    setTheme: function () {
      // No theme families remain; kept as a no-op so ThemeState's JS interop contract
      // still resolves.
    },
    setMode: function (mode) {
      try {
        window.localStorage.setItem(MODE_STORAGE_KEY, mode);
      } catch (error) {
        // Storage unavailable (e.g. private browsing) — the choice still applies for
        // this page load, it just will not survive a refresh.
      }
    },
  };
})();
