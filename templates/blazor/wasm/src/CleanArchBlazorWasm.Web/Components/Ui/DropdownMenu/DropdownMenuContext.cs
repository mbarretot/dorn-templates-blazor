using Dorn.WebUI.Primitives;
using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorWasm.Web.Components.Ui.DropdownMenu;

public sealed class DropdownMenuContext
{
    private readonly List<(string Id, ElementReference Element, bool Disabled)> _items = [];
    private readonly List<DropdownMenuContext> _openMenus = [];

    public RovingFocusState Focus { get; } = new(RovingFocusOrientation.Vertical, loop: true);

    public bool IsOpen { get; internal set; }

    public ElementReference TriggerElement { get; internal set; }

    public required string TriggerId { get; init; }

    public required string ContentId { get; init; }

    public required Func<Task> RequestOpen { get; init; }

    public required Func<Task> RequestClose { get; init; }

    public required Action NotifyStateChanged { get; init; }

    public DropdownMenuContext? Parent { get; init; }

    public DropdownMenuContext Root => Parent?.Root ?? this;

    public bool RestoreTriggerOnClose { get; private set; } = true;

    internal bool PresentationClosed { get; set; }

    internal Func<Task>? ClosePresentation { get; set; }

    internal void RegisterItem(string id, ElementReference element, bool disabled)
    {
        var index = _items.FindIndex(i => i.Id == id);
        if (index >= 0)
        {
            _items[index] = (id, element, disabled);
        }
        else
        {
            _items.Add((id, element, disabled));
        }

        Focus.WithItems([.. _items.Select(i => (i.Id, i.Disabled))]);
    }

    internal void UnregisterItem(string id)
    {
        _items.RemoveAll(i => i.Id == id);
        Focus.WithItems([.. _items.Select(i => (i.Id, i.Disabled))]);
    }

    internal async Task MoveAsync(string key)
    {
        if (!Focus.HandleKey(key))
        {
            return;
        }

        NotifyStateChanged();
        await FocusActiveItemAsync();
    }

    internal async Task FocusActiveItemAsync()
    {
        if (Focus.ActiveId is null)
        {
            return;
        }

        var item = _items.Find(i => i.Id == Focus.ActiveId);
        await item.Element.FocusAsync();
    }

    internal async Task FocusTriggerAsync() => await TriggerElement.FocusAsync();

    internal void RegisterOpen()
    {
        RestoreTriggerOnClose = true;
        Root._openMenus.Remove(this);
        Root._openMenus.Add(this);
    }

    internal void UnregisterOpen() => Root._openMenus.Remove(this);

    internal async Task CloseCurrentAsync(bool restoreImmediateTrigger)
    {
        RestoreTriggerOnClose = restoreImmediateTrigger;
        await RequestClose();
    }

    internal async Task CloseChainAsync()
    {
        var root = Root;
        foreach (var menu in root._openMenus.AsEnumerable().Reverse().ToArray())
        {
            if (menu.ClosePresentation is not null)
            {
                await menu.ClosePresentation();
            }

            await menu.CloseCurrentAsync(false);
        }

        await root.FocusTriggerAsync();
    }

    internal async Task SelectItemAsync(EventCallback onClick)
    {
        if (onClick.HasDelegate)
        {
            await onClick.InvokeAsync();
        }

        await CloseChainAsync();
    }
}
