<div align="center">
  <img src="docs/assets/dorn-icon.jpg" alt="Hand-drawn Dorn architectural mark" width="112" />

# CleanArchBlazorWasm

[![Scaffolded with Dorn](https://img.shields.io/badge/scaffolded_with-Dorn-b0533a?style=flat-square)](https://github.com/mbarretot/dorn)
[![.NET 10](https://img.shields.io/badge/.NET-10-b0533a?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**Blazor WebAssembly with MudBlazor and a developer-ready foundation.**

</div>

## ⚡ Run locally

```bash
dotnet dev-certs https --trust
dotnet tool restore
dotnet dorn run
```

> [!TIP]
> Run `dotnet dorn test` before your first feature to confirm the generated solution is healthy.

## 🧭 Project map

| Project | Responsibility |
| --- | --- |
| `Web` | Browser application, features, theme, and static assets |
| `AppHost` | Local Aspire orchestration when enabled |

## 🎨 UI foundation

- MudBlazor components with matching light and dark palettes
- Warm paper backgrounds, ink text, and 7 selectable color palettes (Terracotta by default)
- Newsreader for editorial content; system fonts for controls
- Theme preference applied before first paint and synchronized at runtime
- Self-hosted assets with no CDN or Node/npm dependency

## ⚙️ Template option

The application palette is selected when the template is generated.

| Option | Default | Choices |
| --- | --- | --- |
| `--Palette` | `Terracotta` | `Terracotta`, `Ocean`, `Forest`, `Sunset`, `Lavender`, `Slate`, `Citrus` |

## ⌨️ Daily commands

| Command | Action |
| --- | --- |
| `dotnet dorn run` | Run the application or Aspire AppHost |
| `dotnet dorn test` | Run all test tiers |
| `dotnet dorn coverage` | Run tests with the coverage gate |

## 🔒 Security headers

`wwwroot/index.html` declares a baseline CSP via `<meta http-equiv="Content-Security-Policy">`.
It allows `'unsafe-inline'` in `style-src` because MudBlazor positions popovers/overlays by
writing inline `style` via JS interop, and `'wasm-unsafe-eval'` in `script-src` because Blazor
WebAssembly can't instantiate its compiled WASM modules without it — tightening either further
will break the app. A
`<meta>` tag can't carry `frame-ancestors`, `X-Frame-Options`, or `Referrer-Policy`: since this is
a standalone WASM app with no server of its own, set those on whatever static host you deploy to
(e.g. Azure Static Web Apps' `staticwebapp.config.json`, or your CDN/reverse proxy's config). If
you point `IToDoRepository` at a different backend, add its origin to `connect-src`.

## 📚 Next step

Build vertical slices inside `Web/Features/{FeatureName}` and keep browser-only infrastructure behind focused interfaces.
