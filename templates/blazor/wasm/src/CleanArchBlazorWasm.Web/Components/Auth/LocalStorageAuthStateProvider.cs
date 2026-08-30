using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CleanArchBlazorWasm.Web.Components.Auth;

// Demo-only: stores a plain username in localStorage with no real credential check or token;
// replace with a real identity provider (OIDC/JWT) before shipping.
// Uses [JSImport] rather than IJSRuntime because this repo's architecture tests forbid Web-project
// types from touching IJSRuntime directly (kept centralized in Dorn.WebUI.Primitives).
// Not annotated [SupportedOSPlatform("browser")]: this app only ever runs in the browser, so the
// guard would only add CA1416 noise without protecting anything.
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
