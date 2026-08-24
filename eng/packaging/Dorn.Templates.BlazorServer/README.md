# Dorn Blazor Server Template

Create a polished .NET 10 Blazor application with Interactive Server rendering, Aspire, Tailwind CSS, accessible UI primitives, six themes, and an optional component observatory.

## 🚀 Start

```bash
dotnet new install Dorn.Templates.BlazorServer
dotnet new dorn-blazor-server -n Acme.Portal --Theme primer
cd Acme.Portal
dotnet run --project src/Acme.Portal.AppHost
```

## ✨ Options

| Option | Default | Effect |
| --- | --- | --- |
| `--Theme <name>` | `slate` | Select the initial theme. |
| `--IncludePlayground <bool>` | `true` | Remove the component observatory. |
| `--IncludeTests <bool>` | `true` | Exclude generated tests. |

> [!TIP]
> Open the web resource from the Aspire dashboard after launch.

> [!NOTE]
> Visual Studio automatically lists the installed template with the Dorn icon in **Create a new project**.

[Source and documentation](https://github.com/mbarretot/dorn-templates-blazor)
