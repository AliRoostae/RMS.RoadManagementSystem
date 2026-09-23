using RMS.Domain.Entities;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;


namespace RMS.Domain.Interfaces;

/// <summary>
/// قرارداد دسترسی به داده‌های افراد حاضر در حادثه را تعریف می‌کند که به خودرویی منتسب نیستند.
/// </summary>
public interface IPeopleRepository
{

    /// <summary>شخص جدیدی را برای یک حادثه ثبت می‌کند.</summary>
    /// <param name="argo">موجودیت شخص جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر تغییری در منبع داده ذخیره شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AddAsync(PeopleEntities argo, CancellationToken token = default);




    /// <summary>اطلاعات شخص مشخص‌شده را به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید شخص.</param>
    /// <param name="id">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر شخص یافت شود و عملیات ذخیره‌سازی بدون خطا پایان یابد؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(BasePeople argo, Guid id, CancellationToken token = default);

    /// <summary>شخص مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> اگر شخص حذف شود؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(Guid argo, CancellationToken token = default);




    /// <summary>جزئیات شخص مشخص‌شده را دریافت می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>نمای شخص، یا <see langword="null"/> در صورت نبودن آن.</returns>
    Task<PeopleResponse?> GetAsync(Guid argo, CancellationToken token = default);

    /// <summary>فهرست صفحه‌بندی‌شدهٔ عابر را دریافت می‌کند.</summary>
    /// <param name="argo">معیارهای جست‌وجو و صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست اشخاص منطبق با معیارها.</returns>
    Task<PagedResponse<PeopleListItemResponse>> GetAllAsync(PeopleQueries argo, CancellationToken token = default);

    /// <summary>وجود دست‌کم یک فرد خارج از خودرو برای حادثهٔ مشخص‌شده را بررسی می‌کند.</summary>
    /// <param name="idAcciden">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود شخص؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> AnyPeopleInAccidenAsync(Guid idAcciden, CancellationToken token = default);
    Task<Guid> GetParentAsync(Guid idPeople, CancellationToken token);
}