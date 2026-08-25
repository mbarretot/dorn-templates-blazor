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
}
