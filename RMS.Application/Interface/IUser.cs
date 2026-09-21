using RMS.Shared.Contracts.Commands;
using RMS.Shared.Enums;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Interface;

/// <summary>عملیات کاربردی مدیریت کاربران و بررسی دسترسی آن‌ها.</summary>
public interface IUser
{
    Task<Guid> AddAsync(CreateUserCommand command, CancellationToken token = default);
    Task<bool> UpdateAsync(UpdateUserCommand command, CancellationToken token = default);
    Task<bool> DeleteAsync(DeleteUserCommand command, CancellationToken token = default);
    Task<UserResponse?> GetAsync(Guid id, CancellationToken token = default);
    Task<PagedResponse<UserListItemResponse>> GetAllAsync(UserQuery query, CancellationToken token = default);

    /// <summary>نقطهٔ ورود Policyهای API برای بررسی مجوز کاربر.</summary>
    Task<bool> HasAccessAsync(
        Guid userId,
        UserSection section,
        UserAccessOperation operation,
        CancellationToken token = default);
}
