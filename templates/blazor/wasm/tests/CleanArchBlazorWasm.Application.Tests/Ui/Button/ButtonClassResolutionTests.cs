using CleanArchBlazorWasm.Web.Components.Ui.Button;
using Xunit;

namespace CleanArchBlazorWasm.Application.Tests.Ui.Button;

/// <summary>
/// Pure C# resolver (design C1: enum + switch, no cva port) — no renderer needed. Button's own
/// class attribute is produced by calling this exact method, so these tests cover both variant
/// resolution and the "consumer Class wins" contract task 5.1 requires.
/// </summary>
public class ButtonClassResolutionTests
{
    [Theory]
    [InlineData(ButtonVariant.Default, "bg-primary")]
    [InlineData(ButtonVariant.Secondary, "bg-secondary")]
    [InlineData(ButtonVariant.Destructive, "bg-destructive")]
    [InlineData(ButtonVariant.Outline, "border-input")]
    [InlineData(ButtonVariant.Ghost, "hover:bg-accent")]
    [InlineData(ButtonVariant.Link, "underline-offset-4")]
    public void ResolveClass_EachVariant_ContainsItsDistinguishingToken(
        ButtonVariant variant,
        string expectedToken
    )
    {
        var result = CleanArchBlazorWasm.Web.Components.Ui.Button.Button.ResolveClass(
            variant,
            ButtonSize.Default,
            null
        );

        Assert.Contains(expectedToken, result);
    }

    [Theory]
    [InlineData(ButtonSize.Sm, "h-8")]
    [InlineData(ButtonSize.Default, "h-9")]
    [InlineData(ButtonSize.Lg, "h-10")]
    [InlineData(ButtonSize.Icon, "w-9")]
    public void ResolveClass_EachSize_ContainsItsDistinguishingToken(
        ButtonSize size,
        string expectedToken
    )
    {
        var result = CleanArchBlazorWasm.Web.Components.Ui.Button.Button.ResolveClass(
            ButtonVariant.Default,
            size,
            null
        );

        Assert.Contains(expectedToken, result);
    }

    [Fact]
    public void ResolveClass_ConsumerClass_OverridesConflictingBaseUtility()
    {
        var result = CleanArchBlazorWasm.Web.Components.Ui.Button.Button.ResolveClass(
            ButtonVariant.Default,
            ButtonSize.Default,
            "bg-emerald-500"
        );
        var tokens = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Assert.Contains("bg-emerald-500", tokens);
        Assert.DoesNotContain("bg-primary", tokens);
    }
}
