using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Interface;

/// <summary>قرارداد سرویس گزارش‌ها و آمارهای داشبورد.</summary>
public interface IAnalyticsService
{
    /// <summary>خلاصهٔ آماری داشبورد را برمی‌گرداند.</summary>
    /// <param name="query">بازه و تنظیمات داشبورد.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>اطلاعات خلاصهٔ داشبورد.</returns>
    Task<DashboardOverviewResponse> GetDashboardAsync(
        DashboardQuery query,
        CancellationToken token = default);

    /// <summary>گزارش آماری حوادث را برمی‌گرداند.</summary>
    /// <param name="query">فیلترهای گزارش.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>گزارش تجمیعی حوادث.</returns>
    Task<AccidentReportResponse> GetAccidentReportAsync(
        AccidentReportQuery query,
        CancellationToken token = default);

    /// <summary>فهرست صفحه‌بندی‌شدهٔ عابرو سرنشین را دریافت می‌کند.</summary>
    /// <param name="argo">معیارهای جست‌وجو و صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست اشخاص منطبق با معیارها.</returns>
    Task<PagedResponse<HumanItemResponse>> GetAllHumanAsync(HumanQueries argo, CancellationToken token = default);

}