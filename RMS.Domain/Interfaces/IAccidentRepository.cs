
using RMS.Domain.Entities;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;

/// <summary>
/// قرارداد دسترسی به داده‌های حوادث و نشانی تصاویر وابسته به آن‌ها را تعریف می‌کند.
/// </summary>
public interface IAccidentRepository
{
    /// <summary>حادثهٔ جدیدی را در منبع داده ثبت می‌کند.</summary>
    /// <param name="accident">موجودیت حادثهٔ جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر تغییری در منبع داده ذخیره شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AddAsync(AccidentEntities accident, CancellationToken token = default);

    /// <summary>اطلاعات حادثهٔ مشخص‌شده را به‌روزرسانی می‌کند.</summary>
    /// <param name="accident">مقادیر جدید حادثه.</param>
    /// <param name="id">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر حادثه یافت شود و عملیات ذخیره‌سازی بدون خطا پایان یابد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(BaseAccident accident, Guid id, CancellationToken token = default);

    /// <summary>حادثهٔ مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="id">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر حادثه حذف شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);

    /// <summary>وجود دست‌کم یک حادثهٔ وابسته به راه یا محدودهٔ مشخص‌شده را بررسی می‌کند.</summary>
    /// <param name="idRoad">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود حادثه؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AnySubsetRoadAsync(Guid idRoad, CancellationToken token = default);
    /// <summary>وجود  حادثه‌ای با شناسهٔ مشخص‌شده را بررسی می‌کند.</summary>
    /// <param name="idAccident">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود حادثه؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AnyAsync(Guid idAccident, CancellationToken token = default);

    /// <summary>جزئیات حادثه‌ای با شناسهٔ مشخص‌شده را دریافت می‌کند.</summary>
    /// <param name="id">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>نمای حادثه، یا <see langword="null"/> در صورت نبودن آن.</returns>
    Task<AccidentResponse?> GetAsync(Guid id, CancellationToken token = default);

    /// <summary>فهرست صفحه‌بندی‌شدهٔ حوادث واقع در بازهٔ زمانی درخواستی را دریافت می‌کند.</summary>
    /// <param name="query">معیارهای بازهٔ زمانی و صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست فقط‌خواندنی حوادث منطبق با معیارها.</returns>
    Task<PagedResponse<AccidentListItemResponse>> GetAllAsync(AccidentQuery query, CancellationToken token = default);

    Task<IReadOnlyList<AccidentMapPointResponse>> GetMapAsync(AccidentMapQuery query, CancellationToken token = default);


}
