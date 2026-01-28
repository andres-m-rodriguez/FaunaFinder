using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class SpeciesRepository(IDbContextFactory<WildlifeDbContext> contextFactory) : ISpeciesRepository
{
    public async Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(string? query, int limit, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var speciesQuery = context.Species.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var search = query.ToLowerInvariant();
            speciesQuery = speciesQuery.Where(s =>
                s.CommonName.Any(cn => cn.Value.ToLower().Contains(search)) ||
                s.ScientificName.ToLower().Contains(search));
        }

        var species = await speciesQuery
            .OrderBy(s => s.ScientificName)
            .Take(limit)
            .Select(s => new SpeciesSearchResult(
                s.Id,
                s.CommonName.ToList(),
                s.ScientificName
            ))
            .ToListAsync(cancellationToken);

        return species;
    }

    public async Task<bool> ExistsAsync(int speciesId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Species.AnyAsync(s => s.Id == speciesId, cancellationToken);
    }
}
