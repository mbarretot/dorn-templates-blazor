using Xunit;

namespace Dorn.Templates.Blazor.Tests;

// A malformed template.json (bad symbols/sources entry) is otherwise only caught indirectly,
// when a full generate-and-build cycle happens to fail. --dry-run asks the template engine to
// resolve symbols/sources and list what it would create without writing anything or running
// restore/build, so it fails fast and close to the actual mistake.
[Trait("Category", "Integration")]
public class TemplateJsonValidationTests : IAsyncLifetime
{
    public Task InitializeAsync() => TemplatePackInstallation.EnsureInstalledAsync();

    public Task DisposeAsync() => Task.CompletedTask;

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
