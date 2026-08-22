using System.Text.RegularExpressions;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

public class PlaygroundCatalogParityTests
{
    private static readonly Regex NodePattern = new(
        """new\(\s*"(?<first>[^"]+)"(?:\s*,\s*"(?<label>[^"]+)"\s*,\s*\[(?<keywords>[^\]]*)\])?""",
        RegexOptions.Singleline
    );

    [Fact]
    public void BlazorTemplates_PlaygroundCatalogs_ShareCategoryAndEntryOrder()
    {
        var wasm = Fingerprint(ReadCatalog("wasm", "CleanArchBlazorWasm"));
        var server = Fingerprint(ReadCatalog("server", "CleanArchBlazorServer"));

        Assert.Equal(5, wasm.Count(l => l.StartsWith("category ", StringComparison.Ordinal)));
        Assert.True(wasm.Count >= 30, $"Only {wasm.Count} catalog nodes parsed.");
        Assert.Equal(wasm, server);
    }

    private static string ReadCatalog(string flavor, string projectPrefix)
    {
        var path = Path.Combine(
            TemplatePackHarness.TemplatesRoot,
            flavor,
            "src",
            $"{projectPrefix}.Web",
            "Features",
            "Playground",
            "PlaygroundCatalog.cs"
        );
        Assert.True(File.Exists(path), $"Expected {path} to exist.");
        return File.ReadAllText(path);
    }

    private static IReadOnlyList<string> Fingerprint(string source)
    {
        return NodePattern
            .Matches(source)
            .Select(match =>
            {
                var first = match.Groups["first"].Value;
                if (!first.StartsWith("/playground/", StringComparison.Ordinal))
                {
                    return $"category {first}";
                }

                var keywords = string.Join(
                    ",",
                    Regex
                        .Matches(match.Groups["keywords"].Value, "\"([^\"]*)\"")
                        .Select(m => m.Groups[1].Value)
                );
                return $"  entry {first}|{match.Groups["label"].Value}|{keywords}";
            })
            .ToList();
    }
}
