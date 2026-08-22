using Dorn.WebUI.Primitives;
using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorServer.Web.Components.Ui.ToggleGroup;

// Same roving-tabindex shape as DropdownMenuContext, but MoveAsync only ever moves focus.
public sealed class ToggleGroupContext<TValue>
{
    private readonly List<(string Id, ElementReference Element, bool Disabled)> _items = [];

    public RovingFocusState Focus { get; } = new(RovingFocusOrientation.Horizontal, loop: true);

    public ToggleGroupType Type { get; internal set; }

    public TValue? Value { get; internal set; }

    public IReadOnlyList<TValue> Values { get; internal set; } = [];

    public required Action NotifyStateChanged { get; init; }

    public required Func<TValue, Task> ToggleValueAsync { get; init; }

    public bool IsSelected(TValue value) =>
        Type == ToggleGroupType.Single
            ? EqualityComparer<TValue>.Default.Equals(Value, value)
            : Values.Contains(value);

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

    internal Task ActivateAsync(TValue value) => ToggleValueAsync(value);
}
