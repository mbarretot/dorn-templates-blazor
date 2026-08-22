using Dorn.WebUI.Primitives;
using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorWasm.Web.Components.Ui.Select;

/// <summary>
/// Cascaded by <see cref="Select{TValue}"/> to its <see cref="SelectItem{TValue}"/> children —
/// same roving-tabindex shape as <c>DropdownMenuContext</c>, plus the bound value comparison and
/// commit/cancel paths the listbox pattern needs (design D, Select row).
/// </summary>
public sealed class SelectContext<TValue>
{
    private readonly List<(
        string Id,
        ElementReference Element,
        TValue Value,
        bool Disabled
    )> _items = [];

    public RovingFocusState Focus { get; } = new(RovingFocusOrientation.Vertical, loop: true);

    public TValue? SelectedValue { get; internal set; }

    public required Action NotifyStateChanged { get; init; }

    public required Func<TValue, Task> CommitValue { get; init; }

    public required Func<Task> Cancel { get; init; }

    public bool IsSelected(TValue value) =>
        EqualityComparer<TValue>.Default.Equals(SelectedValue, value);

    internal void RegisterItem(string id, ElementReference element, TValue value, bool disabled)
    {
        var index = _items.FindIndex(i => i.Id == id);
        if (index >= 0)
        {
            _items[index] = (id, element, value, disabled);
        }
        else
        {
            _items.Add((id, element, value, disabled));
        }

        Focus.WithItems([.. _items.Select(i => (i.Id, i.Disabled))]);

        var selectedItem = _items.Find(i =>
            EqualityComparer<TValue>.Default.Equals(i.Value, SelectedValue)
        );
        if (selectedItem.Id is not null)
        {
            Focus.TrySetActive(selectedItem.Id);
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

    internal async Task CommitAsync(string id)
    {
        var item = _items.Find(i => i.Id == id);
        await CommitValue(item.Value);
    }
}
