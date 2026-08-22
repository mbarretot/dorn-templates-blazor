using System.Diagnostics;
using Xunit;

namespace CleanArchBlazorWasm.Functional.Tests.Ui.Dismiss;

public sealed class DismissStackTests
{
    [Fact]
    public async Task DismissModule_ConsumesEscapeAndOutsideEventsFromTheTopmostActivation()
    {
        var modulePath = Path.Combine(
            FindRepositoryRoot(),
            "templates/blazor/wasm/src/CleanArchBlazorWasm.Web/wwwroot/js/ui/ui-dismiss.js"
        );
        var script = """
            import assert from "node:assert/strict";

            const listeners = new Map();
            globalThis.document = {
              addEventListener(type, handler) {
                const handlers = listeners.get(type) ?? new Set();
                handlers.add(handler);
                listeners.set(type, handlers);
              },
              removeEventListener(type, handler) {
                const handlers = listeners.get(type);
                handlers?.delete(handler);
                if (handlers?.size === 0) {
                  listeners.delete(type);
                }
              }
            };
            const dispatch = (type, event) => {
              for (const handler of listeners.get(type) ?? []) {
                handler(event);
              }
            };

            const module = await import(process.argv[1]);
            const dismissed = [];
            const activation = (id, inside) => ({
              element: { contains: target => target === inside },
              reference: { invokeMethodAsync: () => dismissed.push(id) }
            });
            const parent = activation("parent", "parent");
            const child = activation("child", "child");

            module.activate("parent", parent.element, parent.reference);
            module.activate("child", child.element, child.reference);
            assert.equal(listeners.size, 2);

            let prevented = 0;
            let stopped = 0;
            dispatch("keydown", {
              key: "Escape",
              preventDefault() { prevented += 1; },
              stopPropagation() { stopped += 1; }
            });
            assert.deepEqual(dismissed, ["child"]);
            assert.equal(prevented, 1);
            assert.equal(stopped, 1);

            module.deactivate("child");
            module.deactivate("child");
            dispatch("pointerdown", { target: "outside" });
            assert.deepEqual(dismissed, ["child", "parent"]);

            module.deactivate("parent");
            assert.equal(listeners.size, 0);

            const single = activation("single", "single");
            module.activate("single", single.element, single.reference);
            dispatch("pointerdown", { target: "single" });
            assert.deepEqual(dismissed, ["child", "parent"]);
            dispatch("pointerdown", { target: "outside" });
            assert.deepEqual(dismissed, ["child", "parent", "single"]);
            module.deactivate("single");
            assert.equal(listeners.size, 0);
            """;
        var startInfo = new ProcessStartInfo("node")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add("--input-type=module");
        startInfo.ArgumentList.Add("--eval");
        startInfo.ArgumentList.Add(script);
        startInfo.ArgumentList.Add(new Uri(modulePath).AbsoluteUri);

        using var process = Process.Start(startInfo)!;
        await process.WaitForExitAsync();

        Assert.True(
            process.ExitCode == 0,
            $"node exited with {process.ExitCode}:{Environment.NewLine}{await process.StandardError.ReadToEndAsync()}"
        );
    }

    private static string FindRepositoryRoot()
    {
        for (
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            current is not null;
            current = current.Parent
        )
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
