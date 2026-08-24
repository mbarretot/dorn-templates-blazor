using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

#if (IncludeAspire)
builder.AddServiceDefaults();
#endif
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();

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
