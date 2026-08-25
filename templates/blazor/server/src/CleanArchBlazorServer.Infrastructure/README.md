# CleanArchBlazorServer.Infrastructure

Concrete implementations of `Application`'s interfaces: persistence, external services, and anything else that talks to the outside world. Blazor Server runs in-process, so this layer may use EF Core or any other server-side persistence freely — unlike the WASM template's browser-sandboxed `Infrastructure/` sub-folders.

**Rule**: may depend on `Application` and `Domain`, but never on `Web`. Enforced by `Infrastructure_ShouldNot_DependOnWeb` in `CleanArchBlazorServer.Application.Tests/Architecture/CleanArchitectureLayeringTests.cs`.

## What's here

- `ToDos/JsonPlaceholderToDoRepository.cs` — implements `IToDoRepository` against [jsonplaceholder.typicode.com](https://jsonplaceholder.typicode.com/todos), a public fake REST API. It's registered in `Program.cs` via `AddHttpClient<IToDoRepository, JsonPlaceholderToDoRepository>`. Swap it for a real repository (EF Core, another API, a file) — `Application` and `Domain` never need to change.

## Suggested shape as you grow this layer

- `Persistence/` — `DbContext`, EF configurations, migrations
- `Services/` — concrete implementations of `Application` interfaces (like `ToDos/` here)
