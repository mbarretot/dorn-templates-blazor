<div align="center">
  <img src="docs/assets/dorn-icon.jpg" alt="Dorn" width="148" />

# Dorn Blazor Templates

**Polished .NET 10 Blazor applications—ready in one command.**

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WASM%20%2B%20Server-7C3AED?style=flat-square&logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![NuGet](https://img.shields.io/nuget/v/Dorn.Templates.BlazorWasm?style=flat-square&color=9333EA&label=NuGet&logo=nuget)](https://www.nuget.org/packages/Dorn.Templates.BlazorWasm)
[![CI](https://img.shields.io/github/actions/workflow/status/mbarretot/dorn-templates-blazor/ci.yml?branch=main&style=flat-square&label=build)](https://github.com/mbarretot/dorn-templates-blazor/actions/workflows/ci.yml)

</div>

Build with **Blazor WebAssembly** or **Interactive Server** without rebuilding the UI foundation. Both templates ship MudBlazor, a branded theme with light/dark mode, and optional Aspire orchestration.

---

## 🚀 Quick start

Choose a hosting model:

```bash
# Blazor WebAssembly
dotnet new install Dorn.Templates.BlazorWasm
dotnet new dorn-blazor-wasm -n Acme.Portal

# Blazor Server
dotnet new install Dorn.Templates.BlazorServer
dotnet new dorn-blazor-server -n Acme.Portal
```

Run the generated application:

```bash
cd Acme.Portal
dotnet run --project src/Acme.Portal.AppHost
```

> [!TIP]
> Open the web resource from the Aspire dashboard.

> [!NOTE]
> Visual Studio automatically discovers installed `dotnet new` templates and shows the Dorn icon in **Create a new project**.

---

## ✨ Included

| | Capability |
| --- | --- |
| 🧩 | MudBlazor (Material components) and a responsive shell |
| 🎨 | A branded `AppTheme` palette, self-hosted—no CDN or Node/npm required |
| 🌗 | Light, dark, and system modes without first-paint theme flash |
| 🧪 | xUnit, bUnit, and integration test coverage |

---

## 🎛️ Template options

| Option | Default | Effect |
| --- | --- | --- |
| `--IncludeAspire <bool>` | `false` | Add an AppHost and ServiceDefaults project for Aspire orchestration. |
| `--IncludeTests <bool>` | `true` | Exclude generated test projects. |
| `--IncludeCleanArchitecture <bool>` (Server only) | `false` | Add `{App}.Domain`, `{App}.Application`, and `{App}.Infrastructure` class libraries wired to `{App}.Web`. `Features/{Name}/` stays the top-level shape either way — this only adds an internal layering option, enforced by ArchUnitNET rules once opted in. |

```bash
dotnet new dorn-blazor-server -n Acme.Backoffice \
  --IncludeAspire true \
  --IncludeTests false

# Opt into Clean Architecture class libraries (Server only)
dotnet new dorn-blazor-server -n Acme.Backoffice \
  --IncludeCleanArchitecture true
```

---

## 🧭 Choose your template

| Template | Best for | Package |
| --- | --- | --- |
| **WebAssembly** | Client-side execution and static hosting | [`Dorn.Templates.BlazorWasm`](https://www.nuget.org/packages/Dorn.Templates.BlazorWasm) |
| **Server** | Server-side execution and Interactive Server rendering | [`Dorn.Templates.BlazorServer`](https://www.nuget.org/packages/Dorn.Templates.BlazorServer) |

> [!NOTE]
> WASM and Server share the same UI contracts and visual language. The hosting model changes; the design system does not.

## 🤝 Contributing

Working on the templates themselves? Start with the short [contributor guide](CONTRIBUTING.md).
