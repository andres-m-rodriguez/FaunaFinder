using System.Security.Claims;
using FaunaFinder.Wildlife.Contracts;
using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Contracts.Parameters;
using FaunaFinder.Wildlife.Contracts.Requests;
using FaunaFinder.Wildlife.Contracts.Responses;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FaunaFinder.Wildlife.Api;

public static class WildlifeEndpoints
{
    public static IEndpointRouteBuilder MapWildlifeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wildlife").WithTags("Wildlife");

        // Public endpoints
        group.MapGet("/species/search", SearchSpecies).WithName("SearchSpecies");
        group.MapGet("/sightings/{id:int}/photo", GetSightingPhoto).WithName("GetSightingPhoto");

        // Authenticated endpoints
        group
            .MapPost("/sightings", CreateSighting)
            .RequireAuthorization()
            .WithName("CreateSighting");
        group.MapGet("/sightings", GetSightings).RequireAuthorization().WithName("GetSightings");
        group
            .MapGet("/sightings/{id:int}", GetSightingDetail)
            .RequireAuthorization()
            .WithName("GetSightingDetail");
        group
            .MapPatch("/sightings/{id:int}/photo", UpdateSightingPhoto)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UpdateSightingPhoto");
        group
            .MapGet("/my-sightings", GetMySightings)
            .RequireAuthorization()
            .WithName("GetMySightings");

        // Teacher/Admin endpoints
        group
            .MapPost("/sightings/{id:int}/review", ReviewSighting)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Teacher"))
            .WithName("ReviewSighting");
        group
            .MapGet("/review-queue", GetReviewQueue)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Teacher"))
            .WithName("GetReviewQueue");

        return app;
    }

    private static async Task<
        Results<Ok<IReadOnlyList<SpeciesSearchResult>>, ValidationProblem>
    > SearchSpecies(
        [AsParameters] SpeciesSearchParameters parameters,
        ISpeciesRepository speciesRepository,
        IValidator<SpeciesSearchParameters> validator,
        CancellationToken ct
    )
    {
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var species = await speciesRepository.SearchSpeciesAsync(parameters, ct);
        return TypedResults.Ok(species);
    }

    private static async Task<
        Results<
            Created<SightingCreatedResponse>,
            UnauthorizedHttpResult,
            ValidationProblem,
            BadRequest<string>
        >
    > CreateSighting(
        HttpContext context,
        ISightingRepository sightingRepository,
        ISpeciesRepository speciesRepository,
        IValidator<CreateSightingRequest> validator,
        CreateSightingRequest request,
        CancellationToken ct
    )
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        // Verify species exists
        if (!await speciesRepository.ExistsAsync(request.SpeciesId, ct))
        {
            return TypedResults.BadRequest("Invalid species ID");
        }

        var response = await sightingRepository.CreateSightingAsync(request, userId, ct);

        if (!response.Success)
        {
            return TypedResults.BadRequest(response.Error);
        }

        return TypedResults.Created(
            $"/api/wildlife/sightings/{response.Id}",
            new SightingCreatedResponse(response.Id!.Value)
        );
    }

    private static async Task<Results<Ok<SightingsPage>, ValidationProblem>> GetSightings(
        [AsParameters] SightingsParameters parameters,
        ISightingRepository sightingRepository,
        IValidator<SightingsParameters> validator,
        CancellationToken ct
    )
    {
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var result = await sightingRepository.GetSightingsAsync(parameters, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<SightingDetailDto>, NotFound>> GetSightingDetail(
        int id,
        ISightingRepository sightingRepository,
        CancellationToken ct
    )
    {
        var sighting = await sightingRepository.GetSightingDetailAsync(id, ct);
        return sighting is not null ? TypedResults.Ok(sighting) : TypedResults.NotFound();
    }

    private static async Task<
        Results<
            Ok<PhotoUpdateResponse>,
            UnauthorizedHttpResult,
            NotFound,
            ForbidHttpResult,
            BadRequest<string>
        >
    > UpdateSightingPhoto(
        int id,
        HttpContext context,
        ISightingRepository sightingRepository,
        CancellationToken ct
    )
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        if (!context.Request.HasFormContentType)
        {
            return TypedResults.BadRequest("Request must be multipart/form-data");
        }

        var form = await context.Request.ReadFormAsync(ct);
        var file = form.Files.GetFile("photo");

        if (file is null || file.Length == 0)
        {
            return TypedResults.BadRequest("Photo file is required");
        }

        // Validate content type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return TypedResults.BadRequest(
                "Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed."
            );
        }

        // Read file content
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);
        var photoData = memoryStream.ToArray();

        var (success, error) = await sightingRepository.UpdateSightingPhotoAsync(
            id,
            userId,
            photoData,
            file.ContentType,
            ct
        );

        if (!success)
        {
            return error switch
            {
                "Sighting not found" => TypedResults.NotFound(),
                "Not authorized" => TypedResults.Forbid(),
                _ => TypedResults.BadRequest(error),
            };
        }

        return TypedResults.Ok(new PhotoUpdateResponse(id, "Photo updated successfully"));
    }

    private static async Task<Results<FileContentHttpResult, NotFound>> GetSightingPhoto(
        int id,
        ISightingRepository sightingRepository,
        CancellationToken ct
    )
    {
        var result = await sightingRepository.GetSightingPhotoAsync(id, ct);
        return result is not null
            ? TypedResults.File(result.PhotoData, result.ContentType)
            : TypedResults.NotFound();
    }

    private static async Task<
        Results<
            Ok<ReviewSightingResponse>,
            UnauthorizedHttpResult,
            NotFound,
            ValidationProblem,
            BadRequest<string>
        >
    > ReviewSighting(
        int id,
        HttpContext context,
        ISightingRepository sightingRepository,
        IValidator<ReviewSightingRequest> validator,
        ReviewSightingRequest request,
        CancellationToken ct
    )
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var (success, error) = await sightingRepository.ReviewSightingAsync(
            id,
            request,
            userId,
            ct
        );

        if (!success)
        {
            return error == "Sighting not found"
                ? TypedResults.NotFound()
                : TypedResults.BadRequest(error);
        }

        return TypedResults.Ok(new ReviewSightingResponse(id, request.Status));
    }

    private static async Task<Results<Ok<SightingsPage>, ValidationProblem>> GetReviewQueue(
        [AsParameters] ReviewQueueParameters parameters,
        ISightingRepository sightingRepository,
        IValidator<ReviewQueueParameters> validator,
        CancellationToken ct
    )
    {
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var result = await sightingRepository.GetReviewQueueAsync(parameters, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<
        Results<Ok<SightingsPage>, UnauthorizedHttpResult, ValidationProblem>
    > GetMySightings(
        HttpContext context,
        [AsParameters] UserSightingsParameters parameters,
        ISightingRepository sightingRepository,
        IValidator<UserSightingsParameters> validator,
        CancellationToken ct
    )
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var parametersWithUser = parameters with { UserId = userId };
        var validation = await validator.ValidateAsync(parametersWithUser, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var result = await sightingRepository.GetSightingsByUserAsync(parametersWithUser, ct);
        return TypedResults.Ok(result);
    }
}
