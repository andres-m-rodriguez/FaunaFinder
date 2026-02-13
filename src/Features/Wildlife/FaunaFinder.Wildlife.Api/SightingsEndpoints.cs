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

public static class SightingsEndpoints
{
    public static IEndpointRouteBuilder MapSightingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sightings").WithTags("Sightings");

        // Public
        group.MapGet("/{id:int}/photo", GetPhoto).WithName("GetSightingPhoto");

        // Authenticated
        group.MapGet("/", GetAll).RequireAuthorization().WithName("GetSightings");
        group.MapGet("/mine", GetMine).RequireAuthorization().WithName("GetMySightings");
        group.MapGet("/{id:int}", GetById).RequireAuthorization().WithName("GetSighting");
        group.MapPost("/", Create).RequireAuthorization().WithName("CreateSighting");
        group
            .MapPatch("/{id:int}/photo", UpdatePhoto)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UpdateSightingPhoto");

        // Teacher/Admin
        group
            .MapPost("/{id:int}/review", Review)
            .RequireAuthorization(p => p.RequireRole("Admin", "Teacher"))
            .WithName("ReviewSighting");

        return app;
    }

    private static async Task<Results<Ok<SightingsPage>, ValidationProblem>> GetAll(
        [AsParameters] SightingsParameters parameters,
        ISightingRepository repository,
        IValidator<SightingsParameters> validator,
        CancellationToken ct
    )
    {
        var validation = await validator.ValidateAsync(parameters, ct);
        if (!validation.IsValid)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var result = await repository.GetSightingsAsync(parameters, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<
        Results<Ok<SightingsPage>, UnauthorizedHttpResult, ValidationProblem>
    > GetMine(
        HttpContext context,
        [AsParameters] UserSightingsParameters parameters,
        ISightingRepository repository,
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

        var result = await repository.GetSightingsByUserAsync(parametersWithUser, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<SightingDetailDto>, NotFound>> GetById(
        int id,
        ISightingRepository repository,
        CancellationToken ct
    )
    {
        var sighting = await repository.GetSightingDetailAsync(id, ct);
        return sighting is not null ? TypedResults.Ok(sighting) : TypedResults.NotFound();
    }

    private static async Task<
        Results<
            Created<SightingCreatedResponse>,
            UnauthorizedHttpResult,
            ValidationProblem,
            BadRequest<string>
        >
    > Create(
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
            $"/api/sightings/{response.Id}",
            new SightingCreatedResponse(response.Id!.Value)
        );
    }

    private static async Task<Results<FileContentHttpResult, NotFound>> GetPhoto(
        int id,
        ISightingRepository repository,
        CancellationToken ct
    )
    {
        var result = await repository.GetSightingPhotoAsync(id, ct);
        return result is not null
            ? TypedResults.File(result.PhotoData, result.ContentType)
            : TypedResults.NotFound();
    }

    private static async Task<
        Results<
            Ok<PhotoUpdateResponse>,
            UnauthorizedHttpResult,
            NotFound,
            ForbidHttpResult,
            BadRequest<string>
        >
    > UpdatePhoto(
        int id,
        HttpContext context,
        ISightingRepository repository,
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

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return TypedResults.BadRequest(
                "Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed."
            );
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);
        var photoData = memoryStream.ToArray();

        var (success, error) = await repository.UpdateSightingPhotoAsync(
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

    private static async Task<
        Results<
            Ok<ReviewSightingResponse>,
            UnauthorizedHttpResult,
            NotFound,
            ValidationProblem,
            BadRequest<string>
        >
    > Review(
        int id,
        HttpContext context,
        ISightingRepository repository,
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

        var (success, error) = await repository.ReviewSightingAsync(id, request, userId, ct);

        if (!success)
        {
            return error == "Sighting not found"
                ? TypedResults.NotFound()
                : TypedResults.BadRequest(error);
        }

        return TypedResults.Ok(new ReviewSightingResponse(id, request.Status));
    }
}
