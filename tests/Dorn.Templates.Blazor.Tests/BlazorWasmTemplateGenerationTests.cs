using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

[Trait("Category", "Integration")]
[Collection(TemplatePackCollection.Name)]
public class BlazorWasmTemplateGenerationTests
{
    [Fact]
    public async Task GenerateAndBuild_DornBlazorWasmTemplate_VendorsMudBlazorAssets()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornIntegrationTestBlazorWasmApp",
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
            Assert.Equal("DornIntegrationTestBlazorWasmApp.slnx", Path.GetFileName(slnFiles[0]));

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
                "DornIntegrationTestBlazorWasmApp.Web"
            );
            var endpointsManifestPath = Directory
                .GetFiles(
                    webProjectDir,
                    "*.staticwebassets.endpoints.json",
                    SearchOption.AllDirectories
                )
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
            "wasm",
            "src",
            "CleanArchBlazorWasm.AppHost",
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
    public async Task GenerateWithDefaultIncludeAspire_ExcludesAppHost_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-noaspire-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-noaspire-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornNoAspireBlazorWasmApp",
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
                Directory.Exists(Path.Combine(srcRoot, "DornNoAspireBlazorWasmApp.AppHost")),
                "AppHost must be excluded when IncludeAspire is false (default)."
            );

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
            var slnContent = await File.ReadAllTextAsync(slnFiles[0]);
            Assert.DoesNotContain("AppHost", slnContent, StringComparison.Ordinal);

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
    public async Task GenerateWithIncludeAspireTrue_IncludesAppHost_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-aspire-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-aspire-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornAspireBlazorWasmApp",
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
                Directory.Exists(Path.Combine(srcRoot, "DornAspireBlazorWasmApp.AppHost")),
                "AppHost must be included when IncludeAspire is true."
            );

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
            var slnContent = await File.ReadAllTextAsync(slnFiles[0]);
            Assert.Contains("AppHost", slnContent, StringComparison.Ordinal);

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
    public async Task GenerateWithIncludeTestsFalse_SlnxHasNoTestProjectEntries_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-notests-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-notests-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornNoTestsBlazorWasmApp",
                outputDirectory,
                "--IncludeTests",
                "false"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            Assert.False(
                Directory.Exists(Path.Combine(outputDirectory, "tests")),
                "tests/ must be excluded from the file system when IncludeTests is false."
            );

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
            var slnContent = await File.ReadAllTextAsync(slnFiles[0]);
            Assert.DoesNotContain(".Tests.csproj", slnContent, StringComparison.Ordinal);

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
    public async Task GenerateWithDefaultParameters_FeaturesHomeFolder_HasNoLayeringSubfolders()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-defaultshape-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornDefaultShapeBlazorWasmApp",
                outputDirectory
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var homeFeatureDir = Path.Combine(
                outputDirectory,
                "src",
                "DornDefaultShapeBlazorWasmApp.Web",
                "Features",
                "Home"
            );
            Assert.True(Directory.Exists(homeFeatureDir), $"Expected {homeFeatureDir} to exist.");
            Assert.True(
                File.Exists(Path.Combine(homeFeatureDir, "Home.razor")),
                "Features/Home must contain Home.razor."
            );

            foreach (var layer in new[] { "Domain", "Application", "Infrastructure" })
            {
                Assert.False(
                    Directory.Exists(Path.Combine(homeFeatureDir, layer)),
                    $"Features/Home must not have a {layer} subfolder on a default generate; "
                        + "layering subfolders are opt-in per feature, not scaffolded by default."
                );
            }
        }
        finally
        {
            if (
                Environment.GetEnvironmentVariable("DORN_TEST_KEEP_TEMP") != "true"
                && Directory.Exists(outputDirectory)
            )
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
            }
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task GenerateMatrix_SlnxEntriesMatchFileSystem(
        bool includeAspire,
        bool includeTests
    )
    {
        var name =
            $"DornMatrixWasm{(includeAspire ? "Aspire" : "NoAspire")}{(includeTests ? "Tests" : "NoTests")}App";
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-matrix-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                name,
                outputDirectory,
                "--IncludeAspire",
                includeAspire.ToString(),
                "--IncludeTests",
                includeTests.ToString()
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
            var slnContent = await File.ReadAllTextAsync(slnFiles[0]);

            Assert.Equal(includeAspire, slnContent.Contains("AppHost", StringComparison.Ordinal));
            Assert.Equal(
                includeTests,
                slnContent.Contains(".Tests.csproj", StringComparison.Ordinal)
            );

            var document = XDocument.Parse(slnContent);
            var projectPaths = document
                .Descendants("Project")
                .Select(project => project.Attribute("Path")!.Value)
                .ToList();
            Assert.NotEmpty(projectPaths);
            foreach (var projectPath in projectPaths)
            {
                var fullPath = Path.Combine(
                    outputDirectory,
                    projectPath.Replace('/', Path.DirectorySeparatorChar)
                );
                Assert.True(
                    File.Exists(fullPath),
                    $"Referenced project '{projectPath}' does not exist on disk."
                );
            }
        }
        finally
        {
            if (
                Environment.GetEnvironmentVariable("DORN_TEST_KEEP_TEMP") != "true"
                && Directory.Exists(outputDirectory)
            )
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
            }
        }
    }
}
