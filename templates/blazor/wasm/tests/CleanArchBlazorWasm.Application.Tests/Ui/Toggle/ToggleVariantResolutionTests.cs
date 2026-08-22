using CleanArchBlazorWasm.Web.Components.Ui.Toggle;
using Xunit;
using ToggleComponent = CleanArchBlazorWasm.Web.Components.Ui.Toggle.Toggle;

namespace CleanArchBlazorWasm.Application.Tests.Ui.Toggle;

public class ToggleVariantResolutionTests
{
    [Theory]
    [InlineData(ToggleVariant.Default, "bg-transparent")]
    [InlineData(ToggleVariant.Outline, "border-input")]
    public void ResolveClass_EachVariant_ContainsItsSemanticToken(
        ToggleVariant variant,
        string expectedToken
    )
    {
        var result = ToggleComponent.ResolveClass(variant, ToggleSize.Default, null);

        Assert.Contains(expectedToken, result);
    }

    [Theory]
    [InlineData(ToggleSize.Sm, "h-8")]
    [InlineData(ToggleSize.Default, "h-9")]
    [InlineData(ToggleSize.Lg, "h-10")]
    public void ResolveClass_EachSize_ContainsItsHeightToken(ToggleSize size, string expectedToken)
    {
        var result = ToggleComponent.ResolveClass(ToggleVariant.Default, size, null);

        Assert.Contains(expectedToken, result);
    }

    [Fact]
    public void ResolveClass_ConsumerClass_IsAppended()
    {
        var result = ToggleComponent.ResolveClass(
            ToggleVariant.Default,
            ToggleSize.Default,
            "custom-class"
        );

        Assert.Contains("custom-class", result);
    }
}
