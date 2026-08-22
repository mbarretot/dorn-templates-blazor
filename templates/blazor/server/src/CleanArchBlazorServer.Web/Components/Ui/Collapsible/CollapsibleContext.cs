namespace CleanArchBlazorServer.Web.Components.Ui.Collapsible;

// Cascaded by Collapsible so CollapsibleTrigger/CollapsibleContent (siblings) share open state.
public sealed class CollapsibleContext
{
    public bool Open { get; internal set; }

    public bool Disabled { get; internal set; }

    public required string ContentId { get; init; }

    public required Func<Task> ToggleAsync { get; init; }

    public required Action NotifyStateChanged { get; init; }
}
