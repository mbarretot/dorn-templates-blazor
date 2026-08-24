using System.Diagnostics;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

public class ThemeBootSystemModeExecutionTests
{
    [Fact]
    public async Task ThemeBootSystemModeTests_RunDuringDotnetTest()
    {
        var scriptPath = Path.Combine(
            TemplatePackHarness.RepoRoot,
            "tests",
            "Dorn.Templates.Blazor.Tests",
            "ThemeBootSystemModeTests.mjs"
        );
        var startInfo = new ProcessStartInfo("node")
        {
            WorkingDirectory = TemplatePackHarness.RepoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("--test");
        startInfo.ArgumentList.Add(scriptPath);

        using var process =
            Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the Node test runner.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        Assert.True(
            process.ExitCode == 0,
            $"Node theme boot tests failed (exit {process.ExitCode}).{Environment.NewLine}"
                + $"STDOUT:{Environment.NewLine}{await standardOutput}"
                + $"STDERR:{Environment.NewLine}{await standardError}"
        );
    }
}
