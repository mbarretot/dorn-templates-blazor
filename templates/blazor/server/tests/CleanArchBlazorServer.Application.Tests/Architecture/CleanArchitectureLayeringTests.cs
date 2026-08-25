namespace CleanArchBlazorServer.Application.Tests.Architecture;

public sealed class CleanArchitectureLayeringTests
{
    private const string RootNamespace = "CleanArchBlazorServer";

    // Use LoadAssembliesIncludingDependencies (not LoadFilteredDirectory) so these rules have
    // actual teeth. LoadFilteredDirectory loads only the exact assemblies it matches and never
    // walks their AssemblyReferences, so a NuGet package added directly to e.g. Domain would never
    // enter the ArchUnitNET model — OnlyDependOn(...)/NotDependOnAny(...) below could never see it
    // and would silently pass despite a real violation. LoadAssembliesIncludingDependencies also
    // pulls in each assembly's direct dependencies, so an added third-party reference is evaluated.
    //
    // Each assembly is loaded by its exact simple name — not a "CleanArchBlazorServer.*" wildcard
    // — because AppContext.BaseDirectory also holds this test assembly's own build output, and
    // "CleanArchBlazorServer.Application.Tests"'s default namespace collides with the
    // ApplicationProject provider below (it starts with "CleanArchBlazorServer.Application.").
    // Loading by exact name means this test project's own types are never analyzed.
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

    // All three libraries ship empty (no worked-example entity/use-case), so every provider below
    // matches ZERO types on a fresh --IncludeCleanArchitecture generate. Every rule MUST chain
    // .WithoutRequiringPositiveResults() — see the feature-level LayeringTests.cs in this template
    // and in CleanArchBlazorWasm for the confirmed ArchUnitNET 0.13.3 gotcha.
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
