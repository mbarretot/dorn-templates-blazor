using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CleanArchBlazorServer.Integration.Tests;

/// <summary>
/// Asserts what the build actually produced for the root HTML document, not what the source
/// declares. The fingerprinted-asset reference (design S-T4) and the bare-&lt;html&gt; rule
/// (design S-B1) both fail silently if broken — a build stays green and the page still looks
/// right — so only a rendered-response assertion can catch a regression.
/// </summary>
public class RootDocumentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Regex StylesheetHrefPattern = new(
        "<link rel=\"stylesheet\" href=\"([^\"]*app\\.[a-z0-9]{8,}\\.css)\" ?/?>",
        RegexOptions.IgnoreCase
    );

    private readonly WebApplicationFactory<Program> _factory;

    public RootDocumentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_ReturnsHtmlWithFingerprintedStylesheetLink()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);

        var html = await response.Content.ReadAsStringAsync();
        var match = StylesheetHrefPattern.Match(html);
        Assert.True(
            match.Success,
            $"Expected a fingerprinted app.css href in:{Environment.NewLine}{html}"
        );
    }

    [Fact]
    public async Task FingerprintedStylesheetUrl_ServesRealTailwindCss()
    {
        var client = _factory.CreateClient();

        var rootHtml = await (await client.GetAsync("/")).Content.ReadAsStringAsync();
        var href = StylesheetHrefPattern.Match(rootHtml).Groups[1].Value;

        var cssResponse = await client.GetAsync(href);
        cssResponse.EnsureSuccessStatusCode();
        var css = await cssResponse.Content.ReadAsStringAsync();
        Assert.Contains("--ui-background", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_Root_PlacesClassicThemeBootScriptBeforeStylesheetLink()
    {
        var client = _factory.CreateClient();
        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        var scriptMatch = Regex.Match(
            html,
            "<script[^>]*src=\"[^\"]*theme-boot\\.[a-z0-9]*\\.?js\"[^>]*></script>"
        );
        Assert.True(
            scriptMatch.Success,
            $"Expected a theme-boot.js script tag in:{Environment.NewLine}{html}"
        );
        Assert.DoesNotContain("type=\"module\"", scriptMatch.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("defer", scriptMatch.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("async", scriptMatch.Value, StringComparison.Ordinal);

        var stylesheetMatch = StylesheetHrefPattern.Match(html);
        Assert.True(
            scriptMatch.Index < stylesheetMatch.Index,
            "theme-boot.js must appear before the stylesheet link."
        );
    }

    [Fact]
    public async Task Get_Root_HtmlElementCarriesNoServerEmittedThemeAttributes()
    {
        var client = _factory.CreateClient();
        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        var htmlTagMatch = Regex.Match(html, "<html[^>]*>");
        Assert.True(htmlTagMatch.Success);
        Assert.DoesNotContain("data-ui-theme", htmlTagMatch.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("data-ui-mode", htmlTagMatch.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_Root_ContainsPrerenderedHomeMarkupAndBlazorWebScript()
    {
        var client = _factory.CreateClient();
        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        Assert.Contains("Component Observatory", html, StringComparison.Ordinal);
        Assert.Contains("_framework/blazor.web.js", html, StringComparison.Ordinal);
    }
}
