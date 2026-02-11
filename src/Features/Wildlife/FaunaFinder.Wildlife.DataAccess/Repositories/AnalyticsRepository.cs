using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.Database.Models;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class AnalyticsRepository(IDbContextFactory<WildlifeDbContext> contextFactory) : IAnalyticsRepository
{
    public async Task<IReadOnlyList<MunicipalitySpeciesCountDto>> GetSpeciesPerMunicipalityAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Municipalities
            .AsNoTracking()
            .Select(static m => new
            {
                m.Id,
                m.Name,
                SpeciesCount = m.MunicipalitySpecies.Count
            })
            .OrderBy(static m => m.Name)
            .Select(static m => new MunicipalitySpeciesCountDto(
                m.Id,
                m.Name,
                m.SpeciesCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesMunicipalityCountDto>> GetMunicipalitiesPerSpeciesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Species
            .AsNoTracking()
            .Select(static s => new
            {
                s.Id,
                CommonName = s.CommonName.FirstOrDefault() != null ? s.CommonName.First().Value : s.ScientificName,
                s.ScientificName,
                MunicipalityCount = s.MunicipalitySpecies.Count
            })
            .Where(static s => s.MunicipalityCount > 0)
            .OrderBy(static s => s.CommonName)
            .Select(static s => new SpeciesMunicipalityCountDto(
                s.Id,
                s.CommonName,
                s.ScientificName,
                s.MunicipalityCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesSightingCountDto>> GetTopSightedSpeciesAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Query from Sightings, group by SpeciesId, and join with Species for names
        return await context.Sightings
            .AsNoTracking()
            .Where(s => s.SpeciesId != null && s.Status == SightingStatus.Approved)
            .GroupBy(s => s.SpeciesId)
            .Select(g => new
            {
                SpeciesId = g.Key!.Value,
                ApprovedSightingCount = g.Count()
            })
            .OrderByDescending(g => g.ApprovedSightingCount)
            .Take(limit)
            .Join(
                context.Species.AsNoTracking(),
                g => g.SpeciesId,
                s => s.Id,
                (g, s) => new SpeciesSightingCountDto(
                    s.Id,
                    s.CommonName.FirstOrDefault() != null ? s.CommonName.First().Value : s.ScientificName,
                    s.ScientificName,
                    g.ApprovedSightingCount))
            .ToListAsync(cancellationToken);
    }
}
