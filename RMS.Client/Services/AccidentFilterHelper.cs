using System.Globalization;
using RMS.Client.Models;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Enums;

namespace RMS.Client.Services;

/// <summary>ابزارهای مشترک فیلتر حوادث در صفحات کلاینت.</summary>
public static class AccidentFilterHelper
{
    /// <summary>ابتدا و انتهای ماه انتخاب‌شده در تقویم فارسی را به زمان یونیکس تبدیل می‌کند.</summary>
    public static (long StartTime, long EndTime) GetPersianMonthRange(DateTime selectedDate)
    {
        var calendar = new PersianCalendar();
        var year = calendar.GetYear(selectedDate);
        var month = calendar.GetMonth(selectedDate);
        var start = DateTime.SpecifyKind(calendar.ToDateTime(year, month, 1, 0, 0, 0, 0), DateTimeKind.Local);
        var end = DateTime.SpecifyKind(calendar.ToDateTime(year, month, calendar.GetDaysInMonth(year, month), 23, 59, 59, 999), DateTimeKind.Local);
        return (new DateTimeOffset(start).ToUnixTimeSeconds(), new DateTimeOffset(end).ToUnixTimeSeconds());
    }

    /// <summary>حوادث راه و ماه انتخاب‌شده را با عبارت جستجو دریافت می‌کند.</summary>
    public static async Task<IReadOnlyList<LookupOption>> SearchAccidentsAsync(
        AccidentApiService accidentApi,
        Guid? roadId,
        DateTime selectedDate,
        string term,
        CancellationToken token)
    {
        if (!roadId.HasValue)
            return [];

        var range = GetPersianMonthRange(selectedDate);
        var result = await accidentApi.GetAllAsync(new AccidentQuery
        {
            StartTime = range.StartTime,
            EndTime = range.EndTime,
            RoadId = roadId,
            SearchTerm = term,
            Take = 20,
            OrderBy = AccidentSortOrder.AccidentTimeDescending
        }, token);

        return result.Items
            .Select(accident => new LookupOption(
                accident.Id.ToString(),
                $"{accident.RoadName} · {RmsUi.DateTime(accident.AccidentTimeUnix)}"))
            .ToArray();
    }
}