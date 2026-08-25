using System.Text.Json;
using System.Xml.Linq;
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

    [Fact]
    public async Task GenerateWithIncludeTestsFalse_SlnxHasNoTestProjectEntries_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-notests-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-notests-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornNoTestsBlazorServerApp",
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

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public async Task GenerateMatrix_SlnxEntriesMatchFileSystem(
        bool includeAspire,
        bool includeTests,
        bool includeCleanArchitecture
    )
    {
        var name =
            $"DornMatrixServer{(includeAspire ? "Aspire" : "NoAspire")}{(includeTests ? "Tests" : "NoTests")}{(includeCleanArchitecture ? "CleanArch" : "NoCleanArch")}App";
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-matrix-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                name,
                outputDirectory,
                "--IncludeAspire",
                includeAspire.ToString(),
                "--IncludeTests",
                includeTests.ToString(),
                "--IncludeCleanArchitecture",
                includeCleanArchitecture.ToString()
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
                includeAspire,
                slnContent.Contains("ServiceDefaults", StringComparison.Ordinal)
            );
            Assert.Equal(
                includeTests,
                slnContent.Contains(".Tests.csproj", StringComparison.Ordinal)
            );
            Assert.Equal(
                includeCleanArchitecture,
                slnContent.Contains(".Domain.csproj", StringComparison.Ordinal)
            );
            Assert.Equal(
                includeCleanArchitecture,
                slnContent.Contains(".Infrastructure.csproj", StringComparison.Ordinal)
            );

            var webCsprojPath = Directory
                .GetFiles(outputDirectory, "*.Web.csproj", SearchOption.AllDirectories)
                .Single();
            var webCsproj = await File.ReadAllTextAsync(webCsprojPath);
            Assert.Equal(
                includeCleanArchitecture,
                webCsproj.Contains("Application.csproj", StringComparison.Ordinal)
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

    [Fact]
    public async Task GenerateWithDefaultParameters_FeaturesHomeFolder_HasNoLayeringSubfolders()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-defaultshape-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornDefaultShapeBlazorServerApp",
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
                "DornDefaultShapeBlazorServerApp.Web",
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

    [Fact]
    public async Task GenerateWithIncludeCleanArchitectureTrue_IncludesLibrariesAndReferences_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-cleanarch-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-cleanarch-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornCleanArchBlazorServerApp",
                outputDirectory,
                "--IncludeCleanArchitecture",
                "true"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var srcRoot = Path.Combine(outputDirectory, "src");
            foreach (var layer in new[] { "Domain", "Application", "Infrastructure" })
            {
                var layerDir = Path.Combine(srcRoot, $"DornCleanArchBlazorServerApp.{layer}");
                Assert.True(Directory.Exists(layerDir), $"{layer} project must be included.");
                Assert.True(
                    File.Exists(Path.Combine(layerDir, "README.md")),
                    $"{layer} project must ship a README.md."
                );
            }

            var archTestsDir = Path.Combine(
                outputDirectory,
                "tests",
                "DornCleanArchBlazorServerApp.Application.Tests",
                "Architecture"
            );
            Assert.True(
                File.Exists(Path.Combine(archTestsDir, "CleanArchitectureLayeringTests.cs")),
                "Application.Tests must include the Clean Architecture layering rules."
            );

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
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
    public async Task GenerateWithIncludeCleanArchitectureFalse_ExcludesLibraries_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-nocleanarch-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-server-nocleanarch-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-server",
                "DornNoCleanArchBlazorServerApp",
                outputDirectory,
                "--IncludeCleanArchitecture",
                "false"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var srcRoot = Path.Combine(outputDirectory, "src");
            foreach (var layer in new[] { "Domain", "Application", "Infrastructure" })
            {
                Assert.False(
                    Directory.Exists(
                        Path.Combine(srcRoot, $"DornNoCleanArchBlazorServerApp.{layer}")
                    ),
                    $"{layer} project must be excluded when IncludeCleanArchitecture is false (default)."
                );
            }

            var archTestsDir = Path.Combine(
                outputDirectory,
                "tests",
                "DornNoCleanArchBlazorServerApp.Application.Tests",
                "Architecture"
            );
            Assert.False(
                Directory.Exists(archTestsDir),
                "Architecture/ must be excluded from Application.Tests when IncludeCleanArchitecture is false."
            );

            var slnFiles = Directory.GetFiles(
                outputDirectory,
                "*.slnx",
                SearchOption.TopDirectoryOnly
            );
            Assert.Single(slnFiles);
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
