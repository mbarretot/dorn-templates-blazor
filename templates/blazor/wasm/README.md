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

## ⌨️ Daily commands

| Command | Action |
| --- | --- |
| `dotnet dorn run` | Run the application or Aspire AppHost |
| `dotnet dorn test` | Run all test tiers |
| `dotnet dorn coverage` | Run tests with the coverage gate |

## 📚 Next step

Build vertical slices inside `Web/Features/{FeatureName}` and keep browser-only infrastructure behind focused interfaces.
