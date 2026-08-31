# Dorn Blazor WebAssembly Template

Create a .NET 10 application that runs in the browser with MudBlazor, a warm editorial design system, and optional Aspire orchestration.

## 🚀 Create and run

```bash
dotnet new install Dorn.Templates.BlazorWasm
dotnet new dorn-blazor-wasm -n Acme.Portal
cd Acme.Portal
dotnet run --project src/Acme.Portal.Web
```

> [!TIP]
> Choose a palette while generating the application: `dotnet new dorn-blazor-wasm -n Acme.Portal --Palette Ocean`.

## ✨ What you get

- Browser-side execution and a responsive MudBlazor shell
- Light, dark, and system modes without a first-paint flash
- Self-hosted Newsreader and system UI fonts
- Optional Aspire AppHost for local orchestration
- xUnit, bUnit, architecture, and integration test foundations

## ⚙️ Options

| Option | Default | Effect |
| --- | --- | --- |
| `--IncludeAspire <bool>` | `false` | Adds the AppHost project |
| `--IncludeTests <bool>` | `true` | Includes generated test projects |
| `--IncludeCleanArchitecture <bool>` | `false` | Adds Domain, Application, and Infrastructure layers |
| `--IncludeAuth <bool>` | `false` | Adds a localStorage-backed authentication starting point with demo login and protected pages |
| `--Palette <Terracotta\|Ocean\|Forest\|Sunset\|Lavender\|Slate\|Citrus>` | `Terracotta` | Selects the application color palette |

> [!TIP]
> Visual Studio discovers the installed template automatically in **Create a new project**.

[View source and full documentation](https://github.com/mbarretot/dorn-templates-blazor)
