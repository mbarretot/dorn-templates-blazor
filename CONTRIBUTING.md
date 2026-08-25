# Contributing

Thanks for improving the Dorn Blazor templates. Keep changes focused, mirrored across hosting models when applicable, and backed by tests.

---

## 🔁 Development loop

1. Create a focused branch.
2. Follow **RED → GREEN → REFACTOR** for new logic.
3. Keep WASM and Server behavior aligned.
4. Format touched files with CSharpier.
5. Run the relevant suites below.

```bash
dotnet test templates/blazor/wasm/CleanArchBlazorWasm.slnx
dotnet test templates/blazor/server/CleanArchBlazorServer.slnx
dotnet test tests/Dorn.Templates.Blazor.Tests/Dorn.Templates.Blazor.Tests.csproj
dotnet test tests/Dorn.Templates.Blazor.BrowserTests/Dorn.Templates.Blazor.BrowserTests.csproj -c Release
```

> [!IMPORTANT]
> Run test projects separately. The template and browser suites share the global `dotnet new` store and can race when executed together.

---

## 🧭 Where to change things

| Area | Source of truth | Keep aligned |
| --- | --- | --- |
| Layout & theme | `Components/Layout`, `Components/Theme/AppTheme.cs` | Shell and palette in both templates |
| Home page | `Features/Home/Home.razor` | Branding copy in both templates |
| MudBlazor version | `Directory.Packages.props` | Pinned version in both templates |
| Static assets | Razor asset references | Use `@Assets["..."]` for fingerprinting |

> [!NOTE]
> `theme-boot.js` must stay synchronous so the selected theme is applied before first paint.

<details>
<summary><strong>Browser-suite hosting detail</strong></summary>

The Server fixture runs published output from its own directory. This avoids the .NET 10 development-time `MapStaticAssets()` issue and preserves the correct content root.

</details>

---

## 📦 Releases

- Packages: `Dorn.Templates.BlazorWasm` and `Dorn.Templates.BlazorServer`
- Tags: push `v<version>` to trigger NuGet Trusted Publishing
- Local builds: use non-release fallback versions and are never published

## ✅ Conventions

- Conventional commits: `type(scope): message`
- No `Co-Authored-By` or AI attribution
- English in code, comments, commits, and documentation
- xUnit with plain `Assert.*`; no FluentAssertions or Moq
- Comments only for a compact, non-obvious **why**
