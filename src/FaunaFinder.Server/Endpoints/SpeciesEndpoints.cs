using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FaunaFinder.Server.Endpoints;

public static class SpeciesEndpoints
{
    public static void MapSpeciesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/species").WithTags("Species");

        group.MapGet("/", GetSpecies).WithName("GetSpecies");
        group.MapGet("/{id:int}", GetSpeciesDetail).WithName("GetSpeciesDetail");
        group.MapGet("/by-municipality/{municipalityId:int}", GetSpeciesByMunicipality).WithName("GetSpeciesByMunicipality");
        group.MapGet("/count", GetSpeciesCount).WithName("GetSpeciesCount");
        group.MapGet("/nearby", GetSpeciesNearby).WithName("GetSpeciesNearby");
        group.MapGet("/categories", GetCategories).WithName("GetCategories");
    }

    private static IAsyncEnumerable<SpeciesForSearchDto> GetSpecies(
        [AsParameters] SpeciesParameters parameters,
        ISpeciesRepository repository,
        CancellationToken ct)
    {
        return repository.GetSpeciesAsync(parameters, ct);
    }

    private static async Task<Results<Ok<SpeciesForDetailDto>, NotFound>> GetSpeciesDetail(
        int id,
        ISpeciesRepository repository,
        CancellationToken ct)
    {
        var species = await repository.GetSpeciesDetailAsync(id, ct);
        return species is not null ? TypedResults.Ok(species) : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<SpeciesForListDto>>> GetSpeciesByMunicipality(
        int municipalityId,
        ISpeciesRepository repository,
        CancellationToken ct)
    {
        var species = await repository.GetSpeciesByMunicipalityAsync(municipalityId, ct);
        return TypedResults.Ok(species);
    }

    private static async Task<Ok<int>> GetSpeciesCount(
        string? search,
        ISpeciesRepository repository,
        CancellationToken ct)
    {
        var count = await repository.GetTotalSpeciesCountAsync(search, ct);
        return TypedResults.Ok(count);
    }

    private static async Task<Ok<IReadOnlyList<SpeciesNearbyDto>>> GetSpeciesNearby(
        [AsParameters] NearbySpeciesParameters parameters,
        ISpeciesRepository repository,
        CancellationToken ct)
    {
        var species = await repository.GetSpeciesNearbyAsync(
            parameters.Latitude,
            parameters.Longitude,
            parameters.RadiusMeters,
            ct);
        return TypedResults.Ok(species);
    }

    private static async Task<Ok<IReadOnlyList<SpeciesCategoryDto>>> GetCategories(
        ISpeciesRepository repository,
        CancellationToken ct)
    {
        var categories = await repository.GetAllCategoriesAsync(ct);
        return TypedResults.Ok(categories);
    }
}
