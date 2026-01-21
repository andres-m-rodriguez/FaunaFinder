using FaunaFinder.Contracts.Dtos.Municipalities;
using FaunaFinder.Contracts.Parameters;
using FaunaFinder.Database;
using FaunaFinder.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.DataAccess.Repositories;

public sealed class MunicipalityRepository(
    IDbContextFactory<FaunaFinderContext> contextFactory
) : IMunicipalityRepository
{
    public async Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Municipalities
            .AsNoTracking()
            .OrderBy(static m => m.Name)
            .Select(static m => new MunicipalityForListDto(
                m.Id,
                m.Name,
                m.GeoJsonId
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Municipalities
            .AsNoTracking()
            .Where(m => m.Id == municipalityId)
            .Select(static m => new MunicipalityForDetailDto(
                m.Id,
                m.Name,
                m.GeoJsonId,
                m.MunicipalitySpecies.Count
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MunicipalityCardDto>> GetMunicipalitiesWithSpeciesCountAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Municipalities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(search));
        }

        return await query
            .OrderBy(static m => m.Name)
            .Skip(parameters.Page * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(static m => new MunicipalityCardDto(
                m.Id,
                m.Name,
                m.MunicipalitySpecies.Count
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Municipalities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower));
        }

        return await query.CountAsync(cancellationToken);
    }
}
