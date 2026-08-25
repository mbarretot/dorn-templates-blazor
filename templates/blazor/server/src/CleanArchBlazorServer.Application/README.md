# CleanArchBlazorServer.Application

Use cases and application services that orchestrate `Domain` types. This is where the "what the app does" logic lives, expressed independently of any UI or persistence technology.

**Rule**: may depend on `Domain` only — never on `Infrastructure` or `Web`. Enforced by `Application_ShouldNot_DependOnInfrastructureOrWeb` in `CleanArchBlazorServer.Application.Tests/Architecture/CleanArchitectureLayeringTests.cs`.

This library ships empty on purpose. Add your first use case here when you have one — there is nothing to remove first.
