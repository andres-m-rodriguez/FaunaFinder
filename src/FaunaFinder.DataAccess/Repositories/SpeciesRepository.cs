using FaunaFinder.Contracts.Dtos.FwsActions;
using FaunaFinder.Contracts.Dtos.FwsLinks;
using FaunaFinder.Contracts.Dtos.NrcsPractices;
using FaunaFinder.Contracts.Dtos.Species;
using FaunaFinder.Contracts.Parameters;
using FaunaFinder.Database;
using FaunaFinder.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.DataAccess.Repositories;

public sealed class SpeciesRepository(
    IDbContextFactory<FaunaFinderContext> contextFactory
) : ISpeciesRepository
{
    public async Task<IReadOnlyList<SpeciesForListDto>> GetSpeciesByMunicipalityAsync(
        int municipalityId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.MunicipalitySpecies
            .AsNoTracking()
            .Where(ms => ms.MunicipalityId == municipalityId)
            .OrderBy(ms => ms.Species.CommonName)
            .Select(ms => new SpeciesForListDto(
                ms.Species.Id,
                ms.Species.CommonName,
                ms.Species.ScientificName,
                ms.Species.FwsLinks.Select(fl => new FwsLinkDto(
                    fl.Id,
                    new NrcsPracticeDto(
                        fl.NrcsPractice.Id,
                        fl.NrcsPractice.Code,
                        fl.NrcsPractice.Name
                    ),
                    new FwsActionDto(
                        fl.FwsAction.Id,
                        fl.FwsAction.Code,
                        fl.FwsAction.Name
                    ),
                    fl.Justification
                )).ToList(),
                ms.Species.Translations.Select(t => new SpeciesTranslationDto(
                    t.LanguageCode,
                    t.CommonName
                )).ToList()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(
        int speciesId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // NO .Include() - Project everything in one query
        return await context.Species
            .AsNoTracking()
            .Where(s => s.Id == speciesId)
            .Select(static s => new SpeciesForDetailDto(
                s.Id,
                s.CommonName,
                s.ScientificName,
                s.FwsLinks.Select(static fl => new FwsLinkDto(
                    fl.Id,
                    new NrcsPracticeDto(
                        fl.NrcsPractice.Id,
                        fl.NrcsPractice.Code,
                        fl.NrcsPractice.Name
                    ),
                    new FwsActionDto(
                        fl.FwsAction.Id,
                        fl.FwsAction.Code,
                        fl.FwsAction.Name
                    ),
                    fl.Justification
                )).ToList(),
                s.MunicipalitySpecies
                    .OrderBy(ms => ms.Municipality.Name)
                    .Select(ms => new SpeciesMunicipalityDto(
                        ms.Municipality.Id,
                        ms.Municipality.Name
                    ))
                    .ToList(),
                s.Locations
                    .Select(static l => new SpeciesLocationDto(
                        l.Id,
                        l.Latitude,
                        l.Longitude,
                        l.RadiusMeters,
                        l.Description
                    ))
                    .ToList(),
                s.Translations
                    .Select(static t => new SpeciesTranslationDto(
                        t.LanguageCode,
                        t.CommonName
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesForSearchDto>> GetSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.ToLower();
            query = query.Where(s =>
                s.CommonName.ToLower().Contains(search) ||
                s.ScientificName.ToLower().Contains(search));
        }

        if (parameters.MunicipalityId.HasValue)
        {
            query = query.Where(s =>
                s.MunicipalitySpecies.Any(ms =>
                    ms.MunicipalityId == parameters.MunicipalityId.Value));
        }

        // Project and return with municipality names
        return await query
            .OrderBy(static s => s.CommonName)
            .Skip(parameters.Page * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(static s => new SpeciesForSearchDto(
                s.Id,
                s.CommonName,
                s.ScientificName,
                s.MunicipalitySpecies
                    .Select(ms => ms.Municipality.Name)
                    .OrderBy(n => n)
                    .ToList(),
                s.Translations
                    .Select(t => new SpeciesTranslationDto(
                        t.LanguageCode,
                        t.CommonName
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalSpeciesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(s =>
                s.CommonName.ToLower().Contains(searchLower) ||
                s.ScientificName.ToLower().Contains(searchLower));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesNearbyAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Get all species locations - we'll filter by distance in memory
        // because SQLite doesn't have native geospatial functions
        var speciesWithLocations = await context.Species
            .AsNoTracking()
            .Where(s => s.Locations.Any())
            .Select(s => new
            {
                s.Id,
                s.CommonName,
                s.ScientificName,
                Locations = s.Locations.Select(l => new
                {
                    l.Latitude,
                    l.Longitude,
                    l.RadiusMeters,
                    l.Description
                }).ToList(),
                Translations = s.Translations.Select(t => new SpeciesTranslationDto(
                    t.LanguageCode,
                    t.CommonName
                )).ToList()
            })
            .ToListAsync(cancellationToken);

        // Calculate distances and filter by radius
        var results = new List<SpeciesNearbyDto>();

        foreach (var species in speciesWithLocations)
        {
            foreach (var location in species.Locations)
            {
                var distance = CalculateHaversineDistance(
                    latitude, longitude,
                    location.Latitude, location.Longitude);

                // Check if the species location circle overlaps with the search radius
                // The distance to the edge of the species circle should be within our search radius
                var effectiveDistance = distance - location.RadiusMeters;
                if (effectiveDistance < 0) effectiveDistance = 0;

                if (effectiveDistance <= radiusMeters)
                {
                    results.Add(new SpeciesNearbyDto(
                        species.Id,
                        species.CommonName,
                        species.ScientificName,
                        distance,
                        location.Latitude,
                        location.Longitude,
                        location.RadiusMeters,
                        location.Description,
                        species.Translations
                    ));
                }
            }
        }

        // Return distinct species by ID, keeping the closest location for each
        return results
            .GroupBy(r => r.Id)
            .Select(g => g.OrderBy(r => r.DistanceMeters).First())
            .OrderBy(r => r.DistanceMeters)
            .ToList();
    }

    /// <summary>
    /// Calculates the distance between two points using the Haversine formula.
    /// </summary>
    private static double CalculateHaversineDistance(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        const double EarthRadiusMeters = 6371000;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
