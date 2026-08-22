using Microsoft.AspNetCore.Components.Forms;

namespace CleanArchBlazorServer.Web.Components.Ui.Form;

// Cascaded by FormField so Label/Input/FormMessage share one generated id without a round trip.
public sealed class FieldContext
{
    public required string Id { get; init; }

    public string MessageId => $"{Id}-message";

    public FieldIdentifier FieldIdentifier { get; internal set; }

    public bool HasMessage { get; internal set; }

    // Re-renders FormField's subtree — HasMessage flips in FormMessage but is read by Input.
    public required Action NotifyStateChanged { get; init; }

    internal void SetField(FieldIdentifier fieldIdentifier) => FieldIdentifier = fieldIdentifier;

    internal void SetHasMessage(bool hasMessage)
    {
        if (HasMessage == hasMessage)
        {
            return;
        }

        HasMessage = hasMessage;
        NotifyStateChanged();
    }
}
