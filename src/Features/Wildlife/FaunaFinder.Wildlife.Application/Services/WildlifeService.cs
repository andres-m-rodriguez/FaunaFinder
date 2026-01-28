using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.Database.Models;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FaunaFinder.Wildlife.Application.Services;

public sealed class WildlifeService(
    IDbContextFactory<WildlifeDbContext> contextFactory,
    ISpeciesRepository speciesRepository,
    ISightingRepository sightingRepository) : IWildlifeService
{
    public async Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(string? query, int limit, CancellationToken cancellationToken = default)
    {
        if (limit <= 0) limit = 10;
        if (limit > 50) limit = 50;

        return await speciesRepository.SearchSpeciesAsync(query, limit, cancellationToken);
    }

    public async Task<CreateSightingResponse> CreateSightingAsync(CreateSightingRequest request, int userId, CancellationToken cancellationToken = default)
    {
        // Validate species exists
        var speciesExists = await speciesRepository.ExistsAsync(request.SpeciesId, cancellationToken);
        if (!speciesExists)
        {
            return new CreateSightingResponse(null, "Invalid species ID", false);
        }

        // Parse enums
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

    public async Task<SightingsPage> GetSightingsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        return await sightingRepository.GetSightingsAsync(page, pageSize, status, cancellationToken);
    }

    public async Task<SightingsPage> GetMySightingsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        return await sightingRepository.GetSightingsByUserAsync(userId, page, pageSize, cancellationToken);
    }

    public async Task<SightingsPage> GetReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        return await sightingRepository.GetReviewQueueAsync(page, pageSize, cancellationToken);
    }

    public async Task<(bool Success, string? Error)> ReviewSightingAsync(int sightingId, int reviewerUserId, string status, string? reviewNotes, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SightingStatus>(status, true, out var newStatus))
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
        sighting.ReviewNotes = reviewNotes;
        sighting.ReviewedAt = DateTime.UtcNow;
        sighting.ReviewedByUserId = reviewerUserId;

        await context.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<byte[]?> GetSightingPhotoAsync(int sightingId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var sighting = await context.Sightings
            .Where(s => s.Id == sightingId)
            .Select(s => new { s.PhotoData, s.PhotoContentType })
            .FirstOrDefaultAsync(cancellationToken);

        return sighting?.PhotoData;
    }
}
