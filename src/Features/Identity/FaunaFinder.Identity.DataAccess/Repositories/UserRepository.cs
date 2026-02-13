using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.DataAccess.Interfaces;
using FaunaFinder.Identity.Database;
using FaunaFinder.Pagination.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Identity.DataAccess.Repositories;

public sealed class UserRepository(IDbContextFactory<IdentityDbContext> contextFactory)
    : IUserRepository
{
    public async Task<UserInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Role.ToString(),
                u.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserInfo?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .Users.AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Role.ToString(),
                u.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserInfo>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .Users.AsNoTracking()
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Role.ToString(),
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<CursorPage<UserInfo>> GetCursorPageAsync(
        CursorPageParameter request,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(search) || u.DisplayName.ToLower().Contains(search)
            );
        }

        if (request.Cursor is not null && CursorHelper.TryDecode(request.Cursor, out var cursorId))
        {
            query = query.Where(u => u.Id > cursorId);
        }

        var items = await query
            .OrderBy(u => u.Id)
            .Take(request.PageSize + 1)
            .Select(u => new UserInfo(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.Role.ToString(),
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > request.PageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var nextCursor = hasMore && items.Count > 0 ? CursorHelper.Encode(items[^1].Id) : null;

        return new CursorPage<UserInfo>(items, nextCursor, hasMore);
    }
}
