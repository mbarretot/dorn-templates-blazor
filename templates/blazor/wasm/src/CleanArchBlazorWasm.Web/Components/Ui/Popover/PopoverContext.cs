using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorWasm.Web.Components.Ui.Popover;

// Cascaded by Popover so PopoverTrigger/PopoverContent/PopoverClose (siblings) share open state and positioning.
public sealed class PopoverContext
{
    public bool IsOpen { get; internal set; }

    public string Side { get; internal set; } = "bottom";

    public string Align { get; internal set; } = "center";

    public double Offset { get; internal set; }

    public double CollisionPadding { get; internal set; }

    public ElementReference TriggerElement { get; internal set; }

    public required string TriggerId { get; init; }

    public required string ContentId { get; init; }

    public required Func<Task> RequestOpen { get; init; }

    public required Func<Task> RequestClose { get; init; }

    public required Action NotifyStateChanged { get; init; }

    internal async Task FocusTriggerAsync() => await TriggerElement.FocusAsync();
}
