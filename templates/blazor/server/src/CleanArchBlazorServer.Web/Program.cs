using MudBlazor.Services;
#if (IncludeCleanArchitecture)
using CleanArchBlazorServer.Application.Interfaces;
using CleanArchBlazorServer.Infrastructure.ToDos;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (IncludeAspire)
builder.AddServiceDefaults();
#endif
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();

#if (IncludeCleanArchitecture)
builder.Services.AddHttpClient<IToDoRepository, JsonPlaceholderToDoRepository>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});
#endif

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
#if (IncludeAspire)
app.MapDefaultEndpoints();
#endif

app.Run();

public partial class Program;
