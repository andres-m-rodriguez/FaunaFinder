using FaunaFinder.Api.Components;
using FaunaFinder.Api.Endpoints;
using FaunaFinder.Api.Services.Export;
using FaunaFinder.Api.Services.Localization;
using FaunaFinder.Database;
using FaunaFinder.Database.Extensions;
using FaunaFinder.Database.Models.Users;
using FaunaFinder.DataAccess.Extensions;
using FaunaFinder.Identity.Api;
using Microsoft.AspNetCore.Identity;
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

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FaunaFinderContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
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

// Serve App.razor as the shell with WASM interactivity
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FaunaFinder.Client.App).Assembly);

app.Run();
