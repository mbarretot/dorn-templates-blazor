<div align="center">
  <img src="docs/assets/dorn-icon.jpg" alt="Hand-drawn Dorn architectural mark" width="112" />

# Dorn Blazor Templates

**Developer-ready .NET 10 Blazor applications, generated in one command.**

[![.NET 10](https://img.shields.io/badge/.NET-10-b0533a?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WASM%20%2B%20Server-b0533a?style=flat-square&logo=blazor&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![NuGet](https://img.shields.io/nuget/v/Dorn.Templates.BlazorWasm?style=flat-square&color=b0533a&label=NuGet&logo=nuget&logoColor=white)](https://www.nuget.org/packages/Dorn.Templates.BlazorWasm)
[![Build](https://img.shields.io/github/actions/workflow/status/mbarretot/dorn-templates-blazor/ci.yml?branch=main&style=flat-square&label=build&color=b0533a)](https://github.com/mbarretot/dorn-templates-blazor/actions/workflows/ci.yml)

</div>

Choose **WebAssembly** for browser-side execution or **Server** for Interactive Server rendering. Both templates share the same MudBlazor foundation, warm editorial design system, theme behavior, and testing conventions.

## 🚀 Quick start

```bash
# WebAssembly
dotnet new install Dorn.Templates.BlazorWasm
dotnet new dorn-blazor-wasm -n Acme.Portal

# Interactive Server
dotnet new install Dorn.Templates.BlazorServer
dotnet new dorn-blazor-server -n Acme.Portal
```

Run the generated web project:

```bash
cd Acme.Portal
dotnet run --project src/Acme.Portal.Web
```

> [!TIP]
> Add `--IncludeAspire true` when you want local orchestration, service discovery, and the Aspire dashboard.

## 🧭 Choose a template

| Template | Choose it when | Package |
| --- | --- | --- |
| **WebAssembly** | The application should execute in the browser or support static hosting | [`Dorn.Templates.BlazorWasm`](https://www.nuget.org/packages/Dorn.Templates.BlazorWasm) |
| **Server** | The UI should execute on the server with Interactive Server rendering | [`Dorn.Templates.BlazorServer`](https://www.nuget.org/packages/Dorn.Templates.BlazorServer) |

## ✨ Included

- 🧩 Responsive MudBlazor application shell
- 🎨 Warm paper and ink design tokens with 7 selectable color palettes (Terracotta by default)
- 🌗 Light, dark, and system modes applied before first paint
- 🔤 Self-hosted Newsreader and system UI fonts—no CDN or Node/npm
- 🧪 xUnit, bUnit, architecture, integration, and browser test foundations
- ☁️ Optional .NET Aspire orchestration

## ⚙️ Template options

| Option | Default | Available in | Effect |
| --- | --- | --- | --- |
| `--IncludeAspire <bool>` | `false` | Both | Adds AppHost and ServiceDefaults projects |
| `--IncludeTests <bool>` | `true` | Both | Includes the generated test projects |
| `--IncludeCleanArchitecture <bool>` | `false` | Server | Adds Domain, Application, and Infrastructure layers with architecture rules |
| `--Palette <Terracotta\|Ocean\|Forest\|Sunset\|Lavender\|Slate\|Citrus>` | `Terracotta` | Both | Selects the application color palette |

```bash
dotnet new dorn-blazor-server -n Acme.Backoffice \
  --IncludeAspire true \
  --IncludeCleanArchitecture true
```

## 🛠️ Work on the templates

1. Read the [contributor guide](CONTRIBUTING.md).
2. Make the same UI-foundation change in Server and WASM.
3. Run the focused tests for the files you changed.
