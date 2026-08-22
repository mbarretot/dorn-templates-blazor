namespace CleanArchBlazorServer.Web.Components.Ui.Select;

public sealed class SelectGroupContext
{
    public required Action NotifyStateChanged { get; init; }

    public string? LabelId { get; private set; }

    public void RegisterLabel(string id)
    {
        if (LabelId is not null && LabelId != id)
        {
            throw new InvalidOperationException("A SelectGroup supports only one active label.");
        }

        LabelId = id;
        NotifyStateChanged();
    }

    public void UnregisterLabel(string id)
    {
        if (LabelId != id)
        {
            return;
        }

        LabelId = null;
        NotifyStateChanged();
    }
}
