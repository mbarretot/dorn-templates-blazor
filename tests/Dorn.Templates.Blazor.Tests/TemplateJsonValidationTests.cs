using Xunit;

namespace Dorn.Templates.Blazor.Tests;

// --dry-run resolves symbols/sources without writing or building, catching a malformed
// template.json fast instead of only failing indirectly in a full generate-and-build cycle.
[Trait("Category", "Integration")]
[Collection(TemplatePackCollection.Name)]
public class TemplateJsonValidationTests
{
    [Fact]
    public async Task DornBlazorServerTemplate_DryRun_Succeeds()
    {
        var result = await TemplatePackHarness.GenerateAsync(
            "dorn-blazor-server",
            "DryRunValidationServerApp",
            Path.Combine(BuildSupport.RealTempRoot, $"dorn-dryrun-server-{Guid.NewGuid():N}"),
            "--dry-run"
        );

        Assert.True(
            result.ExitCode == 0,
            $"dotnet new --dry-run failed (exit {result.ExitCode})."
                + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
        );
    }

    [Fact]
    public async Task DornBlazorWasmTemplate_DryRun_Succeeds()
    {
        var result = await TemplatePackHarness.GenerateAsync(
            "dorn-blazor-wasm",
            "DryRunValidationWasmApp",
            Path.Combine(BuildSupport.RealTempRoot, $"dorn-dryrun-wasm-{Guid.NewGuid():N}"),
            "--dry-run"
        );

        Assert.True(
            result.ExitCode == 0,
            $"dotnet new --dry-run failed (exit {result.ExitCode})."
                + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
        );
    }
}
