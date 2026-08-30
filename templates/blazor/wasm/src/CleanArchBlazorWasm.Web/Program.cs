using CleanArchBlazorWasm.Web.Configuration;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
#if (IncludeCleanArchitecture)
using CleanArchBlazorWasm.Application.Interfaces;
using CleanArchBlazorWasm.Infrastructure.ToDos;
#else
using CleanArchBlazorWasm.Web.Features.ToDo;
#endif
#if (IncludeAuth)
using CleanArchBlazorWasm.Web.Components.Auth;
using Microsoft.AspNetCore.Components.Authorization;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<ThemeState>();

#if (IncludeAuth)
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<LocalStorageAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<LocalStorageAuthStateProvider>()
);
#endif

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

await builder.Build().RunAsync();
