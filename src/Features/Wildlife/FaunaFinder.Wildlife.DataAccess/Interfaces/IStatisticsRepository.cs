using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface IStatisticsRepository
{
    Task<PublicStatisticsDto> GetPublicStatisticsAsync(CancellationToken cancellationToken = default);
}
