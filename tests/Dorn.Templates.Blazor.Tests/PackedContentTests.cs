using System.IO.Compression;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

// Same collection as TemplatePackCollection's install fixture: both pack the same csprojs to
// their shared default obj/ path, and concurrent xUnit collections running that in parallel
// collide under Windows' stricter file locking.
[Collection(TemplatePackCollection.Name)]
public class PackedContentTests
{
    [Theory]
    [InlineData("Dorn.Templates.BlazorWasm")]
    [InlineData("Dorn.Templates.BlazorServer")]
    public async Task PackedContent_ContainsTemplateConfigAndDotfiles(string packageId)
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"dorn-templates-blazor-pack-{Guid.NewGuid():N}"
        );
        Directory.CreateDirectory(outputDirectory);
        try
        {
            var nupkgPath = await TemplatePackHarness.PackAsync(packageId, outputDirectory);

            var extractDirectory = Path.Combine(outputDirectory, "extracted");
            await ZipFile.ExtractToDirectoryAsync(nupkgPath, extractDirectory);

            Assert.True(
                File.Exists(
                    Path.Combine(extractDirectory, "content", ".template.config", "template.json")
                )
            );
            Assert.True(
                File.Exists(
                    Path.Combine(extractDirectory, "content", ".config", "dotnet-tools.json")
                )
            );
            Assert.True(
                File.Exists(Path.Combine(extractDirectory, "content", "build", "Tailwind.targets"))
            );
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }
}
