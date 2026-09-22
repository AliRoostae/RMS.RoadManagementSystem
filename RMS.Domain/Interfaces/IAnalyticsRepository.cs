using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;

public interface IAnalyticsRepository
{
    Task<DashboardOverviewResponse> GetDashboardAsync(
        DashboardQuery query,
        CancellationToken token = default);

    Task<AccidentReportResponse> GetAccidentReportAsync(
        AccidentReportQuery query,
        CancellationToken token = default);

    /// <summary>
    /// لیست  تمام افراد ثبت شده چه در سرنشین و چه در عابر در سامانه ثبت شده
    /// </summary>
    /// <param name="argo">معیارهای جست‌وجو و صفحه‌بندی.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns>فهرست اشخاص منطبق با معیارها.</returns>
    Task<PagedResponse<HumanItemResponse>> GetAllHumanAsync(HumanQueries argo, CancellationToken token = default);
}
