# Contributing

This repository owns the Blazor template family (`templates/blazor/{wasm,server}`) and publishes it
as two `dotnet new` template packs, `Dorn.Templates.BlazorWasm` and `Dorn.Templates.BlazorServer`,
consumed by [`mbarretot/dorn`](https://github.com/mbarretot/dorn). Dorn vendors the packed `content/`
back into its own `templates/blazor/{wasm,server}` at build time (see dorn's ADR 0027); it never
references this repository's source directly.

## Local dev loop

No local NuGet feed setup is needed — everything (including `Dorn.WebUI.Primitives`) resolves from
nuget.org.

```bash
dotnet test templates/blazor/wasm/CleanArchBlazorWasm.slnx
dotnet test templates/blazor/server/CleanArchBlazorServer.slnx
dotnet test tests/Dorn.Templates.Blazor.Tests/Dorn.Templates.Blazor.Tests.csproj
```

The first two commands run each template's own Application/Architecture/Functional/Integration test
tiers. The third packs both template packs, installs them via `dotnet new install`, generates real
projects across all 6 themes and the playground toggle, and builds the result — this is the
CLI-channel proof that mirrors what `dorn new blazor-wasm`/`blazor-server` do downstream.

Run it as its own project, not via `dotnet test DornTemplatesBlazor.slnx`: that command would also
run the browser quality suite below in the same pass, and both projects install/uninstall the same
two template package IDs into the shared, global `dotnet new` template store — running them
concurrently races and fails intermittently.

### Browser quality suite

```bash
dotnet test tests/Dorn.Templates.Blazor.BrowserTests/Dorn.Templates.Blazor.BrowserTests.csproj -c Release
```

Packs and publishes both templates, launches the generated WASM and Server hosts on loopback ports,
and drives the Observatory shell with Playwright across mobile and desktop viewports for each host:
Home → Playground navigation, keyboard focus, 200% zoom, `axe-core` accessibility scan, navigation
performance, and System-mode theme persistence. The Server host runs from its published output
(`dotnet publish` + the published dll) rather than `dotnet run`, because .NET 10's
`MapStaticAssets()` mis-serves framework assets when run from source; run it from its own directory,
since ASP.NET Core derives the content root from the working directory, not the executable's path.

## Theme system

Each host ships six theme families under `Styles/themes/*.css` — `slate` (default), `rose`,
`neutral`, `linear`, `primer`, `lightning` — selected at generation time by the template's `--theme`
parameter (`.template.config/template.json`). `wwwroot/theme-boot.js` is a classic, non-module
`<script>` that must stay synchronous: it sets `data-ui-theme`/`data-ui-mode` on `<html>` before
first paint, so C#/Blazor never has to and a themed reload never flashes the wrong theme. To add a
theme family: add a `Styles/themes/<name>.css` file with light/dark token blocks (mirrored in both
`templates/blazor/{wasm,server}`), add its `choice` entry to both `template.json` files, and add it
to the Phase 4 `ThemeFamilyStylesTests` parsing coverage in both hosts.

## Playground

`Features/Playground/PlaygroundCatalog.cs` is the single source of truth for the Playground's nav
and routes — each entry maps to a `/playground/<slug>` route and a `<Component>Playground.razor`
page. To add a new component: create its playground page (reusing `EnumControl`/`BoolControl`/
`TextControl`/`NumberControl` for its inspector controls, each of which forwards `aria-label` and
other unmatched attributes straight to the underlying `Select`/`Switch`/`Input`), then add a
`PlaygroundEntry` to the matching category in both hosts' `PlaygroundCatalog.cs`.

## Assets

Static assets referenced from Razor (`theme-boot.js`, `app.css`) MUST go through `@Assets["..."]`,
never a literal `href`/`src`, so ASP.NET Core's `MapStaticAssets()` fingerprints the URL for cache
busting. The Server host's `App.razor` deliberately sets no `data-ui-theme`/`data-ui-mode` inline
because the server can't read `localStorage` during a static render — `theme-boot.js` stays the sole
writer for both hosts.

## Versioning

Versioning is tag-derived, not GitVersion. Pushing a `v<version>` tag (e.g. `v1.2.0`) triggers
`.github/workflows/publish.yml`, which packs both template projects at that exact version and
publishes them to nuget.org via NuGet Trusted Publishing (no manual API key). Outside of a tag push,
packages built locally or in CI fall back to `0.0.1-local`/`0.0.1-test`, values that can never be
mistaken for a real release.

## Conventions

This is the same author's sibling repository to `dorn` and mirrors its conventions:

- Plain conventional commits (`type(scope): message`); never add `Co-Authored-By` or other AI
  attribution.
- Run `dotnet csharpier format <touched-paths>` before every commit.
- xUnit with plain `Assert.*`; never add FluentAssertions or Moq.
- English only in code, comments, commit messages, and documentation.
- Strict TDD (RED → GREEN → REFACTOR) for new test logic.
