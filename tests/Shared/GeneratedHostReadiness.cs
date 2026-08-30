using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Dorn.Templates.Blazor.TestSupport;

// Shared by GeneratedHostLaunchTests and BrowserHostFixture: both wait for the same thing — a
// freshly launched generated host's first successful response on loopback.
internal static class GeneratedHostReadiness
{
    // 45s: enough margin for a cold `dotnet run`/Kestrel bind on a loaded CI runner.
    public static readonly TimeSpan ReadinessTimeout = TimeSpan.FromSeconds(45);

    private static readonly HttpClient Client = new();

    public static int ReserveLoopbackPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    public static async Task WaitUntilReadyAsync(Process process, Uri url)
    {
        using var timeout = new CancellationTokenSource(ReadinessTimeout);
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

    // Reserving a port, closing the listener, and rebinding it later in a different process is a
    // TOCTOU race: another process on a busy CI runner can claim the same port in the gap. Retry
    // with a freshly reserved port instead of failing the test outright. On final failure, the
    // last attempt's stdout/stderr are folded into the thrown exception (the process is always
    // stopped and disposed here, in every attempt, so no zombie process leaks out).
    public static async Task<(int Port, Process Process)> StartWithPortRetryAsync(
        Func<int, ProcessStartInfo> createStartInfo,
        int maxAttempts = 3
    )
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            var port = ReserveLoopbackPort();
            var process =
                Process.Start(createStartInfo(port))
                ?? throw new InvalidOperationException("Could not start generated host process.");
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            try
            {
                await WaitUntilReadyAsync(process, new Uri($"http://127.0.0.1:{port}/"));
                return (port, process);
            }
            catch (Exception exception)
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
                await process.WaitForExitAsync();

                if (attempt >= maxAttempts)
                {
                    throw new InvalidOperationException(
                        $"{exception.Message}{Environment.NewLine}STDOUT:{Environment.NewLine}{await standardOutput}"
                            + $"{Environment.NewLine}STDERR:{Environment.NewLine}{await standardError}",
                        exception
                    );
                }
                process.Dispose();
            }
        }
    }
}
