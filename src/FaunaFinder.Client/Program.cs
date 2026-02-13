using FaunaFinder.Client.Services.Api;
using FaunaFinder.Client.Services.Auth;
using FaunaFinder.Client.Services.DarkMode;
using FaunaFinder.Client.Services.Localization;
using FaunaFinder.Client.Services.Map;
using FaunaFinder.Identity.Application.Client;
using FaunaFinder.Wildlife.Application.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Same origin - use host's base address for API calls
var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

// Register Wildlife and Identity clients
builder.Services.AddWildlifeClient(baseAddress);
builder.Services.AddIdentityClient(baseAddress);

// Store base address for JS interop
builder.Services.AddSingleton(new ApiConfiguration(baseAddress.ToString()));

// Register application services
builder.Services.AddSingleton<IAppLocalizer, AppLocalizer>();
builder.Services.AddSingleton<IDarkModeService, DarkModeService>();
builder.Services.AddScoped<IMapService, MapService>();

// Authentication
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CookieAuthenticationStateProvider>()
);
builder.Services.AddAuthorizationCore();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
