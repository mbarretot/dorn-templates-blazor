# Dorn Blazor Server Template

Create a polished .NET 10 Blazor application with Interactive Server rendering, MudBlazor, a branded theme with light/dark mode, and optional Aspire orchestration.

## 🚀 Start

```bash
dotnet new install Dorn.Templates.BlazorServer
dotnet new dorn-blazor-server -n Acme.Portal
cd Acme.Portal
dotnet run --project src/Acme.Portal.AppHost
```

## ✨ Options

| Option | Default | Effect |
| --- | --- | --- |
| `--IncludeAspire <bool>` | `false` | Add an AppHost and ServiceDefaults project for Aspire orchestration. |
| `--IncludeTests <bool>` | `true` | Exclude generated tests. |

> [!TIP]
> Open the web resource from the Aspire dashboard after launch.

> [!NOTE]
> Visual Studio automatically lists the installed template with the Dorn icon in **Create a new project**.

[Source and documentation](https://github.com/mbarretot/dorn-templates-blazor)
