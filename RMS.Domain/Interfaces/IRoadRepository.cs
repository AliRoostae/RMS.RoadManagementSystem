using RMS.Shared.Contracts.DTOs;
using RMS.Domain.Entities;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;

/// <summary>
/// قرارداد دسترسی به داده‌های راه‌ها و محدوده‌های جغرافیایی را تعریف می‌کند.
/// </summary>
public interface IRoadRepository
{

    /// <summary>راه یا محدودهٔ جغرافیایی جدیدی را ثبت می‌کند.</summary>
    /// <param name="argo">موجودیت راه یا محدودهٔ جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر تغییری در منبع داده ذخیره شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AddAsync(RoadsEntities argo, CancellationToken token = default);



    /// <summary>اطلاعات راه یا محدودهٔ مشخص‌شده را به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید راه یا محدوده.</param>
    /// <param name="id">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر راه یافت شود و عملیات ذخیره‌سازی بدون خطا پایان یابد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(BaseRoads argo, Guid id, CancellationToken token = default);



    /// <summary>
    /// قرارگرفتن مختصات جغرافیایی در مرز راه یا محدودهٔ مشخص‌شده را بررسی می‌کند.
    /// </summary>
    /// <param name="latitude">عرض جغرافیایی نقطه.</param>
    /// <param name="longitude">طول جغرافیایی نقطه.</param>
    /// <param name="idRoad">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر نقطه داخل مرز یا روی آن باشد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> IsValidCoordinate(double latitude, double longitude, Guid idRoad, CancellationToken token = default);

    /// <summary>تکراری‌بودن نام راه یا محدوده را هنگام ثبت بررسی می‌کند.</summary>
    /// <param name="name">نام راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت تکراری‌بودن نام؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DuplicateNameAsync(string name, CancellationToken token = default);

    /// <summary>تکراری‌بودن نام راه یا محدوده را هنگام ویرایش، با صرف‌نظر از رکورد جاری، بررسی می‌کند.</summary>
    /// <param name="name">نام راه یا محدوده.</param>
    /// <param name="id">شناسهٔ رکورد در حال ویرایش.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت تکراری‌بودن نام؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DuplicateEditNameAsync(string name, Guid id, CancellationToken token = default);


    /// <summary>راه یا محدودهٔ مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر راه یا محدوده حذف شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(Guid argo, CancellationToken token = default);


    Task<bool> AnyAsync(Guid id, CancellationToken token = default);

    /// <summary>جزئیات راه یا محدودهٔ مشخص‌شده را دریافت می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>نمای راه یا محدوده، یا <see langword="null"/> در صورت نبودن آن.</returns>
    Task<RoadResponse?> GetAsync(Guid argo, CancellationToken token = default);

    Task<RoadGeometryResponse?> GetGeometryAsync(Guid id, CancellationToken token = default);

    Task<RoadLocationResponse?> GetNearestPointAsync(
        Guid id,
        RoadLocationQuery query,
        CancellationToken token = default);

    /// <summary>فهرست صفحه‌بندی‌شدهٔ راه‌ها و محدوده‌ها را دریافت می‌کند.</summary>
    /// <param name="argo">معیارهای صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست راه‌ها و محدوده‌ها.</returns>
    Task<PagedResponse<RoadListItemResponse>> GetAllAsync(RoadQueries argo, CancellationToken token = default);

    Task<IReadOnlyList<RoadMapItemResponse>> GetMapAsync(RoadMapQuery query, CancellationToken token = default);
}
