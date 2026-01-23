using System.Text.Json;
using FaunaFinder.Api.Components;
using FaunaFinder.Api.Services.DarkMode;
using FaunaFinder.Api.Services.Export;
using FaunaFinder.Api.Services.Localization;
using FaunaFinder.Database;
using FaunaFinder.Database.Extensions;
using FaunaFinder.DataAccess.Extensions;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using NetTopologySuite.IO.Converters;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add Blazor Server with circuit error handling
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

// Add MudBlazor
builder.Services.AddMudServices();

// Add Localization
builder.Services.AddScoped<IAppLocalizer, AppLocalizer>();

// Add Dark Mode
builder.Services.AddScoped<IDarkModeService, DarkModeService>();

// Add Export Service
builder.Services.AddScoped<IMunicipalityReportService, MunicipalityReportService>();

// Configure QuestPDF license (Community license for open source projects)
QuestPDF.Settings.License = LicenseType.Community;

// Add FaunaFinder Database (with snake_case naming and NoTracking)
builder.AddFaunaFinderDatabase();

// Add FaunaFinder DataAccess (repositories)
builder.Services.AddFaunaFinderDataAccess();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".geojson"] = "application/geo+json";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypeProvider });
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

// GeoJSON API endpoint for municipality boundaries
app.MapGet("/api/municipalities/geojson", async (IDbContextFactory<FaunaFinderContext> dbFactory) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();

    var municipalities = await db.Municipalities
        .Where(m => m.Boundary != null)
        .Select(m => new { m.Name, m.GeoJsonId, m.Boundary })
        .ToListAsync();

    // Build GeoJSON FeatureCollection
    var features = municipalities.Select(m =>
    {
        // Extract STATE and COUNTY from GeoJsonId (e.g., "72113" -> STATE="72", COUNTY="113")
        var geoJsonId = m.GeoJsonId;
        var state = geoJsonId.Length >= 2 ? geoJsonId[..2] : geoJsonId;
        var county = geoJsonId.Length > 2 ? geoJsonId[2..] : geoJsonId;

        return new
        {
            type = "Feature",
            properties = new { STATE = state, COUNTY = county, NAME = m.Name },
            geometry = m.Boundary
        };
    }).ToList();

    var featureCollection = new { type = "FeatureCollection", features };

    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    jsonOptions.Converters.Add(new GeoJsonConverterFactory());

    return Results.Json(featureCollection, jsonOptions, contentType: "application/geo+json");
}).WithName("GetMunicipalitiesGeoJson");

app.Run();
