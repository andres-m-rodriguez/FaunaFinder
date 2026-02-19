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

        var sightingsByMonth = await approvedSightings
            .Where(s => s.ObservedAt >= startDate)
            .GroupBy(s => new { s.ObservedAt.Year, s.ObservedAt.Month })
            .Select(g => new SightingsByMonthDto(g.Key.Year, g.Key.Month, g.Count()))
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        // Species by category (fauna vs flora)
        var speciesByCategory = await context.Species
            .GroupBy(s => s.IsFauna)
            .Select(g => new SpeciesByCategoryDto(
                g.Key ? "Fauna" : "Flora",
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        // Sightings by municipality (top 10)
        var sightingsByMunicipality = await approvedSightings
            .Where(s => s.MunicipalityId != null)
            .GroupBy(s => new { s.MunicipalityId, s.Municipality!.Name })
            .Select(g => new SightingsByMunicipalityDto(
                g.Key.MunicipalityId!.Value,
                g.Key.Name,
                g.Count()
            ))
            .OrderByDescending(x => x.SightingsCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Top observed species (top 10)
        var topSpecies = await approvedSightings
            .Where(s => s.SpeciesId != null && s.Species != null)
            .GroupBy(s => new
            {
                s.SpeciesId,
                Name = s.Species!.CommonName.FirstOrDefault(c => c.Code == "en")
                       ?? s.Species.CommonName.FirstOrDefault()
            })
            .Select(g => new TopSpeciesDto(
                g.Key.SpeciesId!.Value,
                g.Key.Name != null ? g.Key.Name.Value : "Unknown",
                g.Count()
            ))
            .OrderByDescending(x => x.SightingsCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new PublicStatisticsDto(
            overview,
            sightingsByMonth,
            speciesByCategory,
            sightingsByMunicipality,
            topSpecies
        );
    }
}
