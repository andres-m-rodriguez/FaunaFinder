using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FaunaFinder.Server.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/statistics").WithTags("Statistics");

        group.MapGet("/", GetPublicStatistics).WithName("GetPublicStatistics");
    }

    private static async Task<Ok<PublicStatisticsDto>> GetPublicStatistics(
        IStatisticsRepository repository,
        CancellationToken ct)
    {
        var statistics = await repository.GetPublicStatisticsAsync(ct);
        return TypedResults.Ok(statistics);
    }
}
