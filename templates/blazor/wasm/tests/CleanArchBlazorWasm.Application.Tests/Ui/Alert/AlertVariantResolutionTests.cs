using CleanArchBlazorWasm.Web.Components.Ui.Alert;
using Xunit;
using AlertComponent = CleanArchBlazorWasm.Web.Components.Ui.Alert.Alert;

namespace CleanArchBlazorWasm.Application.Tests.Ui.Alert;

public class AlertVariantResolutionTests
{
    [Theory]
    [InlineData(AlertVariant.Default, "bg-background")]
    [InlineData(AlertVariant.Destructive, "text-destructive")]
    [InlineData(AlertVariant.Success, "text-success")]
    [InlineData(AlertVariant.Warning, "text-warning")]
    public void ResolveClass_EachVariant_ContainsItsSemanticToken(
        AlertVariant variant,
        string expectedToken
    )
    {
        var result = AlertComponent.ResolveClass(variant, null);

        Assert.Contains(expectedToken, result);
    }

    [Fact]
    public void ResolveClass_ConsumerClass_OverridesConflictingBaseUtility()
    {
        var result = AlertComponent.ResolveClass(AlertVariant.Default, "bg-secondary");

        Assert.Contains("bg-secondary", result);
        Assert.DoesNotContain("bg-background", result.Split(' '));
    }
}
