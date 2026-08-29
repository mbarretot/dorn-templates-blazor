using CleanArchBlazorServer.Web.Configuration;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
#if (IncludeCleanArchitecture)
using CleanArchBlazorServer.Application.Interfaces;
using CleanArchBlazorServer.Infrastructure.ToDos;
#else
using CleanArchBlazorServer.Web.Features.ToDo;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (IncludeAspire)
builder.AddServiceDefaults();
#endif
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();

builder.Services.Configure<ToDoApiOptions>(
    builder.Configuration.GetSection(ToDoApiOptions.SectionName)
);
builder.Services.AddHttpClient<IToDoRepository, ToDoRepository>(
    (sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<ToDoApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseAddress);
    }
);

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
