using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Domain.Security;
using RMS.Infrastructure.Persistence.EF.Core;

namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="IUserRepository"/>
public sealed class UserRepository(RmsDbContext db) : IUserRepository
{
    public async Task<bool> AddAsync(UserEntities user, CancellationToken token = default)
    {
        await db.UserDs.AddAsync(user, token);
        return await db.SaveChangesAsync(token) > 0;
    }

    public Task<UserEntities?> GetForUpdateAsync(Guid id, CancellationToken token = default) =>
        db.UserDs
            .Include(user => user.Permissions)
            .FirstOrDefaultAsync(user => user.Id == id, token);

    public Task<UserEntities?> GetForAuthenticationAsync(
        string mobileNumber,
        CancellationToken token = default) =>
        db.UserDs
            .AsNoTracking()
            .Include(user => user.Permissions)
            .FirstOrDefaultAsync(user => user.MobileNumber == mobileNumber, token);

    public async Task<bool> UpdateAsync(UserEntities user, CancellationToken token = default)
    {
        if (db.Entry(user).State == EntityState.Detached)
            db.UserDs.Update(user);

        return await db.SaveChangesAsync(token) >= 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        var user = await db.UserDs.FirstOrDefaultAsync(item => item.Id == id, token);
        if (user is null)
            return false;

        db.UserDs.Remove(user);
        return await db.SaveChangesAsync(token) > 0;
    }

    public Task<bool> MobileExistsAsync(
        string mobileNumber,
        Guid? exceptUserId = null,
        CancellationToken token = default) =>
        db.UserDs.AsNoTracking().AnyAsync(
            user => user.MobileNumber == mobileNumber &&
                    (!exceptUserId.HasValue || user.Id != exceptUserId.Value),
            token);

    public Task<UserResponse?> GetAsync(Guid id, CancellationToken token = default) =>
        ProjectToResponse(db.UserDs.AsNoTracking().Where(user => user.Id == id))
            .FirstOrDefaultAsync(token);

    public async Task<PagedResponse<UserListItemResponse>> GetAllAsync(
        UserQuery query,
        CancellationToken token = default)
    {
        var users = db.UserDs.AsNoTracking();

        if (query.Role.HasValue)
            users = users.Where(user => user.Role == query.Role.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim();
            users = users.Where(user =>
                user.FirstName.Contains(searchTerm) ||
                user.LastName.Contains(searchTerm) ||
                user.MobileNumber.Contains(searchTerm));
        }

        var totalCount = await users.CountAsync(token);
        var page = users
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Id)
            .Skip(query.Skip)
            .Take(query.Take);

        var items = await page
            .Select(user => new UserListItemResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MobileNumber = user.MobileNumber,
                Role = user.Role,
                PermissionSectionCount = user.Permissions.Count()
            })
            .ToListAsync(token);

        return new PagedResponse<UserListItemResponse>(items, totalCount, query.Skip, query.Take);
    }

    public Task<UserAccessSnapshot?> GetAccessSnapshotAsync(Guid id, CancellationToken token = default) =>
        db.UserDs.AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserAccessSnapshot(
                user.Id,
                user.Role,
                user.Permissions
                    .OrderBy(permission => permission.Section)
                    .Select(permission => new UserPermissionValue(
                        permission.Section,
                        permission.Operations))
                    .ToArray()))
            .FirstOrDefaultAsync(token);

    private static IQueryable<UserResponse> ProjectToResponse(IQueryable<UserEntities> users) =>
        users.Select(user => new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            Role = user.Role,
            Permissions = user.Permissions
                .OrderBy(permission => permission.Section)
                .Select(permission => new UserPermissionResponse
                {
                    Section = permission.Section,
                    Operations = permission.Operations
                })
                .ToArray()
        });
}
