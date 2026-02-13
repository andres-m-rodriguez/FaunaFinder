using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.Database;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class MunicipalityRepository(IDbContextFactory<WildlifeDbContext> contextFactory)
    : IMunicipalityRepository
{
    public async Task<IReadOnlyList<MunicipalityForListDto>> GetAllMunicipalitiesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .Municipalities.AsNoTracking()
            .OrderBy(static m => m.Name)
            .Select(static m => new MunicipalityForListDto(m.Id, m.Name, m.GeoJsonId))
            .ToListAsync(cancellationToken);
    }

    public async Task<MunicipalityForDetailDto?> GetMunicipalityDetailAsync(
        int municipalityId,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .Municipalities.AsNoTracking()
            .Where(m => m.Id == municipalityId)
            .Select(static m => new MunicipalityForDetailDto(
                m.Id,
                m.Name,
                m.GeoJsonId,
                m.MunicipalitySpecies.Count
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IAsyncEnumerable<MunicipalityCardDto> GetMunicipalitiesWithSpeciesCountAsync(
        MunicipalityParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        return GetMunicipalitiesInternalAsync(parameters, cancellationToken);
    }

    private async IAsyncEnumerable<MunicipalityCardDto> GetMunicipalitiesInternalAsync(
        MunicipalityParameters parameters,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Municipalities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(search));
        }

        // Apply cursor filter
        if (parameters.Cursor.HasValue)
        {
            query = query.Where(m => m.Id > parameters.Cursor.Value);
        }

        // Fetch PageSize + 1 for client-side HasMore detection
        await foreach (var municipality in query
            .OrderBy(m => m.Id)
            .Take(parameters.PageSize + 1)
            .Select(static m => new MunicipalityCardDto(m.Id, m.Name, m.MunicipalitySpecies.Count))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            yield return municipality;
        }
    }

    public async Task<int> GetTotalMunicipalitiesCountAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
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
