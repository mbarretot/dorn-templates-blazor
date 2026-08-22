using System.Text.Json;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

/// <summary>Drift guard: both templates' <c>template.json</c> Theme choices must exactly match each other and <see cref="ExpectedThemes"/>.</summary>
public class TemplateJsonThemeChoicesTests
{
    [Fact]
    public void TemplateJsonThemeChoices_ExactlyMatch_ExpectedThemesAndEachOther()
    {
        var wasmChoices = ReadThemeChoices("wasm");
        var serverChoices = ReadThemeChoices("server");

        Assert.Equal(ExpectedThemes.Values, wasmChoices);
        Assert.Equal(ExpectedThemes.Values, serverChoices);
        Assert.Equal(wasmChoices, serverChoices);
    }

    private static string[] ReadThemeChoices(string flavor)
    {
        var templateJsonPath = Path.Combine(
            TemplatePackHarness.TemplatesRoot,
            flavor,
            ".template.config",
            "template.json"
        );
        Assert.True(File.Exists(templateJsonPath), $"Expected {templateJsonPath} to exist.");

        using var templateJson = JsonDocument.Parse(File.ReadAllText(templateJsonPath));
        return templateJson
            .RootElement.GetProperty("symbols")
            .GetProperty("Theme")
            .GetProperty("choices")
            .EnumerateArray()
            .Select(choice => choice.GetProperty("choice").GetString()!)
            .ToArray();
    }
}
