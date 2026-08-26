# Application layer

Orchestrate use cases here without depending on UI or infrastructure details.

## ✅ Dependency rule

`Application` may depend on `Domain` only. It must never reference `Infrastructure` or `Web`.

> [!NOTE]
> `Application_ShouldNot_DependOnInfrastructureOrWeb` enforces this boundary in the architecture tests.

## 🧭 Start here

1. Define the use case in a focused feature folder.
2. Add the port the use case needs under `Interfaces/`.
3. Keep persistence and HTTP details outside this project.
4. Cover orchestration with unit tests.

## 📦 Current example

| Path | Purpose |
| --- | --- |
| `Interfaces/IToDoRepository.cs` | Port implemented by Infrastructure for the sample to-do flow |

## 📁 Suggested shape

- `Interfaces/` — ports implemented by outer layers
- `Services/` — use-case orchestration
- `DTOs/` — boundary shapes when Domain entities should stay internal

No MediatR convention is imposed. Prefer the simplest organization that keeps the dependency rule obvious.
