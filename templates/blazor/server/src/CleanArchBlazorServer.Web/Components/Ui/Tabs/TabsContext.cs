using Microsoft.AspNetCore.Components;

namespace CleanArchBlazorServer.Web.Components.Ui.Tabs;

public enum TabsOrientation
{
    Horizontal,
    Vertical,
}

public enum TabsActivationMode
{
    Automatic,
    Manual,
}

// Cascades active-tab state and the trigger list that drives arrow-key navigation; zero IJSRuntime.
public sealed class TabsContext
{
    private readonly List<(string Value, ElementReference Element)> _triggers = [];

    public string? ActiveValue { get; internal set; }

    public TabsActivationMode ActivationMode { get; internal set; }

    public TabsOrientation Orientation { get; internal set; }

    public required string IdPrefix { get; init; }

    public required Func<string, Task> ActivateAsync { get; init; }

    public string TriggerId(string value) => $"{IdPrefix}-trigger-{value}";

    public string PanelId(string value) => $"{IdPrefix}-panel-{value}";

    internal void RegisterTrigger(string value, ElementReference element)
    {
        var index = _triggers.FindIndex(t => t.Value == value);
        if (index >= 0)
        {
            _triggers[index] = (value, element);
        }
        else
        {
            _triggers.Add((value, element));
        }
    }

    internal void UnregisterTrigger(string value) => _triggers.RemoveAll(t => t.Value == value);

    internal async Task FocusAdjacentAsync(string currentValue, int direction)
    {
        if (_triggers.Count == 0)
        {
            return;
        }

        var currentIndex = Math.Max(_triggers.FindIndex(t => t.Value == currentValue), 0);
        var nextIndex =
            ((currentIndex + direction) % _triggers.Count + _triggers.Count) % _triggers.Count;
        var target = _triggers[nextIndex];

        await target.Element.FocusAsync();

        if (ActivationMode == TabsActivationMode.Automatic)
        {
            await ActivateAsync(target.Value);
        }
    }
}
