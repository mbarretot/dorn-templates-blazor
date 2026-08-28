using MudBlazor.Services;
#if (IncludeCleanArchitecture)
using CleanArchBlazorWasm.Application.Interfaces;
using CleanArchBlazorWasm.Infrastructure.ToDos;
#else
using CleanArchBlazorWasm.Web.Features.ToDo;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();

builder.Services.AddHttpClient<IToDoRepository, ToDoRepository>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});

await builder.Build().RunAsync();
