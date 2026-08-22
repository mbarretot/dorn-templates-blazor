using Dorn.WebUI.Primitives;
using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorWasm.Web.Components.Ui.RadioGroup;

// Selection-follows-focus: MoveAsync moves focus then immediately commits the newly active item's value, unlike ToggleGroupContext where MoveAsync only ever moves focus.
public sealed class RadioGroupContext<TValue>(RovingFocusOrientation orientation)
{
    private readonly List<(
        string Id,
        ElementReference Element,
        TValue Value,
        bool Disabled
    )> _items = [];

    public RovingFocusState Focus { get; } = new(orientation, loop: true);

    public TValue? Value { get; internal set; }

    public required Action NotifyStateChanged { get; init; }

    public required Func<TValue, Task> CommitValueAsync { get; init; }

    public bool IsSelected(TValue value) => EqualityComparer<TValue>.Default.Equals(Value, value);

    internal void RegisterItem(string id, ElementReference element, TValue value, bool disabled)
    {
        var index = _items.FindIndex(i => i.Id == id);
        var entry = (id, element, value, disabled);
        if (index >= 0)
        {
            _items[index] = entry;
        }
        else
        {
            _items.Add(entry);
        }

        var hadActiveId = Focus.ActiveId is not null;
        Focus.WithItems([.. _items.Select(i => (i.Id, i.Disabled))]);
        if (!hadActiveId && Focus.ActiveId is not null)
        {
            NotifyStateChanged();
        }
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

        var item = _items.Find(i => i.Id == Focus.ActiveId);
        await CommitValueAsync(item.Value);
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

    internal async Task ActivateAsync(string id, TValue value)
    {
        Focus.TrySetActive(id);
        await CommitValueAsync(value);
        NotifyStateChanged();
    }
}
