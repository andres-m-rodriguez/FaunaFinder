using FaunaFinder.Contracts.Parameters;
using FaunaFinder.DataAccess.Interfaces;
using FaunaFinder.Pagination.Contracts;

namespace FaunaFinder.Api.Endpoints;

public static class SpeciesEndpoints
{
    public static void MapSpeciesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/species")
            .WithTags("Species");

        group.MapGet("/", async (
            int pageSize,
            int page,
            string? search,
            int? municipalityId,
            ISpeciesRepository repository,
            CancellationToken ct) =>
        {
            var parameters = new SpeciesParameters(pageSize, page, search, municipalityId);
            var species = await repository.GetSpeciesAsync(parameters, ct);
            return Results.Ok(species);
        }).WithName("GetSpecies");

        group.MapGet("/{id:int}", async (int id, ISpeciesRepository repository, CancellationToken ct) =>
        {
            var species = await repository.GetSpeciesDetailAsync(id, ct);
            return species is not null ? Results.Ok(species) : Results.NotFound();
        }).WithName("GetSpeciesDetail");

        group.MapGet("/by-municipality/{municipalityId:int}", async (
            int municipalityId,
            ISpeciesRepository repository,
            CancellationToken ct) =>
        {
            var species = await repository.GetSpeciesByMunicipalityAsync(municipalityId, ct);
            return Results.Ok(species);
        }).WithName("GetSpeciesByMunicipality");

        group.MapGet("/count", async (string? search, ISpeciesRepository repository, CancellationToken ct) =>
        {
            var count = await repository.GetTotalSpeciesCountAsync(search, ct);
            return Results.Ok(count);
        }).WithName("GetSpeciesCount");

        group.MapGet("/nearby", async (
            double latitude,
            double longitude,
            double radiusMeters,
            ISpeciesRepository repository,
            CancellationToken ct) =>
        {
            var species = await repository.GetSpeciesNearbyAsync(latitude, longitude, radiusMeters, ct);
            return Results.Ok(species);
        }).WithName("GetSpeciesNearby");

        group.MapGet("/cursor", async (
            string? cursor,
            int pageSize,
            string? search,
            ISpeciesRepository repository,
            CancellationToken ct) =>
        {
            var request = new CursorPageRequest(cursor, pageSize, search);
            var page = await repository.GetSpeciesCursorPageAsync(request, ct);
            return Results.Ok(page);
        }).WithName("GetSpeciesCursor");
    }
}
