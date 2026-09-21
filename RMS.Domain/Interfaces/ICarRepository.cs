using RMS.Domain.Entities;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;


namespace RMS.Domain.Interfaces;

/// <summary>
/// قرارداد دسترسی به داده‌های خودروهای درگیر در حوادث را تعریف می‌کند.
/// </summary>
public interface ICarRepository

{

    /// <summary>خودروی جدیدی را در منبع داده ثبت می‌کند.</summary>
    /// <param name="argo">موجودیت خودروی جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر تغییری در منبع داده ذخیره شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AddAsync(CarEntities argo, CancellationToken token = default);



    /// <summary>اطلاعات خودروی مشخص‌شده را به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید خودرو.</param>
    /// <param name="id">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر خودرو یافت شود و عملیات ذخیره‌سازی بدون خطا پایان یابد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(BaseCarEdit argo, Guid id, CancellationToken token = default);

    /// <summary>خودروی مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر خودرو حذف شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(Guid argo, CancellationToken token = default);

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

    /// <summary>
    /// وجود دست‌کم یک خودرو برای حادثهٔ مشخص‌شده را بررسی می‌کند.
    /// </summary>
    /// <param name="idAcciden">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود خودرو؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AnyCarInAccidenAsync(Guid idAcciden, CancellationToken token = default);


    /// <summary>
    /// وجود خودرویی با شناسهٔ مشخص‌شده را بررسی می‌کند.
    /// </summary>
    /// <param name="idcar">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود خودرو؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AnyAsync(Guid idcar, CancellationToken token = default);



    /// <summary>
    /// تکراری‌بودن پلاک در حادثهٔ مشخص‌شده را هنگام ثبت بررسی می‌کند.
    /// </summary>
    /// <param name="plateNumber">شمارهٔ پلاک خودرو.</param>
    /// <param name="fkAccident">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت تکراری‌بودن پلاک؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DuplicatePlateAsync(string plateNumber, Guid fkAccident, CancellationToken token = default);
    /// <summary>
    /// تکراری‌بودن پلاک در حادثهٔ مشخص‌شده را هنگام ویرایش، با صرف‌نظر از خودروی جاری، بررسی می‌کند.
    /// </summary>
    /// <param name="plateNumber">شمارهٔ پلاک خودرو.</param>
    /// <param name="fkAccident">شناسهٔ یکتای حادثه.</param>
    /// <param name="id">شناسهٔ خودروی در حال ویرایش.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت تکراری‌بودن پلاک؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DuplicatePlateAsync(string plateNumber, Guid fkAccident, Guid id, CancellationToken token = default);
    Task<bool> CarIsAccidentAsync(Guid idCar, Guid idAccident, CancellationToken token);
    Task<Guid> GetParentAsync(Guid idcar, CancellationToken token);
}
