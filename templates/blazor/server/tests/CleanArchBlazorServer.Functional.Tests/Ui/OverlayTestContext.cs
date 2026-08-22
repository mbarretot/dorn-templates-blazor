using Bunit;
using Xunit;

namespace CleanArchBlazorServer.Functional.Tests.Ui;

// Shared by Tooltip/Popover/HoverCard: asserts AnchorInterop.position's Side/Align/Offset/CollisionPadding args.
public abstract class OverlayTestContext : UiTestContext
{
    protected JSRuntimeInvocationHandler SetupPosition() =>
        AnchorModule.SetupVoid("position", _ => true).SetVoidResult();

    protected static void AssertPositionedWith(
        JSRuntimeInvocationHandler position,
        string side,
        string align,
        double offset,
        double collisionPadding
    )
    {
        var invocation = Assert.Single(position.Invocations);
        Assert.Equal(side, invocation.Arguments[2]);
        Assert.Equal(align, invocation.Arguments[3]);
        Assert.Equal(offset, invocation.Arguments[4]);
        Assert.Equal(collisionPadding, invocation.Arguments[5]);
    }
}
