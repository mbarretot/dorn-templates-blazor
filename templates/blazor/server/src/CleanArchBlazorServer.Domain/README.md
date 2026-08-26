# Domain layer

Model business concepts and rules here, independent of frameworks and delivery mechanisms.

## ✅ Dependency rule

`Domain` depends on no other project or third-party package. Only `System.*` BCL dependencies are allowed.

> [!NOTE]
> `Domain_Should_DependOnNothingButBcl` and `Domain_ShouldNot_DependOnOuterLayers` enforce this boundary.

## 🧭 Start here

1. Replace the sample entity with the first real business concept.
2. Put invariants beside the data they protect.
3. Keep persistence, HTTP, UI, and configuration concerns outside this project.
4. Test business behavior directly.

## 📦 Current example

| Path | Purpose |
| --- | --- |
| `Entities/ToDoItem.cs` | Minimal entity wired through the sample application flow |

## 📁 Suggested shape

- `Entities/` — objects with identity
- `ValueObjects/` — immutable concepts defined by their values
- `Enums/` — domain vocabulary with a closed set of values
- `Exceptions/` — domain-specific invariant failures
