# CleanArchBlazorServer.Domain

The innermost layer: entities, value objects, and domain logic that describes the business, not the framework.

**Rule**: zero dependencies on any other project or third-party package, not even the BCL beyond `System.*`. Enforced by `Domain_Should_DependOnNothingButBcl` and `Domain_ShouldNot_DependOnOuterLayers` in `CleanArchBlazorServer.Application.Tests/Architecture/CleanArchitectureLayeringTests.cs`.

## What's here

- `Entities/ToDoItem.cs`: a minimal worked example (`Id`, `Title`, `IsCompleted`), wired end-to-end through `Application` and `Infrastructure`. Replace it with your first real entity; there is nothing to remove first, just this one file.

## Suggested shape as you grow this layer

- `Entities/`: objects with identity (like `ToDoItem`)
- `ValueObjects/`: immutable objects defined by their values, not an id
- `Enums/`
- `Exceptions/`: domain-specific exceptions (e.g. invariant violations)
