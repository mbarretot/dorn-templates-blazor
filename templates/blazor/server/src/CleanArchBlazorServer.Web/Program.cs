using Dorn.WebUI.Primitives.Toast;

var builder = WebApplication.CreateBuilder(args);

#if (IncludeAspire)
builder.AddServiceDefaults();
#endif
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<ToastStore>();

builder.Services.AddScoped<ModalInterop>();
builder.Services.AddScoped<DismissInterop>();
builder.Services.AddScoped<AnchorInterop>();
builder.Services.AddScoped<ClipboardInterop>();
builder.Services.AddScoped<PlaygroundShortcutInterop>();

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
