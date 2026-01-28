using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.Database.Models;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class SightingRepository(IDbContextFactory<WildlifeDbContext> contextFactory) : ISightingRepository
{
    public async Task<SightingsPage> GetSightingsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Sightings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SightingStatus>(status, true, out var statusFilter))
        {
            query = query.Where(s => s.Status == statusFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new SightingsPage(sightings, totalCount, page, pageSize);
    }

    public async Task<SightingsPage> GetSightingsByUserAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Sightings
            .Where(s => s.ReportedByUserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var sightings = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new SightingsPage(sightings, totalCount, page, pageSize);
    }

    public async Task<SightingsPage> GetReviewQueueAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Sightings
            .Where(s => s.Status == SightingStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var sightings = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new SightingsPage(sightings, totalCount, page, pageSize);
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
}
