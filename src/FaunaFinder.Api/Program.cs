using FaunaFinder.Api.Components;
using FaunaFinder.Api.Services.Localization;
using FaunaFinder.Database.Extensions;
using FaunaFinder.DataAccess.Extensions;
using MudBlazor.Services;

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

app.Run();
