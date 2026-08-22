namespace Dorn.Templates.Blazor.Tests;

internal static class BuildSupport
{
    private const string DornToolsHomeEnvironmentVariableName = "DORN_TOOLS_HOME";

    /// <summary>
    /// On macOS, <see cref="Path.GetTempPath"/> returns a path through the <c>/var</c> ->
    /// <c>/private/var</c> symlink. Passing that symlinked absolute path as a nested
    /// <c>dotnet build</c> project argument makes the Razor component source generator resolve
    /// it to the canonical <c>/private/...</c> form while restore artifacts stay keyed to the
    /// symlinked form, breaking folder-based namespace computation for <c>Components/**</c>
    /// (manifests as a spurious CS0234 on generated <c>_Imports.razor</c>). Resolving once via
    /// an actual chdir/getcwd round-trip keeps every path used by these tests canonical.
    /// </summary>
    public static readonly string RealTempRoot = ResolveRealPath(Path.GetTempPath());

    private static string ResolveRealPath(string path)
    {
        var original = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(path);
            return Directory.GetCurrentDirectory();
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
        }
    }

    public static async Task<(int ExitCode, string StdOut, string StdErr)> RunDotnetBuildAsync(
        string solutionPath,
        string toolsHome
    )
    {
        var environment = new Dictionary<string, string?>
        {
            [DornToolsHomeEnvironmentVariableName] = toolsHome,
        };

        var restoreResult = await RestoreWithRetryAsync(solutionPath, environment);
        if (restoreResult.ExitCode != 0)
        {
            return restoreResult;
        }

        return await TemplatePackHarness.RunProcessAsync(
            Path.GetDirectoryName(solutionPath)!,
            environment,
            "build",
            solutionPath,
            "-c",
            "Release",
            "--no-restore",
            "-nodeReuse:false"
        );
    }

    /// <summary>
    /// Retries restore only for the known concurrent generated-file race; other failures return immediately.
    /// </summary>
    private static async Task<(int ExitCode, string StdOut, string StdErr)> RestoreWithRetryAsync(
        string solutionPath,
        Dictionary<string, string?> environment,
        int maxAttempts = 3
    )
    {
        (int ExitCode, string StdOut, string StdErr) result = (1, "", "");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            result = await TemplatePackHarness.RunProcessAsync(
                Path.GetDirectoryName(solutionPath)!,
                environment,
                "restore",
                solutionPath,
                "-nodeReuse:false",
                "-maxCpuCount:1"
            );

            if (result.ExitCode == 0)
            {
                return result;
            }

            var isKnownRace =
                result.StdOut.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                || result.StdErr.Contains("already exists", StringComparison.OrdinalIgnoreCase);
            if (!isKnownRace || attempt == maxAttempts)
            {
                return result;
            }
        }

        return result;
    }

    // Windows can briefly hold a handle on the just-exited Tailwind CLI process.
    public static async Task DeleteDirectoryWithRetryAsync(string path)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 5)
            {
                await Task.Delay(200);
            }
        }
    }
}
