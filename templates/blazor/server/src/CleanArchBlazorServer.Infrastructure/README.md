# Infrastructure layer

Implement Application ports for persistence, external services, files, queues, and other outside-world concerns.

## ✅ Dependency rule

`Infrastructure` may depend on `Application` and `Domain`, but never on `Web`.

> [!NOTE]
> `Infrastructure_ShouldNot_DependOnWeb` enforces this boundary in the architecture tests.

## 🧭 Replace the sample adapter

1. Implement the existing Application interface with your real technology.
2. Register the adapter in `Web/Program.cs`.
3. Add focused integration tests for external behavior.
4. Delete the sample adapter after the real path is covered.

## 📦 Current example

| Path | Purpose |
| --- | --- |
| `ToDos/ToDoRepository.cs` | Implements `IToDoRepository` against the JSONPlaceholder API |

Because Blazor Server executes on the server, this layer can use EF Core, file access, or other server-side integrations.

## 📁 Suggested shape

- `Persistence/` — DbContext, mappings, migrations, and repositories
- `Services/` — external API, storage, messaging, and platform adapters
