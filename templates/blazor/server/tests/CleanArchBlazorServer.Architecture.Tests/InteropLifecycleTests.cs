namespace CleanArchBlazorServer.Architecture.Tests;

// Reflection fallback (IL call-site walking judged brittle): no Ui type injecting Interop may override a pre-connect lifecycle hook.
public sealed class InteropLifecycleTests
{
    private const string UiRoot = "CleanArchBlazorServer.Web.Components.Ui";

    private static readonly System.Reflection.Assembly WebAssembly = typeof(Program).Assembly;

    // OnParametersSet (sync) is excluded — the "set pending flag" shape never touches JS there.
    private static readonly string[] PreConnectHooks =
    [
        "OnInitialized",
        "OnInitializedAsync",
        "OnParametersSetAsync",
    ];

    // Select<TValue>'s OnInitialized only constructs SelectContext (pure C#, zero JS) — same safety property as DropdownMenu, just co-located with the Interop fields.
    private static readonly System.Type[] AllowListedPreConnectOverrides =
    [
        typeof(CleanArchBlazorServer.Web.Components.Ui.Select.Select<>),
    ];

    [Fact]
    public void InteropInjectingComponents_Should_NotOverridePreConnectLifecycleHooks()
    {
        var violators = WebAssembly
            .GetTypes()
            .Where(type =>
                type.Namespace is not null
                && type.Namespace.StartsWith(UiRoot, StringComparison.Ordinal)
            )
            .Where(InjectsAnInteropModule)
            .Where(OverridesAPreConnectHook)
            .Except(AllowListedPreConnectOverrides)
            .ToList();

        Assert.Empty(violators);
    }

    [Fact]
    public void GeneratedTree_Should_NotConfigureCircuitOptionsOrShipReconnectUi()
    {
        var violators = WebAssembly
            .GetTypes()
            .Where(type =>
                type.GetFields(AllDeclared).Any(f => f.FieldType.Name == "CircuitOptions")
                || type.GetProperties(AllDeclared).Any(p => p.PropertyType.Name == "CircuitOptions")
                || type.Name.Contains("Reconnect", StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        Assert.Empty(violators);
    }

    private static bool InjectsAnInteropModule(System.Type type) =>
        type.GetFields(AllDeclared)
            .Any(f => f.FieldType.Name.EndsWith("Interop", StringComparison.Ordinal))
        || type.GetProperties(AllDeclared)
            .Any(p => p.PropertyType.Name.EndsWith("Interop", StringComparison.Ordinal));

    private static bool OverridesAPreConnectHook(System.Type type) =>
        PreConnectHooks.Any(hook =>
            type.GetMethod(
                hook,
                BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly
            )
                is not null
        );

    private const BindingFlags AllDeclared =
        BindingFlags.Instance
        | BindingFlags.Static
        | BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.DeclaredOnly;
}
