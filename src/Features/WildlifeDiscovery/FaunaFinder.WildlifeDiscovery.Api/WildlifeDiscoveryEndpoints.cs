using FaunaFinder.Database;
using FaunaFinder.Database.Models.Sightings;
using FaunaFinder.Database.Models.Users;
using FaunaFinder.WildlifeDiscovery.Contracts.Requests;
using FaunaFinder.WildlifeDiscovery.Contracts.Responses;
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

        // Species search for autocomplete
        group.MapGet("/species/search", SearchSpecies);

        // Sightings
        group.MapPost("/sightings", CreateSighting).RequireAuthorization();
        group.MapGet("/sightings", GetSightings).RequireAuthorization();
        group.MapGet("/sightings/{id:int}", GetSighting).RequireAuthorization();
        group.MapGet("/sightings/{id:int}/photo", GetSightingPhoto);
        group.MapGet("/sightings/{id:int}/audio", GetSightingAudio);
        group.MapPost("/sightings/{id:int}/review", ReviewSighting).RequireAuthorization();

        // User Species (new species submissions)
        group.MapPost("/user-species", CreateUserSpecies).RequireAuthorization();
        group.MapGet("/user-species/{id:int}", GetUserSpecies).RequireAuthorization();
        group.MapGet("/user-species/{id:int}/photo", GetUserSpeciesPhoto);
        group.MapPost("/user-species/{id:int}/verify", VerifyUserSpecies).RequireAuthorization();

        // Review queue for teachers/experts
        group.MapGet("/review-queue", GetReviewQueue).RequireAuthorization();

        // User's own sightings
        group.MapGet("/my-sightings", GetMySightings).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> SearchSpecies(
        string? query,
        int limit,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.Ok(Array.Empty<SpeciesSearchDto>());
        }

        var searchTerm = query.ToLower();

        var species = await context.Species
            .Where(s => s.ScientificName.ToLower().Contains(searchTerm) ||
                        s.CommonName.Any(cn => cn.Value.ToLower().Contains(searchTerm)))
            .Take(Math.Min(limit, 20))
            .Select(s => new SpeciesSearchDto(
                s.Id,
                s.CommonName.First().Value,
                s.ScientificName,
                false)) // IsEndangered - would need to be added to Species model or FwsLinks
            .ToListAsync(ct);

        return Results.Ok(species);
    }

    private static async Task<IResult> CreateSighting(
        CreateSightingRequest request,
        HttpContext httpContext,
        UserManager<User> userManager,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        // Validate species reference
        if (request.SpeciesId.HasValue)
        {
            var speciesExists = await context.Species.AnyAsync(s => s.Id == request.SpeciesId.Value, ct);
            if (!speciesExists)
            {
                return Results.NotFound($"Species with ID {request.SpeciesId.Value} not found");
            }
        }
        else if (request.UserSpeciesId.HasValue)
        {
            var userSpeciesExists = await context.UserSpecies.AnyAsync(us => us.Id == request.UserSpeciesId.Value, ct);
            if (!userSpeciesExists)
            {
                return Results.NotFound($"User species with ID {request.UserSpeciesId.Value} not found");
            }
        }
        else
        {
            return Results.BadRequest("Either SpeciesId or UserSpeciesId must be provided");
        }

        var location = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

        // Check if this is a new municipality record
        var municipality = await context.Municipalities
            .Where(m => m.Boundary != null && m.Boundary.Contains(location))
            .FirstOrDefaultAsync(ct);

        var isNewMunicipalityRecord = false;
        if (municipality != null && request.SpeciesId.HasValue)
        {
            var existingRecord = await context.MunicipalitySpecies
                .AnyAsync(ms => ms.MunicipalityId == municipality.Id && ms.SpeciesId == request.SpeciesId.Value, ct);
            isNewMunicipalityRecord = !existingRecord;
        }

        // Check if species is endangered (flag for priority review)
        var isFlaggedForReview = isNewMunicipalityRecord;
        if (request.SpeciesId.HasValue)
        {
            var hasEndangeredLink = await context.FwsLinks
                .AnyAsync(l => l.SpeciesId == request.SpeciesId.Value, ct);
            if (hasEndangeredLink)
            {
                isFlaggedForReview = true;
            }
        }

        var sighting = new Sighting
        {
            Id = 0,
            SpeciesId = request.SpeciesId,
            UserSpeciesId = request.UserSpeciesId,
            Mode = Enum.Parse<SightingMode>(request.Mode),
            Confidence = Enum.Parse<ConfidenceLevel>(request.Confidence),
            Count = Enum.Parse<CountRange>(request.Count),
            Behaviors = (Behavior)request.Behaviors,
            Evidence = (EvidenceType)request.Evidence,
            Weather = string.IsNullOrEmpty(request.Weather) ? null : Enum.Parse<Weather>(request.Weather),
            Notes = request.Notes,
            Location = location,
            MunicipalityId = municipality?.Id,
            ObservedAt = request.ObservedAt,
            CreatedAt = DateTime.UtcNow,
            PhotoData = string.IsNullOrEmpty(request.PhotoBase64) ? null : Convert.FromBase64String(request.PhotoBase64),
            PhotoContentType = request.PhotoContentType,
            AudioData = string.IsNullOrEmpty(request.AudioBase64) ? null : Convert.FromBase64String(request.AudioBase64),
            AudioContentType = request.AudioContentType,
            Status = SightingStatus.Pending,
            IsFlaggedForReview = isFlaggedForReview,
            IsNewMunicipalityRecord = isNewMunicipalityRecord,
            ReportedByUserId = user.Id
        };

        context.Sightings.Add(sighting);
        await context.SaveChangesAsync(ct);

        var dto = await GetSightingDtoAsync(context, sighting.Id, ct);
        return Results.Created($"/api/wildlife/sightings/{sighting.Id}", dto);
    }

    private static async Task<IResult> GetSightings(
        int? page,
        int? pageSize,
        string? status,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var query = context.Sightings.AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SightingStatus>(status, out var statusEnum))
        {
            query = query.Where(s => s.Status == statusEnum);
        }

        var actualPage = page ?? 1;
        var actualPageSize = Math.Min(pageSize ?? 20, 100);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((actualPage - 1) * actualPageSize)
            .Take(actualPageSize)
            .Select(s => new SightingListDto(
                s.Id,
                s.Species != null ? s.Species.CommonName.First().Value : s.UserSpecies!.CommonName,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Location.Y,
                s.Location.X,
                s.Municipality != null ? s.Municipality.Name : null,
                s.ObservedAt,
                s.Status.ToString(),
                s.IsFlaggedForReview,
                s.IsNewMunicipalityRecord,
                s.ReportedByUser.DisplayName))
            .ToListAsync(ct);

        return Results.Ok(sightings);
    }

    private static async Task<IResult> GetSighting(
        int id,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var dto = await GetSightingDtoAsync(context, id, ct);
        if (dto is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetSightingPhoto(
        int id,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var sighting = await context.Sightings
            .Where(s => s.Id == id)
            .Select(s => new { s.PhotoData, s.PhotoContentType })
            .FirstOrDefaultAsync(ct);

        if (sighting?.PhotoData is null)
        {
            return Results.NotFound();
        }

        return Results.File(sighting.PhotoData, sighting.PhotoContentType ?? "image/jpeg");
    }

    private static async Task<IResult> GetSightingAudio(
        int id,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var sighting = await context.Sightings
            .Where(s => s.Id == id)
            .Select(s => new { s.AudioData, s.AudioContentType })
            .FirstOrDefaultAsync(ct);

        if (sighting?.AudioData is null)
        {
            return Results.NotFound();
        }

        return Results.File(sighting.AudioData, sighting.AudioContentType ?? "audio/mpeg");
    }

    private static async Task<IResult> ReviewSighting(
        int id,
        ReviewSightingRequest request,
        HttpContext httpContext,
        UserManager<User> userManager,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.Role != UserRole.Teacher && user.Role != UserRole.Admin)
        {
            return Results.Forbid();
        }

        var sighting = await context.Sightings.FindAsync([id], ct);
        if (sighting is null)
        {
            return Results.NotFound();
        }

        sighting.Status = Enum.Parse<SightingStatus>(request.Status);
        sighting.ReviewNotes = request.ReviewNotes;
        sighting.ReviewedAt = DateTime.UtcNow;
        sighting.ReviewedByUserId = user.Id;

        await context.SaveChangesAsync(ct);

        var dto = await GetSightingDtoAsync(context, id, ct);
        return Results.Ok(dto);
    }

    private static async Task<IResult> CreateUserSpecies(
        CreateUserSpeciesRequest request,
        HttpContext httpContext,
        UserManager<User> userManager,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var userSpecies = new UserSpecies
        {
            Id = 0,
            CommonName = request.CommonName,
            ScientificName = request.ScientificName,
            Description = request.Description,
            PhotoData = string.IsNullOrEmpty(request.PhotoBase64) ? null : Convert.FromBase64String(request.PhotoBase64),
            PhotoContentType = request.PhotoContentType,
            IsVerified = false,
            IsEndangered = false,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.UserSpecies.Add(userSpecies);
        await context.SaveChangesAsync(ct);

        var dto = await GetUserSpeciesDtoAsync(context, userSpecies.Id, ct);
        return Results.Created($"/api/wildlife/user-species/{userSpecies.Id}", dto);
    }

    private static async Task<IResult> GetUserSpecies(
        int id,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var dto = await GetUserSpeciesDtoAsync(context, id, ct);
        if (dto is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetUserSpeciesPhoto(
        int id,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var userSpecies = await context.UserSpecies
            .Where(us => us.Id == id)
            .Select(us => new { us.PhotoData, us.PhotoContentType })
            .FirstOrDefaultAsync(ct);

        if (userSpecies?.PhotoData is null)
        {
            return Results.NotFound();
        }

        return Results.File(userSpecies.PhotoData, userSpecies.PhotoContentType ?? "image/jpeg");
    }

    private static async Task<IResult> VerifyUserSpecies(
        int id,
        VerifyUserSpeciesRequest request,
        HttpContext httpContext,
        UserManager<User> userManager,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.Role != UserRole.Teacher && user.Role != UserRole.Admin)
        {
            return Results.Forbid();
        }

        var userSpecies = await context.UserSpecies.FindAsync([id], ct);
        if (userSpecies is null)
        {
            return Results.NotFound();
        }

        if (userSpecies.IsVerified)
        {
            return Results.BadRequest("This species has already been verified");
        }

        userSpecies.IsVerified = true;
        userSpecies.VerifiedAt = DateTime.UtcNow;
        userSpecies.VerifiedByUserId = user.Id;

        if (request.Approve)
        {
            userSpecies.IsEndangered = request.IsEndangered;
            userSpecies.ApprovedSpeciesId = request.ExistingSpeciesId;
        }

        await context.SaveChangesAsync(ct);

        var dto = await GetUserSpeciesDtoAsync(context, id, ct);
        return Results.Ok(dto);
    }

    private static async Task<IResult> GetReviewQueue(
        HttpContext httpContext,
        UserManager<User> userManager,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.Role != UserRole.Teacher && user.Role != UserRole.Admin)
        {
            return Results.Forbid();
        }

        var pendingSightings = await context.Sightings
            .Where(s => s.Status == SightingStatus.Pending)
            .OrderByDescending(s => s.IsFlaggedForReview)
            .ThenByDescending(s => s.CreatedAt)
            .Take(50)
            .Select(s => new SightingListDto(
                s.Id,
                s.Species != null ? s.Species.CommonName.First().Value : s.UserSpecies!.CommonName,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Location.Y,
                s.Location.X,
                s.Municipality != null ? s.Municipality.Name : null,
                s.ObservedAt,
                s.Status.ToString(),
                s.IsFlaggedForReview,
                s.IsNewMunicipalityRecord,
                s.ReportedByUser.DisplayName))
            .ToListAsync(ct);

        var pendingSpecies = await context.UserSpecies
            .Where(us => !us.IsVerified)
            .OrderByDescending(us => us.CreatedAt)
            .Take(50)
            .Select(us => new UserSpeciesDto(
                us.Id,
                us.CommonName,
                us.ScientificName,
                us.Description,
                us.PhotoData != null,
                us.IsVerified,
                us.IsEndangered,
                us.CreatedByUserId,
                us.CreatedByUser.DisplayName,
                us.CreatedAt,
                us.VerifiedAt,
                us.VerifiedByUserId,
                us.VerifiedByUser != null ? us.VerifiedByUser.DisplayName : null,
                us.ApprovedSpeciesId))
            .ToListAsync(ct);

        var totalSightings = await context.Sightings.CountAsync(s => s.Status == SightingStatus.Pending, ct);
        var totalPendingSpecies = await context.UserSpecies.CountAsync(us => !us.IsVerified, ct);

        var dto = new ReviewQueueDto(pendingSightings, pendingSpecies, totalSightings, totalPendingSpecies);
        return Results.Ok(dto);
    }

    private static async Task<IResult> GetMySightings(
        int? page,
        int? pageSize,
        HttpContext httpContext,
        UserManager<User> userManager,
        FaunaFinderContext context,
        CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var actualPage = page ?? 1;
        var actualPageSize = Math.Min(pageSize ?? 20, 100);

        var sightings = await context.Sightings
            .Where(s => s.ReportedByUserId == user.Id)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((actualPage - 1) * actualPageSize)
            .Take(actualPageSize)
            .Select(s => new SightingListDto(
                s.Id,
                s.Species != null ? s.Species.CommonName.First().Value : s.UserSpecies!.CommonName,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Location.Y,
                s.Location.X,
                s.Municipality != null ? s.Municipality.Name : null,
                s.ObservedAt,
                s.Status.ToString(),
                s.IsFlaggedForReview,
                s.IsNewMunicipalityRecord,
                s.ReportedByUser.DisplayName))
            .ToListAsync(ct);

        return Results.Ok(sightings);
    }

    private static async Task<SightingDto?> GetSightingDtoAsync(FaunaFinderContext context, int id, CancellationToken ct)
    {
        return await context.Sightings
            .Where(s => s.Id == id)
            .Select(s => new SightingDto(
                s.Id,
                s.SpeciesId,
                s.Species != null ? s.Species.CommonName.First().Value : null,
                s.UserSpeciesId,
                s.UserSpecies != null ? s.UserSpecies.CommonName : null,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                GetBehaviorStrings(s.Behaviors),
                GetEvidenceStrings(s.Evidence),
                s.Weather != null ? s.Weather.ToString() : null,
                s.Notes,
                s.Location.Y,
                s.Location.X,
                s.MunicipalityId,
                s.Municipality != null ? s.Municipality.Name : null,
                s.ObservedAt,
                s.CreatedAt,
                s.PhotoData != null,
                s.AudioData != null,
                s.Status.ToString(),
                s.IsFlaggedForReview,
                s.IsNewMunicipalityRecord,
                s.ReviewNotes,
                s.ReviewedAt,
                s.ReportedByUserId,
                s.ReportedByUser.DisplayName))
            .FirstOrDefaultAsync(ct);
    }

    private static async Task<UserSpeciesDto?> GetUserSpeciesDtoAsync(FaunaFinderContext context, int id, CancellationToken ct)
    {
        return await context.UserSpecies
            .Where(us => us.Id == id)
            .Select(us => new UserSpeciesDto(
                us.Id,
                us.CommonName,
                us.ScientificName,
                us.Description,
                us.PhotoData != null,
                us.IsVerified,
                us.IsEndangered,
                us.CreatedByUserId,
                us.CreatedByUser.DisplayName,
                us.CreatedAt,
                us.VerifiedAt,
                us.VerifiedByUserId,
                us.VerifiedByUser != null ? us.VerifiedByUser.DisplayName : null,
                us.ApprovedSpeciesId))
            .FirstOrDefaultAsync(ct);
    }

    private static string[] GetBehaviorStrings(Behavior behaviors)
    {
        var result = new List<string>();
        if (behaviors.HasFlag(Behavior.Feeding)) result.Add("Feeding");
        if (behaviors.HasFlag(Behavior.Resting)) result.Add("Resting");
        if (behaviors.HasFlag(Behavior.Moving)) result.Add("Moving");
        if (behaviors.HasFlag(Behavior.Calling)) result.Add("Calling");
        return result.ToArray();
    }

    private static string[] GetEvidenceStrings(EvidenceType evidence)
    {
        var result = new List<string>();
        if (evidence.HasFlag(EvidenceType.Visual)) result.Add("Visual");
        if (evidence.HasFlag(EvidenceType.Heard)) result.Add("Heard");
        if (evidence.HasFlag(EvidenceType.Tracks)) result.Add("Tracks");
        if (evidence.HasFlag(EvidenceType.Photo)) result.Add("Photo");
        return result.ToArray();
    }
}
