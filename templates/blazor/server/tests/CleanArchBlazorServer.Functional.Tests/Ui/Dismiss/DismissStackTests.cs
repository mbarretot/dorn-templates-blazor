using System.Diagnostics;
using Xunit;

namespace CleanArchBlazorServer.Functional.Tests.Ui.Dismiss;

public sealed class DismissStackTests
{
    [Fact]
    public async Task DismissModule_RoutesEventsToTopmostActiveLayer()
    {
        var modulePath = Path.Combine(
            FindRepositoryRoot(),
            "templates",
            "blazor",
            "server",
            "src",
            "CleanArchBlazorServer.Web",
            "wwwroot",
            "js",
            "ui",
            "ui-dismiss.js"
        );
        var harnessPath = Path.Combine(Path.GetTempPath(), $"dorn-dismiss-{Guid.NewGuid():N}.mjs");

        await File.WriteAllTextAsync(
            harnessPath,
            """
            import assert from "node:assert/strict";
            import { pathToFileURL } from "node:url";

            const listeners = new Map();
            globalThis.document = {
              addEventListener(type, handler) {
                const handlers = listeners.get(type) ?? [];
                handlers.push(handler);
                listeners.set(type, handlers);
              },
              removeEventListener(type, handler) {
                const handlers = listeners.get(type) ?? [];
                listeners.set(type, handlers.filter((candidate) => candidate !== handler));
              },
              dispatch(type, event) {
                for (const handler of [...(listeners.get(type) ?? [])]) handler(event);
              },
              count(type) {
                return (listeners.get(type) ?? []).length;
              },
            };

            const { activate, deactivate } = await import(pathToFileURL(process.argv[2]).href);
            const calls = [];
            const layer = (id) => ({ contains: (target) => target === id });
            const reference = (id) => ({ invokeMethodAsync: () => calls.push(id) });
            const escape = () => {
              let prevented = 0;
              let stopped = 0;
              document.dispatch("keydown", {
                key: "Escape",
                preventDefault: () => prevented += 1,
                stopPropagation: () => stopped += 1,
              });
              return { prevented, stopped };
            };

            activate("parent", layer("parent"), reference("parent"));
            activate("child", layer("child"), reference("child"));
            assert.deepEqual(escape(), { prevented: 1, stopped: 1 });
            assert.deepEqual(calls, ["child"]);

            calls.length = 0;
            document.dispatch("pointerdown", { target: "outside" });
            assert.deepEqual(calls, ["child"]);

            deactivate("child");
            deactivate("child");
            calls.length = 0;
            escape();
            assert.deepEqual(calls, ["parent"]);

            deactivate("parent");
            assert.equal(document.count("keydown"), 0);
            assert.equal(document.count("pointerdown"), 0);

            activate("single", layer("single"), reference("single"));
            calls.length = 0;
            document.dispatch("pointerdown", { target: "outside" });
            assert.deepEqual(calls, ["single"]);
            deactivate("single");
            """
        );

        try
        {
            var start = new ProcessStartInfo("node")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            start.ArgumentList.Add(harnessPath);
            start.ArgumentList.Add(modulePath);

            using var process = Process.Start(start)!;
            await process.WaitForExitAsync();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            Assert.True(process.ExitCode == 0, $"{output}{error}");
        }
        finally
        {
            File.Delete(harnessPath);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
