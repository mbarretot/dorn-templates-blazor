using Xunit;

namespace Dorn.Templates.Blazor.Tests;

public class TemplateUiParityTests
{
    [Fact]
    public void FoundationContracts_AreNormalizedAcrossHosts()
    {
        Assert.Equal(
            Normalize(
                ReadWebFile("wasm", "CleanArchBlazorWasm", "Components", "Theme", "AppTheme.cs"),
                "CleanArchBlazorWasm"
            ),
            Normalize(
                ReadWebFile(
                    "server",
                    "CleanArchBlazorServer",
                    "Components",
                    "Theme",
                    "AppTheme.cs"
                ),
                "CleanArchBlazorServer"
            )
        );
        Assert.Equal(
            ReadWebFile("wasm", "CleanArchBlazorWasm", "Components", "Theme", "ThemeToggle.razor"),
            ReadWebFile(
                "server",
                "CleanArchBlazorServer",
                "Components",
                "Theme",
                "ThemeToggle.razor"
            )
        );
        Assert.Equal(
            ReadWebFile("wasm", "CleanArchBlazorWasm", "wwwroot", "theme-boot.js"),
            ReadWebFile("server", "CleanArchBlazorServer", "wwwroot", "theme-boot.js")
        );
    }

    [Theory]
    [InlineData("wasm", "CleanArchBlazorWasm", "wwwroot", "index.html")]
    [InlineData("server", "CleanArchBlazorServer", "Components", "App.razor")]
    public void HostDocument_RestoresZoom_AndBootsThemeBeforeMudBlazorCss(
        string host,
        string project,
        string folder,
        string fileName
    )
    {
        var document = ReadWebFile(host, project, folder, fileName);

        Assert.DoesNotContain("maximum-scale", document, StringComparison.Ordinal);
        Assert.DoesNotContain("user-scalable", document, StringComparison.Ordinal);
        var themeBootPosition = document.IndexOf("theme-boot", StringComparison.Ordinal);
        var stylesheetPosition = document.IndexOf("MudBlazor.min.css", StringComparison.Ordinal);

        Assert.True(themeBootPosition >= 0, "Expected theme-boot to be present.");
        Assert.True(stylesheetPosition >= 0, "Expected MudBlazor.min.css to be present.");
        Assert.True(
            themeBootPosition < stylesheetPosition,
            "theme-boot must appear before MudBlazor.min.css."
        );
    }

    [Theory]
    [InlineData("wasm", "CleanArchBlazorWasm")]
    [InlineData("server", "CleanArchBlazorServer")]
    public void HomeRazor_DoesNotClaimLayeringSeparation(string host, string project)
    {
        var document = ReadWebFile(host, project, "Features", "Home", "Home.razor");

        Assert.DoesNotContain(
            "Domain, Application, Infrastructure",
            document,
            StringComparison.Ordinal
        );
    }

    private static string ReadWebFile(string host, string project, params string[] segments)
    {
        var path = Path.Combine(
            TemplatePackHarness.TemplatesRoot,
            host,
            "src",
            $"{project}.Web",
            Path.Combine(segments)
        );
        Assert.True(File.Exists(path), $"Expected {path} to exist.");
        return File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string Normalize(string source, string projectName) =>
        source.Replace(
            $"namespace {projectName}.Web.Components.Theme;",
            "namespace <Project>.Web.Components.Theme;",
            StringComparison.Ordinal
        );
}
