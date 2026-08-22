using CleanArchBlazorServer.Web.Components.Ui.Badge;
using Xunit;
using BadgeComponent = CleanArchBlazorServer.Web.Components.Ui.Badge.Badge;

namespace CleanArchBlazorServer.Application.Tests.Ui.Badge;

public class BadgeVariantResolutionTests
{
    [Theory]
    [InlineData(BadgeVariant.Default, "bg-primary")]
    [InlineData(BadgeVariant.Secondary, "bg-secondary")]
    [InlineData(BadgeVariant.Destructive, "bg-destructive")]
    [InlineData(BadgeVariant.Outline, "text-foreground")]
    [InlineData(BadgeVariant.Success, "bg-success")]
    [InlineData(BadgeVariant.Warning, "bg-warning")]
    public void ResolveClass_EachVariant_ContainsItsSemanticToken(
        BadgeVariant variant,
        string expectedToken
    )
    {
        var result = BadgeComponent.ResolveClass(variant, null);

        Assert.Contains(expectedToken, result);
    }

    [Fact]
    public void ResolveClass_ConsumerClass_OverridesConflictingBaseUtility()
    {
        var result = BadgeComponent.ResolveClass(BadgeVariant.Default, "bg-secondary");

        Assert.Contains("bg-secondary", result);
        Assert.DoesNotContain("bg-primary", result.Split(' '));
    }
}
