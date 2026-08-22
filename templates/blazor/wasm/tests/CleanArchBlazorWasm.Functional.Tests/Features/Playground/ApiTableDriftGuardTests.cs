using System.Reflection;
using CleanArchBlazorWasm.Web.Components.Ui.Alert;
using CleanArchBlazorWasm.Web.Components.Ui.Avatar;
using CleanArchBlazorWasm.Web.Components.Ui.Badge;
using CleanArchBlazorWasm.Web.Components.Ui.Button;
using CleanArchBlazorWasm.Web.Components.Ui.Checkbox;
using CleanArchBlazorWasm.Web.Components.Ui.Collapsible;
using CleanArchBlazorWasm.Web.Components.Ui.Dialog;
using CleanArchBlazorWasm.Web.Components.Ui.DropdownMenu;
using CleanArchBlazorWasm.Web.Components.Ui.Form;
using CleanArchBlazorWasm.Web.Components.Ui.HoverCard;
using CleanArchBlazorWasm.Web.Components.Ui.Popover;
using CleanArchBlazorWasm.Web.Components.Ui.Progress;
using CleanArchBlazorWasm.Web.Components.Ui.RadioGroup;
using CleanArchBlazorWasm.Web.Components.Ui.Select;
using CleanArchBlazorWasm.Web.Components.Ui.Separator;
using CleanArchBlazorWasm.Web.Components.Ui.Switch;
using CleanArchBlazorWasm.Web.Components.Ui.Tabs;
using CleanArchBlazorWasm.Web.Components.Ui.Textarea;
using CleanArchBlazorWasm.Web.Components.Ui.Toggle;
using CleanArchBlazorWasm.Web.Components.Ui.ToggleGroup;
using CleanArchBlazorWasm.Web.Components.Ui.Tooltip;
using CleanArchBlazorWasm.Web.Features.Playground;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace CleanArchBlazorWasm.Functional.Tests.Features.Playground;

// Reflects over each demonstrated component's real [Parameter] surface and asserts every name
// appears in that page's ApiParameters table, so a future parameter add/remove/rename without a
// matching doc update goes RED here instead of shipping silently stale docs.
public sealed class ApiTableDriftGuardTests
{
    private static readonly HashSet<string> ExcludedParameterNames =
    [
        "Class",
        "ChildContent",
        "AdditionalAttributes",
        "ValueExpression",
        "DisplayName",
    ];

    private static readonly (Type PageType, Type[] ComponentTypes)[] Registry =
    [
        (typeof(ButtonPlayground), [typeof(Button)]),
        (typeof(BadgePlayground), [typeof(Badge)]),
        (typeof(SeparatorPlayground), [typeof(Separator)]),
        (typeof(AvatarPlayground), [typeof(Avatar), typeof(AvatarImage)]),
        (typeof(AlertPlayground), [typeof(Alert)]),
        (typeof(ProgressPlayground), [typeof(Progress)]),
        (typeof(CheckboxPlayground), [typeof(Checkbox)]),
        (typeof(SwitchPlayground), [typeof(Switch)]),
        (typeof(TextareaPlayground), [typeof(Textarea)]),
        (typeof(TogglePlayground), [typeof(Toggle)]),
        (typeof(ToggleGroupPlayground), [typeof(ToggleGroup<>)]),
        (typeof(RadioGroupPlayground), [typeof(RadioGroup<>)]),
        (typeof(CollapsiblePlayground), [typeof(Collapsible)]),
        (typeof(TooltipPlayground), [typeof(Tooltip)]),
        (typeof(PopoverPlayground), [typeof(Popover)]),
        (typeof(HoverCardPlayground), [typeof(HoverCard)]),
        (typeof(DialogPlayground), [typeof(Dialog)]),
        (typeof(DropdownMenuPlayground), [typeof(DropdownMenu)]),
        (typeof(FormPlayground), [typeof(Input)]),
        (typeof(SelectPlayground), [typeof(Select<>)]),
        (typeof(TabsPlayground), [typeof(Tabs)]),
    ];

    public static TheoryData<Type, Type[]> RegistryCases()
    {
        var data = new TheoryData<Type, Type[]>();
        foreach (var (pageType, componentTypes) in Registry)
        {
            data.Add(pageType, componentTypes);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(RegistryCases))]
    public void ApiTable_DocumentsEveryRealParameter(Type pageType, Type[] componentTypes)
    {
        var realParameterNames = componentTypes
            .SelectMany(componentType =>
                componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            )
            .Where(property => property.GetCustomAttribute<ParameterAttribute>() is not null)
            .Where(property => !IsEventCallback(property.PropertyType))
            .Select(property => property.Name)
            .Where(name => !ExcludedParameterNames.Contains(name))
            .Distinct()
            .ToArray();

        var documentedNames = GetApiParameters(pageType)
            .Select(parameter => parameter.Name)
            .ToHashSet();

        var missing = realParameterNames.Where(name => !documentedNames.Contains(name)).ToArray();

        Assert.True(
            missing.Length == 0,
            $"{pageType.Name}.ApiParameters is missing documented rows for: {string.Join(", ", missing)}"
        );
    }

    private static bool IsEventCallback(Type type) =>
        type == typeof(EventCallback)
        || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>));

    private static IReadOnlyList<PlaygroundParameter> GetApiParameters(Type pageType)
    {
        var field =
            pageType.GetField("ApiParameters", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"{pageType.Name} has no private static ApiParameters field."
            );

        return (PlaygroundParameter[])field.GetValue(null)!;
    }
}
