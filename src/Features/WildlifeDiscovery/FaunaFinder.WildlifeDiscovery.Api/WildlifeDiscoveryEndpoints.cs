using FaunaFinder.Contracts.Localization;
using FaunaFinder.Database;
using FaunaFinder.Database.Models.Sightings;
using FaunaFinder.Database.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FaunaFinder.WildlifeDiscovery.Api;

public static class WildlifeDiscoveryEndpoints
{
    public static IEndpointRouteBuilder MapWildlifeDiscoveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/wildlife")
            .WithTags("Wildlife Discovery");

        // Public endpoints
        group.MapGet("/species/search", SearchSpecies)
            .WithName("SearchSpecies");

        group.MapGet("/sightings/{id:int}/photo", GetSightingPhoto)
            .WithName("GetSightingPhoto");

        // Authenticated endpoints
        group.MapPost("/sightings", CreateSighting)
            .RequireAuthorization()
            .DisableAntiforgery()
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
        FaunaFinderContext db,
        CancellationToken ct)
    {
        if (limit <= 0) limit = 10;
        if (limit > 50) limit = 50;

        var speciesQuery = db.Species.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var search = query.ToLowerInvariant();
            speciesQuery = speciesQuery.Where(s =>
                s.CommonName.Any(cn => cn.Value.ToLower().Contains(search)) ||
                s.ScientificName.ToLower().Contains(search));
        }

        var species = await speciesQuery
            .OrderBy(s => s.ScientificName)
            .Take(limit)
            .Select(s => new
            {
                s.Id,
                CommonName = s.CommonName.ToList(),
                s.ScientificName
            })
            .ToListAsync(ct);

        return Results.Ok(species);
    }

    private static async Task<IResult> CreateSighting(
        HttpContext context,
        UserManager<User> userManager,
        FaunaFinderContext db,
        IFormFile? photo,
        int speciesId,
        double latitude,
        double longitude,
        DateTime observedAt,
        string mode,
        string confidence,
        string count,
        int behaviors,
        int evidence,
        string? weather,
        string? notes,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        // Validate species exists
        var speciesExists = await db.Species.AnyAsync(s => s.Id == speciesId, ct);
        if (!speciesExists)
        {
            return Results.BadRequest("Invalid species ID");
        }

        // Parse enums
        if (!Enum.TryParse<SightingMode>(mode, true, out var sightingMode))
        {
            return Results.BadRequest("Invalid mode");
        }

        if (!Enum.TryParse<ConfidenceLevel>(confidence, true, out var confidenceLevel))
        {
            return Results.BadRequest("Invalid confidence level");
        }

        if (!Enum.TryParse<CountRange>(count, true, out var countRange))
        {
            return Results.BadRequest("Invalid count range");
        }

        Weather? weatherEnum = null;
        if (!string.IsNullOrWhiteSpace(weather))
        {
            if (!Enum.TryParse<Weather>(weather, true, out var parsedWeather))
            {
                return Results.BadRequest("Invalid weather");
            }
            weatherEnum = parsedWeather;
        }

        // Process photo if provided
        byte[]? photoData = null;
        string? photoContentType = null;
        if (photo is not null && photo.Length > 0)
        {
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(photo.ContentType.ToLower()))
            {
                return Results.BadRequest("Invalid image type. Allowed types: JPEG, PNG, GIF, WebP");
            }

            using var memoryStream = new MemoryStream();
            await photo.CopyToAsync(memoryStream, ct);
            photoData = memoryStream.ToArray();
            photoContentType = photo.ContentType;
        }

        // Create the sighting
        var sighting = new Sighting
        {
            Id = 0,
            SpeciesId = speciesId,
            UserSpeciesId = null,
            Mode = sightingMode,
            Confidence = confidenceLevel,
            Count = countRange,
            Behaviors = (Behavior)behaviors,
            Evidence = (EvidenceType)evidence,
            Weather = weatherEnum,
            Notes = notes,
            Location = new Point(longitude, latitude) { SRID = 4326 },
            MunicipalityId = null,
            ObservedAt = observedAt,
            CreatedAt = DateTime.UtcNow,
            PhotoData = photoData,
            PhotoContentType = photoContentType,
            AudioData = null,
            AudioContentType = null,
            Status = SightingStatus.Pending,
            IsFlaggedForReview = false,
            IsNewMunicipalityRecord = false,
            ReviewNotes = null,
            ReviewedAt = null,
            ReviewedByUserId = null,
            ReportedByUserId = user.Id
        };

        db.Sightings.Add(sighting);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/wildlife/sightings/{sighting.Id}", new { sighting.Id });
    }

    private static async Task<IResult> GetSightings(
        int pageSize,
        int page,
        string? status,
        FaunaFinderContext db,
        CancellationToken ct)
    {
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = db.Sightings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SightingStatus>(status, true, out var statusFilter))
        {
            query = query.Where(s => s.Status == statusFilter);
        }

        var totalCount = await query.CountAsync(ct);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.SpeciesId,
                SpeciesName = s.Species != null ? s.Species.CommonName : null,
                s.Mode,
                s.Confidence,
                s.Count,
                s.Status,
                s.ObservedAt,
                s.CreatedAt,
                Latitude = s.Location.Y,
                Longitude = s.Location.X,
                HasPhoto = s.PhotoData != null,
                ReportedByUserName = s.ReportedByUser != null ? s.ReportedByUser.DisplayName : null
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            Items = sightings,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    private static async Task<IResult> GetSightingDetail(
        int id,
        FaunaFinderContext db,
        CancellationToken ct)
    {
        var sighting = await db.Sightings
            .Include(s => s.Species)
            .Include(s => s.ReportedByUser)
            .Include(s => s.ReviewedByUser)
            .Include(s => s.Municipality)
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
            MunicipalityName = sighting.Municipality?.Name,
            sighting.ObservedAt,
            sighting.CreatedAt,
            HasPhoto = sighting.PhotoData != null,
            sighting.Status,
            sighting.IsFlaggedForReview,
            sighting.IsNewMunicipalityRecord,
            sighting.ReviewNotes,
            sighting.ReviewedAt,
            ReviewedByUserName = sighting.ReviewedByUser?.DisplayName,
            sighting.ReportedByUserId,
            ReportedByUserName = sighting.ReportedByUser?.DisplayName
        });
    }

    private static async Task<IResult> GetSightingPhoto(
        int id,
        FaunaFinderContext db,
        CancellationToken ct)
    {
        var sighting = await db.Sightings
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
        UserManager<User> userManager,
        FaunaFinderContext db,
        string status,
        string? reviewNotes,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        // Check if user is Teacher or Admin
        if (user.Role != UserRole.Teacher && user.Role != UserRole.Admin)
        {
            return Results.Forbid();
        }

        if (!Enum.TryParse<SightingStatus>(status, true, out var newStatus))
        {
            return Results.BadRequest("Invalid status");
        }

        var sighting = await db.Sightings.FindAsync([id], ct);
        if (sighting is null)
        {
            return Results.NotFound();
        }

        sighting.Status = newStatus;
        sighting.ReviewNotes = reviewNotes;
        sighting.ReviewedAt = DateTime.UtcNow;
        sighting.ReviewedByUserId = user.Id;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { sighting.Id, sighting.Status });
    }

    private static async Task<IResult> GetReviewQueue(
        HttpContext context,
        UserManager<User> userManager,
        FaunaFinderContext db,
        int pageSize,
        int page,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        // Check if user is Teacher or Admin
        if (user.Role != UserRole.Teacher && user.Role != UserRole.Admin)
        {
            return Results.Forbid();
        }

        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = db.Sightings
            .Where(s => s.Status == SightingStatus.Pending);

        var totalCount = await query.CountAsync(ct);

        var sightings = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.SpeciesId,
                SpeciesName = s.Species != null ? s.Species.CommonName : null,
                s.Mode,
                s.Confidence,
                s.Count,
                s.ObservedAt,
                s.CreatedAt,
                Latitude = s.Location.Y,
                Longitude = s.Location.X,
                HasPhoto = s.PhotoData != null,
                s.IsFlaggedForReview,
                s.IsNewMunicipalityRecord,
                ReportedByUserName = s.ReportedByUser != null ? s.ReportedByUser.DisplayName : null
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            Items = sightings,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    private static async Task<IResult> GetMySightings(
        HttpContext context,
        UserManager<User> userManager,
        FaunaFinderContext db,
        int pageSize,
        int page,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = db.Sightings
            .Where(s => s.ReportedByUserId == user.Id);

        var totalCount = await query.CountAsync(ct);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.SpeciesId,
                SpeciesName = s.Species != null ? s.Species.CommonName : null,
                s.Mode,
                s.Confidence,
                s.Count,
                s.Status,
                s.ObservedAt,
                s.CreatedAt,
                Latitude = s.Location.Y,
                Longitude = s.Location.X,
                HasPhoto = s.PhotoData != null
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            Items = sightings,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }
}
