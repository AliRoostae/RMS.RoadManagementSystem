using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;


namespace RMS.Application.Interface;

/// <summary>
/// عملیات کاربردی مدیریت راه‌ها و محدوده‌های جغرافیایی را تعریف می‌کند.
/// </summary>
public interface IRoad
{


    /// <summary>راه یا محدودهٔ جغرافیایی جدیدی را پس از اعتبارسنجی ثبت می‌کند.</summary>
    /// <param name="argo">اطلاعات راه یا محدودهٔ جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>شناسهٔ یکتای راه یا محدودهٔ ثبت‌شده؛ در صورت ثبت‌نشدن، <see cref="Guid.Empty"/>.</returns>
    Task<Guid> AddAsync(CreateRoadCommand command, CancellationToken token = default);




    /// <summary>اطلاعات راه یا محدودهٔ مشخص‌شده را پس از اعتبارسنجی به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید راه یا محدوده.</param>
    /// <param name="id">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت یافتن و اجرای به‌روزرسانی؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(UpdateRoadCommand command, CancellationToken token = default);

    /// <summary>راه یا محدودهٔ مشخص‌شده را در صورت نداشتن حادثهٔ وابسته حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای راه یا محدوده.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت حذف راه یا محدوده؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(DeleteRoadCommand command, CancellationToken token = default);




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