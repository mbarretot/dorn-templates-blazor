# CleanArchBlazorServer.Infrastructure

Concrete implementations of `Application`'s interfaces: persistence, external services, and anything else that talks to the outside world. Blazor Server runs in-process, so this layer may use EF Core or any other server-side persistence freely — unlike the WASM template's browser-sandboxed `Infrastructure/` sub-folders.

**Rule**: may depend on `Application` and `Domain`, but never on `Web`. Enforced by `Infrastructure_ShouldNot_DependOnWeb` in `CleanArchBlazorServer.Application.Tests/Architecture/CleanArchitectureLayeringTests.cs`.

This library ships empty on purpose. Add your first repository or service implementation here when you have one — there is nothing to remove first.
