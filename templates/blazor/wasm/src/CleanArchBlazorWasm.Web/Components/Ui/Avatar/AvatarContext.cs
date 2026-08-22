namespace CleanArchBlazorWasm.Web.Components.Ui.Avatar;

// Cascaded by Avatar so AvatarImage/AvatarFallback (siblings) share load-failure state.
public sealed class AvatarContext
{
    public bool ImageFailed { get; internal set; }

    public required Action NotifyStateChanged { get; init; }

    internal void SetImageFailed()
    {
        if (ImageFailed)
        {
            return;
        }

        ImageFailed = true;
        NotifyStateChanged();
    }
}
