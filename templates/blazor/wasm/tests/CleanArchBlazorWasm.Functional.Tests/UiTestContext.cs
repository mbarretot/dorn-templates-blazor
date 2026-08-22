using Bunit;
using Dorn.WebUI.Primitives.Interop;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// Real Task.Delay-based open/close waits race under xUnit's default cross-class parallelism.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CleanArchBlazorWasm.Functional.Tests;

/// <summary>
/// Shared bUnit harness (design's Functional-tier description): Strict JS interop mode so an
/// unconfigured call fails the test instead of silently returning a default, the three owned
/// interop modules stubbed via <see cref="BunitJSInterop.SetupModule"/>, and their C# wrappers
/// pre-registered as scoped services (mirrors <c>Program.cs</c>). Also stubs the framework's own
/// <c>ElementReference.FocusAsync()</c> interop call — needed by design C5's roving-tabindex
/// components (Tabs, and PR6's DropdownMenu/Select) — which is a Blazor-internal call, not one
/// of the three owned modules, so it is not a violation of "zero owned IJSRuntime calls".
/// </summary>
public abstract class UiTestContext : BunitContext
{
    protected UiTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;

        ModalModule = JSInterop.SetupModule("./js/ui/ui-modal.js");
        DismissModule = JSInterop.SetupModule("./js/ui/ui-dismiss.js");
        AnchorModule = JSInterop.SetupModule("./js/ui/ui-anchor.js");
        ClipboardModule = JSInterop.SetupModule("./js/ui/ui-clipboard.js");
        PlaygroundShortcutModule = JSInterop.SetupModule("./js/playground/playground-shortcut.js");
        ActivateShortcut = PlaygroundShortcutModule
            .SetupVoid("activate", _ => true)
            .SetVoidResult();
        DeactivateShortcut = PlaygroundShortcutModule
            .SetupVoid("deactivate", _ => true)
            .SetVoidResult();
        JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetVoidResult();

        Services.AddScoped(_ => new ModalInterop(JSInterop.JSRuntime));
        Services.AddScoped(_ => new DismissInterop(JSInterop.JSRuntime));
        Services.AddScoped(_ => new AnchorInterop(JSInterop.JSRuntime));
        Services.AddScoped(_ => new ClipboardInterop(JSInterop.JSRuntime));
        Services.AddScoped(_ => new PlaygroundShortcutInterop(JSInterop.JSRuntime));
    }

    protected BunitJSModuleInterop ModalModule { get; }

    protected BunitJSModuleInterop DismissModule { get; }

    protected BunitJSModuleInterop AnchorModule { get; }

    protected BunitJSModuleInterop ClipboardModule { get; }

    protected BunitJSModuleInterop PlaygroundShortcutModule { get; }

    protected JSRuntimeInvocationHandler ActivateShortcut { get; }

    protected JSRuntimeInvocationHandler DeactivateShortcut { get; }
}
