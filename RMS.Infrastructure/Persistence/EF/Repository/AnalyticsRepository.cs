using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Infrastructure.Persistence.EF.Repository;

public sealed class AnalyticsRepository(RmsDbContext db) : IAnalyticsRepository
{
    /// <inheritdoc/>
    public async Task<PagedResponse<HumanItemResponse>> GetAllHumanAsync(HumanQueries argo, CancellationToken token = default)
    {

        List<HumanItemResponse> itemHuman = [];
        int humanCount;
        // در صورتی که فیلتر راننده وارد شده  عملا بی فایده است و فقط در خودرو جستجو می کنم 
        if (!(argo.IsDriver.HasValue && argo.IsDriver.Value))
        {
            #region People
            var sourcePeople = db.PeopleDs.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(argo.NationalCode))
                sourcePeople = sourcePeople.Where(person => person.NationalCode.Contains(argo.NationalCode.Trim()));
            if (argo.DamageType.HasValue)
                sourcePeople = sourcePeople.Where(person => person.TypePersonDamage == argo.DamageType.Value);
            var totalCountPeople = await sourcePeople.CountAsync(token);
            humanCount = +totalCountPeople;
            var rawItemsPeople = await sourcePeople
                .OrderBy(person => person.NationalCode)
                .Skip(argo.Skip)
                .Take(argo.Take)
                .Select(person => new
                {
                    person.Id,
                    AccidentId = person.FkAccident,
                    person.NationalCode,
                    FullName = person.PassengerFullName,
                    person.Gender,
                    person.Age,
                    person.InjuryPercentage,
                    DamageType = person.TypePersonDamage
                })
                .ToListAsync(token);
            var itemsPeople = rawItemsPeople.Select(person => new HumanItemResponse(
                    person.Id,
                    person.AccidentId,
                    AccidentCode(person.AccidentId),
                    person.NationalCode,
                    person.FullName,
                    person.Gender,
                    person.Age,
                    person.InjuryPercentage,
                    person.DamageType,
                    false, false))
                .ToList();

            itemHuman.AddRange(itemsPeople);
            #endregion

        }

        #region Passenger

