using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Responses;


namespace RMS.Application.Interface;

/// <summary>
/// عملیات کاربردی مدیریت خودروهای درگیر در حوادث را تعریف می‌کند.
/// </summary>
public interface ICar

{

    /// <summary>خودروی جدیدی را پس از اعتبارسنجی ثبت می‌کند.</summary>
    /// <param name="argo">اطلاعات خودروی جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>شناسهٔ یکتای خودروی ثبت‌شده؛ در صورت ثبت‌نشدن، <see cref="Guid.Empty"/>.</returns>
    Task<Guid> AddAsync(CreateCarCommand command, CancellationToken token = default);



    /// <summary>اطلاعات خودروی مشخص‌شده را پس از اعتبارسنجی به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید خودرو.</param>
    /// <param name="id">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت یافتن و اجرای به‌روزرسانی؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(UpdateCarCommand command, CancellationToken token = default);

    /// <summary>خودروی مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت حذف خودرو؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(DeleteCarCommand command, CancellationToken token = default);

    /// <summary>جزئیات خودروی مشخص‌شده را دریافت می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>نمای خودرو، یا <see langword="null"/> در صورت نبودن آن.</returns>
    Task<CarResponse?> GetAsync(Guid argo, CancellationToken token = default);

    /// <summary>فهرست صفحه‌بندی‌شدهٔ خودروها را دریافت می‌کند.</summary>
    /// <param name="argo">معیارهای جست‌وجو و صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست خودروهای منطبق با معیارها.</returns>
    Task<PagedResponse<CarListItemResponse>> GetAllAsync(CarQueries argo, CancellationToken token = default);




}
