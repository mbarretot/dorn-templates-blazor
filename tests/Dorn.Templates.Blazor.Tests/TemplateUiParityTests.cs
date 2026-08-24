using Xunit;

namespace Dorn.Templates.Blazor.Tests;

public class TemplateUiParityTests
{
    [Fact]
    public void FoundationContracts_AreNormalizedAcrossHosts()
    {
        Assert.Equal(
            Normalize(ReadWebFile("wasm", "CleanArchBlazorWasm", "Styles", "app.tailwind.css")),
            Normalize(ReadWebFile("server", "CleanArchBlazorServer", "Styles", "app.tailwind.css"))
        );
        Assert.Equal(
            ReadWebFile(
                "wasm",
                "CleanArchBlazorWasm",
                "Components",
                "Theme",
                "ThemeSwitcher.razor"
            ),
            ReadWebFile(
                "server",
                "CleanArchBlazorServer",
                "Components",
                "Theme",
                "ThemeSwitcher.razor"
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
    public void HostDocument_RestoresZoom_AndBootsThemeBeforeCss(
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
        var stylesheetPosition = document.IndexOf("app.css", StringComparison.Ordinal);

        Assert.True(themeBootPosition >= 0, "Expected theme-boot to be present.");
        Assert.True(stylesheetPosition >= 0, "Expected app.css to be present.");
        Assert.True(
            themeBootPosition < stylesheetPosition,
            "theme-boot must appear before app.css."
        );
    }

    [Fact]
    public void FoundationStyles_ExposeSemanticTypographyElevationFocusAndMotionTokens()
    {
        var styles = ReadWebFile("wasm", "CleanArchBlazorWasm", "Styles", "app.tailwind.css");

        Assert.Contains("--font-sans:", styles, StringComparison.Ordinal);
        Assert.Contains("--shadow-elevation", styles, StringComparison.Ordinal);
        Assert.Contains("--ui-focus-ring", styles, StringComparison.Ordinal);
        Assert.Contains(":focus-visible", styles, StringComparison.Ordinal);
        Assert.Contains("prefers-reduced-motion", styles, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("slate")]
    [InlineData("rose")]
    [InlineData("neutral")]
    [InlineData("linear")]
    [InlineData("primer")]
    [InlineData("lightning")]
    public void ThemeStyles_ExposeAaSafeFoundationAliases(string theme)
    {
        var wasm = ReadWebFile("wasm", "CleanArchBlazorWasm", "Styles", "themes", $"{theme}.css");
        var server = ReadWebFile(
            "server",
            "CleanArchBlazorServer",
            "Styles",
            "themes",
            $"{theme}.css"
        );

        Assert.Equal(wasm, server);
        Assert.Contains("--ui-font-sans:", wasm, StringComparison.Ordinal);
        Assert.Contains("--ui-shadow-elevation:", wasm, StringComparison.Ordinal);
        Assert.Contains("--ui-focus-ring: var(--ui-ring);", wasm, StringComparison.Ordinal);
        Assert.Contains("--ui-motion-duration:", wasm, StringComparison.Ordinal);
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

    private static string Normalize(string source) =>
        source.Replace(
            "@source \"../wwwroot/index.html\";\n",
            string.Empty,
            StringComparison.Ordinal
        );
}
