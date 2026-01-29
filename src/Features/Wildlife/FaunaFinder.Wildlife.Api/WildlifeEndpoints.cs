using FluentValidation;
using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.Contracts.Requests;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace FaunaFinder.Wildlife.Api;

public static class WildlifeEndpoints
{
    public static IEndpointRouteBuilder MapWildlifeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wildlife")
            .WithTags("Wildlife");

        // Public endpoints
        group.MapGet("/species/search", SearchSpecies)
            .WithName("SearchSpecies");

        group.MapGet("/sightings/{id:int}/photo", GetSightingPhoto)
            .WithName("GetSightingPhoto");

        // Authenticated endpoints
        group.MapPost("/sightings", CreateSighting)
            .RequireAuthorization()
            .WithName("CreateSighting");

        group.MapGet("/sightings", GetSightings)
            .RequireAuthorization()
            .WithName("GetSightings");

        group.MapGet("/sightings/{id:int}", GetSightingDetail)
            .RequireAuthorization()
            .WithName("GetSightingDetail");

        group.MapPatch("/sightings/{id:int}/photo", UpdateSightingPhoto)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UpdateSightingPhoto");

        group.MapGet("/my-sightings", GetMySightings)
            .RequireAuthorization()
            .WithName("GetMySightings");

        // Teacher/Admin endpoints
        group.MapPost("/sightings/{id:int}/review", ReviewSighting)
            .RequireAuthorization()
            .WithName("ReviewSighting");

        group.MapGet("/review-queue", GetReviewQueue)
            .RequireAuthorization()
            .WithName("GetReviewQueue");

        return app;
    }

    private static async Task<IResult> SearchSpecies(
        string? query,
        int limit,
        ISpeciesRepository speciesRepository,
        IValidator<SpeciesSearchParameters> validator,
        CancellationToken ct)
    {
        var parameters = new SpeciesSearchParameters(query, limit);
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var species = await speciesRepository.SearchSpeciesAsync(parameters, ct);
        return Results.Ok(species);
    }

    private static async Task<IResult> CreateSighting(
        HttpContext context,
        ISightingRepository sightingRepository,
        ISpeciesRepository speciesRepository,
        IValidator<CreateSightingRequest> validator,
        CreateSightingRequest request,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        // Verify species exists
        if (!await speciesRepository.ExistsAsync(request.SpeciesId, ct))
        {
            return Results.BadRequest("Invalid species ID");
        }

        var response = await sightingRepository.CreateSightingAsync(request, userId, ct);

        if (!response.Success)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Created($"/api/wildlife/sightings/{response.Id}", new { response.Id });
    }

    private static async Task<IResult> GetSightings(
        int page,
        int pageSize,
        string? status,
        ISightingRepository sightingRepository,
        IValidator<SightingsParameters> validator,
        CancellationToken ct)
    {
        var parameters = new SightingsParameters(page, pageSize, status);
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sightingRepository.GetSightingsAsync(parameters, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetSightingDetail(
        int id,
        ISightingRepository sightingRepository,
        CancellationToken ct)
    {
        var sighting = await sightingRepository.GetSightingDetailAsync(id, ct);

        if (sighting is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(sighting);
    }

    private static async Task<IResult> UpdateSightingPhoto(
        int id,
        HttpContext context,
        ISightingRepository sightingRepository,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        if (!context.Request.HasFormContentType)
        {
            return Results.BadRequest("Request must be multipart/form-data");
        }

        var form = await context.Request.ReadFormAsync(ct);
        var file = form.Files.GetFile("photo");

        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("Photo file is required");
        }

        // Validate content type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return Results.BadRequest("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");
        }

        // Read file content
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);
        var photoData = memoryStream.ToArray();

        var (success, error) = await sightingRepository.UpdateSightingPhotoAsync(id, userId, photoData, file.ContentType, ct);

        if (!success)
        {
            return error switch
            {
                "Sighting not found" => Results.NotFound(),
                "Not authorized" => Results.Forbid(),
                _ => Results.BadRequest(error)
            };
        }

        return Results.Ok(new { Id = id, Message = "Photo updated successfully" });
    }

    private static async Task<IResult> GetSightingPhoto(
        int id,
        ISightingRepository sightingRepository,
        CancellationToken ct)
    {
        var result = await sightingRepository.GetSightingPhotoAsync(id, ct);

        if (result is null)
        {
            return Results.NotFound();
        }

        return Results.File(result.PhotoData, result.ContentType);
    }

    private static async Task<IResult> ReviewSighting(
        int id,
        HttpContext context,
        ISightingRepository sightingRepository,
        IValidator<ReviewSightingRequest> validator,
        ReviewSightingRequest request,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var (success, error) = await sightingRepository.ReviewSightingAsync(id, request, userId, ct);

        if (!success)
        {
            return error == "Sighting not found" ? Results.NotFound() : Results.BadRequest(error);
        }

        return Results.Ok(new { Id = id, request.Status });
    }

    private static async Task<IResult> GetReviewQueue(
        HttpContext context,
        ISightingRepository sightingRepository,
        IValidator<ReviewQueueParameters> validator,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var parameters = new ReviewQueueParameters(page, pageSize);
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sightingRepository.GetReviewQueueAsync(parameters, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetMySightings(
        HttpContext context,
        ISightingRepository sightingRepository,
        IValidator<UserSightingsParameters> validator,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var parameters = new UserSightingsParameters(userId, page, pageSize);
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await sightingRepository.GetSightingsByUserAsync(parameters, ct);
        return Results.Ok(result);
    }
}
