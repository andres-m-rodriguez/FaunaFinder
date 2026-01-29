using FaunaFinder.Wildlife.Contracts.Dtos;

namespace FaunaFinder.Wildlife.DataAccess.Interfaces;

public interface ISpeciesImageRepository
{
    /// <summary>
    /// Gets the profile image for a species.
    /// </summary>
    Task<SpeciesProfileImageDto?> GetProfileImageAsync(
        int speciesId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or replaces the profile image for a species.
    /// </summary>
    Task<bool> SetProfileImageAsync(
        int speciesId,
        byte[] imageData,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the profile image from a species.
    /// </summary>
    Task<bool> DeleteProfileImageAsync(
        int speciesId,
        CancellationToken cancellationToken = default);
}
