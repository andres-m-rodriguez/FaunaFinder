using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using FaunaFinder.Client.Services.Api;
using FaunaFinder.Client.Services.DarkMode;
using FaunaFinder.Client.Services.Localization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Same origin - use host's base address for API calls
var baseAddress = builder.HostEnvironment.BaseAddress;

// Register typed HttpClient services
builder.Services.AddHttpClient<IMunicipalityService, MunicipalityApiService>(client =>
{
    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<ISpeciesService, SpeciesApiService>(client =>
{
    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddHttpClient<IExportService, ExportApiService>(client =>
{
    client.BaseAddress = new Uri(baseAddress);
});

// Store base address for JS interop
builder.Services.AddSingleton(new ApiConfiguration(baseAddress));

// Register application services
builder.Services.AddSingleton<IAppLocalizer, AppLocalizer>();
builder.Services.AddSingleton<IDarkModeService, DarkModeService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
