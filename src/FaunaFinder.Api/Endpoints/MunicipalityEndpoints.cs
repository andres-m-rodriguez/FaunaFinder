using System.Text.Json;
using FaunaFinder.Pagination.Contracts;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;

namespace FaunaFinder.Api.Endpoints;

public static class MunicipalityEndpoints
{
    public static void MapMunicipalityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/municipalities")
            .WithTags("Municipalities");

        group.MapGet("/", async (IMunicipalityRepository repository, CancellationToken ct) =>
        {
            var municipalities = await repository.GetAllMunicipalitiesAsync(ct);
            return Results.Ok(municipalities);
        }).WithName("GetAllMunicipalities");

        group.MapGet("/{id:int}", async (int id, IMunicipalityRepository repository, CancellationToken ct) =>
        {
            var municipality = await repository.GetMunicipalityDetailAsync(id, ct);
            return municipality is not null ? Results.Ok(municipality) : Results.NotFound();
        }).WithName("GetMunicipalityDetail");

        group.MapGet("/cards", async (
            int pageSize,
            int page,
            string? search,
            IMunicipalityRepository repository,
            CancellationToken ct) =>
        {
            var parameters = new MunicipalityParameters(pageSize, page, search);
            var municipalities = await repository.GetMunicipalitiesWithSpeciesCountAsync(parameters, ct);
            return Results.Ok(municipalities);
        }).WithName("GetMunicipalityCards");

        group.MapGet("/count", async (string? search, IMunicipalityRepository repository, CancellationToken ct) =>
        {
            var count = await repository.GetTotalMunicipalitiesCountAsync(search, ct);
            return Results.Ok(count);
        }).WithName("GetMunicipalitiesCount");

        group.MapGet("/geojson", async (IDbContextFactory<WildlifeDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var municipalities = await db.Municipalities
                .Where(m => m.Boundary != null)
                .Select(m => new { m.Name, m.GeoJsonId, m.Boundary })
                .ToListAsync();

            var features = municipalities.Select(m =>
            {
                var geoJsonId = m.GeoJsonId;
                var state = geoJsonId.Length >= 2 ? geoJsonId[..2] : geoJsonId;
                var county = geoJsonId.Length > 2 ? geoJsonId[2..] : geoJsonId;

                return new
                {
                    type = "Feature",
                    properties = new { STATE = state, COUNTY = county, NAME = m.Name },
                    geometry = m.Boundary
                };
            }).ToList();

            var featureCollection = new { type = "FeatureCollection", features };

            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new GeoJsonConverterFactory());

            return Results.Json(featureCollection, jsonOptions, contentType: "application/geo+json");
        }).WithName("GetMunicipalitiesGeoJson");

        group.MapGet("/cursor", async (
            string? cursor,
            int pageSize,
            string? search,
            IMunicipalityRepository repository,
            CancellationToken ct) =>
        {
            var request = new CursorPageRequest(cursor, pageSize, search);
            var page = await repository.GetMunicipalitiesCursorPageAsync(request, ct);
            return Results.Ok(page);
        }).WithName("GetMunicipalitiesCursor");
    }
}
