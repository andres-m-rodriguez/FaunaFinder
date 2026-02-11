using FaunaFinder.Wildlife.DataAccess.Interfaces;

namespace FaunaFinder.Server.Endpoints;
public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/analytics")
            .WithTags("Analytics");

        group.MapGet("/species-per-municipality", async (
            IAnalyticsRepository repository,
            CancellationToken ct) =>
        {
            var data = await repository.GetSpeciesPerMunicipalityAsync(ct);
            return Results.Ok(data);
        }).WithName("GetSpeciesPerMunicipality");

        group.MapGet("/municipalities-per-species", async (
            IAnalyticsRepository repository,
            CancellationToken ct) =>
        {
            var data = await repository.GetMunicipalitiesPerSpeciesAsync(ct);
            return Results.Ok(data);
        }).WithName("GetMunicipalitiesPerSpecies");

        group.MapGet("/top-sighted-species", async (
            int limit,
            IAnalyticsRepository repository,
            CancellationToken ct) =>
        {
            var data = await repository.GetTopSightedSpeciesAsync(limit, ct);
            return Results.Ok(data);
        }).WithName("GetTopSightedSpecies");
    }
}
