using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CleanArchBlazorServer.Integration.Tests;

public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_InProduction_SetsHstsHeader()
    {
        // UseHsts() skips loopback hosts by design, so a non-loopback host is required.
        var client = _factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"))
            .CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://app.example.com/") });

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Get_Root_InDevelopment_DoesNotSetHstsHeader()
    {
        var client = _factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"))
            .CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://app.example.com/") });

        var response = await client.GetAsync("/");

        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Get_Error_ReturnsFriendlyErrorPage()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Error");
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Something went wrong", html, StringComparison.Ordinal);
    }
}
