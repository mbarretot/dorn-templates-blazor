using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Real Task.Delay-based open/close waits race under xUnit's default cross-class parallelism.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CleanArchBlazorServer.Functional.Tests;

// Loose JS interop: MudBlazor's own components make internal JS calls (key interceptor,
// popover, ripple, ...) that aren't part of this template's code and aren't worth stubbing
// one by one, so unconfigured calls fall back to bUnit's default result instead of throwing.
// Implements xUnit's IAsyncLifetime so the framework awaits BunitContext.DisposeAsync()
// instead of only calling the synchronous Dispose(). MudTable registers services (e.g.
// PointerEventsNoneService) that only implement IAsyncDisposable; disposing the DI container
// synchronously throws InvalidOperationException for those.
public abstract class UiTestContext : BunitContext, IAsyncLifetime
{
    protected UiTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddScoped<IKeyInterceptorService, NoOpKeyInterceptorService>();
    }

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}
