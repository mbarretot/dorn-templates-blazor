using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

/// <summary>
/// Phase 1 go/no-go: proves the Tailwind standalone CLI can be acquired, checksum-verified,
/// and invoked from MSBuild to produce real CSS output, on every CI runner. Threat-matrix
/// cases build <c>CleanArchBlazorWasm.Web.csproj</c> directly (not through <c>dotnet new</c>)
/// since they exercise <c>build/Tailwind.targets</c> itself, not template substitution.
/// </summary>
[Trait("Category", "Integration")]
[Collection(TemplatePackCollection.Name)]
public class BlazorWasmTemplateGenerationTests
{
    private const string DornToolsHomeEnvironmentVariableName = "DORN_TOOLS_HOME";
    private const string DornTailwindPathEnvironmentVariableName = "DORN_TAILWIND_PATH";

    [Fact]
    public async Task GenerateAndBuild_DornBlazorWasmTemplate_ProducesRealTailwindCss()
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

            var appCssPath = Path.Combine(
                outputDirectory,
                "src",
                "DornIntegrationTestBlazorWasmApp.Web",
                "wwwroot",
                "app.css"
            );
            Assert.True(File.Exists(appCssPath), $"Expected generated CSS at '{appCssPath}'.");

            var appCss = await File.ReadAllTextAsync(appCssPath);
            Assert.False(string.IsNullOrWhiteSpace(appCss));
            Assert.Contains("bg-primary", appCss, StringComparison.Ordinal);
            Assert.Contains("--ui-primary", appCss, StringComparison.Ordinal);
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

    /// <summary>
    /// Phase 2 go/no-go for the theming slice: <c>--theme rose</c> must replace the single
    /// literal <c>DEFAULT_THEME</c> seed in <c>theme-boot.js</c> and must not corrupt
    /// <c>Styles/themes/slate.css</c>'s own <c>data-ui-theme='slate'</c> selector, proving
    /// the single-quote convention keeps the two from colliding.
    /// </summary>
    [Fact]
    public async Task GenerateWithThemeRose_ReplacesBootDefaultThemeLiteral_WithoutCorruptingSlateCss()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-theme-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornThemeRoseApp",
                outputDirectory,
                "--Theme",
                "rose"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var themeBootPath = Path.Combine(
                outputDirectory,
                "src",
                "DornThemeRoseApp.Web",
                "wwwroot",
                "theme-boot.js"
            );
            Assert.True(File.Exists(themeBootPath), $"Expected boot script at '{themeBootPath}'.");

            var themeBoot = await File.ReadAllTextAsync(themeBootPath);
            Assert.Contains("DEFAULT_THEME = \"rose\"", themeBoot, StringComparison.Ordinal);
            Assert.DoesNotContain("DEFAULT_THEME = \"slate\"", themeBoot, StringComparison.Ordinal);

