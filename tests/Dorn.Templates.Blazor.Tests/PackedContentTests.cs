using System.IO.Compression;
using System.Xml.Linq;
using Xunit;

namespace Dorn.Templates.Blazor.Tests;

// Shared pack paths collide under Windows file locking when xUnit runs collections concurrently.
[Collection(TemplatePackCollection.Name)]
public class PackedContentTests
{
    [Theory]
    [InlineData("Dorn.Templates.BlazorWasm")]
    [InlineData("Dorn.Templates.BlazorServer")]
    public async Task PackedContent_ContainsTemplateConfigDotfilesAndBranding(string packageId)
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
            Assert.True(File.Exists(Path.Combine(extractDirectory, "README.md")));
            Assert.True(File.Exists(Path.Combine(extractDirectory, "dorn-icon.jpg")));
            Assert.True(
                File.Exists(
                    Path.Combine(extractDirectory, "content", ".template.config", "icon.png")
                )
            );
            Assert.True(File.Exists(Path.Combine(extractDirectory, "content", "README.md")));
            Assert.True(
                File.Exists(
                    Path.Combine(extractDirectory, "content", "docs", "assets", "dorn-icon.jpg")
                )
            );

            var nuspecPath = Directory.GetFiles(extractDirectory, "*.nuspec").Single();
            var metadata = XDocument
                .Load(nuspecPath)
                .Root!.Elements()
                .Single(x => x.Name.LocalName == "metadata");
            Assert.Equal(
                "dorn-icon.jpg",
                metadata.Elements().Single(x => x.Name.LocalName == "icon").Value
            );
            Assert.Equal(
                "README.md",
                metadata.Elements().Single(x => x.Name.LocalName == "readme").Value
            );
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }
}
