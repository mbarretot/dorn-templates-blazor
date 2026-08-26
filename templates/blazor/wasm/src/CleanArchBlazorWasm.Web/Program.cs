using MudBlazor.Services;
#if (IncludeCleanArchitecture)
using CleanArchBlazorWasm.Application.Interfaces;
using CleanArchBlazorWasm.Infrastructure.ToDos;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();

#if (IncludeCleanArchitecture)
builder.Services.AddHttpClient<IToDoRepository, JsonPlaceholderToDoRepository>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});
#endif

await builder.Build().RunAsync();
