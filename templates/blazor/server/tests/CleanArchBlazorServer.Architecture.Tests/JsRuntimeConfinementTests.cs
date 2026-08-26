namespace CleanArchBlazorServer.Architecture.Tests;

public sealed class JsRuntimeConfinementTests
{
    private static readonly System.Reflection.Assembly WebAssembly = typeof(Program).Assembly;

    [Fact]
    public void NoWebAssemblyType_Should_TouchJsRuntimeDirectly()
    {
        var violators = WebAssembly.GetTypes().Where(InjectsJsRuntime).ToList();

        Assert.Empty(violators);
    }

    [Fact]
    public void Server_Should_NeverDefineATemplateLocalClipboardInterop()
    {
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
