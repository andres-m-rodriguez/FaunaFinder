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
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesForSearchDto>> SearchSpeciesAsync(
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

        // Keyset pagination
        if (parameters.FromCursor.HasValue)
        {
            query = query.Where(s => s.Id > parameters.FromCursor.Value);
        }

        // Project and return with municipality names
        return await query
            .OrderBy(static s => s.CommonName)
            .Take(parameters.Limit ?? 50)
            .Select(static s => new SpeciesForSearchDto(
                s.Id,
                s.CommonName,
                s.ScientificName,
                s.MunicipalitySpecies
                    .Select(ms => ms.Municipality.Name)
                    .OrderBy(n => n)
                    .ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}
