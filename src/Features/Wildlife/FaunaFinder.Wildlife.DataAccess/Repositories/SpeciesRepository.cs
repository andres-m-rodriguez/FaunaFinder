using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.Database.Models;
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
                            fl.NrcsPractice.Name
                        ),
                        new FwsActionDto(fl.FwsAction.Id, fl.FwsAction.Code, fl.FwsAction.Name),
                        fl.Justification
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<SpeciesForDetailDto?> GetSpeciesDetailAsync(
        int speciesId,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // NO .Include() - Project everything in one query
        return await context
            .Species.AsNoTracking()
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
                            fl.NrcsPractice.Name
                        ),
                        new FwsActionDto(fl.FwsAction.Id, fl.FwsAction.Code, fl.FwsAction.Name),
                        fl.Justification
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
                s.ImageSourceUrl
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesForSearchDto>> GetSpeciesAsync(
        SpeciesParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();
        var search = parameters.Search?.Trim().ToLower();
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        // Apply keyword filter (comma-separated string)
        if (!string.IsNullOrWhiteSpace(parameters.Keywords))
        {
            var keywords = parameters.Keywords
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(k => k.ToLower())
                .ToList();
            query = query.Where(s => s.SearchKeywords.Any(sk => keywords.Contains(sk.ToLower())));
        }

        // Apply search filter (fuzzy or exact)
        if (hasSearch)
        {
            if (parameters.FuzzySearch)
            {
                // Fuzzy search using pg_trgm + keyword matching
                query = query.Where(s =>
                    (s.SearchText != null && EF.Functions.TrigramsSimilarity(s.SearchText, search!) > 0.2)
                    || s.SearchKeywords.Any(k => k.ToLower().Contains(search!))
                );
            }
            else
            {
                // Exact substring search (fallback)
                query = query.Where(s =>
                    s.CommonName.Any(cn => cn.Value.ToLower().Contains(search!))
                    || s.ScientificName.ToLower().Contains(search!)
                );
            }
        }

        if (parameters.MunicipalityId.HasValue)
        {
            query = query.Where(s =>
                s.MunicipalitySpecies.Any(ms =>
                    ms.MunicipalityId == parameters.MunicipalityId.Value
                )
            );
        }

        // Order by relevance score when searching, otherwise by name
        IOrderedQueryable<Species> orderedQuery;
        if (hasSearch && parameters.FuzzySearch)
        {
            orderedQuery = query.OrderByDescending(s =>
                s.SearchText != null ? EF.Functions.TrigramsSimilarity(s.SearchText, search!) : 0
            );
        }
        else
        {
            orderedQuery = query.OrderBy(s => s.ScientificName);
        }

        // Project and return with municipality names
        return await orderedQuery
            .Skip(parameters.Page * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(s => new SpeciesForSearchDto(
                s.Id,
                s.CommonName.ToList(),
                s.ScientificName,
                s.MunicipalitySpecies.Select(ms => ms.Municipality.Name).OrderBy(n => n).ToList()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalSpeciesCountAsync(
        string? search = null,
        bool fuzzySearch = true,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            if (fuzzySearch)
            {
                query = query.Where(s =>
                    (s.SearchText != null && EF.Functions.TrigramsSimilarity(s.SearchText, searchLower) > 0.2)
                    || s.SearchKeywords.Any(k => k.ToLower().Contains(searchLower))
                );
            }
            else
            {
                query = query.Where(s =>
                    s.CommonName.Any(cn => cn.Value.ToLower().Contains(searchLower))
                    || s.ScientificName.ToLower().Contains(searchLower)
                );
            }
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

    public async Task<CursorPage<SpeciesForSearchDto>> GetSpeciesCursorPageAsync(
        CursorPageParameter request,
        bool fuzzySearch = true,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Species.AsNoTracking().AsQueryable();
        var search = request.Search?.Trim().ToLower();
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        // Apply search filter (fuzzy or exact)
        if (hasSearch)
        {
            if (fuzzySearch)
            {
                query = query.Where(s =>
                    (s.SearchText != null && EF.Functions.TrigramsSimilarity(s.SearchText, search!) > 0.2)
                    || s.SearchKeywords.Any(k => k.ToLower().Contains(search!))
                );
            }
            else
            {
                query = query.Where(s =>
                    s.CommonName.Any(cn => cn.Value.ToLower().Contains(search!))
                    || s.ScientificName.ToLower().Contains(search!)
                );
            }
        }

        // Apply cursor filter
        if (request.Cursor is not null && CursorHelper.TryDecode(request.Cursor, out var cursorId))
        {
            query = query.Where(s => s.Id > cursorId);
        }

        // Fetch one extra to determine HasMore
        var items = await query
            .OrderBy(s => s.Id)
            .Take(request.PageSize + 1)
            .Select(s => new SpeciesForSearchDto(
                s.Id,
                s.CommonName.ToList(),
                s.ScientificName,
                s.MunicipalitySpecies.Select(ms => ms.Municipality.Name).OrderBy(n => n).ToList()
            ))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > request.PageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var nextCursor = hasMore && items.Count > 0 ? CursorHelper.Encode(items[^1].Id) : null;

        return new CursorPage<SpeciesForSearchDto>(items, nextCursor, hasMore);
    }
}
