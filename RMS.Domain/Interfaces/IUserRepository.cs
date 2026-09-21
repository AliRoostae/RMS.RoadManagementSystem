using RMS.Domain.Entities;
using RMS.Domain.Security;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;

/// <summary>قرارداد ذخیره‌سازی و خواندن کاربران سامانه.</summary>
public interface IUserRepository
{
    /// <summary>کاربر جدید را ذخیره می‌کند.</summary>
    Task<bool> AddAsync(UserEntities user, CancellationToken token = default);
    /// <summary>کاربر را برای ویرایش همراه با مجوزهایش می‌خواند.</summary>
    Task<UserEntities?> GetForUpdateAsync(Guid id, CancellationToken token = default);
    /// <summary>کاربر را برای احراز هویت با شماره موبایل پیدا می‌کند.</summary>
    Task<UserEntities?> GetForAuthenticationAsync(string mobileNumber, CancellationToken token = default);
    /// <summary>تغییرات کاربر را ذخیره می‌کند.</summary>
    Task<bool> UpdateAsync(UserEntities user, CancellationToken token = default);
    /// <summary>کاربر مشخص‌شده را حذف می‌کند.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);
    /// <summary>تکراری بودن شماره موبایل را بررسی می‌کند.</summary>
    Task<bool> MobileExistsAsync(string mobileNumber, Guid? exceptUserId = null, CancellationToken token = default);
    /// <summary>نمایش عمومی کاربر را برمی‌گرداند.</summary>
    Task<UserResponse?> GetAsync(Guid id, CancellationToken token = default);
    /// <summary>فهرست صفحه‌بندی‌شدهٔ کاربران را برمی‌گرداند.</summary>
    Task<PagedResponse<UserListItemResponse>> GetAllAsync(UserQuery query, CancellationToken token = default);
    /// <summary>اطلاعات لازم برای تصمیم‌گیری دسترسی کاربر را برمی‌گرداند.</summary>
    Task<UserAccessSnapshot?> GetAccessSnapshotAsync(Guid id, CancellationToken token = default);
}
