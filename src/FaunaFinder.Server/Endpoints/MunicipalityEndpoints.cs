using System.Text.Json;
using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FaunaFinder.Wildlife.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;

namespace FaunaFinder.Server.Endpoints;

public static class MunicipalityEndpoints
{
    public static void MapMunicipalityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/municipalities").WithTags("Municipalities");

        group.MapGet("/", GetAllMunicipalities).WithName("GetAllMunicipalities");
        group.MapGet("/{id:int}", GetMunicipalityDetail).WithName("GetMunicipalityDetail");
        group.MapGet("/cards", GetMunicipalityCards).WithName("GetMunicipalityCards");
        group.MapGet("/count", GetMunicipalitiesCount).WithName("GetMunicipalitiesCount");
        group.MapGet("/geojson", GetMunicipalitiesGeoJson).WithName("GetMunicipalitiesGeoJson");
        group.MapGet("/cursor", GetMunicipalitiesCursor).WithName("GetMunicipalitiesCursor");
    }

    private static async Task<Ok<IReadOnlyList<MunicipalityForListDto>>> GetAllMunicipalities(
        IMunicipalityRepository repository,
        CancellationToken ct
    )
    {
        var municipalities = await repository.GetAllMunicipalitiesAsync(ct);
        return TypedResults.Ok(municipalities);
    }

    private static async Task<
        Results<Ok<MunicipalityForDetailDto>, NotFound>
    > GetMunicipalityDetail(int id, IMunicipalityRepository repository, CancellationToken ct)
    {
        var municipality = await repository.GetMunicipalityDetailAsync(id, ct);
        return municipality is not null ? TypedResults.Ok(municipality) : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<MunicipalityCardDto>>> GetMunicipalityCards(
        [AsParameters] MunicipalityParameters parameters,
        IMunicipalityRepository repository,
        CancellationToken ct
    )
    {
        var municipalities = await repository.GetMunicipalitiesWithSpeciesCountAsync(
            parameters,
            ct
        );
        return TypedResults.Ok(municipalities);
    }

    private static async Task<Ok<int>> GetMunicipalitiesCount(
        string? search,
        IMunicipalityRepository repository,
        CancellationToken ct
    )
    {
        var count = await repository.GetTotalMunicipalitiesCountAsync(search, ct);
        return TypedResults.Ok(count);
    }

    private static async Task<IResult> GetMunicipalitiesGeoJson(
        IDbContextFactory<WildlifeDbContext> dbFactory
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var municipalities = await db
            .Municipalities.Where(m => m.Boundary != null)
            .Select(m => new
            {
                m.Name,
                m.GeoJsonId,
                m.Boundary,
            })
            .ToListAsync();

        var features = municipalities
            .Select(m =>
            {
                var geoJsonId = m.GeoJsonId;
                var state = geoJsonId.Length >= 2 ? geoJsonId[..2] : geoJsonId;
                var county = geoJsonId.Length > 2 ? geoJsonId[2..] : geoJsonId;

                return new
                {
                    type = "Feature",
                    properties = new
                    {
                        STATE = state,
                        COUNTY = county,
                        NAME = m.Name,
                    },
                    geometry = m.Boundary,
                };
            })
            .ToList();

        var featureCollection = new { type = "FeatureCollection", features };

        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new GeoJsonConverterFactory());

        return Results.Json(featureCollection, jsonOptions, contentType: "application/geo+json");
    }

    private static async Task<Ok<CursorPage<MunicipalityCardDto>>> GetMunicipalitiesCursor(
        [AsParameters] CursorPageParameter parameters,
        IMunicipalityRepository repository,
        CancellationToken ct
    )
    {
        var page = await repository.GetMunicipalitiesCursorPageAsync(parameters, ct);
        return TypedResults.Ok(page);
    }
}
