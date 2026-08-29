using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanArchBlazorWasm.Web.Components.Auth;

// Demo-only: stores a plain username in the browser's localStorage, with no real credential
// check or token. Uses System.Runtime.InteropServices.JavaScript ([JSImport]) rather than
// Microsoft.JSInterop.IJSRuntime: this repo's architecture tests
// (NoWebAssemblyType_Should_TouchJsRuntimeDirectly) forbid Web-project types from touching
// IJSRuntime directly, keeping that surface centralized in the Dorn.WebUI.Primitives package.
// Not annotated [SupportedOSPlatform("browser")] on purpose: this whole app only ever runs in
// the browser (Blazor WebAssembly), so a platform guard here would only add CA1416 noise at
// every call site (DI registration, Login.razor, NavMenu.razor) without protecting anything.
// Replace with a real identity provider (OIDC/JWT against a backend API) before shipping.
public sealed partial class LocalStorageAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "dorn-demo-username";
    private static readonly AuthenticationState Anonymous = new(
        new ClaimsPrincipal(new ClaimsIdentity())
    );

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(BuildState(GetItem(StorageKey)));

    public Task SignInAsync(string username)
    {
        SetItem(StorageKey, username);
        NotifyAuthenticationStateChanged(Task.FromResult(BuildState(username)));
        return Task.CompletedTask;
    }

    public Task SignOutAsync()
    {
        RemoveItem(StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        return Task.CompletedTask;
    }

    private static AuthenticationState BuildState(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Anonymous;
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, username)], "demo");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    [JSImport("globalThis.localStorage.getItem")]
    private static partial string? GetItem(string key);

    [JSImport("globalThis.localStorage.setItem")]
    private static partial void SetItem(string key, string value);

    [JSImport("globalThis.localStorage.removeItem")]
    private static partial void RemoveItem(string key);
}
