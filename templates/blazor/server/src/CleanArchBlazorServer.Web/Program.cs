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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Blazor Server's Interactive circuit endpoint appends its own frame-ancestors CSP fragment, so
// this assigns (not Appends) via OnStarting to be the final word on these headers. See README.
app.Use(
    (context, next) =>
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Content-Security-Policy"] =
                "default-src 'self'; "
                + "script-src 'self'; "
                + "style-src 'self' 'unsafe-inline'; "
                + "img-src 'self' data:; "
                + "font-src 'self' data:; "
                + "connect-src 'self' https://jsonplaceholder.typicode.com; "
                + "frame-ancestors 'self'";
            return Task.CompletedTask;
        });
        return next();
    }
);

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
#if (IncludeAspire)
app.MapDefaultEndpoints();
#endif

app.Run();

public partial class Program;
