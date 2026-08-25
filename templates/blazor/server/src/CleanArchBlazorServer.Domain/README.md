# CleanArchBlazorServer.Domain

The innermost layer: entities, value objects, and domain logic that describes the business, not the framework.

**Rule**: zero dependencies on any other project or third-party package — not even the BCL beyond `System.*`. Enforced by `Domain_Should_DependOnNothingButBcl` and `Domain_ShouldNot_DependOnOuterLayers` in `CleanArchBlazorServer.Application.Tests/Architecture/CleanArchitectureLayeringTests.cs`.

This library ships empty on purpose. Add your first entity here when you have one — there is nothing to remove first.
