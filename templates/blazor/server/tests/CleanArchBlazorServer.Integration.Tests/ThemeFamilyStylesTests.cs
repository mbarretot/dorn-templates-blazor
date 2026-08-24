using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace CleanArchBlazorServer.Integration.Tests;

public class ThemeFamilyStylesTests
{
    private static readonly string[] Themes =
    [
        "slate",
        "rose",
        "neutral",
        "linear",
        "primer",
        "lightning",
    ];

    private static readonly (string Foreground, string Background)[] ContrastPairs =
    [
        ("foreground", "background"),
        ("card-foreground", "card"),
        ("primary-foreground", "primary"),
        ("accent-foreground", "accent"),
        ("destructive-foreground", "destructive"),
        ("success-foreground", "success"),
        ("warning-foreground", "warning"),
    ];

    [Fact]
    public void ThemeFamilies_ProvideDistinctTypographySurfaceRadiusDensityAndMotionTokens()
    {
        var fingerprints = new HashSet<string>(StringComparer.Ordinal);

        foreach (var theme in Themes)
        {
            var light = ReadThemeBlock(theme, isDark: false);
            var dark = ReadThemeBlock(theme, isDark: true);

            Assert.NotEmpty(Token(light, "--ui-font-sans"));
            Assert.NotEmpty(Token(light, "--ui-surface"));
            Assert.NotEmpty(Token(light, "--ui-radius"));
            Assert.NotEmpty(Token(light, "--ui-density"));
            Assert.NotEmpty(Token(light, "--ui-shadow-elevation"));
            Assert.NotEmpty(Token(light, "--ui-motion-duration"));
            Assert.NotEmpty(Token(light, "--ui-motion-easing"));
            Assert.NotEmpty(Token(dark, "--ui-background"));

            fingerprints.Add(
                string.Join(
                    "|",
                    Token(light, "--ui-font-sans"),
                    Token(light, "--ui-surface"),
                    Token(light, "--ui-radius"),
                    Token(light, "--ui-density")
                )
            );
        }

        Assert.Equal(Themes.Length, fingerprints.Count);
    }

    [Theory]
    [MemberData(nameof(ThemeModes))]
    public void ThemeColorPairs_MeetWcagAaContrast(string theme, bool isDark)
    {
        var tokens = ReadThemeBlock(theme, isDark);

        foreach (var (foreground, background) in ContrastPairs)
        {
            var ratio = ContrastRatio(
                Token(tokens, $"--ui-{foreground}"),
                Token(tokens, $"--ui-{background}")
            );

            Assert.True(
                ratio >= 4.5,
                $"{theme} {(isDark ? "dark" : "light")} {foreground}/{background} was {ratio:F2}:1."
            );
        }
    }

    [Fact]
    public void ThemeStyles_SwitchByFamilyAndSuppressMotion()
    {
        var appCss = File.ReadAllText(Path.Combine(WebProjectRoot(), "Styles", "app.tailwind.css"));

        Assert.Contains(
            "@media (prefers-reduced-motion: reduce)",
            appCss,
            StringComparison.Ordinal
        );
        Assert.Contains("transition-duration: 0.01ms !important", appCss, StringComparison.Ordinal);

        foreach (var theme in Themes)
        {
            Assert.NotEmpty(ReadThemeBlock(theme, isDark: false));
            Assert.NotEmpty(ReadThemeBlock(theme, isDark: true));
        }
    }

    public static IEnumerable<object[]> ThemeModes() =>
        Themes.SelectMany(theme =>
            new[] { new object[] { theme, false }, new object[] { theme, true } }
        );

    private static string ReadThemeBlock(string theme, bool isDark)
    {
        var css = File.ReadAllText(
            Path.Combine(WebProjectRoot(), "Styles", "themes", $"{theme}.css")
        );
        var selector = $"[data-ui-theme='{theme}']" + (isDark ? "[data-ui-mode='dark']" : "");
        var match = Regex.Match(
            css,
            Regex.Escape(selector) + @"\s*\{(?<block>[^}]*)\}",
            RegexOptions.Singleline
        );

        Assert.True(
            match.Success,
            $"Expected the {theme} {(isDark ? "dark" : "light")} token block."
        );
        return match.Groups["block"].Value;
    }

    private static string Token(string block, string name)
    {
        var match = Regex.Match(block, @"(?m)^\s*" + Regex.Escape(name) + @":\s*(?<value>[^;]+);");

        Assert.True(match.Success, $"Expected {name} in the token block.");
        return match.Groups["value"].Value.Trim();
    }

    private static double ContrastRatio(string first, string second)
    {
        var firstLuminance = RelativeLuminance(first);
        var secondLuminance = RelativeLuminance(second);
        var lighter = Math.Max(firstLuminance, secondLuminance);
        var darker = Math.Min(firstLuminance, secondLuminance);

        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(string color)
    {
        var match = Regex.Match(
            color,
            @"^oklch\((?<lightness>[0-9.]+)\s+(?<chroma>[0-9.]+)\s+(?<hue>[0-9.]+)\)$"
        );

        Assert.True(match.Success, $"Expected an opaque oklch color, received '{color}'.");
        var lightness = double.Parse(match.Groups["lightness"].Value, CultureInfo.InvariantCulture);
        var chroma = double.Parse(match.Groups["chroma"].Value, CultureInfo.InvariantCulture);
        var hue =
            double.Parse(match.Groups["hue"].Value, CultureInfo.InvariantCulture) * Math.PI / 180;
        var a = chroma * Math.Cos(hue);
        var b = chroma * Math.Sin(hue);
        var l = Cube(lightness + 0.3963377774 * a + 0.2158037573 * b);
        var m = Cube(lightness - 0.1055613458 * a - 0.0638541728 * b);
        var s = Cube(lightness - 0.0894841775 * a - 1.2914855480 * b);
        var red = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        var green = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        var blue = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
    }

    private static double Cube(double value) => value * value * value;

    private static string WebProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (
            current is not null
            && !File.Exists(Path.Combine(current.FullName, "CleanArchBlazorServer.slnx"))
        )
        {
            current = current.Parent;
        }

        if (current is null)
        {
            throw new DirectoryNotFoundException("Could not locate the template solution root.");
        }

        return Path.Combine(current.FullName, "src", "CleanArchBlazorServer.Web");
    }
}
