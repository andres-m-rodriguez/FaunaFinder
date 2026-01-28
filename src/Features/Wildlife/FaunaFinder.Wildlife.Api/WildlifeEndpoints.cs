using FaunaFinder.Wildlife.Application.Services;
using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FaunaFinder.Wildlife.Api;

public static class WildlifeEndpoints
{
    public static IEndpointRouteBuilder MapWildlifeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wildlife")
            .WithTags("Wildlife");

        // Public endpoints
        group.MapGet("/species/search", SearchSpecies)
            .WithName("SearchSpecies");

        group.MapGet("/sightings/{id:int}/photo", GetSightingPhoto)
            .WithName("GetSightingPhoto");

        // Authenticated endpoints
        group.MapPost("/sightings", CreateSighting)
            .RequireAuthorization()
            .WithName("CreateSighting");

        group.MapGet("/sightings", GetSightings)
            .RequireAuthorization()
            .WithName("GetSightings");

        group.MapGet("/sightings/{id:int}", GetSightingDetail)
            .RequireAuthorization()
            .WithName("GetSightingDetail");

        group.MapGet("/my-sightings", GetMySightings)
            .RequireAuthorization()
            .WithName("GetMySightings");

        // Teacher/Admin endpoints
        group.MapPost("/sightings/{id:int}/review", ReviewSighting)
            .RequireAuthorization()
            .WithName("ReviewSighting");

        group.MapGet("/review-queue", GetReviewQueue)
            .RequireAuthorization()
            .WithName("GetReviewQueue");

        return app;
    }

    private static async Task<IResult> SearchSpecies(
        string? query,
        int limit,
        IWildlifeService wildlifeService,
        CancellationToken ct)
    {
        var species = await wildlifeService.SearchSpeciesAsync(query, limit, ct);
        return Results.Ok(species);
    }

    private static async Task<IResult> CreateSighting(
        HttpContext context,
        IWildlifeService wildlifeService,
        CreateSightingRequest request,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var response = await wildlifeService.CreateSightingAsync(request, userId, ct);

        if (!response.Success)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Created($"/api/wildlife/sightings/{response.Id}", new { response.Id });
    }

    private static async Task<IResult> GetSightings(
        int pageSize,
        int page,
        string? status,
        IWildlifeService wildlifeService,
        CancellationToken ct)
    {
        var result = await wildlifeService.GetSightingsAsync(page, pageSize, status, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetSightingDetail(
        int id,
        IDbContextFactory<WildlifeDbContext> contextFactory,
        CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var sighting = await context.Sightings
            .Include(s => s.Species)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sighting is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            sighting.Id,
            sighting.SpeciesId,
            SpeciesName = sighting.Species?.CommonName,
            SpeciesScientificName = sighting.Species?.ScientificName,
            sighting.Mode,
            sighting.Confidence,
            sighting.Count,
            sighting.Behaviors,
            sighting.Evidence,
            sighting.Weather,
            sighting.Notes,
            Latitude = sighting.Location.Y,
            Longitude = sighting.Location.X,
            sighting.MunicipalityId,
            sighting.ObservedAt,
            sighting.CreatedAt,
            HasPhoto = sighting.PhotoData != null,
            sighting.Status,
            sighting.IsFlaggedForReview,
            sighting.IsNewMunicipalityRecord,
            sighting.ReviewNotes,
            sighting.ReviewedAt,
            sighting.ReviewedByUserId,
            sighting.ReportedByUserId
        });
    }

    private static async Task<IResult> GetSightingPhoto(
        int id,
        IDbContextFactory<WildlifeDbContext> contextFactory,
        CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var sighting = await context.Sightings
            .Where(s => s.Id == id)
            .Select(s => new { s.PhotoData, s.PhotoContentType })
            .FirstOrDefaultAsync(ct);

        if (sighting?.PhotoData is null || sighting.PhotoContentType is null)
        {
            return Results.NotFound();
        }

        return Results.File(sighting.PhotoData, sighting.PhotoContentType);
    }

    private static async Task<IResult> ReviewSighting(
        int id,
        HttpContext context,
        IWildlifeService wildlifeService,
        string status,
        string? reviewNotes,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        // Note: Role checking should be done via authorization policy or in the service layer
        // For now, we trust the authorization middleware

        var (success, error) = await wildlifeService.ReviewSightingAsync(id, userId, status, reviewNotes, ct);

        if (!success)
        {
            return error == "Sighting not found" ? Results.NotFound() : Results.BadRequest(error);
        }

        return Results.Ok(new { Id = id, Status = status });
    }

    private static async Task<IResult> GetReviewQueue(
        HttpContext context,
        IWildlifeService wildlifeService,
        int pageSize,
        int page,
        CancellationToken ct)
    {
        // Note: Role checking should be done via authorization policy
        var result = await wildlifeService.GetReviewQueueAsync(page, pageSize, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetMySightings(
        HttpContext context,
        IWildlifeService wildlifeService,
        int pageSize,
        int page,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await wildlifeService.GetMySightingsAsync(userId, page, pageSize, ct);
        return Results.Ok(result);
    }
}
