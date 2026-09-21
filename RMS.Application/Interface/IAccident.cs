using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Interface;

/// <summary>
/// عملیات کاربردی مدیریت حوادث و تصاویر وابسته به آن‌ها را تعریف می‌کند.
/// </summary>
public interface IAccident
{
    /// <summary>حادثهٔ جدیدی را پس از اعتبارسنجی ثبت می‌کند.</summary>
    /// <param name="accident">اطلاعات حادثهٔ جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>شناسهٔ یکتای حادثهٔ ثبت‌شده؛ در صورت ثبت‌نشدن، <see cref="Guid.Empty"/>.</returns>
    Task<Guid> AddAsync(CreateAccidentCommand command, CancellationToken token = default);

    /// <summary>اطلاعات حادثهٔ مشخص‌شده را پس از اعتبارسنجی به‌روزرسانی می‌کند.</summary>
    /// <param name="accident">مقادیر جدید حادثه.</param>
    /// <param name="id">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت یافتن و اجرای به‌روزرسانی؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(UpdateAccidentCommand command, CancellationToken token = default);

    /// <summary>حادثهٔ مشخص‌شده را در صورت نداشتن وابستگی غیرمجاز حذف می‌کند.</summary>
    /// <param name="id">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت حذف حادثه؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(DeleteAccidentCommand command, CancellationToken token = default);

    /// <summary>جزئیات حادثهٔ مشخص‌شده را دریافت می‌کند.</summary>
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
