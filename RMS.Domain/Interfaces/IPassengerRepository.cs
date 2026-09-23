using RMS.Domain.Entities;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;



namespace RMS.Domain.Interfaces;

/// <summary>
/// قرارداد دسترسی به داده‌های رانندگان و سرنشینان خودروها را تعریف می‌کند.
/// </summary>
public interface IPassengerRepository
{

    /// <summary>سرنشین یا رانندهٔ جدیدی را ثبت می‌کند.</summary>
    /// <param name="argo">موجودیت شخص جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر تغییری در منبع داده ذخیره شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AddAsync(PassengerEntities argo, CancellationToken token = default);



    /// <summary>اطلاعات سرنشین یا رانندهٔ مشخص‌شده را به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید شخص.</param>
    /// <param name="id">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر شخص یافت شود و عملیات ذخیره‌سازی بدون خطا پایان یابد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(BasePassenger argo, Guid id, CancellationToken token = default);


    /// <summary>
    /// اعتبار ثبت راننده برای خودرو را بررسی می‌کند.
    /// </summary>
    /// <param name="fkCar">شناسهٔ یکتای خودرو.</param>
    /// <param name="isDriver">مشخص می‌کند شخص جدید راننده است یا خیر.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر راننده‌ای از قبل برای خودرو ثبت شده باشد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> OneDriverValidAsync(Guid fkCar, bool isDriver, CancellationToken token = default);
    /// <summary>
    /// اعتبار ثبت راننده برای خودرو را هنگام ویرایش، با صرف‌نظر از رکورد جاری، بررسی می‌کند.
    /// </summary>
    /// <param name="id">شناسهٔ شخص در حال ویرایش.</param>
    /// <param name="fkCar">شناسهٔ یکتای خودرو.</param>
    /// <param name="isDriver">مشخص می‌کند شخص در حال ویرایش راننده است یا خیر.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر راننده‌ای غیر از رکورد جاری برای خودرو ثبت شده باشد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> OneDriverValidEditAsync(Guid id, Guid fkCar, bool isDriver, CancellationToken token = default);

    /// <summary>سرنشین یا رانندهٔ مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر شخص حذف شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(Guid argo, CancellationToken token = default);

    /// <summary>جزئیات سرنشین یا رانندهٔ مشخص‌شده را دریافت می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>نمای شخص، یا <see langword="null"/> در صورت نبودن آن.</returns>
    Task<PassengerResponse?> GetAsync(Guid argo, CancellationToken token = default);

    /// <summary>فهرست صفحه‌بندی‌شدهٔ رانندگان و سرنشینان را دریافت می‌کند.</summary>
    /// <param name="argo">معیارهای جست‌وجو و صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست اشخاص منطبق با معیارها.</returns>
    Task<PagedResponse<PassengerListItemResponse>> GetAllAsync(PassengerQueries argo, CancellationToken token = default);

    /// <summary>وجود دست‌کم یک راننده یا سرنشین وابسته به خودروی مشخص‌شده را بررسی می‌کند.</summary>
    /// <param name="idCar">شناسهٔ یکتای خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود شخص منتسب به خودرو؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AnySubsetPassengerAsync(Guid idCar, CancellationToken token = default);
    Task<Guid> GetParentAsync(Guid idpassenger, CancellationToken token);
}