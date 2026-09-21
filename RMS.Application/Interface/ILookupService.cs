using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Interface;

/// <summary>قرارداد lookupهای سبک برای انتخاب در فرم‌ها.</summary>
public interface ILookupService
{
    /// <summary>فهرست راه‌های قابل انتخاب را برمی‌گرداند.</summary>
    /// <param name="query">عبارت جست‌وجو و سقف نتایج.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task<IReadOnlyList<RoadLookupResponse>> GetRoadsAsync(
        LookupQuery query,
        CancellationToken token = default);

    /// <summary>فهرست حوادث قابل انتخاب را برمی‌گرداند.</summary>
    /// <param name="query">فیلتر lookup حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task<IReadOnlyList<AccidentLookupResponse>> GetAccidentsAsync(
        AccidentLookupQuery query,
        CancellationToken token = default);

    /// <summary>فهرست خودروهای قابل انتخاب را برمی‌گرداند.</summary>
    /// <param name="query">فیلتر lookup خودرو.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task<IReadOnlyList<CarLookupResponse>> GetCarsAsync(
        CarLookupQuery query,
        CancellationToken token = default);
}
