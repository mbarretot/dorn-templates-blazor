using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Xunit;

namespace Dorn.Templates.Blazor.BrowserTests;

[Collection(BrowserHostFixture.CollectionName)]
public sealed class ObservatoryBrowserTests(BrowserHostFixture fixture)
{
    [Theory]
    [InlineData("wasm", 390, 844)]
    [InlineData("wasm", 1440, 900)]
    [InlineData("server", 390, 844)]
    [InlineData("server", 1440, 900)]
    public async Task Observatory_is_accessible_responsive_and_persistent(
        string host,
        int width,
        int height
    )
    {
        await using var context = await fixture.Browser.NewContextAsync(
            new() { ColorScheme = ColorScheme.Dark }
        );
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(5000);
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync(fixture.Url(host));
        await page.GetByRole(AriaRole.Link, new() { Name = "Playground", Exact = true })
            .ClickAsync();
        await page.GotoAsync($"{fixture.Url(host)}/playground/button");
        await page.GetByTestId("playground-preview").WaitForAsync();
        await page.EvaluateAsync("document.body.style.zoom = '2'");
        await page.Keyboard.PressAsync("Tab");
        Assert.Equal(
            "system",
            await page.EvaluateAsync<string>("localStorage.getItem('ui-mode') || 'system'")
        );
        Assert.Equal("dark", await page.Locator("html").GetAttributeAsync("data-ui-mode"));
        Assert.Empty((await page.RunAxe()).Violations);
        Assert.True(
            await page.EvaluateAsync<double>(
                "performance.getEntriesByType('navigation')[0].duration"
            ) < 10000
        );
        Assert.True((await page.ScreenshotAsync(new() { FullPage = true })).Length > 0);
    }
}
