using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

[Collection(TemplatePackCollection.Name)]
public class GeneratedHostLaunchTests
{
    private static readonly HttpClient Client = new();

    [Theory]
    [InlineData("dorn-blazor-wasm", "GeneratedWasmHost")]
    [InlineData("dorn-blazor-server", "GeneratedServerHost")]
    public async Task Generated_template_host_launches_on_loopback(string shortName, string name)
    {
        var outputRoot = OperatingSystem.IsMacOS() ? "/private/tmp" : Path.GetTempPath();
        var outputDirectory = Path.Combine(outputRoot, $"dorn-host-{Guid.NewGuid():N}");
        Process? process = null;
        Task<string>? standardOutput = null;
        Task<string>? standardError = null;
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
                File.Exists(
                    Path.Combine(
                        Path.GetDirectoryName(project)!,
                        "Components",
                        "Ui",
                        "_Imports.razor"
                    )
                ),
                "Generated host must preserve Components/Ui/_Imports.razor."
            );
            var port = ReserveLoopbackPort();
            process = Process.Start(CreateStartInfo(project, port));
            Assert.NotNull(process);
            standardOutput = process.StandardOutput.ReadToEndAsync();
            standardError = process.StandardError.ReadToEndAsync();

            await WaitUntilReadyAsync(process, new Uri($"http://127.0.0.1:{port}/"));
        }
        catch (Exception exception) when (process is not null)
        {
            await StopAsync(process);
            throw new Xunit.Sdk.XunitException(
                $"{exception.Message}{Environment.NewLine}STDOUT:{Environment.NewLine}{await standardOutput!}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{await standardError!}"
            );
        }
        finally
        {
            if (process is not null)
            {
                await StopAsync(process);
                process.Dispose();
            }
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
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

    private static async Task WaitUntilReadyAsync(Process process, Uri url)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!timeout.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException("Generated host exited before becoming ready.");
            }
            try
            {
                if ((await Client.GetAsync(url, timeout.Token)).IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException) { }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested) { }
            await Task.Delay(200);
        }
        throw new TimeoutException($"Generated host did not become ready at '{url}'.");
    }

    private static int ReserveLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static async Task StopAsync(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
        }
        await process.WaitForExitAsync();
    }
}
