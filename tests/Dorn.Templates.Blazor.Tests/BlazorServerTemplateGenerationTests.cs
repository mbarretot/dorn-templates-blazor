using System.Text.Json;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

[Trait("Category", "Integration")]
[Collection(TemplatePackCollection.Name)]
public class BlazorServerTemplateGenerationTests
{
    [Fact]
    public async Task GenerateAndBuild_DornBlazorServerTemplate_VendorsMudBlazorAssets()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornIntegrationTestBlazorServerApp",
                outputDirectory
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
            Assert.Equal("DornIntegrationTestBlazorServerApp.slnx", Path.GetFileName(slnFiles[0]));

            var buildResult = await BuildSupport.RunDotnetBuildAsync(slnFiles[0], toolsHome);

            Assert.True(
                buildResult.ExitCode == 0,
                $"dotnet build exited with {buildResult.ExitCode}."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{buildResult.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{buildResult.StdErr}"
            );

            var webProjectDir = Path.Combine(
                outputDirectory,
                "src",
                "DornIntegrationTestBlazorServerApp.Web"
            );
            var endpointsManifestPath = Directory
                .GetFiles(webProjectDir, "*.staticwebassets.endpoints.json", SearchOption.AllDirectories)
                .SingleOrDefault();
            Assert.NotNull(endpointsManifestPath);

            var endpointsManifest = await File.ReadAllTextAsync(endpointsManifestPath);
            Assert.Contains("MudBlazor.min.css", endpointsManifest, StringComparison.Ordinal);
            Assert.Contains("MudBlazor.min.js", endpointsManifest, StringComparison.Ordinal);
        }
        finally
        {
            if (Environment.GetEnvironmentVariable("DORN_TEST_KEEP_TEMP") != "true")
            {
                if (Directory.Exists(outputDirectory))
                {
                    await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
                }
                if (Directory.Exists(toolsHome))
                {
                    await BuildSupport.DeleteDirectoryWithRetryAsync(toolsHome);
                }
            }
            else
            {
                Console.WriteLine("KEPT: " + outputDirectory);
            }
        }
    }

    [Fact]
    public void AppHostLaunchSettings_ExistsWithHttpAndHttpsProfiles()
    {
        var launchSettingsPath = Path.Combine(
            TemplatePackHarness.TemplatesRoot,
            "server",
            "src",
            "CleanArchBlazorServer.AppHost",
            "Properties",
            "launchSettings.json"
        );
        Assert.True(File.Exists(launchSettingsPath), $"Expected {launchSettingsPath} to exist.");

        using var launchSettings = JsonDocument.Parse(File.ReadAllText(launchSettingsPath));
        var profiles = launchSettings.RootElement.GetProperty("profiles");
        Assert.True(profiles.TryGetProperty("http", out _));
        Assert.True(profiles.TryGetProperty("https", out _));
    }

    [Fact]
    public async Task GenerateWithDefaultIncludeAspire_ExcludesAppHostAndServiceDefaults_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-noaspire-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-noaspire-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornNoAspireBlazorServerApp",
                outputDirectory
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var srcRoot = Path.Combine(outputDirectory, "src");
            Assert.False(
                Directory.Exists(Path.Combine(srcRoot, "DornNoAspireBlazorServerApp.AppHost")),
                "AppHost must be excluded when IncludeAspire is false (default)."
            );
            Assert.False(
                Directory.Exists(
                    Path.Combine(srcRoot, "DornNoAspireBlazorServerApp.ServiceDefaults")
                ),
                "ServiceDefaults must be excluded when IncludeAspire is false (default)."
            );

            var webCsproj = await File.ReadAllTextAsync(
                Path.Combine(
                    srcRoot,
                    "DornNoAspireBlazorServerApp.Web",
                    "DornNoAspireBlazorServerApp.Web.csproj"
                )
            );
            Assert.DoesNotContain("ServiceDefaults", webCsproj, StringComparison.Ordinal);

            var program = await File.ReadAllTextAsync(
                Path.Combine(srcRoot, "DornNoAspireBlazorServerApp.Web", "Program.cs")
            );
            Assert.DoesNotContain("AddServiceDefaults", program, StringComparison.Ordinal);
            Assert.DoesNotContain("MapDefaultEndpoints", program, StringComparison.Ordinal);

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
            var slnContent = await File.ReadAllTextAsync(slnFiles[0]);
            Assert.DoesNotContain("AppHost", slnContent, StringComparison.Ordinal);
            Assert.DoesNotContain("ServiceDefaults", slnContent, StringComparison.Ordinal);

            var buildResult = await BuildSupport.RunDotnetBuildAsync(slnFiles[0], toolsHome);

            Assert.True(
                buildResult.ExitCode == 0,
                $"dotnet build exited with {buildResult.ExitCode}."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{buildResult.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{buildResult.StdErr}"
            );
        }
        finally
        {
            if (Environment.GetEnvironmentVariable("DORN_TEST_KEEP_TEMP") != "true")
            {
                if (Directory.Exists(outputDirectory))
                {
                    await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
                }
                if (Directory.Exists(toolsHome))
                {
                    await BuildSupport.DeleteDirectoryWithRetryAsync(toolsHome);
                }
            }
            else
            {
                Console.WriteLine("KEPT: " + outputDirectory);
            }
        }
    }

    [Fact]
    public async Task GenerateWithIncludeAspireTrue_IncludesAppHostAndServiceDefaults_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-aspire-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-aspire-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornAspireBlazorServerApp",
                outputDirectory,
                "--IncludeAspire",
                "true"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var srcRoot = Path.Combine(outputDirectory, "src");
            Assert.True(
                Directory.Exists(Path.Combine(srcRoot, "DornAspireBlazorServerApp.AppHost")),
                "AppHost must be included when IncludeAspire is true."
            );
            Assert.True(
                Directory.Exists(
                    Path.Combine(srcRoot, "DornAspireBlazorServerApp.ServiceDefaults")
                ),
                "ServiceDefaults must be included when IncludeAspire is true."
            );

            var webCsproj = await File.ReadAllTextAsync(
                Path.Combine(
                    srcRoot,
                    "DornAspireBlazorServerApp.Web",
                    "DornAspireBlazorServerApp.Web.csproj"
                )
            );
            Assert.Contains("ServiceDefaults", webCsproj, StringComparison.Ordinal);

            var program = await File.ReadAllTextAsync(
                Path.Combine(srcRoot, "DornAspireBlazorServerApp.Web", "Program.cs")
            );
            Assert.Contains("AddServiceDefaults();", program, StringComparison.Ordinal);
            Assert.Contains("MapDefaultEndpoints();", program, StringComparison.Ordinal);

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
            var slnContent = await File.ReadAllTextAsync(slnFiles[0]);
            Assert.Contains("AppHost", slnContent, StringComparison.Ordinal);
            Assert.Contains("ServiceDefaults", slnContent, StringComparison.Ordinal);

            var buildResult = await BuildSupport.RunDotnetBuildAsync(slnFiles[0], toolsHome);

            Assert.True(
                buildResult.ExitCode == 0,
                $"dotnet build exited with {buildResult.ExitCode}."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{buildResult.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{buildResult.StdErr}"
            );
        }
        finally
        {
            if (Environment.GetEnvironmentVariable("DORN_TEST_KEEP_TEMP") != "true")
            {
                if (Directory.Exists(outputDirectory))
                {
                    await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
                }
                if (Directory.Exists(toolsHome))
                {
                    await BuildSupport.DeleteDirectoryWithRetryAsync(toolsHome);
                }
            }
            else
            {
                Console.WriteLine("KEPT: " + outputDirectory);
            }
        }
    }
}
