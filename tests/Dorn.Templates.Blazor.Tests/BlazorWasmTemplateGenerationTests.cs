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

    [Fact]
    public async Task GenerateWithDefaultParameters_ToDoFeatureIsFlatVerticalSlice_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-todoflat-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-todoflat-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornToDoFlatBlazorWasmApp",
                outputDirectory
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var srcRoot = Path.Combine(outputDirectory, "src");
            var todoFeatureDir = Path.Combine(
                srcRoot,
                "DornToDoFlatBlazorWasmApp.Web",
                "Features",
                "ToDo"
            );

            Assert.True(
                File.Exists(Path.Combine(todoFeatureDir, "ToDoList.razor")),
                "Features/ToDo must contain ToDoList.razor by default (no arguments)."
            );
            Assert.True(
                File.Exists(Path.Combine(todoFeatureDir, "ToDoItem.cs")),
                "Features/ToDo must contain the flat ToDoItem.cs by default."
            );
            Assert.True(
                File.Exists(Path.Combine(todoFeatureDir, "IToDoRepository.cs")),
                "Features/ToDo must contain the flat IToDoRepository.cs by default."
            );
            Assert.True(
                File.Exists(Path.Combine(todoFeatureDir, "ToDoRepository.cs")),
                "Features/ToDo must contain the flat ToDoRepository.cs by default."
            );

            foreach (var layer in new[] { "Domain", "Application", "Infrastructure" })
            {
                Assert.False(
                    Directory.Exists(Path.Combine(srcRoot, $"DornToDoFlatBlazorWasmApp.{layer}")),
                    $"{layer} project must not exist on a default generate (no --IncludeCleanArchitecture)."
                );
            }

            var razorContent = await File.ReadAllTextAsync(
                Path.Combine(todoFeatureDir, "ToDoList.razor")
            );
            Assert.DoesNotContain("#if", razorContent, StringComparison.Ordinal);
            Assert.DoesNotContain("#else", razorContent, StringComparison.Ordinal);
            Assert.DoesNotContain("#endif", razorContent, StringComparison.Ordinal);

            var programContent = await File.ReadAllTextAsync(
                Path.Combine(srcRoot, "DornToDoFlatBlazorWasmApp.Web", "Program.cs")
            );
            Assert.DoesNotContain("#if", programContent, StringComparison.Ordinal);
            Assert.DoesNotContain("#else", programContent, StringComparison.Ordinal);
            Assert.DoesNotContain("#endif", programContent, StringComparison.Ordinal);

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
    public async Task GenerateWithIncludeCleanArchitectureTrue_ToDoUsesClassLibraries_AndExcludesFlatFiles()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-todocleanarch-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-todocleanarch-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornToDoCleanArchBlazorWasmApp",
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
            var appName = "DornToDoCleanArchBlazorWasmApp";

            Assert.True(
                File.Exists(Path.Combine(srcRoot, $"{appName}.Domain", "Entities", "ToDoItem.cs")),
                "Domain/Entities/ToDoItem.cs must exist when IncludeCleanArchitecture is true."
            );
            Assert.True(
                File.Exists(
                    Path.Combine(
                        srcRoot,
                        $"{appName}.Application",
                        "Interfaces",
                        "IToDoRepository.cs"
                    )
                ),
                "Application/Interfaces/IToDoRepository.cs must exist when IncludeCleanArchitecture is true."
            );
            Assert.True(
                File.Exists(
                    Path.Combine(srcRoot, $"{appName}.Infrastructure", "ToDos", "ToDoRepository.cs")
                ),
                "Infrastructure/ToDos/ToDoRepository.cs must exist when IncludeCleanArchitecture is true."
            );

            var todoFeatureDir = Path.Combine(srcRoot, $"{appName}.Web", "Features", "ToDo");
            Assert.True(
                File.Exists(Path.Combine(todoFeatureDir, "ToDoList.razor")),
                "Features/ToDo/ToDoList.razor must still exist when IncludeCleanArchitecture is true."
            );
            Assert.False(
                File.Exists(Path.Combine(todoFeatureDir, "ToDoItem.cs")),
                "Flat ToDoItem.cs must not exist when IncludeCleanArchitecture is true."
            );
            Assert.False(
                File.Exists(Path.Combine(todoFeatureDir, "IToDoRepository.cs")),
                "Flat IToDoRepository.cs must not exist when IncludeCleanArchitecture is true."
            );
            Assert.False(
                File.Exists(Path.Combine(todoFeatureDir, "ToDoRepository.cs")),
                "Flat ToDoRepository.cs must not exist when IncludeCleanArchitecture is true."
            );

            var razorContent = await File.ReadAllTextAsync(
                Path.Combine(todoFeatureDir, "ToDoList.razor")
            );
            Assert.DoesNotContain("#if", razorContent, StringComparison.Ordinal);
            Assert.DoesNotContain("#else", razorContent, StringComparison.Ordinal);
            Assert.DoesNotContain("#endif", razorContent, StringComparison.Ordinal);

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
            $"DornMatrixWasm{(includeAspire ? "Aspire" : "NoAspire")}{(includeTests ? "Tests" : "NoTests")}{(includeCleanArchitecture ? "CleanArch" : "NoCleanArch")}App";
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
    public async Task GenerateWithIncludeCleanArchitectureTrue_IncludesLibrariesAndReferences_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-cleanarch-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-cleanarch-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornCleanArchBlazorWasmApp",
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
                var layerDir = Path.Combine(srcRoot, $"DornCleanArchBlazorWasmApp.{layer}");
                Assert.True(Directory.Exists(layerDir), $"{layer} project must be included.");
            }

            var archTestsDir = Path.Combine(
                outputDirectory,
                "tests",
                "DornCleanArchBlazorWasmApp.Application.Tests",
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
            $"dorn-tests-blazor-wasm-nocleanarch-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-nocleanarch-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornNoCleanArchBlazorWasmApp",
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
                    Directory.Exists(Path.Combine(srcRoot, $"DornNoCleanArchBlazorWasmApp.{layer}")),
                    $"{layer} project must be excluded when IncludeCleanArchitecture is false (default)."
                );
            }

            var archTestsDir = Path.Combine(
                outputDirectory,
                "tests",
                "DornNoCleanArchBlazorWasmApp.Application.Tests",
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

    [Theory]
    [InlineData("Terracotta")]
    [InlineData("Ocean")]
    [InlineData("Forest")]
    [InlineData("Sunset")]
    [InlineData("Lavender")]
    [InlineData("Slate")]
    [InlineData("Citrus")]
    public async Task GenerateWithPalette_Builds(string palette)
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-palette-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-palette-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                $"DornPalette{palette}BlazorWasmApp",
                outputDirectory,
                "--Palette",
                palette
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
    public async Task GenerateWithDefaultParameters_ExcludesAuthPages()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-noauth-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornNoAuthBlazorWasmApp",
                outputDirectory
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var webProjectDir = Path.Combine(
                outputDirectory,
                "src",
                "DornNoAuthBlazorWasmApp.Web"
            );

            Assert.False(
                File.Exists(
                    Path.Combine(
                        webProjectDir,
                        "Components",
                        "Auth",
                        "LocalStorageAuthStateProvider.cs"
                    )
                ),
                "LocalStorageAuthStateProvider.cs must be excluded when IncludeAuth is false (default)."
            );
            Assert.False(
                File.Exists(Path.Combine(webProjectDir, "Features", "Auth", "Login.razor")),
                "Login.razor must be excluded when IncludeAuth is false (default)."
            );
            Assert.False(
                File.Exists(Path.Combine(webProjectDir, "Features", "Auth", "Secure.razor")),
                "Secure.razor must be excluded when IncludeAuth is false (default)."
            );
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
    public async Task GenerateWithIncludeAuthTrue_IncludesLoginAndSecurePages_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-auth-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-auth-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornAuthBlazorWasmApp",
                outputDirectory,
                "--IncludeAuth",
                "true"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var webProjectDir = Path.Combine(
                outputDirectory,
                "src",
                "DornAuthBlazorWasmApp.Web"
            );

            Assert.True(
                File.Exists(
                    Path.Combine(
                        webProjectDir,
                        "Components",
                        "Auth",
                        "LocalStorageAuthStateProvider.cs"
                    )
                ),
                "LocalStorageAuthStateProvider.cs must be included when IncludeAuth is true."
            );
            Assert.True(
                File.Exists(Path.Combine(webProjectDir, "Features", "Auth", "Login.razor")),
                "Login.razor must be included when IncludeAuth is true."
            );
            Assert.True(
                File.Exists(Path.Combine(webProjectDir, "Features", "Auth", "Secure.razor")),
                "Secure.razor must be included when IncludeAuth is true."
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