        var sourcePassenger = db.PassengerDs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(argo.NationalCode))
            sourcePassenger = sourcePassenger.Where(person => person.NationalCode.Contains(argo.NationalCode.Trim()));
        if (argo.DamageType.HasValue)
            sourcePassenger = sourcePassenger.Where(person => person.TypePersonDamage == argo.DamageType.Value);
        if (argo.IsDriver.HasValue)
            sourcePassenger = sourcePassenger.Where(passenger => passenger.IsDriver == argo.IsDriver.Value);
        var totalCountPassenger = await sourcePassenger.CountAsync(token);
        humanCount = +totalCountPassenger;
        var rawItemsPassenger = await sourcePassenger
            .OrderBy(person => person.NationalCode)
            .Skip(argo.Skip)
            .Take(argo.Take)
            .Select(person => new
            {
                person.Id,
                AccidentId = person.Car.FkAccident,
                person.NationalCode,
                FullName = person.PassengerFullName,
                person.Gender,
                person.Age,
                person.InjuryPercentage,
                DamageType = person.TypePersonDamage,
                person.IsDriver
            })
            .ToListAsync(token);
        var itemsPassenger = rawItemsPassenger.Select(person => new HumanItemResponse(
                person.Id,
                person.AccidentId,
                AccidentCode(person.AccidentId),
                person.NationalCode,
                person.FullName,
                person.Gender,
                person.Age,
                person.InjuryPercentage,
                person.DamageType,
                true, person.IsDriver))
            .ToList();

        itemHuman.AddRange(itemsPassenger);
        #endregion


        return new PagedResponse<HumanItemResponse>(itemHuman, humanCount, argo.Skip, argo.Take);
    }

    private static string AccidentCode(Guid id) => $"ACC-{id.ToString("N")[..8].ToUpperInvariant()}";
    public async Task<DashboardOverviewResponse> GetDashboardAsync(
        DashboardQuery query,
        CancellationToken token = default)
    {
        var accidents = FilterAccidents(query.StartTime, query.EndTime);
        var metrics = ProjectMetrics(accidents);
        var totals = await GetTotalsAsync(metrics, token);
        var roadCount = await db.RoadsDs.AsNoTracking().CountAsync(token);
        var monthlyTrend = await GetMonthlyTrendAsync(
            accidents,
            query.StartTime,
            query.EndTime,
            query.Months,
            token);
        var causes = await GetCategoryCountsAsync(metrics, metric => (byte)metric.Cause, token);
        var roadConditions = await GetCategoryCountsAsync(metrics, metric => (byte)metric.RoadCondition, token);
        var recent = await metrics
            .OrderByDescending(metric => metric.AccidentTimeUnix)
            .ThenBy(metric => metric.Id)
            .Take(query.RecentCount)
            .Select(metric => new RecentAccidentResponse(
                metric.Id,
                metric.AccidentTimeUnix,
                metric.RoadId,
                metric.RoadName,
                metric.Type,
                (int)metric.InjuryPercentage))
            .ToListAsync(token);

        return new DashboardOverviewResponse(
            totals.AccidentCount,
            roadCount,
            totals.PeopleCount,
            totals.AverageInjury,
            monthlyTrend,
            causes,
            roadConditions,
            recent);
    }

    public async Task<AccidentReportResponse> GetAccidentReportAsync(
        AccidentReportQuery query,
        CancellationToken token = default)
    {
        var accidents = FilterAccidents(query.StartTime, query.EndTime);
        if (query.RoadId.HasValue)
            accidents = accidents.Where(accident => accident.FkIdRoad == query.RoadId.Value);
        if (query.AccidentType.HasValue)
            accidents = accidents.Where(accident => accident.AccidentType == query.AccidentType.Value);

        var metrics = ProjectMetrics(accidents);
        var totals = await GetTotalsAsync(metrics, token);
        var carCount = await accidents.SelectMany(accident => accident.CarList).CountAsync(token);
        var monthlyTrend = await GetMonthlyTrendAsync(
            accidents,
            query.StartTime,
            query.EndTime,
            query.Months,
            token);
        var types = await GetCategoryCountsAsync(metrics, metric => (byte)metric.Type, token);
        var weather = await GetCategoryCountsAsync(metrics, metric => (byte)metric.Weather, token);
        var causes = await GetCategoryCountsAsync(metrics, metric => (byte)metric.Cause, token);

        return new AccidentReportResponse(
            totals.AccidentCount,
            totals.PeopleCount,
            carCount,
            totals.AverageInjury,
            monthlyTrend,
            types,
            weather,
            causes);
    }

    private IQueryable<AccidentEntities> FilterAccidents(long startTime, long endTime) =>
        db.AccidentDs
            .AsNoTracking()
            .Where(accident =>
                accident.AccidentTimeUnix >= startTime &&
                accident.AccidentTimeUnix < endTime);

    private static IQueryable<AccidentMetric> ProjectMetrics(IQueryable<AccidentEntities> accidents) =>
        accidents.Select(accident => new AccidentMetric
        {
            Id = accident.Id,
            AccidentTimeUnix = accident.AccidentTimeUnix,
            RoadId = accident.FkIdRoad,
            RoadName = accident.Road.Name,
            Type = accident.AccidentType,
            Cause = accident.AccidentCause,
            Weather = accident.Weather,
            RoadCondition = accident.RoadCondition,
            PeopleCount = accident.PeopleList.Count() +
                accident.CarList.SelectMany(car => car.PassengerList).Count(),
            InjuryPercentage = accident.PeopleList.Count() +
                accident.CarList.SelectMany(car => car.PassengerList).Count() == 0
                    ? 0
                    : ((accident.PeopleList.Sum(person => (double?)person.InjuryPercentage) ?? 0) +
                       (accident.CarList.SelectMany(car => car.PassengerList)
                           .Sum(person => (double?)person.InjuryPercentage) ?? 0)) /
                      (accident.PeopleList.Count() +
                       accident.CarList.SelectMany(car => car.PassengerList).Count())
        });

    private static async Task<AnalyticsTotals> GetTotalsAsync(
        IQueryable<AccidentMetric> metrics,
        CancellationToken token)
    {
        var totals = await metrics
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AccidentCount = group.Count(),
                PeopleCount = group.Sum(metric => metric.PeopleCount),
                AverageInjury = group.Average(metric => metric.InjuryPercentage)
            })
            .FirstOrDefaultAsync(token);

        return totals is null
            ? new AnalyticsTotals(0, 0, 0)
            : new AnalyticsTotals(
                totals.AccidentCount,
                totals.PeopleCount,
                (int)Math.Round(totals.AverageInjury));
    }

    private static async Task<IReadOnlyList<MonthlyAccidentMetricResponse>> GetMonthlyTrendAsync(
        IQueryable<AccidentEntities> accidents,
        long startTime,
        long endTime,
        int months,
        CancellationToken token)
    {
        var endDate = DateTimeOffset.FromUnixTimeSeconds(endTime - 1).ToUniversalTime();
        var lastMonth = new DateTimeOffset(endDate.Year, endDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var firstMonth = lastMonth.AddMonths(-(months - 1));
        var result = new List<MonthlyAccidentMetricResponse>(months);

        for (var index = 0; index < months; index++)
        {
            var monthStart = firstMonth.AddMonths(index);
            var monthEnd = monthStart.AddMonths(1);
            var bucketStart = Math.Max(startTime, monthStart.ToUnixTimeSeconds());
            var bucketEnd = Math.Min(endTime, monthEnd.ToUnixTimeSeconds());
            if (bucketEnd <= bucketStart)
            {
                result.Add(new MonthlyAccidentMetricResponse(monthStart.ToUnixTimeSeconds(), 0, 0));
                continue;
            }

            var totals = await GetTotalsAsync(
                ProjectMetrics(accidents.Where(accident =>
                    accident.AccidentTimeUnix >= bucketStart &&
                    accident.AccidentTimeUnix < bucketEnd)),
                token);
            result.Add(new MonthlyAccidentMetricResponse(
                monthStart.ToUnixTimeSeconds(),
                totals.AccidentCount,
                totals.AverageInjury));
        }

        return result;
    }

    private static async Task<IReadOnlyList<CategoryCountResponse>> GetCategoryCountsAsync(
        IQueryable<AccidentMetric> metrics,
        System.Linq.Expressions.Expression<Func<AccidentMetric, byte>> selector,
        CancellationToken token)
    {
        var counts = await metrics
            .GroupBy(selector)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(token);

        return counts
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Key)
            .Select(item => new CategoryCountResponse(item.Key, item.Count))
            .ToList();
    }

    private sealed class AccidentMetric
    {
        public Guid Id
        {
            get; init;
        }
        public long AccidentTimeUnix
        {
            get; init;
        }
        public Guid RoadId
        {
            get; init;
        }
        public string RoadName { get; init; } = string.Empty;
        public RMS.Shared.Enums.AccidentTypeEnums Type
        {
            get; init;
        }
        public RMS.Shared.Enums.AccidentCauseEnums Cause
        {
            get; init;
        }
        public RMS.Shared.Enums.WeatherConditionEnums Weather
        {
            get; init;
        }
        public RMS.Shared.Enums.RoadConditionEnums RoadCondition
        {
            get; init;
        }
        public int PeopleCount
        {
            get; init;
        }
        public double InjuryPercentage
        {
            get; init;
        }
    }

    private sealed record AnalyticsTotals(
        int AccidentCount,
        int PeopleCount,
        int AverageInjury);
}