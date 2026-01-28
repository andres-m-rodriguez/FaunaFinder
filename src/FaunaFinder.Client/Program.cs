using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using FaunaFinder.Client.Services.Api;
using FaunaFinder.Client.Services.Auth;
using FaunaFinder.Client.Services.DarkMode;
using FaunaFinder.Client.Services.Localization;
using FaunaFinder.Identity.Application.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Same origin - use host's base address for API calls
var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

// Register typed HttpClient services
builder.Services.AddHttpClient<IMunicipalityService, MunicipalityApiService>(client =>
{
    client.BaseAddress = baseAddress;
});

builder.Services.AddHttpClient<ISpeciesService, SpeciesApiService>(client =>
{
    client.BaseAddress = baseAddress;
});

builder.Services.AddHttpClient<IExportService, ExportApiService>(client =>
{
    client.BaseAddress = baseAddress;
});

builder.Services.AddHttpClient<IWildlifeService, WildlifeApiService>(client =>
{
    client.BaseAddress = baseAddress;
});

// Identity client
builder.Services.AddIdentityClient(baseAddress);

// Store base address for JS interop
builder.Services.AddSingleton(new ApiConfiguration(baseAddress.ToString()));

// Register application services
builder.Services.AddSingleton<IAppLocalizer, AppLocalizer>();
builder.Services.AddSingleton<IDarkModeService, DarkModeService>();

// Authentication
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
