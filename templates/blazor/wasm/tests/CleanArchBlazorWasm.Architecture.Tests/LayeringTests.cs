namespace CleanArchBlazorWasm.Architecture.Tests;

public sealed class LayeringTests
{
    private const string FeaturesRoot = "CleanArchBlazorWasm.Web.Features";

    private static readonly System.Reflection.Assembly WebAssembly = typeof(App).Assembly;

    private static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies(WebAssembly)
        .Build();

    private static IObjectProvider<IType> InNamespace(string root) =>
        Types().That().ResideInNamespaceMatching($@"^{Regex.Escape(root)}(\.|$)");

    private static readonly IObjectProvider<IType> Features = InNamespace(FeaturesRoot);

    [Fact]
    public void Features_ShouldNot_DependOnJsInterop()
    {
        Types()
            .That()
            .Are(Features)
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^Microsoft\.JSInterop"))
            .Check(Architecture);
    }

    [Fact]
    public void NoWebAssemblyType_Should_TouchJsRuntimeDirectly()
    {
        // ArchUnitNET has no member-level type predicate, so this uses reflection directly.
        var violators = WebAssembly.GetTypes().Where(InjectsJsRuntime).ToList();

        Assert.Empty(violators);
    }

    [Fact]
    public void WebAssembly_Should_NeverDefineATemplateLocalClipboardInterop()
    {
        // ClipboardInterop lives in the Dorn.WebUI.Primitives package (Interop namespace),
        // matching AnchorInterop/DismissInterop/ModalInterop — never a template-local copy.
        var violators = WebAssembly
            .GetTypes()
            .Where(type => type.Name == "ClipboardInterop")
            .ToList();

        Assert.Empty(violators);
    }

    private static bool InjectsJsRuntime(System.Type type)
    {
        const BindingFlags flags =
            BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

        return type.GetFields(flags).Any(f => typeof(IJSRuntime).IsAssignableFrom(f.FieldType))
            || type.GetProperties(flags)
                .Any(p => typeof(IJSRuntime).IsAssignableFrom(p.PropertyType))
            || type.GetConstructors(flags).Any(HasJsRuntimeParameter)
            || type.GetMethods(flags)
                .Where(m => m.GetCustomAttribute<JSInvokableAttribute>() is null)
                .Any(m => HasJsRuntimeParameter(m) || HasJsRuntimeReturnType(m));
    }

    private static bool HasJsRuntimeParameter(MethodBase method) =>
        method.GetParameters().Any(p => typeof(IJSRuntime).IsAssignableFrom(p.ParameterType));

    private static bool HasJsRuntimeReturnType(MethodInfo method) =>
        typeof(IJSRuntime).IsAssignableFrom(method.ReturnType);
}
