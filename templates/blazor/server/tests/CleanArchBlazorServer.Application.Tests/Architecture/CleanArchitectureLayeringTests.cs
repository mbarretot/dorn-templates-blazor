namespace CleanArchBlazorServer.Application.Tests.Architecture;

public sealed class CleanArchitectureLayeringTests
{
    private const string RootNamespace = "CleanArchBlazorServer";

    private static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssembliesIncludingDependencies(
            LoadAssembly("Domain"),
            LoadAssembly("Application"),
            LoadAssembly("Infrastructure"),
            LoadAssembly("Web")
        )
        .Build();

    private static System.Reflection.Assembly LoadAssembly(string layer) =>
        System.Reflection.Assembly.Load($"{RootNamespace}.{layer}");

    private static IObjectProvider<IType> InNamespace(string root) =>
        Types().That().ResideInNamespaceMatching($@"^{Regex.Escape(root)}(\.|$)");

    private static readonly IObjectProvider<IType> DomainProject = InNamespace(
        $"{RootNamespace}.Domain"
    );
    private static readonly IObjectProvider<IType> ApplicationProject = InNamespace(
        $"{RootNamespace}.Application"
    );
    private static readonly IObjectProvider<IType> InfrastructureProject = InNamespace(
        $"{RootNamespace}.Infrastructure"
    );

    [Fact]
    public void Domain_Should_DependOnNothingButBcl()
    {
        Types()
            .That()
            .Are(DomainProject)
            .Should()
            .OnlyDependOn(
                Types().That().Are(DomainProject).Or().ResideInNamespaceMatching(@"^System(\.|$)")
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void Domain_ShouldNot_DependOnOuterLayers()
    {
        Types()
            .That()
            .Are(DomainProject)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(
                        $@"^{RootNamespace}\.(Application|Infrastructure|Web)(\.|$)"
                    )
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void Application_ShouldNot_DependOnInfrastructureOrWeb()
    {
        Types()
            .That()
            .Are(ApplicationProject)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching($@"^{RootNamespace}\.(Infrastructure|Web)(\.|$)")
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Fact]
    public void Infrastructure_ShouldNot_DependOnWeb()
    {
        Types()
            .That()
            .Are(InfrastructureProject)
            .Should()
            .NotDependOnAny(
                Types().That().ResideInNamespaceMatching($@"^{RootNamespace}\.Web(\.|$)")
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
