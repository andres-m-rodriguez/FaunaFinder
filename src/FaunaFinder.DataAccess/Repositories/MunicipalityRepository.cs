using FaunaFinder.Contracts.Dtos.Municipalities;
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
}
