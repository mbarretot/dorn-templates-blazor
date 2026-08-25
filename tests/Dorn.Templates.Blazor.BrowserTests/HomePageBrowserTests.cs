using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Xunit;

namespace Dorn.Templates.Blazor.BrowserTests;

[Collection(BrowserHostFixture.CollectionName)]
public sealed class HomePageBrowserTests(BrowserHostFixture fixture)
{
    [Theory]
    [InlineData("wasm", 390, 844)]
    [InlineData("wasm", 1440, 900)]
    [InlineData("server", 390, 844)]
    [InlineData("server", 1440, 900)]
    public async Task Home_boots_andRendersBranding(string host, int width, int height)
    {
        await using var context = await fixture.Browser.NewContextAsync(
            new() { ColorScheme = ColorScheme.Dark, DeviceScaleFactor = 1 }
        );
        var page = await context.NewPageAsync();
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        page.SetDefaultTimeout(5000);
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync(fixture.Url(host));

        await page.GetByText("Dorn Blazor Templates").WaitForAsync();
        await page.GetByTestId("theme-mode-toggle").WaitForAsync();
        await page.EvaluateAsync("document.fonts.ready");

        Assert.Equal(
            "system",
            await page.EvaluateAsync<string>("localStorage.getItem('ui-mode') || 'system'")
        );

        Assert.True(
            await page.EvaluateAsync<double>(
                "performance.getEntriesByType('navigation')[0].duration"
            ) < 10000
        );
        Assert.True((await page.ScreenshotAsync(new() { FullPage = true })).Length > 0);
        Assert.Empty((await page.RunAxe()).Violations);
    }
}
