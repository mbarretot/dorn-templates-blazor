# CleanArchBlazorServer

[![Scaffolded with Dorn](https://img.shields.io/badge/scaffolded_with-Dorn-1A1A1A?style=flat-square)](https://github.com/mbarretot/dorn)

A front-end-only Blazor Server app with Interactive Server rendering, a Tailwind CSS pipeline,
and Aspire orchestration.

## ⚡ Start here

```bash
dotnet dev-certs https --trust
dotnet tool restore
dotnet dorn run
```

Verify the project:

```bash
dotnet dorn test
```

## 🏛️ Project map

| Area | Responsibility |
| --- | --- |
| `Web` | The Blazor Server app: components, styles, and static assets, served by Kestrel |
| `ServiceDefaults` | Shared Aspire wiring: OpenTelemetry, health checks, service discovery |
| `AppHost` | Aspire orchestration for local `dotnet run` |

## 🎨 Styling

`wwwroot/app.css` is generated at build time from `Styles/app.tailwind.css` by the pinned,
checksum-verified Tailwind CSS standalone CLI (`build/Tailwind.targets`) — no Node, no npm.
The file is gitignored; it always regenerates from source.

## ⌨️ Project CLI

| Command | Action |
| --- | --- |
| `dotnet dorn run` | Run the Aspire AppHost |
| `dotnet dorn test` | Run every tier |
| `dotnet dorn coverage` | Test with the coverage gate |

> [!NOTE]
> This scoped template does not generate a CI workflow yet.

## 📚 Details

- [Blazor Server template reference](https://github.com/mbarretot/dorn/blob/main/docs/templates/blazor-server.md)
- [Dorn architecture decisions](https://github.com/mbarretot/dorn/tree/main/docs/adr)
