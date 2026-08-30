<div align="center">
  <img src="docs/assets/dorn-icon.jpg" alt="Hand-drawn Dorn architectural mark" width="112" />

# CleanArchBlazorServer

[![Scaffolded with Dorn](https://img.shields.io/badge/scaffolded_with-Dorn-b0533a?style=flat-square)](https://github.com/mbarretot/dorn)
[![.NET 10](https://img.shields.io/badge/.NET-10-b0533a?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**Interactive Server rendering with MudBlazor and a developer-ready foundation.**

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
| `Web` | Interactive Server UI, features, theme, and static assets |
| `Domain` | Business entities and rules when Clean Architecture is enabled |
| `Application` | Use cases and ports when Clean Architecture is enabled |
| `Infrastructure` | External adapters when Clean Architecture is enabled |
| `ServiceDefaults` | OpenTelemetry, health checks, and service discovery when Aspire is enabled |
| `AppHost` | Local orchestration when Aspire is enabled |

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

`Program.cs` sets a baseline CSP plus `X-Content-Type-Options`, `X-Frame-Options`, and
`Referrer-Policy` on every response. The CSP allows `'unsafe-inline'` in `style-src` because
MudBlazor positions popovers/overlays by writing inline `style` via JS interop — tightening that
further will break those components. If you point `IToDoRepository` at a different backend, add
its origin to `connect-src`; if you embed the app in an iframe, relax `frame-ancestors` and
`X-Frame-Options` accordingly.

## 📚 Next step

Build features inside `Web/Features/{FeatureName}`. If Clean Architecture is enabled, keep dependencies pointed inward and use each layer README as a boundary guide.
