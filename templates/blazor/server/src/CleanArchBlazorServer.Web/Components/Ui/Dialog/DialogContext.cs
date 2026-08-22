namespace CleanArchBlazorServer.Web.Components.Ui.Dialog;

// RequestOpen/RequestClose is the single path every dismissal source funnels through, so OnOpenChange fires exactly once.
public sealed class DialogContext
{
    public bool IsOpen { get; internal set; }

    public required string TitleId { get; init; }

    public required string DescriptionId { get; init; }

    public required Func<Task> RequestOpen { get; init; }

    public required Func<Task> RequestClose { get; init; }
}
