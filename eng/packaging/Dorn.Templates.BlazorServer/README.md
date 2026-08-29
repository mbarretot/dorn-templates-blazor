# Dorn Blazor Server Template

Create a .NET 10 application with Interactive Server rendering, MudBlazor, a warm editorial design system, and optional Aspire orchestration.

## 🚀 Create and run

```bash
dotnet new install Dorn.Templates.BlazorServer
dotnet new dorn-blazor-server -n Acme.Portal
cd Acme.Portal
dotnet run --project src/Acme.Portal.Web
```

> [!TIP]
> Choose a palette while generating the application: `dotnet new dorn-blazor-server -n Acme.Portal --Palette Ocean`.

## ✨ What you get

- Interactive Server rendering and a responsive MudBlazor shell
- Light, dark, and system modes without a first-paint flash
- Self-hosted Newsreader and system UI fonts
- Optional Clean Architecture layers with enforced dependency rules
- Optional Aspire AppHost and ServiceDefaults projects
- xUnit, bUnit, architecture, and integration test foundations

## ⚙️ Options

| Option | Default | Effect |
| --- | --- | --- |
| `--IncludeAspire <bool>` | `false` | Adds Aspire orchestration projects |
| `--IncludeTests <bool>` | `true` | Includes generated test projects |
| `--IncludeCleanArchitecture <bool>` | `false` | Adds Domain, Application, and Infrastructure layers |

> [!TIP]
> Visual Studio discovers the installed template automatically in **Create a new project**.

[View source and full documentation](https://github.com/mbarretot/dorn-templates-blazor)
