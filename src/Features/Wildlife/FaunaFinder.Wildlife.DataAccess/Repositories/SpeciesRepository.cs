using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.Database;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class SpeciesRepository(IDbContextFactory<WildlifeDbContext> contextFactory)
    : ISpeciesRepository
{
    public async Task<bool> ExistsAsync(int speciesId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Species.AnyAsync(s => s.Id == speciesId, cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesForListDto>> GetSpeciesByMunicipalityAsync(
        int municipalityId,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .MunicipalitySpecies.AsNoTracking()
            .Where(ms => ms.MunicipalityId == municipalityId)
            .OrderBy(ms => ms.Species.ScientificName)
            .Select(ms => new SpeciesForListDto(
                ms.Species.Id,
                ms.Species.CommonName.ToList(),
                ms.Species.ScientificName,
                ms.Species.FwsLinks.Select(fl => new FwsLinkDto(
                        fl.Id,
                        new NrcsPracticeDto(
                            fl.NrcsPractice.Id,
                            fl.NrcsPractice.Code,
                            fl.NrcsPractice.Name.ToList()
                        ),
                        new FwsActionDto(fl.FwsAction.Id, fl.FwsAction.Code, fl.FwsAction.Name.ToList()),
                        fl.Justification.ToList()
                    ))
                    .ToList(),
                ms.Species.IsFauna
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(
        int speciesId,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Use split query to avoid cartesian explosion with multiple collections
        return await context
            .Species.AsNoTracking()
            .AsSplitQuery()
            .Where(s => s.Id == speciesId)
            .Select(s => new SpeciesForDetailDto(
                s.Id,
                s.CommonName.ToList(),
                s.ScientificName,
                s.FwsLinks.Select(fl => new FwsLinkDto(
                        fl.Id,
                        new NrcsPracticeDto(
                            fl.NrcsPractice.Id,
                            fl.NrcsPractice.Code,
                            fl.NrcsPractice.Name.ToList()
                        ),
                        new FwsActionDto(fl.FwsAction.Id, fl.FwsAction.Code, fl.FwsAction.Name.ToList()),
                        fl.Justification.ToList()
                    ))
                    .ToList(),
                s.MunicipalitySpecies.OrderBy(ms => ms.Municipality.Name)
                    .Select(ms => new SpeciesMunicipalityDto(
                        ms.Municipality.Id,
                        ms.Municipality.Name
                    ))
                    .ToList(),
                s.Locations.Select(l => new SpeciesLocationDto(
                        l.Id,
                        l.Latitude,
                        l.Longitude,
                        l.RadiusMeters,
                        l.Description
                    ))
                    .ToList(),
                s.ProfileImageData != null,
                s.ImageSourceUrl,
                s.IsFauna
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IAsyncEnumerable<SpeciesForSearchDto> GetSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        return GetSpeciesInternalAsync(parameters, cancellationToken);
    }

    private async IAsyncEnumerable<SpeciesForSearchDto> GetSpeciesInternalAsync(
        SpeciesParameters parameters,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();

        // Apply category filter (comma-separated IDs)
        if (!string.IsNullOrWhiteSpace(parameters.CategoryIds))
        {
            var categoryIds = parameters.CategoryIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => int.TryParse(id, out var parsed) ? parsed : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (categoryIds.Count > 0)
            {
                query = query.Where(s =>
                    s.CategoryLinks.Any(cl => categoryIds.Contains(cl.CategoryId))
                );
            }
        }

        // Apply search filter (name contains)
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLower();
            query = query.Where(s =>
                s.CommonName.Any(cn => cn.Value.ToLower().Contains(search))
                || s.ScientificName.ToLower().Contains(search)
            );
        }

        if (parameters.MunicipalityId.HasValue)
        {
            query = query.Where(s =>
                s.MunicipalitySpecies.Any(ms =>
                    ms.MunicipalityId == parameters.MunicipalityId.Value
                )
            );
        }

        // Apply cursor filter
        if (parameters.Cursor.HasValue)
        {
            query = query.Where(s => s.Id > parameters.Cursor.Value);
        }

        // Fetch PageSize + 1 for client-side HasMore detection
        await foreach (var species in query
            .OrderBy(s => s.Id)
            .Take(parameters.PageSize + 1)
            .Select(s => new SpeciesForSearchDto(
                s.Id,
                s.CommonName.ToList(),
                s.ScientificName,
                s.MunicipalitySpecies.Select(ms => ms.Municipality.Name).OrderBy(n => n).ToList(),
                s.IsFauna,
                s.ProfileImageData != null,
                s.GRank ?? string.Empty,
                s.SRank ?? string.Empty
            ))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            yield return species;
        }
    }

    public async Task<int> GetTotalSpeciesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(s =>
                s.CommonName.Any(cn => cn.Value.ToLower().Contains(searchLower))
                || s.ScientificName.ToLower().Contains(searchLower)
            );
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesNearbyAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Get all species locations - we'll filter by distance in memory
        // because SQLite doesn't have native geospatial functions
        var speciesWithLocations = await context
            .Species.AsNoTracking()
            .Where(s => s.Locations.Any())
            .Select(s => new
            {
                s.Id,
                CommonName = s.CommonName.ToList(),
                s.ScientificName,
                Locations = s
                    .Locations.Select(l => new
                    {
                        l.Latitude,
                        l.Longitude,
                        l.RadiusMeters,
                        l.Description,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        // Calculate distances and filter by radius
        var results = new List<SpeciesNearbyDto>();

        foreach (var species in speciesWithLocations)
        {
            foreach (var location in species.Locations)
            {
                var distance = CalculateHaversineDistance(
                    latitude,
                    longitude,
                    location.Latitude,
                    location.Longitude
                );

                // Check if the species location circle overlaps with the search radius
                // The distance to the edge of the species circle should be within our search radius
                var effectiveDistance = distance - location.RadiusMeters;
                if (effectiveDistance < 0)
                    effectiveDistance = 0;

                if (effectiveDistance <= radiusMeters)
                {
                    results.Add(
                        new SpeciesNearbyDto(
                            species.Id,
                            species.CommonName,
                            species.ScientificName,
                            distance,
                            location.Latitude,
                            location.Longitude,
                            location.RadiusMeters,
                            location.Description
                        )
                    );
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
        double lat1,
        double lon1,
        double lat2,
        double lon2
    )
    {
        const double EarthRadiusMeters = 6371000;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(DegreesToRadians(lat1))
                * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2)
                * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public async Task<IReadOnlyList<SpeciesNearbyDto>> GetSpeciesInPolygonAsync(
        IReadOnlyList<PolygonCoordinate> coordinates,
        CancellationToken cancellationToken = default
    )
    {
        if (coordinates.Count < 3)
            return [];

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Get all species locations
        var speciesWithLocations = await context
            .Species.AsNoTracking()
            .Where(s => s.Locations.Any())
            .Select(s => new
            {
                s.Id,
                CommonName = s.CommonName.ToList(),
                s.ScientificName,
                Locations = s
                    .Locations.Select(l => new
                    {
                        l.Latitude,
                        l.Longitude,
                        l.RadiusMeters,
                        l.Description,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        // Calculate polygon centroid for distance calculation
        var centroidLat = coordinates.Average(c => c.Latitude);
        var centroidLng = coordinates.Average(c => c.Longitude);

        // Filter by point-in-polygon test
        var results = new List<SpeciesNearbyDto>();

        foreach (var species in speciesWithLocations)
        {
            foreach (var location in species.Locations)
            {
                // Check if the location center is inside the polygon
                if (IsPointInPolygon(location.Latitude, location.Longitude, coordinates))
                {
                    // Calculate distance from polygon centroid
                    var distance = CalculateHaversineDistance(
                        centroidLat,
                        centroidLng,
                        location.Latitude,
                        location.Longitude
                    );

                    results.Add(
                        new SpeciesNearbyDto(
                            species.Id,
                            species.CommonName,
                            species.ScientificName,
                            distance,
                            location.Latitude,
                            location.Longitude,
                            location.RadiusMeters,
                            location.Description
                        )
                    );
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
    /// Determines if a point is inside a polygon using the ray casting algorithm.
    /// </summary>
    private static bool IsPointInPolygon(
        double latitude,
        double longitude,
        IReadOnlyList<PolygonCoordinate> polygon
    )
    {
        var inside = false;
        var j = polygon.Count - 1;

        for (var i = 0; i < polygon.Count; j = i++)
        {
            var xi = polygon[i].Longitude;
            var yi = polygon[i].Latitude;
            var xj = polygon[j].Longitude;
            var yj = polygon[j].Latitude;

            if (((yi > latitude) != (yj > latitude)) &&
                (longitude < (xj - xi) * (latitude - yi) / (yj - yi) + xi))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public async Task<IReadOnlyList<SpeciesCategoryDto>> GetAllCategoriesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SpeciesCategories
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new SpeciesCategoryDto(c.Id, c.Code, c.Name.ToList()))
            .ToListAsync(cancellationToken);
    }
}
