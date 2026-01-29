using FaunaFinder.Api.Components;
using FaunaFinder.Api.Endpoints;
using FaunaFinder.Api.Services.Export;
using FaunaFinder.Api.Services.Localization;
using FaunaFinder.Identity.Api;
using FaunaFinder.Identity.Application.Extensions;
using FaunaFinder.Identity.Database.Extensions;
using FaunaFinder.Wildlife.Api;
using FaunaFinder.Wildlife.Application.Extensions;
using FaunaFinder.Wildlife.Database.Extensions;
using FaunaFinder.Wildlife.DataAccess.Extensions;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<IAppLocalizer, AppLocalizer>();
builder.Services.AddScoped<IMunicipalityReportService, MunicipalityReportService>();

QuestPDF.Settings.License = LicenseType.Community;

// Identity feature
builder.AddIdentityDatabase();
builder.Services.AddIdentityApplication();

// Wildlife feature
builder.AddWildlifeDatabase();
builder.Services.AddWildlifeApplication();
builder.Services.AddWildlifeDataAccess();

builder.Services.AddAuthorization();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapStaticAssets();

// API endpoints
var api = app.MapGroup("/api");
api.MapMunicipalityEndpoints();
api.MapSpeciesEndpoints();
api.MapSpeciesImageEndpoints();
api.MapExportEndpoints();
api.MapIdentityEndpoints();
app.MapWildlifeEndpoints();

// Serve App.razor as the shell with WASM interactivity
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FaunaFinder.Client.App).Assembly);

app.Run();
