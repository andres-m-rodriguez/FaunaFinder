using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class MunicipalityRepository(
    IDbContextFactory<WildlifeDbContext> contextFactory
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

    public async Task<CursorPage<MunicipalityCardDto>> GetMunicipalitiesCursorPageAsync(
        CursorPageRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Municipalities.AsNoTracking().AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(search));
        }

        // Apply cursor filter
        if (request.Cursor is not null && CursorHelper.TryDecode(request.Cursor, out var cursorId))
        {
            query = query.Where(m => m.Id > cursorId);
        }

        // Fetch one extra to determine HasMore
        var items = await query
            .OrderBy(m => m.Id)
            .Take(request.PageSize + 1)
            .Select(static m => new MunicipalityCardDto(
                m.Id,
                m.Name,
                m.MunicipalitySpecies.Count
            ))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > request.PageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var nextCursor = hasMore && items.Count > 0
            ? CursorHelper.Encode(items[^1].Id)
            : null;

        return new CursorPage<MunicipalityCardDto>(items, nextCursor, hasMore);
    }
}
