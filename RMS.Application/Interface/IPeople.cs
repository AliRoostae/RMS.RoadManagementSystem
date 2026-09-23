using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;



namespace RMS.Application.Interface;

/// <summary>
/// عملیات کاربردی مدیریت افراد حاضر در حادثه را تعریف می‌کند که به خودرویی منتسب نیستند.
/// </summary>
public interface IPeople
{

    /// <summary>شخص جدیدی را پس از اعتبارسنجی برای یک حادثه ثبت می‌کند.</summary>
    /// <param name="argo">اطلاعات شخص جدید.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>شناسهٔ یکتای شخص ثبت‌شده؛ در صورت ثبت‌نشدن، <see cref="Guid.Empty"/>.</returns>
    Task<Guid> AddAsync(CreatePeopleCommand command, CancellationToken token = default);



    /// <summary>اطلاعات شخص مشخص‌شده را پس از اعتبارسنجی به‌روزرسانی می‌کند.</summary>
    /// <param name="argo">مقادیر جدید شخص.</param>
    /// <param name="id">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت یافتن و اجرای به‌روزرسانی؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(UpdatePeopleCommand command, CancellationToken token = default);

    /// <summary>شخص مشخص‌شده را حذف می‌کند.</summary>
    /// <param name="argo">شناسهٔ یکتای شخص.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت حذف شخص؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(DeletePeopleCommand command, CancellationToken token = default);




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

}