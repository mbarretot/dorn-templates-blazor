namespace CleanArchBlazorServer.Architecture.Tests;

public sealed class LayeringTests
{
    private const string FeaturesRoot = "CleanArchBlazorServer.Web.Features";

    private static readonly System.Reflection.Assembly WebAssembly = typeof(Program).Assembly;

    private static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies(WebAssembly)
        .Build();

    private static IObjectProvider<IType> InNamespace(string root) =>
        Types().That().ResideInNamespaceMatching($@"^{Regex.Escape(root)}(\.|$)");

    // Matches any feature's given layer sub-folder, e.g. Features.Home.Domain,
    // Features.Orders.Application. No feature scaffolds these sub-folders by default (see the
    // feature-slice-convention spec) — they are added only when a feature earns internal layering.
    // Consequently these providers always match ZERO types on a fresh generate. Every rule using
    // one of them MUST chain .WithoutRequiringPositiveResults(): ArchUnitNET 0.13.3 fails a rule
    // whose filtered type set is empty unless that flag is chained, even though no violation
    // exists — confirmed by the equivalent spike in CleanArchBlazorWasm's LayeringTests.cs before
    // this rule set was ported here. Without it, `dotnet new` would ship an app that fails its own
    // test suite immediately.
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
                    .ResideInNamespaceMatching(@"^CleanArchBlazorServer\.Web\.Components(\.|$)")
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    // No FeatureInfrastructure_ShouldNot_DependOnServerOnlyPersistence rule here (see WASM's
    // LayeringTests.cs for the WASM-only equivalent). Blazor Server runs in-process on the
    // server, so the browser-sandbox constraint that bans EF Core / direct DB access in WASM's
    // Infrastructure/ does not apply — Server's Infrastructure/ MAY use persistence freely.
}
