using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Xunit;

namespace Dorn.Templates.Blazor.BrowserTests;

// Other tests only exercise default template parameters; Aspire, Clean Architecture, and Palette
// variants are otherwise only checked via `dotnet build`, which doesn't catch runtime/rendering
// regressions. These hosts are generated with --IncludeCleanArchitecture true --Palette Ocean.
[Collection(BrowserHostFixture.CollectionName)]
public sealed class NonDefaultParametersBrowserTests(BrowserHostFixture fixture)
{
    [Theory]
    [InlineData("wasm-nondefault")]
    [InlineData("server-nondefault")]
    public async Task NonDefaultHost_boots_andPassesAccessibilityChecks(string host)
    {
        await using var context = await fixture.Browser.NewContextAsync(
            new() { ColorScheme = ColorScheme.Dark, DeviceScaleFactor = 1 }
        );
        var page = await context.NewPageAsync();
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        page.SetDefaultTimeout(5000);
        await page.GotoAsync(fixture.Url(host));

        await page.GetByText("Dorn Blazor Templates").WaitForAsync();
        await page.GetByTestId("theme-mode-toggle").WaitForAsync();
        await page.EvaluateAsync("document.fonts.ready");

        Assert.True((await page.ScreenshotAsync(new() { FullPage = true })).Length > 0);
        Assert.Empty((await page.RunAxe()).Violations);
    }
}
