using System.ComponentModel.DataAnnotations;
using RMS.Application.Excep;
using RMS.Application.Interface;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Service;

public sealed class AnalyticsService(IAnalyticsRepository repository) : IAnalyticsService
{
    public Task<DashboardOverviewResponse> GetDashboardAsync(
        DashboardQuery query,
        CancellationToken token = default)
    {
        ValidateRange(query, query.StartTime, query.EndTime);
        return repository.GetDashboardAsync(query, token);
    }

    public Task<AccidentReportResponse> GetAccidentReportAsync(
        AccidentReportQuery query,
        CancellationToken token = default)
    {
        ValidateRange(query, query.StartTime, query.EndTime);
        if (query.AccidentType.HasValue && !Enum.IsDefined(query.AccidentType.Value))
            throw new ValidationException("نوع حادثه نامعتبر است.");
        return repository.GetAccidentReportAsync(query, token);
    }

    private static void ValidateRange(object query, long startTime, long endTime)
    {
        if (!query.TryValidate(out var validationErrors))
            throw new ValidationException(validationErrors.JsonErrors());
        if (endTime <= startTime)
            throw new ValidationException("پایان بازه باید بعد از شروع بازه باشد.");
    }

    public Task<PagedResponse<HumanItemResponse>> GetAllHumanAsync(HumanQueries argo, CancellationToken token = default)
        => repository.GetAllHumanAsync(argo, token);


}