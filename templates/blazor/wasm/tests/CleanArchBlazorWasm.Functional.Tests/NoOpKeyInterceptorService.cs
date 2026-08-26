using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Services;

namespace CleanArchBlazorWasm.Functional.Tests;

// bUnit's ServiceProviderEngineScope.Dispose() throws when a scoped service implements only
// IAsyncDisposable (MudBlazor.KeyInterceptorService's case). Replacing it with a no-op that
// also implements IDisposable keeps rendering key-interceptor-bound components (MudSwitch,
// MudCheckbox, ...) from crashing test teardown.
public sealed class NoOpKeyInterceptorService : IKeyInterceptorService, IDisposable
{
    public Task SubscribeAsync(IKeyInterceptorObserver observer, KeyInterceptorOptions options) =>
        Task.CompletedTask;

    public Task SubscribeAsync(string elementId, KeyInterceptorOptions options, Action<KeyMapBuilder> configure) =>
        Task.CompletedTask;

    public Task SubscribeAsync(
        string elementId,
        KeyInterceptorOptions options,
        IKeyDownObserver? keyDown = null,
        IKeyUpObserver? keyUp = null
    ) => Task.CompletedTask;

    public Task SubscribeAsync(
        string elementId,
        KeyInterceptorOptions options,
        Action<KeyboardEventArgs>? keyDown = null,
        Action<KeyboardEventArgs>? keyUp = null
    ) => Task.CompletedTask;

    public Task SubscribeAsync(
        string elementId,
        KeyInterceptorOptions options,
        Func<KeyboardEventArgs, Task>? keyDown = null,
        Func<KeyboardEventArgs, Task>? keyUp = null
    ) => Task.CompletedTask;

    public Task DispatchAsync(string elementId, KeyEventKind kind, KeyboardEventArgs args) =>
        Task.CompletedTask;

    public Task UpdateKeyAsync(IKeyInterceptorObserver observer, KeyOptions option) => Task.CompletedTask;

    public Task UpdateKeyAsync(string elementId, KeyOptions option) => Task.CompletedTask;

    public Task UnsubscribeAsync(IKeyInterceptorObserver observer) => Task.CompletedTask;

    public Task UnsubscribeAsync(string elementId) => Task.CompletedTask;

    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
