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

    // Matches any feature's given layer sub-folder, e.g. Features.Home.Domain,
    // Features.Orders.Infrastructure. No feature scaffolds these sub-folders by default (see the
    // feature-slice-convention spec) — they are added only when a feature earns internal layering.
    // Consequently these providers always match ZERO types on a fresh generate. Every rule using
    // one of them MUST chain .WithoutRequiringPositiveResults(), confirmed by the task 3.1 spike:
    // without it, ArchUnitNET 0.13.3 fails a rule whose filtered type set is empty even though no
    // violation exists; with it, a zero-match rule passes vacuously as intended. Without this,
    // `dotnet new` would ship an app that fails its own test suite immediately.
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
        // WASM ONLY (browser sandbox constraint) — Server's Infrastructure/ carries no such
        // restriction. See CleanArchBlazorServer's LayeringTests.cs for the documented asymmetry.
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
