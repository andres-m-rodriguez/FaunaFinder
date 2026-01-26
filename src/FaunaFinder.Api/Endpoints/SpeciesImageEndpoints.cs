using FaunaFinder.DataAccess.Interfaces;

namespace FaunaFinder.Api.Endpoints;

public static class SpeciesImageEndpoints
{
    public static void MapSpeciesImageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/species")
            .WithTags("Species Profile Image");

        // Get the profile image for a species
        group.MapGet("/{speciesId:int}/image", async (
            int speciesId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var image = await repository.GetProfileImageAsync(speciesId, ct);
            if (image is null)
            {
                return Results.NotFound();
            }
            return Results.File(image.ImageData, image.ContentType);
        }).WithName("GetSpeciesProfileImage");

        // Upload or replace the profile image for a species
        group.MapPost("/{speciesId:int}/image", async (
            int speciesId,
            IFormFile file,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("No file provided");
            }

            // Validate content type
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                return Results.BadRequest("Invalid image type. Allowed types: JPEG, PNG, GIF, WebP");
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, ct);
            var imageData = memoryStream.ToArray();

            var success = await repository.SetProfileImageAsync(
                speciesId,
                imageData,
                file.ContentType,
                ct);

            return success
                ? Results.Ok(new { message = "Profile image updated" })
                : Results.NotFound();
        })
        .DisableAntiforgery()
        .WithName("UploadSpeciesProfileImage");

        // Delete the profile image for a species
        group.MapDelete("/{speciesId:int}/image", async (
            int speciesId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var deleted = await repository.DeleteProfileImageAsync(speciesId, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).WithName("DeleteSpeciesProfileImage");
    }
}
