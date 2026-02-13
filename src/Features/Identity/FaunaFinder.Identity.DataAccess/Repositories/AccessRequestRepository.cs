using FaunaFinder.Identity.Contracts.Requests;
using FaunaFinder.Identity.Contracts.Responses;
using FaunaFinder.Identity.DataAccess.Interfaces;
using FaunaFinder.Identity.Database;
using FaunaFinder.Identity.Database.Models;
using FaunaFinder.Pagination.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Identity.DataAccess.Repositories;

public sealed class AccessRequestRepository(IDbContextFactory<IdentityDbContext> contextFactory)
    : IAccessRequestRepository
{
    public async Task<IReadOnlyList<AccessRequestInfo>> GetPendingAsync(
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .AccessRequests.AsNoTracking()
            .Where(r => r.Status == AccessRequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new AccessRequestInfo(
                r.Id,
                r.Email,
                r.DisplayName,
                r.Message,
                r.Status.ToString(),
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AccessRequestInfo?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .AccessRequests.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new AccessRequestInfo(
                r.Id,
                r.Email,
                r.DisplayName,
                r.Message,
                r.Status.ToString(),
                r.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AccessRequest?> GetEntityByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context
            .AccessRequests.AsNoTracking()
            .Where(r => r.Email == email && r.Status == AccessRequestStatus.Pending)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AccessRequestInfo> CreateAsync(
        AccessRequest accessRequest,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.AccessRequests.Add(accessRequest);
        await context.SaveChangesAsync(cancellationToken);

        return new AccessRequestInfo(
            accessRequest.Id,
            accessRequest.Email,
            accessRequest.DisplayName,
            accessRequest.Message,
            accessRequest.Status.ToString(),
            accessRequest.CreatedAt
        );
    }

    public async Task<AccessRequestInfo?> UpdateStatusAsync(
        int id,
        AccessRequestStatus status,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var request = await context
            .AccessRequests.AsTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (request is null)
            return null;

        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new AccessRequestInfo(
            request.Id,
            request.Email,
            request.DisplayName,
            request.Message,
            request.Status.ToString(),
            request.CreatedAt
        );
    }

    public async Task<CursorPage<AccessRequestInfo>> GetCursorPageAsync(
        AccessRequestPageParameter request,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.AccessRequests.AsNoTracking();

        if (
            !string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<AccessRequestStatus>(request.Status, out var statusFilter)
        )
        {
            query = query.Where(r => r.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(r =>
                r.Email.ToLower().Contains(search) || r.DisplayName.ToLower().Contains(search)
            );
        }

        if (request.Cursor is not null && CursorHelper.TryDecode(request.Cursor, out var cursorId))
        {
            query = query.Where(r => r.Id > cursorId);
        }

        var items = await query
            .OrderBy(r => r.Id)
            .Take(request.PageSize + 1)
            .Select(r => new AccessRequestInfo(
                r.Id,
                r.Email,
                r.DisplayName,
                r.Message,
                r.Status.ToString(),
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > request.PageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var nextCursor = hasMore && items.Count > 0 ? CursorHelper.Encode(items[^1].Id) : null;

        return new CursorPage<AccessRequestInfo>(items, nextCursor, hasMore);
    }
}
