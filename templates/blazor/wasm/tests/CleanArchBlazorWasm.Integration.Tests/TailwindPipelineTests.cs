using System.Text.RegularExpressions;
using Xunit;

namespace CleanArchBlazorWasm.Integration.Tests;

/// <summary>
/// Asserts the CSS the build actually produced, not what the source declares (design's stated
/// non-negotiable: without this, a broken pipeline still ships green). The
/// <c>ProjectReference</c> in this project's csproj guarantees <c>DornBuildTailwindCss</c> has
/// already run by the time these tests execute.
/// </summary>
public class TailwindPipelineTests
{
    [Fact]
    public void GeneratedAppCss_ContainsTokenLayerAndNonDefaultThemeSelector()
    {
        var appCss = ReadGeneratedAppCss();

        Assert.True(
            appCss.Length > 5 * 1024,
            $"Expected app.css to exceed 5 KB, was {appCss.Length} bytes."
        );
        Assert.Contains("--ui-background", appCss, StringComparison.Ordinal);

        // Both theme files always ship regardless of the --theme generation choice (design B6),
        // so the non-default theme's selector proves both token sets survived the build, not
        // just the one matching this template's boot default.
        Assert.Matches(new Regex("\\[data-ui-theme=[\"']?rose[\"']?\\]"), appCss);
    }

    [Fact]
    public void GeneratedAppCss_ContainsThemeStatusTokensAndUtilityMappings()
    {
        var appCss = ReadGeneratedAppCss();

        AssertStatusTokens(appCss, "slate", isDark: false);
        AssertStatusTokens(appCss, "slate", isDark: true);
        AssertStatusTokens(appCss, "rose", isDark: false);
        AssertStatusTokens(appCss, "rose", isDark: true);

        Assert.Contains(
            ".bg-success{background-color:var(--ui-success)}",
            appCss,
            StringComparison.Ordinal
        );
        Assert.Contains(
            ".text-success-foreground{color:var(--ui-success-foreground)}",
            appCss,
            StringComparison.Ordinal
        );
        Assert.Contains(
            ".bg-warning{background-color:var(--ui-warning)}",
            appCss,
            StringComparison.Ordinal
        );
        Assert.Contains(
            ".text-warning-foreground{color:var(--ui-warning-foreground)}",
            appCss,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void GeneratedAppCss_ContainsNeutralAndLinearThemeBlocksWithExpectedRadii()
    {
        var appCss = ReadGeneratedAppCss();

        AssertThemeRadius(appCss, "neutral", ".625rem");
        AssertThemeRadius(appCss, "linear", ".5rem");
        AssertThemeBlock(appCss, "neutral", isDark: true);
        AssertThemeBlock(appCss, "linear", isDark: true);
    }

    [Fact]
    public void GeneratedAppCss_ContainsPrimerAndLightningThemeBlocksWithLightOnlyRadii()
    {
        var appCss = ReadGeneratedAppCss();

        AssertThemeRadius(appCss, "primer", ".375rem");
        AssertThemeRadius(appCss, "lightning", ".25rem");
        AssertThemeBlock(appCss, "primer", isDark: true);
        AssertThemeBlock(appCss, "lightning", isDark: true);
        AssertThemeBlockDoesNotContainRadius(appCss, "primer");
        AssertThemeBlockDoesNotContainRadius(appCss, "lightning");
    }

    [Fact]
    public void GeneratedAppCss_ContainsTokenUtilitiesEmittedByComponentsAndPreflightMarker()
    {
        var appCss = ReadGeneratedAppCss();

        Assert.Contains(".bg-primary", appCss, StringComparison.Ordinal);
        Assert.Contains(".rounded-lg", appCss, StringComparison.Ordinal);

        // Stable substring from Tailwind v4's preflight base layer, present regardless of
        // minification — proves preflight (not just utilities) made it into the build.
        Assert.Contains("-webkit-text-size-adjust", appCss, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedAppCss_ExcludesUtilityNoComponentUses()
    {
        var appCss = ReadGeneratedAppCss();

        // Proves @source content scanning actually ran instead of dumping the full utility
        // corpus: nothing in this template uses aspect-video, so it must not appear.
        Assert.DoesNotContain("aspect-video", appCss, StringComparison.Ordinal);
    }

    private static string ReadGeneratedAppCss()
    {
        var appCssPath = ResolveAppCssPath();
        Assert.True(
            File.Exists(appCssPath),
            $"Expected a built stylesheet at '{appCssPath}'. Build the Web project first."
        );

        return File.ReadAllText(appCssPath);
    }

    private static void AssertStatusTokens(string appCss, string theme, bool isDark)
    {
        var modeSelector = isDark ? "\\[data-ui-mode=[\"']?dark[\"']?\\]" : string.Empty;
        var selector = $"\\[data-ui-theme=[\"']?{theme}[\"']?\\]{modeSelector}";
        var tokenBlock = new Regex($"{selector}\\s*\\{{[^}}]*\\}}", RegexOptions.Singleline);
        var match = tokenBlock.Match(appCss);

        Assert.True(
            match.Success,
            $"Expected the {theme} {(isDark ? "dark" : "light")} token block."
        );
        Assert.Contains("--ui-success:", match.Value, StringComparison.Ordinal);
        Assert.Contains("--ui-success-foreground:", match.Value, StringComparison.Ordinal);
        Assert.Contains("--ui-warning:", match.Value, StringComparison.Ordinal);
        Assert.Contains("--ui-warning-foreground:", match.Value, StringComparison.Ordinal);
    }

    private static void AssertThemeRadius(string appCss, string theme, string radius)
    {
        var match = AssertThemeBlock(appCss, theme, isDark: false);

        Assert.Contains($"--ui-radius:{radius}", match.Value, StringComparison.Ordinal);
    }

    private static void AssertThemeBlockDoesNotContainRadius(string appCss, string theme)
    {
        var darkBlock = AssertThemeBlock(appCss, theme, isDark: true);

        Assert.DoesNotContain("--ui-radius:", darkBlock.Value, StringComparison.Ordinal);
    }

    private static Match AssertThemeBlock(string appCss, string theme, bool isDark)
    {
        var modeSelector = isDark ? "\\[data-ui-mode=[\"']?dark[\"']?\\]" : string.Empty;
        var selector = $"\\[data-ui-theme=[\"']?{theme}[\"']?\\]{modeSelector}";
        var tokenBlock = new Regex($"{selector}\\s*\\{{[^}}]*\\}}", RegexOptions.Singleline);
        var match = tokenBlock.Match(appCss);

        Assert.True(
            match.Success,
            $"Expected the {theme} {(isDark ? "dark" : "light")} token block."
        );

        return match;
    }

    private static string ResolveAppCssPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (
            current is not null
            && !File.Exists(Path.Combine(current.FullName, "CleanArchBlazorWasm.slnx"))
        )
        {
            current = current.Parent;
        }

        if (current is null)
        {
            throw new DirectoryNotFoundException(
                "Could not locate the generated solution root (CleanArchBlazorWasm.slnx) by "
                    + $"walking up from '{AppContext.BaseDirectory}'."
            );
        }

        return Path.Combine(
            current.FullName,
            "src",
            "CleanArchBlazorWasm.Web",
            "wwwroot",
            "app.css"
        );
    }
}
