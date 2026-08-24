<div align="center">

<img src="docs/assets/dorn-icon.jpg" alt="Dorn" width="112" />

# CleanArchBlazorServer

[![Scaffolded with Dorn](https://img.shields.io/badge/scaffolded_with-Dorn-7C3AED?style=flat-square)](https://github.com/mbarretot/dorn)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)

**Interactive Server rendering with Aspire, Tailwind CSS, and an accessible UI foundation.**

</div>

---

## ⚡ Start

```bash
dotnet dev-certs https --trust
dotnet tool restore
dotnet dorn run
```

The Aspire dashboard opens the application and its development resources.

> [!TIP]
> Run `dotnet dorn test` before your first feature to confirm the generated solution is healthy.

---

## 🧭 Project map

| Project | Purpose |
| --- | --- |
| `Web` | Interactive Server application, UI components, and static assets |
| `ServiceDefaults` | OpenTelemetry, health checks, and service discovery |
| `AppHost` | Local Aspire orchestration |

## 🎨 UI foundation

- Accessible Razor primitives under `Components/Ui`
- Six runtime themes with light, dark, and system modes
- Optional component observatory under `Features/Playground`
- Tailwind CSS generated during build—no Node or npm

> [!NOTE]
> Edit `Styles/app.tailwind.css`, not the generated `wwwroot/app.css` file.

## ⌨️ Commands

| Command | Action |
| --- | --- |
| `dotnet dorn run` | Run the Aspire AppHost |
| `dotnet dorn test` | Run all test tiers |
| `dotnet dorn coverage` | Test with the coverage gate |

## 📚 Reference

- [Blazor Server template guide](https://github.com/mbarretot/dorn/blob/main/docs/templates/blazor-server.md)
- [Dorn architecture decisions](https://github.com/mbarretot/dorn/tree/main/docs/adr)
