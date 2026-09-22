using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;


/// <summary>
/// قرارداد بررسی یکتایی کد ملی تمام افراد درگیر در یک حادثه، شامل سرنشینان و عابر را تعریف می‌کند.
/// </summary>
public interface INationalCode
{


    /// <summary>
    /// تکراری‌بودن کد ملی را میان تمام افراد یک حادثه هنگام ثبت بررسی می‌کند.
    /// </summary>
    /// <param name="nationalCode">کد ملی شخص.</param>
    /// <param name="fkAccident">شناسهٔ یکتای حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود کد ملی؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DuplicateNationalCodeAsync(string nationalCode, Guid fkAccident, CancellationToken token = default);


    /// <summary>
    /// تکراری‌بودن کد ملی را هنگام ویرایش، با صرف‌نظر از رکورد جاری، میان تمام افراد حادثه بررسی می‌کند.
    /// </summary>
    /// <param name="nationalCode">کد ملی شخص.</param>
    /// <param name="fkId">شناسهٔ یکتای حادثه.</param>
    /// <param name="id">شناسهٔ رکورد شخص در حال ویرایش.</param>
    /// <param name="peoplOrPass">اگر <see langword="true"/> باشد رکورد جاری از عابر و در غیر این صورت از سرنشینان است.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود کد ملی در رکوردی دیگر؛ در غیر این صورت <see langword="false"/>.</returns>
    Task<bool> DuplicateNationalCodeEditAsync(string nationalCode, Guid fkId, Guid id, bool peoplOrPass, CancellationToken token = default);


  
}
