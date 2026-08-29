using System.Diagnostics;
using Dorn.Templates.Blazor.TestSupport;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

[Collection(TemplatePackCollection.Name)]
public class GeneratedHostLaunchTests
{
    [Theory]
    [InlineData("dorn-blazor-wasm", "GeneratedWasmHost")]
    [InlineData("dorn-blazor-server", "GeneratedServerHost")]
    public async Task Generated_template_host_launches_on_loopback(string shortName, string name)
    {
        var outputRoot = OperatingSystem.IsMacOS() ? "/private/tmp" : Path.GetTempPath();
        var outputDirectory = Path.Combine(outputRoot, $"dorn-host-{Guid.NewGuid():N}");
        Process? process = null;
        try
        {
            var generated = await TemplatePackHarness.GenerateAsync(
                shortName,
                name,
                outputDirectory
            );
            Assert.True(generated.ExitCode == 0, generated.StdErr);

            var project = Directory
                .GetFiles(outputDirectory, "*.Web.csproj", SearchOption.AllDirectories)
                .Single();
            Assert.True(
                File.Exists(Path.Combine(Path.GetDirectoryName(project)!, "_Imports.razor")),
                "Generated host must preserve _Imports.razor."
            );

            (_, process) = await GeneratedHostReadiness.StartWithPortRetryAsync(port =>
                CreateStartInfo(project, port)
            );
        }
        finally
        {
            if (process is not null)
            {
                await StopAsync(process);
                process.Dispose();
            }
            await DeleteWithRetryAsync(outputDirectory);
        }
    }

    private static ProcessStartInfo CreateStartInfo(string project, int port)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(project);
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add($"http://127.0.0.1:{port}");
        return startInfo;
    }

    private static async Task StopAsync(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
        await process.WaitForExitAsync();
    }

    private static async Task DeleteWithRetryAsync(string path)
    {
        // Windows can keep a brief file-system lock on the killed host's exe/dlls even after
        // WaitForExitAsync returns, so an immediate recursive delete intermittently throws
        // UnauthorizedAccessException/IOException. Retry with backoff instead of failing the test.
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
                return;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                if (attempt == maxAttempts)
                {
                    throw;
                }
                await Task.Delay(attempt * 200);
            }
        }
    }
}
