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
dotnet test DornTemplatesBlazor.slnx
```

The first two commands run each template's own Application/Architecture/Functional/Integration test
tiers. The third packs both template packs, installs them via `dotnet new install`, generates real
projects across all 6 themes and the playground toggle, and builds the result — this is the
CLI-channel proof that mirrors what `dorn new blazor-wasm`/`blazor-server` do downstream.

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
