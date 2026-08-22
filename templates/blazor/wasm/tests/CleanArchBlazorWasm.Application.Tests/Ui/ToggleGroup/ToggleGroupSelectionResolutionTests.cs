using Xunit;
using ToggleGroupComponent = CleanArchBlazorWasm.Web.Components.Ui.ToggleGroup.ToggleGroup<string>;

namespace CleanArchBlazorWasm.Application.Tests.Ui.ToggleGroup;

public class ToggleGroupSelectionResolutionTests
{
    [Fact]
    public void ResolveNextValue_Single_SelectsClickedItem_WhenNoneSelected()
    {
        var result = ToggleGroupComponent.ResolveNextValue(null, "bold");

        Assert.Equal("bold", result);
    }

    [Fact]
    public void ResolveNextValue_Single_DeselectsClickedItem_WhenAlreadySelected()
    {
        var result = ToggleGroupComponent.ResolveNextValue("bold", "bold");

        Assert.Null(result);
    }

    [Fact]
    public void ResolveNextValue_Single_ReplacesSelection_WhenDifferentItemClicked()
    {
        var result = ToggleGroupComponent.ResolveNextValue("bold", "italic");

        Assert.Equal("italic", result);
    }

    [Fact]
    public void ResolveNextValues_Multiple_AddsItem_WhenNotPresent()
    {
        var result = ToggleGroupComponent.ResolveNextValues(["bold"], "italic");

        Assert.Equal(["bold", "italic"], result);
    }

    [Fact]
    public void ResolveNextValues_Multiple_RemovesItem_WhenAlreadyPresent()
    {
        var result = ToggleGroupComponent.ResolveNextValues(["bold", "italic"], "bold");

        Assert.Equal(["italic"], result);
    }
}
