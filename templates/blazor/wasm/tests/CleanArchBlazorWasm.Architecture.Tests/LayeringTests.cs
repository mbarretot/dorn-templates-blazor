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

    private static IObjectProvider<IType> FeatureLayer(string layer) =>
        Types()
            .That()
            .ResideInNamespaceMatching($@"^{Regex.Escape(FeaturesRoot)}\.[^.]+\.{layer}(\.|$)");

    private static readonly IObjectProvider<IType> Features = InNamespace(FeaturesRoot);
    private static readonly IObjectProvider<IType> FeatureDomain = FeatureLayer("Domain");
    private static readonly IObjectProvider<IType> FeatureApplication = FeatureLayer("Application");
    private static readonly IObjectProvider<IType> FeatureInfrastructure = FeatureLayer(
        "Infrastructure"
    );

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
    public void FeatureDomain_ShouldNot_DependOnFrameworkOrUi()
    {
        Types()
            .That()
            .Are(FeatureDomain)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(
                        @"^(Microsoft\.AspNetCore|Microsoft\.JSInterop|Microsoft\.Extensions\.DependencyInjection|MudBlazor|Dorn\.WebUI)(\.|$)"
                    )
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void FeatureApplication_ShouldNot_DependOnFeatureInfrastructure()
    {
        Types()
            .That()
            .Are(FeatureApplication)
            .Should()
            .NotDependOnAny(FeatureInfrastructure)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void FeatureApplication_ShouldNot_DependOnUiComponents()
    {
        Types()
            .That()
            .Are(FeatureApplication)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(@"^CleanArchBlazorWasm\.Web\.Components(\.|$)")
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void FeatureInfrastructure_ShouldNot_DependOnServerOnlyPersistence()
    {
        Types()
            .That()
            .Are(FeatureInfrastructure)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(
                        @"^(Microsoft\.EntityFrameworkCore|Microsoft\.Data|System\.Data|Npgsql|MySql|Oracle|MongoDB|StackExchange\.Redis)(\.|$)"
                    )
            )
            .AndShould()
            .NotDependOnAny(
                Types()
                    .That()
                    .HaveFullNameMatching(
                        @"^System\.IO\.(File|Directory|FileInfo|DirectoryInfo|FileStream)$"
                    )
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void NoWebAssemblyType_Should_TouchJsRuntimeDirectly()
    {
        var violators = WebAssembly.GetTypes().Where(InjectsJsRuntime).ToList();

        Assert.Empty(violators);
    }

    [Fact]
    public void WebAssembly_Should_NeverDefineATemplateLocalClipboardInterop()
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
