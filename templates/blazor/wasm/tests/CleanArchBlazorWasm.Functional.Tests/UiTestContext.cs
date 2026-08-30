using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

// Real Task.Delay-based open/close waits race under xUnit's default cross-class parallelism.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CleanArchBlazorWasm.Functional.Tests;

// Loose JS interop: unconfigured internal MudBlazor JS calls (key interceptor, popover, ripple)
// fall back to bUnit's default result instead of throwing.
// Implements IAsyncLifetime so xUnit awaits DisposeAsync(); MudTable's PointerEventsNoneService
// only implements IAsyncDisposable, so a synchronous Dispose() would throw.
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
