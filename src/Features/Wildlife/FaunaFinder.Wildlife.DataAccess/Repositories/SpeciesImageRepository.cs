using FaunaFinder.Wildlife.Contracts.Dtos;
using FaunaFinder.Wildlife.Database;
using FaunaFinder.Wildlife.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.DataAccess.Repositories;

public sealed class SpeciesImageRepository(
    IDbContextFactory<WildlifeDbContext> contextFactory
) : ISpeciesImageRepository
{
    public async Task<SpeciesProfileImageDto?> GetProfileImageAsync(
        int speciesId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Species
            .AsNoTracking()
            .Where(s => s.Id == speciesId && s.ProfileImageData != null && s.ProfileImageContentType != null)
            .Select(s => new SpeciesProfileImageDto(
                s.Id,
                s.ProfileImageData!,
                s.ProfileImageContentType!
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> SetProfileImageAsync(
        int speciesId,
        byte[] imageData,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var rowsAffected = await context.Species
            .Where(s => s.Id == speciesId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(sp => sp.ProfileImageData, imageData)
                    .SetProperty(sp => sp.ProfileImageContentType, contentType),
                cancellationToken);

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteProfileImageAsync(
        int speciesId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var rowsAffected = await context.Species
            .Where(s => s.Id == speciesId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(sp => sp.ProfileImageData, (byte[]?)null)
                    .SetProperty(sp => sp.ProfileImageContentType, (string?)null),
                cancellationToken);

        return rowsAffected > 0;
    }
}
