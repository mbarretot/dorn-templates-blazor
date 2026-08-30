namespace Dorn.Templates.Blazor.Tests;

// Installing the templates is the only thing that must happen before (and exactly once for) any
// generation test — the generate/build cycles that follow run against isolated, per-test output
// directories and don't need to be serialized against each other. A Lazy<Task> gives a
// thread-safe run-once guard without forcing every test class onto xUnit's single-threaded
// collection-execution model, which used to serialize the entire assembly.
internal static class TemplatePackInstallation
{
    private static readonly Lazy<Task> InstallOnce = new(InstallBothAsync);

    public static Task EnsureInstalledAsync() => InstallOnce.Value;

    private static async Task InstallBothAsync()
    {
        await TemplatePackHarness.InstallAsync("Dorn.Templates.BlazorWasm");
        await TemplatePackHarness.InstallAsync("Dorn.Templates.BlazorServer");
    }
}
