using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Responses;



namespace RMS.Application.Interface;

/// <summary>
/// عملیات کاربردی مدیریت رانندگان و سرنشینان خودروها را تعریف می‌کند.
/// </summary>
public interface IPassenger
{

    /// <summary>سرنشین یا رانندهٔ جدیدی را پس از اعتبارسنجی ثبت می‌کند.</summary>
    /// <param name="argo">اطلاعات شخص جدید.</param>
    /// <param name="idAccident">شناسهٔ حادثه‌ای که خودرو باید به آن تعلق داشته باشد.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>شناسهٔ یکتای شخص ثبت‌شده؛ در صورت ثبت‌نشدن، <see cref="Guid.Empty"/>.</returns>
    Task<Guid> AddAsync(CreatePassengerCommand command, CancellationToken token = default);



    /// <summary>اطلاعات سرنشین یا رانندهٔ مشخص‌شده را پس از اعتبارسنجی به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید شخص.</param>
    /// <param name="id">شناسهٔ یکتای شخص.</param>
 
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت یافتن و اجرای به‌روزرسانی؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(UpdatePassengerCommand command, CancellationToken token = default);


    /// <summary>سرنشین یا رانندهٔ مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت حذف شخص؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(DeletePassengerCommand command, CancellationToken token = default);

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



}
