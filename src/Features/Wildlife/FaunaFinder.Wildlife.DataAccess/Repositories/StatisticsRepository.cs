using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class StatisticsRepository(IDbContextFactory<WildlifeDbContext> contextFactory)
    : IStatisticsRepository
{
    public async Task<PublicStatisticsDto> GetPublicStatisticsAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var approvedSightings = context.Sightings.Where(s => s.Status == SightingStatus.Approved);

        // Overview statistics
        var totalApprovedSightings = await approvedSightings.CountAsync(cancellationToken);

        var totalSpecies = await context.Species.CountAsync(cancellationToken);

        var totalMunicipalities = await context.Municipalities.CountAsync(cancellationToken);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var sightingsThisMonth = await approvedSightings
            .Where(s => s.ObservedAt >= startOfMonth)
            .CountAsync(cancellationToken);

        var overview = new StatisticsOverviewDto(
            totalApprovedSightings,
            totalSpecies,
            totalMunicipalities,
            sightingsThisMonth
        );

        // Sightings by month (last 12 months)
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
        var startDate = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var sightingsByMonthData = await approvedSightings
            .Where(s => s.ObservedAt >= startDate)
            .GroupBy(s => new { s.ObservedAt.Year, s.ObservedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var sightingsByMonth = sightingsByMonthData
            .Select(x => new SightingsByMonthDto(x.Year, x.Month, x.Count))
            .ToList();

        // Species by category
        var speciesByCategoryData = await context.SpeciesCategoryLinks
            .GroupBy(scl => scl.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var categoryIds = speciesByCategoryData.Select(x => x.CategoryId).ToList();
        var categories = await context.SpeciesCategories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.Name.ToList() })
            .ToListAsync(cancellationToken);

        var categoryNameLookup = categories.ToDictionary(
            x => x.Id,
            x => x.Name.FirstOrDefault(n => n.Code == "en")?.Value
                 ?? x.Name.FirstOrDefault()?.Value
                 ?? "Unknown");

        var speciesByCategory = speciesByCategoryData
            .Select(x => new SpeciesByCategoryDto(
                categoryNameLookup.GetValueOrDefault(x.CategoryId, "Unknown"),
                x.Count))
            .OrderByDescending(x => x.Count)
            .ToList();

        // Sightings by municipality (top 10)
        var sightingsByMunicipalityData = await approvedSightings
            .Where(s => s.MunicipalityId != null)
            .GroupBy(s => new { s.MunicipalityId, s.Municipality!.Name })
            .Select(g => new { g.Key.MunicipalityId, g.Key.Name, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        var sightingsByMunicipality = sightingsByMunicipalityData
            .Select(x => new SightingsByMunicipalityDto(x.MunicipalityId!.Value, x.Name, x.Count))
            .ToList();

        // Top observed species (top 10)
        var topSpeciesData = await approvedSightings
            .Where(s => s.SpeciesId != null)
            .GroupBy(s => s.SpeciesId)
            .Select(g => new { SpeciesId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        var speciesIds = topSpeciesData.Select(x => x.SpeciesId!.Value).ToList();
        var speciesNames = await context.Species
            .Where(s => speciesIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                CommonName = s.CommonName.ToList()
            })
            .ToListAsync(cancellationToken);

        var speciesNameLookup = speciesNames.ToDictionary(
            x => x.Id,
            x => x.CommonName.FirstOrDefault(c => c.Code == "en")?.Value
                 ?? x.CommonName.FirstOrDefault()?.Value
                 ?? "Unknown");

        var topSpecies = topSpeciesData
            .Select(x => new TopSpeciesDto(
                x.SpeciesId!.Value,
                speciesNameLookup.GetValueOrDefault(x.SpeciesId.Value, "Unknown"),
                x.Count))
            .ToList();

        return new PublicStatisticsDto(
            overview,
            sightingsByMonth,
            speciesByCategory,
            sightingsByMunicipality,
            topSpecies
        );
    }
}
