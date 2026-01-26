using FaunaFinder.DataAccess.Interfaces;

namespace FaunaFinder.Api.Endpoints;

public static class SpeciesImageEndpoints
{
    public static void MapSpeciesImageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/species")
            .WithTags("Species Images");

        // Get all images for a species (metadata only)
        group.MapGet("/{speciesId:int}/images", async (
            int speciesId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var images = await repository.GetImagesBySpeciesIdAsync(speciesId, ct);
            return Results.Ok(images);
        }).WithName("GetSpeciesImages");

        // Get image data by ID
        group.MapGet("/images/{imageId:int}", async (
            int imageId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var image = await repository.GetImageAsync(imageId, ct);
            if (image is null)
            {
                return Results.NotFound();
            }
            return Results.File(image.ImageData, image.ContentType, image.FileName);
        }).WithName("GetSpeciesImage");

        // Get image metadata by ID
        group.MapGet("/images/{imageId:int}/metadata", async (
            int imageId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var metadata = await repository.GetImageMetadataAsync(imageId, ct);
            return metadata is not null ? Results.Ok(metadata) : Results.NotFound();
        }).WithName("GetSpeciesImageMetadata");

        // Get primary image for a species
        group.MapGet("/{speciesId:int}/images/primary", async (
            int speciesId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var image = await repository.GetPrimaryImageAsync(speciesId, ct);
            if (image is null)
            {
                return Results.NotFound();
            }
            return Results.File(image.ImageData, image.ContentType, image.FileName);
        }).WithName("GetSpeciesPrimaryImage");

        // Upload a new image for a species
        group.MapPost("/{speciesId:int}/images", async (
            int speciesId,
            IFormFile file,
            string? description,
            bool isPrimary,
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

            var imageId = await repository.AddImageAsync(
                speciesId,
                imageData,
                file.ContentType,
                file.FileName,
                description,
                isPrimary,
                ct);

            return Results.Created($"/species/images/{imageId}", new { id = imageId });
        })
        .DisableAntiforgery()
        .WithName("UploadSpeciesImage");

        // Delete an image
        group.MapDelete("/images/{imageId:int}", async (
            int imageId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var deleted = await repository.DeleteImageAsync(imageId, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).WithName("DeleteSpeciesImage");

        // Set an image as primary
        group.MapPut("/images/{imageId:int}/primary", async (
            int imageId,
            ISpeciesImageRepository repository,
            CancellationToken ct) =>
        {
            var updated = await repository.SetPrimaryImageAsync(imageId, ct);
            return updated ? Results.NoContent() : Results.NotFound();
        }).WithName("SetPrimarySpeciesImage");
    }
}
