namespace CleanArchBlazorWasm.Web.Components.Ui.Dialog;

/// <summary>
/// Cascades open state and the ids <see cref="DialogTitle"/>/<see cref="DialogDescription"/>
/// render into, so <see cref="DialogContent"/> can wire <c>aria-labelledby</c>/
/// <c>aria-describedby</c> without a child-to-parent round trip (design D, Dialog part).
/// <see cref="RequestOpen"/>/<see cref="RequestClose"/> are the single path every dismissal
/// source — trigger click, close button, or the platform's cancel/outside-click via
/// <see cref="DialogContent.RequestDismissAsync"/> — funnels through, so <c>OnOpenChange</c>
/// fires exactly once regardless of source.
/// </summary>
public sealed class DialogContext
{
    public bool IsOpen { get; internal set; }

    public required string TitleId { get; init; }

    public required string DescriptionId { get; init; }

    public required Func<Task> RequestOpen { get; init; }

    public required Func<Task> RequestClose { get; init; }
}
