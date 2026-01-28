using FaunaFinder.Wildlife.Contracts;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface ISpeciesRepository
{
    Task<IReadOnlyList<SpeciesSearchResult>> SearchSpeciesAsync(string? query, int limit, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int speciesId, CancellationToken cancellationToken = default);
}
