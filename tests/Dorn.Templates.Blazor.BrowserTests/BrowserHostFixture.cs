using System.Diagnostics;
using Dorn.Templates.Blazor.TestSupport;
using Microsoft.Playwright;
using Xunit;

namespace Dorn.Templates.Blazor.BrowserTests;

[CollectionDefinition(CollectionName)]
public sealed class BrowserHostCollection : ICollectionFixture<BrowserHostFixture>
{
    public const string CollectionName = "BrowserHosts";
}

public sealed class BrowserHostFixture : IAsyncLifetime
{
    private readonly List<Host> _hosts = [];
    private readonly string _root = Path.Combine(RealTempRoot, $"dorn-browser-{Guid.NewGuid():N}");
    private IPlaywright? _playwright;

    public const string CollectionName = BrowserHostCollection.CollectionName;
    public IBrowser Browser =>
        _browser ?? throw new InvalidOperationException("Browser is not initialized.");
    private IBrowser? _browser;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_root);
        await InstallTemplatesAsync();
        foreach (
            var template in new (
                string HostName,
                string ShortName,
                string ProjectName,
                string[] ExtraArgs
            )[]
            {
                ("wasm", "dorn-blazor-wasm", "Browserwasm", []),
                ("server", "dorn-blazor-server", "Browserserver", []),
                (
                    "wasm-nondefault",
                    "dorn-blazor-wasm",
                    "BrowserWasmNonDefault",
                    ["--IncludeCleanArchitecture", "true", "--Palette", "Ocean"]
                ),
                (
                    "server-nondefault",
                    "dorn-blazor-server",
                    "BrowserServerNonDefault",
                    ["--IncludeCleanArchitecture", "true", "--Palette", "Ocean"]
                ),
            }
        )
        {
            var output = Path.Combine(_root, template.HostName);
            var newArgs = new List<string>
            {
                "new",
                template.ShortName,
                "-n",
                template.ProjectName,
                "-o",
                output,
            };
            newArgs.AddRange(template.ExtraArgs);
            await Run(_root, [.. newArgs]);
            var project = Directory
                .GetFiles(output, "*.Web.csproj", SearchOption.AllDirectories)
                .Single();
            var (workingDirectory, arguments) =
                template.HostName.StartsWith("server", StringComparison.Ordinal)
                    ? await PublishArgumentsAsync(template.HostName, project)
                    : (output, (string[])["run", "--project", project, "--no-launch-profile"]);
            var host = await Host.StartAsync(template.HostName, workingDirectory, arguments);
            _hosts.Add(host);
        }
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public string Url(string name) => _hosts.Single(host => host.Name == name).Url;

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
        foreach (var host in _hosts)
        {
            await host.DisposeAsync();
        }

        await TryRun(_root, "new", "uninstall", "Dorn.Templates.BlazorWasm");
        await TryRun(_root, "new", "uninstall", "Dorn.Templates.BlazorServer");
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    private async Task<(string WorkingDirectory, string[] Arguments)> PublishArgumentsAsync(
        string name,
        string project
    )
    {
        var publishDir = Path.Combine(_root, $"{name}-publish");
        await Run(_root, "publish", project, "-c", "Release", "-o", publishDir);
        var dll = $"{Path.GetFileNameWithoutExtension(project)}.dll";
        return (publishDir, [dll]);
    }

    private async Task InstallTemplatesAsync()
    {
        foreach (
            var package in new[] { "Dorn.Templates.BlazorWasm", "Dorn.Templates.BlazorServer" }
        )
        {
            await TryRun(_root, "new", "uninstall", package);
            var project = Path.Combine(
                RepositoryRoot,
                "eng",
                "packaging",
                package,
                $"{package}.csproj"
            );
            await Run(
                _root,
                "pack",
                project,
                "-c",
                "Release",
                "-p:PackageVersion=0.0.1-browser",
                "-o",
                _root
            );
            await Run(
                _root,
                "new",
                "install",
                Directory.GetFiles(_root, $"{package}.*.nupkg").Single()
            );
        }
    }

    private static async Task Run(string workingDirectory, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process =
            Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet {string.Join(' ', arguments)} failed.{Environment.NewLine}{await output}{Environment.NewLine}{await error}"
            );
        }
    }

    private static async Task TryRun(string workingDirectory, params string[] arguments)
    {
        try
        {
            await Run(workingDirectory, arguments);
        }
        catch (InvalidOperationException) { }
    }

    private static string RealTempRoot =>
        OperatingSystem.IsMacOS() ? "/private/tmp" : Path.GetTempPath();

    private static string RepositoryRoot
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DornTemplatesBlazor.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate the repository root.");
        }
    }

    private sealed class Host : IAsyncDisposable
    {
        private readonly Process _process;
        public string Name { get; }
        public string Url { get; }

        private Host(string name, Process process, int port)
        {
            Name = name;
            _process = process;
            Url = $"http://127.0.0.1:{port}";
        }

        public static async Task<Host> StartAsync(
            string name,
            string workingDirectory,
            string[] arguments
        )
        {
            var (port, process) = await GeneratedHostReadiness.StartWithPortRetryAsync(port =>
            {
                var start = new ProcessStartInfo("dotnet")
                {
                    WorkingDirectory = workingDirectory,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };
                foreach (var argument in arguments)
                {
                    start.ArgumentList.Add(argument);
                }
                start.ArgumentList.Add("--urls");
                start.ArgumentList.Add($"http://127.0.0.1:{port}");
                return start;
            });
            return new Host(name, process, port);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_process.HasExited)
            {
                _process.Kill(true);
            }

            await _process.WaitForExitAsync();
            _process.Dispose();
        }
    }
}
