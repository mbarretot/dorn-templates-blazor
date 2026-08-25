# CleanArchBlazorServer.Application

Use cases and application services that orchestrate `Domain` types. This is where the "what the app does" logic lives, expressed independently of any UI or persistence technology.

**Rule**: may depend on `Domain` only — never on `Infrastructure` or `Web`. Enforced by `Application_ShouldNot_DependOnInfrastructureOrWeb` in `CleanArchBlazorServer.Application.Tests/Architecture/CleanArchitectureLayeringTests.cs`.

## What's here

- `Interfaces/IToDoRepository.cs` — the abstraction `Infrastructure` implements for the worked `ToDoItem` example. `Application` only knows the interface; it never sees `HttpClient` or JSON.

## Suggested shape as you grow this layer

- `Interfaces/` — abstractions that `Infrastructure` implements (like `IToDoRepository`)
- `Services/` — application services orchestrating one or more `Domain` types
- `DTOs/` — shapes for crossing the boundary into `Web`, when you don't want to expose `Domain` entities directly

No CQRS/MediatR convention is imposed — organize `Application` however fits your team.
