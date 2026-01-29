using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.Contracts.Requests;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.Database.Models;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class SightingRepository(IDbContextFactory<WildlifeDbContext> contextFactory) : ISightingRepository
{
    public async Task<SightingsPage> GetSightingsAsync(SightingsParameters parameters, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Sightings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Status) && Enum.TryParse<SightingStatus>(parameters.Status, true, out var statusFilter))
        {
            query = query.Where(s => s.Status == statusFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(s => new SightingListItem(
                s.Id,
                s.SpeciesId ?? 0,
                s.Species != null ? (s.Species.CommonName.FirstOrDefault(c => c.Code == "en") ?? s.Species.CommonName.FirstOrDefault())!.Value : null,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Status.ToString(),
                s.ObservedAt,
                s.CreatedAt,
                s.Location.Y,
                s.Location.X,
                s.PhotoData != null
            ))
            .ToListAsync(cancellationToken);

        return new SightingsPage(sightings, totalCount, parameters.Page, parameters.PageSize);
    }

    public async Task<SightingsPage> GetSightingsByUserAsync(UserSightingsParameters parameters, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Sightings
            .Where(s => s.ReportedByUserId == parameters.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(s => new SightingListItem(
                s.Id,
                s.SpeciesId ?? 0,
                s.Species != null ? (s.Species.CommonName.FirstOrDefault(c => c.Code == "en") ?? s.Species.CommonName.FirstOrDefault())!.Value : null,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Status.ToString(),
                s.ObservedAt,
                s.CreatedAt,
                s.Location.Y,
                s.Location.X,
                s.PhotoData != null
            ))
            .ToListAsync(cancellationToken);

        return new SightingsPage(sightings, totalCount, parameters.Page, parameters.PageSize);
    }

    public async Task<SightingsPage> GetReviewQueueAsync(ReviewQueueParameters parameters, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Sightings
            .Where(s => s.Status == SightingStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var sightings = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(s => new SightingListItem(
                s.Id,
                s.SpeciesId ?? 0,
                s.Species != null ? (s.Species.CommonName.FirstOrDefault(c => c.Code == "en") ?? s.Species.CommonName.FirstOrDefault())!.Value : null,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Status.ToString(),
                s.ObservedAt,
                s.CreatedAt,
                s.Location.Y,
                s.Location.X,
                s.PhotoData != null
            ))
            .ToListAsync(cancellationToken);

        return new SightingsPage(sightings, totalCount, parameters.Page, parameters.PageSize);
    }

    public async Task<SightingListItem?> GetSightingAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Sightings
            .Where(s => s.Id == id)
            .Select(s => new SightingListItem(
                s.Id,
                s.SpeciesId ?? 0,
                s.Species != null ? (s.Species.CommonName.FirstOrDefault(c => c.Code == "en") ?? s.Species.CommonName.FirstOrDefault())!.Value : null,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                s.Status.ToString(),
                s.ObservedAt,
                s.CreatedAt,
                s.Location.Y,
                s.Location.X,
                s.PhotoData != null
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SightingDetailDto?> GetSightingDetailAsync(int sightingId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Sightings
            .Where(s => s.Id == sightingId)
            .Select(s => new SightingDetailDto(
                s.Id,
                s.SpeciesId ?? 0,
                s.Species != null ? (s.Species.CommonName.FirstOrDefault(c => c.Code == "en") ?? s.Species.CommonName.FirstOrDefault())!.Value : null,
                s.Species != null ? s.Species.ScientificName : null,
                s.Mode.ToString(),
                s.Confidence.ToString(),
                s.Count.ToString(),
                (int)s.Behaviors,
                (int)s.Evidence,
                s.Weather != null ? s.Weather.ToString() : null,
                s.Notes,
                s.Location.Y,
                s.Location.X,
                s.MunicipalityId,
                s.ObservedAt,
                s.CreatedAt,
                s.PhotoData != null,
                s.Status.ToString(),
                s.IsFlaggedForReview,
                s.IsNewMunicipalityRecord,
                s.ReviewNotes,
                s.ReviewedAt,
                s.ReviewedByUserId,
                s.ReportedByUserId
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SightingPhotoResult?> GetSightingPhotoAsync(int sightingId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Sightings
            .Where(s => s.Id == sightingId)
            .Select(s => new { s.PhotoData, s.PhotoContentType })
            .FirstOrDefaultAsync(cancellationToken);

        if (result?.PhotoData is null || result.PhotoContentType is null)
        {
            return null;
        }

        return new SightingPhotoResult(result.PhotoData, result.PhotoContentType);
    }

    public async Task<CreateSightingResponse> CreateSightingAsync(CreateSightingRequest request, int userId, CancellationToken cancellationToken = default)
    {
        // Parse enums - validation should have already been performed by FluentValidation
        if (!Enum.TryParse<SightingMode>(request.Mode, true, out var sightingMode))
        {
            return new CreateSightingResponse(null, "Invalid mode", false);
        }

        if (!Enum.TryParse<ConfidenceLevel>(request.Confidence, true, out var confidenceLevel))
        {
            return new CreateSightingResponse(null, "Invalid confidence level", false);
        }

        if (!Enum.TryParse<CountRange>(request.Count, true, out var countRange))
        {
            return new CreateSightingResponse(null, "Invalid count range", false);
        }

        Weather? weatherEnum = null;
        if (!string.IsNullOrWhiteSpace(request.Weather))
        {
            if (!Enum.TryParse<Weather>(request.Weather, true, out var parsedWeather))
            {
                return new CreateSightingResponse(null, "Invalid weather", false);
            }
            weatherEnum = parsedWeather;
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var sighting = new Sighting
        {
            Id = 0,
            SpeciesId = request.SpeciesId,
            UserSpeciesId = null,
            Mode = sightingMode,
            Confidence = confidenceLevel,
            Count = countRange,
            Behaviors = (Behavior)request.Behaviors,
            Evidence = (EvidenceType)request.Evidence,
            Weather = weatherEnum,
            Notes = request.Notes,
            Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 },
            MunicipalityId = null,
            ObservedAt = DateTime.SpecifyKind(request.ObservedAt, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            PhotoData = request.PhotoData,
            PhotoContentType = request.PhotoContentType,
            AudioData = null,
            AudioContentType = null,
            Status = SightingStatus.Pending,
            IsFlaggedForReview = false,
            IsNewMunicipalityRecord = false,
            ReviewNotes = null,
            ReviewedAt = null,
            ReviewedByUserId = null,
            ReportedByUserId = userId
        };

        context.Sightings.Add(sighting);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateSightingResponse(sighting.Id, null, true);
    }

    public async Task<(bool Success, string? Error)> ReviewSightingAsync(int sightingId, ReviewSightingRequest request, int reviewerUserId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SightingStatus>(request.Status, true, out var newStatus))
        {
            return (false, "Invalid status");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var sighting = await context.Sightings.FindAsync([sightingId], cancellationToken);
        if (sighting is null)
        {
            return (false, "Sighting not found");
        }

        sighting.Status = newStatus;
        sighting.ReviewNotes = request.ReviewNotes;
        sighting.ReviewedAt = DateTime.UtcNow;
        sighting.ReviewedByUserId = reviewerUserId;

        await context.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateSightingPhotoAsync(int sightingId, int userId, byte[] photoData, string contentType, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var sighting = await context.Sightings
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == sightingId, cancellationToken);

        if (sighting is null)
        {
            return (false, "Sighting not found");
        }

        if (sighting.ReportedByUserId != userId)
        {
            return (false, "Not authorized");
        }

        sighting.PhotoData = photoData;
        sighting.PhotoContentType = contentType;

        await context.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
