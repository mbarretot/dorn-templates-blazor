using Xunit;

namespace CleanArchBlazorWasm.Integration.Tests;

/// <summary>
/// Asserts the source root document (<c>wwwroot/index.html</c>) references MudBlazor's own
/// static assets with no CDN dependency (no-CDN, no-network-at-runtime posture).
/// </summary>
public class RootDocumentTests
{
    [Fact]
    public void RootDocument_ReferencesMudBlazorCssAndJs()
    {
        var indexHtmlPath = ResolveWebRootPath("index.html");
        Assert.True(File.Exists(indexHtmlPath), $"Expected the root document at '{indexHtmlPath}'.");

        var markup = File.ReadAllText(indexHtmlPath);
        Assert.Contains("_content/MudBlazor/MudBlazor.min.css", markup, StringComparison.Ordinal);
        Assert.Contains("_content/MudBlazor/MudBlazor.min.js", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("cdn.", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fonts.googleapis.com", markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RootDocument_ReferencesThemeBootScript()
    {
        var indexHtmlPath = ResolveWebRootPath("index.html");
        var markup = File.ReadAllText(indexHtmlPath);

        Assert.Contains("<script src=\"theme-boot.js\"></script>", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void RootDocument_DeclaresBaselineContentSecurityPolicy()
    {
        var indexHtmlPath = ResolveWebRootPath("index.html");
        var markup = File.ReadAllText(indexHtmlPath);

        Assert.Contains(
            "<meta http-equiv=\"Content-Security-Policy\"",
            markup,
            StringComparison.Ordinal
        );
        Assert.Contains("default-src 'self'", markup, StringComparison.Ordinal);
        // Without this, Blazor WebAssembly can't instantiate its compiled WASM modules and the
        // app never renders past the loading shell — a regression that only shows up at runtime.
        Assert.Contains("'wasm-unsafe-eval'", markup, StringComparison.Ordinal);
    }

    private static string ResolveWebRootPath(params string[] relativeSegments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (
            current is not null
            && !File.Exists(Path.Combine(current.FullName, "CleanArchBlazorWasm.slnx"))
        )
        {
            current = current.Parent;
        }

        return current is null
            ? throw new DirectoryNotFoundException(
                "Could not locate the generated solution root (CleanArchBlazorWasm.slnx) by "
                    + $"walking up from '{AppContext.BaseDirectory}'."
            )
            : Path.Combine(
                current.FullName,
                "src",
                "CleanArchBlazorWasm.Web",
                "wwwroot",
                Path.Combine(relativeSegments)
        );
    }
}
