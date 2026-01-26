using FaunaFinder.Contracts.Dtos.Species;

namespace FaunaFinder.DataAccess.Interfaces;

public interface ISpeciesImageRepository
{
    /// <summary>
    /// Gets a species image by ID without the image data (for metadata retrieval).
    /// </summary>
    Task<SpeciesImageDto?> GetImageMetadataAsync(
        int imageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a species image by ID with the full image data.
    /// </summary>
    Task<SpeciesImageDataDto?> GetImageAsync(
        int imageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all images for a species (metadata only, no image data).
    /// </summary>
    Task<IReadOnlyList<SpeciesImageDto>> GetImagesBySpeciesIdAsync(
        int speciesId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the primary image for a species (with full image data).
    /// </summary>
    Task<SpeciesImageDataDto?> GetPrimaryImageAsync(
        int speciesId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new image for a species.
    /// </summary>
    Task<int> AddImageAsync(
        int speciesId,
        byte[] imageData,
        string contentType,
        string? fileName,
        string? description,
        bool isPrimary,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image by ID.
    /// </summary>
    Task<bool> DeleteImageAsync(
        int imageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an image as the primary image for its species.
    /// </summary>
    Task<bool> SetPrimaryImageAsync(
        int imageId,
        CancellationToken cancellationToken = default);
}
