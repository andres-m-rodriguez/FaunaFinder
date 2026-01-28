using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.Database;
using FaunaFinder.Identity.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Identity.DataAccess.Repositories;

public sealed class UserRepository(
    IDbContextFactory<IdentityDbContext> contextFactory
) : IUserRepository
{
    public async Task<UserInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Status.ToString(),
                u.Role.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Status.ToString(),
                u.Role.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Status.ToString(),
                u.Role.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserInfo>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Where(u => u.Status == Database.Models.UserStatus.Pending)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Status.ToString(),
                u.Role.ToString()))
            .ToListAsync(cancellationToken);
    }
}
