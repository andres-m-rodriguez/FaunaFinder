using FaunaFinder.Contracts.Dtos.Species;
using FaunaFinder.Database;
using FaunaFinder.Database.Models.Species;
using FaunaFinder.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.DataAccess.Repositories;

public sealed class SpeciesImageRepository(
    IDbContextFactory<FaunaFinderContext> contextFactory
) : ISpeciesImageRepository
{
    public async Task<SpeciesImageDto?> GetImageMetadataAsync(
        int imageId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SpeciesImages
            .AsNoTracking()
            .Where(i => i.Id == imageId)
            .Select(i => new SpeciesImageDto(
                i.Id,
                i.SpeciesId,
                i.ContentType,
                i.FileName,
                i.Description,
                i.IsPrimary,
                i.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SpeciesImageDataDto?> GetImageAsync(
        int imageId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SpeciesImages
            .AsNoTracking()
            .Where(i => i.Id == imageId)
            .Select(i => new SpeciesImageDataDto(
                i.Id,
                i.SpeciesId,
                i.ImageData,
                i.ContentType,
                i.FileName,
                i.Description,
                i.IsPrimary,
                i.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpeciesImageDto>> GetImagesBySpeciesIdAsync(
        int speciesId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SpeciesImages
            .AsNoTracking()
            .Where(i => i.SpeciesId == speciesId)
            .OrderByDescending(i => i.IsPrimary)
            .ThenByDescending(i => i.CreatedAt)
            .Select(i => new SpeciesImageDto(
                i.Id,
                i.SpeciesId,
                i.ContentType,
                i.FileName,
                i.Description,
                i.IsPrimary,
                i.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<SpeciesImageDataDto?> GetPrimaryImageAsync(
        int speciesId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.SpeciesImages
            .AsNoTracking()
            .Where(i => i.SpeciesId == speciesId && i.IsPrimary)
            .Select(i => new SpeciesImageDataDto(
                i.Id,
                i.SpeciesId,
                i.ImageData,
                i.ContentType,
                i.FileName,
                i.Description,
                i.IsPrimary,
                i.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> AddImageAsync(
        int speciesId,
        byte[] imageData,
        string contentType,
        string? fileName,
        string? description,
        bool isPrimary,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // If this is marked as primary, unset any existing primary image
        if (isPrimary)
        {
            await context.SpeciesImages
                .Where(i => i.SpeciesId == speciesId && i.IsPrimary)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(i => i.IsPrimary, false),
                    cancellationToken);
        }

        var image = new SpeciesImage
        {
            Id = 0,
            SpeciesId = speciesId,
            ImageData = imageData,
            ContentType = contentType,
            FileName = fileName,
            Description = description,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow
        };

        context.SpeciesImages.Add(image);
        await context.SaveChangesAsync(cancellationToken);

        return image.Id;
    }

    public async Task<bool> DeleteImageAsync(
        int imageId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var rowsAffected = await context.SpeciesImages
            .Where(i => i.Id == imageId)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsAffected > 0;
    }

    public async Task<bool> SetPrimaryImageAsync(
        int imageId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Get the image to find its species ID
        var image = await context.SpeciesImages
            .AsNoTracking()
            .Where(i => i.Id == imageId)
            .Select(i => new { i.Id, i.SpeciesId })
            .FirstOrDefaultAsync(cancellationToken);

        if (image is null)
        {
            return false;
        }

        // Unset any existing primary image for this species
        await context.SpeciesImages
            .Where(i => i.SpeciesId == image.SpeciesId && i.IsPrimary)
            .ExecuteUpdateAsync(
                s => s.SetProperty(i => i.IsPrimary, false),
                cancellationToken);

        // Set the new primary image
        var rowsAffected = await context.SpeciesImages
            .Where(i => i.Id == imageId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(i => i.IsPrimary, true),
                cancellationToken);

        return rowsAffected > 0;
    }
}