            var slateThemePath = Path.Combine(
                outputDirectory,
                "src",
                "DornThemeRoseApp.Web",
                "Styles",
                "themes",
                "slate.css"
            );
            var slateTheme = await File.ReadAllTextAsync(slateThemePath);
            Assert.Contains("[data-ui-theme='slate']", slateTheme, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
            }
        }
    }

    /// <summary>
    /// Each of <c>--Theme neutral|linear|primer|lightning</c> must replace <c>theme-boot.js</c>'s
    /// literal (mirrors <c>GenerateWithThemeRose...</c> above) and must not corrupt the other 5
    /// themes' own <c>data-ui-theme</c> selectors.
    /// </summary>
    [Theory]
    [InlineData("neutral")]
    [InlineData("linear")]
    [InlineData("primer")]
    [InlineData("lightning")]
    public async Task GenerateWithNewTheme_ReplacesBootDefaultThemeLiteral_WithoutCorruptingOtherThemeCss(
        string theme
    )
    {
        var projectName = $"DornTheme{theme}App";
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-theme-{theme}-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                projectName,
                outputDirectory,
                "--Theme",
                theme
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var webRoot = Path.Combine(outputDirectory, "src", $"{projectName}.Web");
            var themeBootPath = Path.Combine(webRoot, "wwwroot", "theme-boot.js");
            Assert.True(File.Exists(themeBootPath), $"Expected boot script at '{themeBootPath}'.");

            var themeBoot = await File.ReadAllTextAsync(themeBootPath);
            Assert.Contains($"DEFAULT_THEME = \"{theme}\"", themeBoot, StringComparison.Ordinal);
            Assert.DoesNotContain("DEFAULT_THEME = \"slate\"", themeBoot, StringComparison.Ordinal);

            foreach (var otherTheme in ExpectedThemes.Values)
            {
                var otherThemeCssPath = Path.Combine(
                    webRoot,
                    "Styles",
                    "themes",
                    $"{otherTheme}.css"
                );
                Assert.True(
                    File.Exists(otherThemeCssPath),
                    $"Expected theme stylesheet at '{otherThemeCssPath}'."
                );
                var otherThemeCss = await File.ReadAllTextAsync(otherThemeCssPath);
                Assert.Contains(
                    $"[data-ui-theme='{otherTheme}']",
                    otherThemeCss,
                    StringComparison.Ordinal
                );
            }
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
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

    /// <summary>
    /// Phase 7 go/no-go for the playground toggle: <c>IncludePlayground=false</c> must exclude
    /// <c>Features/Playground/**</c> entirely, rename <c>NavMenu.Lean.razor</c> (not
    /// <c>NavMenu.Playground.razor</c>) to the canonical <c>NavMenu.razor</c>, and still build
    /// cleanly — proving the lean project is a real, buildable output.
    /// </summary>
    [Fact]
    public async Task GenerateWithIncludePlaygroundFalse_ExcludesPlaygroundAndRenamesLeanNavMenu_AndBuilds()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-lean-{Guid.NewGuid():N}"
        );
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-lean-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornLeanBlazorWasmApp",
                outputDirectory,
                "--IncludePlayground",
                "false"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var webRoot = Path.Combine(outputDirectory, "src", "DornLeanBlazorWasmApp.Web");
            var playgroundDir = Path.Combine(webRoot, "Features", "Playground");
            Assert.False(
                Directory.Exists(playgroundDir),
                $"Expected no playground directory at '{playgroundDir}'."
            );

            var layoutDir = Path.Combine(webRoot, "Components", "Layout");
            var navMenuPath = Path.Combine(layoutDir, "NavMenu.razor");
            Assert.True(File.Exists(navMenuPath), $"Expected renamed NavMenu at '{navMenuPath}'.");
            Assert.False(
                File.Exists(Path.Combine(layoutDir, "NavMenu.Lean.razor")),
                "NavMenu.Lean.razor must be renamed away, not left alongside NavMenu.razor."
            );
            Assert.False(
                File.Exists(Path.Combine(layoutDir, "NavMenu.Playground.razor")),
                "NavMenu.Playground.razor must be excluded when IncludePlayground=false."
            );

            var navMenu = await File.ReadAllTextAsync(navMenuPath);
            Assert.DoesNotContain("/playground", navMenu, StringComparison.Ordinal);

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
            Assert.DoesNotContain("RZ10012", buildResult.StdOut, StringComparison.Ordinal);
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

    /// <summary>Aspire opt-in toggle: default generation must NOT include AppHost.</summary>
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

    /// <summary>Aspire opt-in toggle: --IncludeAspire true must include AppHost.</summary>
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
    public async Task GenerateAndVerify_DornBlazorWasmTemplate_WiresWave2ToastSelectDropdownMenu()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-wave2-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornWave2BlazorWasmApp",
                outputDirectory
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var webRoot = Path.Combine(outputDirectory, "src", "DornWave2BlazorWasmApp.Web");

            var program = await File.ReadAllTextAsync(Path.Combine(webRoot, "Program.cs"));
            Assert.Contains("AddScoped<ToastStore>()", program, StringComparison.Ordinal);

            var mainLayout = await File.ReadAllTextAsync(
                Path.Combine(webRoot, "Components", "Layout", "MainLayout.razor")
            );
            Assert.Contains("<ToastHost />", mainLayout, StringComparison.Ordinal);

            var playgroundLayout = await File.ReadAllTextAsync(
                Path.Combine(webRoot, "Features", "Playground", "PlaygroundLayout.razor")
            );
            Assert.Contains("@layout MainLayout", playgroundLayout, StringComparison.Ordinal);
            Assert.DoesNotContain("<ToastHost />", playgroundLayout, StringComparison.Ordinal);

            var selectDir = Path.Combine(webRoot, "Components", "Ui", "Select");
            Assert.True(File.Exists(Path.Combine(selectDir, "SelectGroup.razor")));
            Assert.True(File.Exists(Path.Combine(selectDir, "SelectLabel.razor")));

            var dropdownMenuDir = Path.Combine(webRoot, "Components", "Ui", "DropdownMenu");
            foreach (
                var file in new[]
                {
                    "DropdownMenuSub.razor",
                    "DropdownMenuSubTrigger.razor",
                    "DropdownMenuSubContent.razor",
                    "DropdownMenuCheckboxItem.razor",
                    "DropdownMenuRadioGroup.razor",
                    "DropdownMenuRadioItem.razor",
                }
            )
            {
                Assert.True(
                    File.Exists(Path.Combine(dropdownMenuDir, file)),
                    $"Expected generated '{file}' under '{dropdownMenuDir}'."
                );
            }

            var catalog = await File.ReadAllTextAsync(
                Path.Combine(webRoot, "Features", "Playground", "PlaygroundCatalog.cs")
            );
            Assert.Contains("/playground/toast", catalog, StringComparison.Ordinal);
            Assert.Contains("\"submenu\"", catalog, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
            }
        }
    }

    [Fact]
    public async Task GenerateWithIncludePlaygroundFalse_StillWiresToastHostInMainLayout()
    {
        var outputDirectory = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-wave2-lean-{Guid.NewGuid():N}"
        );
        try
        {
            var result = await TemplatePackHarness.GenerateAsync(
                "dorn-blazor-wasm",
                "DornWave2LeanBlazorWasmApp",
                outputDirectory,
                "--IncludePlayground",
                "false"
            );

            Assert.True(
                result.ExitCode == 0,
                $"Template generation failed (exit {result.ExitCode})."
                    + $"{Environment.NewLine}STDOUT:{Environment.NewLine}{result.StdOut}"
                    + $"{Environment.NewLine}STDERR:{Environment.NewLine}{result.StdErr}"
            );

            var webRoot = Path.Combine(outputDirectory, "src", "DornWave2LeanBlazorWasmApp.Web");
            var mainLayout = await File.ReadAllTextAsync(
                Path.Combine(webRoot, "Components", "Layout", "MainLayout.razor")
            );
            Assert.Contains("<ToastHost />", mainLayout, StringComparison.Ordinal);

            Assert.False(
                Directory.Exists(Path.Combine(webRoot, "Features", "Playground")),
                "Expected no playground directory when IncludePlayground=false."
            );
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(outputDirectory);
            }
        }
    }

    /// <summary>
    /// Threat matrix: a wrong expected checksum must fail the build with the mismatch message
    /// and must never leave an executable behind in the tool cache.
    /// </summary>
    [Fact]
    public async Task Build_WithWrongExpectedTailwindChecksum_FailsWithMismatchError_AndLeavesCacheEmpty()
    {
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var webCsprojPath = ResolveWebCsprojPath();
            var buildResult = await TemplatePackHarness.RunProcessAsync(
                Path.GetDirectoryName(webCsprojPath)!,
                new Dictionary<string, string?>
                {
                    [DornToolsHomeEnvironmentVariableName] = toolsHome,
                },
                "build",
                webCsprojPath,
                "-c",
                "Release",
                "-nodeReuse:false",
                "-p:TailwindSha256=0000000000000000000000000000000000000000000000000000000000000000"
            );

            Assert.NotEqual(0, buildResult.ExitCode);
            Assert.Contains(
                "checksum mismatch",
                buildResult.StdOut,
                StringComparison.OrdinalIgnoreCase
            );

            if (Directory.Exists(toolsHome))
            {
                var leftoverExecutables = Directory.GetFiles(
                    toolsHome,
                    "tailwindcss*",
                    SearchOption.AllDirectories
                );
                Assert.Empty(leftoverExecutables);
            }
        }
        finally
        {
            if (Directory.Exists(toolsHome))
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(toolsHome);
            }
        }
    }

    /// <summary>
    /// Threat matrix: an unmapped RID must fail with the override instruction instead of
    /// silently downloading an arbitrary asset.
    /// </summary>
    [Fact]
    public async Task Build_WithUnmappedTailwindRid_FailsWithOverrideInstruction()
    {
        var toolsHome = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-tools-{Guid.NewGuid():N}"
        );
        try
        {
            var webCsprojPath = ResolveWebCsprojPath();
            var buildResult = await TemplatePackHarness.RunProcessAsync(
                Path.GetDirectoryName(webCsprojPath)!,
                new Dictionary<string, string?>
                {
                    [DornToolsHomeEnvironmentVariableName] = toolsHome,
                },
                "build",
                webCsprojPath,
                "-c",
                "Release",
                "-nodeReuse:false",
                "-p:DornTailwindRidOverride=bogus-unmapped-rid"
            );

            Assert.NotEqual(0, buildResult.ExitCode);
            Assert.Contains("DORN_TAILWIND_PATH", buildResult.StdOut, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(toolsHome))
            {
                await BuildSupport.DeleteDirectoryWithRetryAsync(toolsHome);
            }
        }
    }

    /// <summary>
    /// Threat matrix: <c>DORN_TAILWIND_PATH</c> pointing at a missing file must fail the build
    /// with an actionable message, not an opaque exec error.
    /// </summary>
    [Fact]
    public async Task Build_WithDornTailwindPathPointingAtMissingFile_FailsWithActionableMessage()
    {
        var missingPath = Path.Combine(
            BuildSupport.RealTempRoot,
            $"dorn-tests-blazor-wasm-missing-{Guid.NewGuid():N}.exe"
        );

        var webCsprojPath = ResolveWebCsprojPath();
        var buildResult = await TemplatePackHarness.RunProcessAsync(
            Path.GetDirectoryName(webCsprojPath)!,
            new Dictionary<string, string?>
            {
                [DornTailwindPathEnvironmentVariableName] = missingPath,
            },
            "build",
            webCsprojPath,
            "-c",
            "Release",
            "-nodeReuse:false"
        );

        Assert.NotEqual(0, buildResult.ExitCode);
        Assert.Contains("DORN_TAILWIND_PATH", buildResult.StdOut, StringComparison.Ordinal);
        Assert.Contains(missingPath, buildResult.StdOut, StringComparison.Ordinal);
    }

    /// <summary>Drift guard: every RID mapped in <c>Tailwind.targets</c> carries a real, non-placeholder SHA-256.</summary>
    [Fact]
    public void TailwindTargets_EveryMappedRid_HasNonPlaceholderChecksum()
    {
        var targetsPath = Path.Combine(
            TemplatePackHarness.TemplatesRoot,
            "wasm",
            "build",
            "Tailwind.targets"
        );
        Assert.True(File.Exists(targetsPath), $"Expected {targetsPath} to exist.");

        var contents = File.ReadAllText(targetsPath);
        var assetNameMatches = Regex.Matches(
            contents,
            @"<TailwindAssetName Condition=""'\$\(TailwindRid\)' == '([^']+)'"""
        );
        Assert.NotEmpty(assetNameMatches);

        foreach (Match match in assetNameMatches)
        {
            var rid = match.Groups[1].Value;
            var shaMatch = Regex.Match(
                contents,
                $@"<TailwindSha256 Condition=""'\$\(TailwindRid\)' == '{Regex.Escape(rid)}'""\s*>\s*([0-9a-fA-F]+)\s*</TailwindSha256>"
            );
            Assert.True(shaMatch.Success, $"Expected a TailwindSha256 entry for RID '{rid}'.");
            var hash = shaMatch.Groups[1].Value;
            Assert.Equal(64, hash.Length);
            Assert.False(
                hash.All(c => c == '0'),
                $"RID '{rid}' has a placeholder (all-zero) checksum."
            );
        }
    }

    /// <summary>No template file may hardcode a raw Tailwind palette class; theming flows through <c>--ui-*</c> tokens only.</summary>
    [Fact]
    public void TemplateFiles_ContainNoRawTailwindPaletteClass()
    {
        var wasmRoot = Path.Combine(TemplatePackHarness.TemplatesRoot, "wasm");
        Assert.True(Directory.Exists(wasmRoot));

        var paletteClassPattern = new Regex(
            @"\b(?:bg|text|border|ring|from|via|to|fill|stroke|divide|outline|decoration|caret|accent)-(?:slate|rose)-\d{2,3}\b"
        );
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".razor",
            ".css",
            ".cs",
            ".html",
        };

        var offendingFiles = new List<string>();
        foreach (var file in Directory.EnumerateFiles(wasmRoot, "*", SearchOption.AllDirectories))
        {
            if (!extensions.Contains(Path.GetExtension(file)))
                continue;

            var text = File.ReadAllText(file);
            if (paletteClassPattern.IsMatch(text))
            {
                offendingFiles.Add(Path.GetRelativePath(wasmRoot, file));
            }
        }

        Assert.Empty(offendingFiles);
    }

    private static string ResolveWebCsprojPath()
    {
        return Path.Combine(
            TemplatePackHarness.TemplatesRoot,
            "wasm",
            "src",
            "CleanArchBlazorWasm.Web",
            "CleanArchBlazorWasm.Web.csproj"
        );
    }
}
