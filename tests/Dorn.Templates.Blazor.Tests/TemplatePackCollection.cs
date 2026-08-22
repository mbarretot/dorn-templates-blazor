using Xunit;

namespace Dorn.Templates.Blazor.Tests;

[CollectionDefinition(Name)]
public sealed class TemplatePackCollection : ICollectionFixture<TemplatePackFixture>
{
    public const string Name = "TemplatePack";
}

public sealed class TemplatePackFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await TemplatePackHarness.InstallAsync("Dorn.Templates.BlazorWasm");
        await TemplatePackHarness.InstallAsync("Dorn.Templates.BlazorServer");
    }

    public async Task DisposeAsync()
    {
        await TemplatePackHarness.UninstallAsync("Dorn.Templates.BlazorWasm");
        await TemplatePackHarness.UninstallAsync("Dorn.Templates.BlazorServer");
    }
}
