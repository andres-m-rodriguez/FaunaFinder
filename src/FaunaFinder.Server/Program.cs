using System.Threading.RateLimiting;
using FaunaFinder.Identity.Api;
using FaunaFinder.Identity.Application.Extensions;
using FaunaFinder.Identity.DataAccess.Extensions;
using FaunaFinder.Identity.Database.Extensions;
using FaunaFinder.Server.Components;
using FaunaFinder.Server.Endpoints;
using FaunaFinder.Server.Services.Export;
using FaunaFinder.Server.Services.Localization;
using FaunaFinder.Wildlife.Api;
using FaunaFinder.Wildlife.Application.Extensions;
using FaunaFinder.Wildlife.DataAccess.Extensions;
using FaunaFinder.Wildlife.Database.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<IAppLocalizer, AppLocalizer>();
builder.Services.AddScoped<IMunicipalityReportService, MunicipalityReportService>();
builder.Services.AddScoped<ISpeciesReportService, SpeciesReportService>();

QuestPDF.Settings.License = LicenseType.Community;

// Identity feature
builder.AddIdentityDatabase();
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityDataAccess();

// Wildlife feature
builder.AddWildlifeDatabase();
builder.Services.AddWildlifeApplication();
builder.Services.AddWildlifeDataAccess();

builder.Services.AddAuthorization();

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General API rate limit: 100 requests per minute per IP
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Strict login rate limit: 5 attempts per minute per IP
    options.AddFixedWindowLimiter("auth-login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Strict register rate limit: 3 attempts per minute per IP
    options.AddFixedWindowLimiter("auth-register", limiterOptions =>
    {
        limiterOptions.PermitLimit = 3;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("0");
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
    };
});

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
app.UseRateLimiter();

app.MapDefaultEndpoints();
app.MapStaticAssets();

// API endpoints
var api = app.MapGroup("/api").RequireRateLimiting("api");
api.MapMunicipalityEndpoints();
api.MapSpeciesEndpoints();
api.MapSpeciesImageEndpoints();
api.MapExportEndpoints();
api.MapIdentityEndpoints();
api.MapSightingsEndpoints();

// Serve App.razor as the shell with WASM interactivity
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FaunaFinder.Client.App).Assembly);

app.Run();
