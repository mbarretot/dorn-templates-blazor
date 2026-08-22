# CleanArchBlazorWasm

[![Scaffolded with Dorn](https://img.shields.io/badge/scaffolded_with-Dorn-1A1A1A?style=flat-square)](https://github.com/mbarretot/dorn)

A front-end-only Blazor WebAssembly app with a Tailwind CSS pipeline and Aspire orchestration.

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
| `Web` | The Blazor WebAssembly app: components, styles, and static assets |
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

- [Blazor WASM template reference](https://github.com/mbarretot/dorn/blob/main/docs/templates/blazor-wasm.md)
- [Dorn architecture decisions](https://github.com/mbarretot/dorn/tree/main/docs/adr)
