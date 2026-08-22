using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorServer.Web.Components.Ui.HoverCard;

// Cascaded by HoverCard; owns the shared close-grace timer so the pointer can travel from the trigger into the card without closing it.
public sealed class HoverCardContext
{
    private CancellationTokenSource? _closeCts;

    public bool IsOpen { get; internal set; }

    public int OpenDelay { get; internal set; }

    public int CloseDelay { get; internal set; }

    public string Side { get; internal set; } = "bottom";

    public string Align { get; internal set; } = "center";

    public double Offset { get; internal set; }

    public double CollisionPadding { get; internal set; }

    public ElementReference TriggerElement { get; internal set; }

    public required string TriggerId { get; init; }

    public required string ContentId { get; init; }

    public required Func<Task> RequestOpen { get; init; }

    public required Func<Task> RequestClose { get; init; }

    public required Func<Func<Task>, Task> InvokeAsync { get; init; }

    public required Action NotifyStateChanged { get; init; }

    internal void CancelPendingClose()
    {
        _closeCts?.Cancel();
        _closeCts?.Dispose();
        _closeCts = null;
    }

    internal async Task ScheduleCloseAsync()
    {
        CancelPendingClose();
        var cts = new CancellationTokenSource();
        _closeCts = cts;

        try
        {
            if (CloseDelay > 0)
            {
                await Task.Delay(CloseDelay, cts.Token);
            }

            if (!cts.Token.IsCancellationRequested)
            {
                await InvokeAsync(RequestClose);
            }
        }
        catch (TaskCanceledException)
        {
            // Superseded by a re-enter (pointer travelled into the card) or another close request — expected.
        }
    }
}
