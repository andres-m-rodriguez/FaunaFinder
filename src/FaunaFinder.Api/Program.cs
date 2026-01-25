using FaunaFinder.Api.Components;
using FaunaFinder.Api.Endpoints;
using FaunaFinder.Api.Services.Export;
using FaunaFinder.Api.Services.Localization;
using FaunaFinder.Database.Extensions;
using FaunaFinder.DataAccess.Extensions;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<IAppLocalizer, AppLocalizer>();
builder.Services.AddScoped<IMunicipalityReportService, MunicipalityReportService>();

QuestPDF.Settings.License = LicenseType.Community;

builder.AddFaunaFinderDatabase();
builder.Services.AddFaunaFinderDataAccess();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.MapDefaultEndpoints();

// API endpoints
var api = app.MapGroup("/api");
api.MapMunicipalityEndpoints();
api.MapSpeciesEndpoints();
api.MapExportEndpoints();

// Serve App.razor as the shell with WASM interactivity
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FaunaFinder.Client.App).Assembly);

app.Run();
