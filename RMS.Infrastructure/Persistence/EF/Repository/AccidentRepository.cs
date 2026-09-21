using Microsoft.EntityFrameworkCore;
using RMS.Shared.Contracts.DTOs;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Domain.Entities;


namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="IAccidentRepository"/>
public sealed class AccidentRepository
    (
      RmsDbContext _db


    )
    : IAccidentRepository
{
    /// <inheritdoc/>
    public async Task<bool> AddAsync(AccidentEntities argo, CancellationToken token = default)
    {
        await _db.AccidentDs.AddAsync(argo);
        return await _db.SaveChangesAsync(token) > 0;
    }

   

    /// <inheritdoc/>
    public async Task<bool> AnySubsetRoadAsync(Guid idRoad, CancellationToken token = default) => await _db.AccidentDs.AnyAsync(a => a.FkIdRoad == idRoad, token);

   

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid argo, CancellationToken token = default)
    {
        var find = await _db.AccidentDs.FirstOrDefaultAsync(a => a.Id == argo, token);
        if (find == null) return false;
        _db.AccidentDs.Remove(find);
        return await _db.SaveChangesAsync(token) > 0;
    }

    

    /// <inheritdoc/>
    public async Task<PagedResponse<AccidentListItemResponse>> GetAllAsync(AccidentQuery argo, CancellationToken token = default)
    {
        var source = _db.AccidentDs
            .AsNoTracking()
            .Where(a =>
                a.AccidentTimeUnix >= argo.StartTime &&
                a.AccidentTimeUnix < argo.EndTime);

        if (argo.RoadId.HasValue)
            source = source.Where(accident => accident.FkIdRoad == argo.RoadId.Value);
        if (argo.AccidentType.HasValue)
            source = source.Where(accident => accident.AccidentType == argo.AccidentType.Value);
        if (argo.Weather.HasValue)
            source = source.Where(accident => accident.Weather == argo.Weather.Value);
        if (argo.Cause.HasValue)
            source = source.Where(accident => accident.AccidentCause == argo.Cause.Value);
        if (!string.IsNullOrWhiteSpace(argo.SearchTerm))
        {
            var term = argo.SearchTerm.Trim();
            if (Guid.TryParse(term, out var accidentId))
                source = source.Where(accident => accident.Id == accidentId || accident.Road.Name.Contains(term));
            else
                source = source.Where(accident => accident.Road.Name.Contains(term));
        }

        var totalCount = await source.CountAsync(token);
        source = argo.OrderBy == RMS.Shared.Enums.AccidentSortOrder.AccidentTimeAscending
            ? source.OrderBy(accident => accident.AccidentTimeUnix).ThenBy(accident => accident.Id)
            : source.OrderByDescending(accident => accident.AccidentTimeUnix).ThenBy(accident => accident.Id);

        var items = await source
            .Skip(argo.Skip)
            .Take(argo.Take)
            .Select(accident => new
            {
                accident.Id,
                accident.AccidentTimeUnix,
                RoadId = accident.FkIdRoad,
                RoadName = accident.Road.Name,
                accident.AccidentType,
                accident.Weather,
                OutsidePeopleCount = accident.PeopleList.Count(),
                PassengersCount = accident.CarList.SelectMany(car => car.PassengerList).Count(),
                OutsidePeopleInjurySum = accident.PeopleList.Sum(person => (double?)person.InjuryPercentage) ?? 0,
                PassengersInjurySum = accident.CarList.SelectMany(car => car.PassengerList)
                    .Sum(person => (double?)person.InjuryPercentage) ?? 0
            })
            .Select(item => new AccidentListItemResponse(
                item.Id,
                item.AccidentTimeUnix,
                item.RoadId,
                item.RoadName,
                item.AccidentType,
                item.Weather,
                item.OutsidePeopleCount + item.PassengersCount,
                item.OutsidePeopleCount + item.PassengersCount == 0
                    ? 0
                    : (int)((item.OutsidePeopleInjurySum + item.PassengersInjurySum) /
                        (item.OutsidePeopleCount + item.PassengersCount))))
            .ToListAsync(token);

        return new PagedResponse<AccidentListItemResponse>(items, totalCount, argo.Skip, argo.Take);
    }

    public async Task<IReadOnlyList<AccidentMapPointResponse>> GetMapAsync(
        AccidentMapQuery query,
        CancellationToken token = default)
    {
        var source = _db.AccidentDs
            .AsNoTracking()
            .Where(accident =>
                accident.AccidentTimeUnix >= query.StartTime &&
                accident.AccidentTimeUnix < query.EndTime);

        if (query.RoadId.HasValue)
            source = source.Where(accident => accident.FkIdRoad == query.RoadId.Value);
        if (query.MinLatitude.HasValue)
        {
            source = source.Where(accident =>
                accident.Latitude >= query.MinLatitude.Value &&
                accident.Latitude <= query.MaxLatitude!.Value &&
                accident.Longitude >= query.MinLongitude!.Value &&
                accident.Longitude <= query.MaxLongitude!.Value);
        }

        return await source
            .OrderByDescending(accident => accident.AccidentTimeUnix)
            .ThenBy(accident => accident.Id)
            .Take(query.Take)
            .Select(accident => new AccidentMapPointResponse(
                accident.Id,
                accident.FkIdRoad,
                accident.AccidentTimeUnix,
                accident.Latitude,
                accident.Longitude))
            .ToListAsync(token);
    }

 

    /// <inheritdoc/>
    public async Task<AccidentResponse?> GetAsync(Guid id, CancellationToken token = default)
    {
        var source = _db.AccidentDs
            .AsNoTracking()
            .Where(a => a.Id == id);

        return await ProjectToResponse(source)
            .FirstOrDefaultAsync(token);
    }

    /// <summary>پرس‌وجوی موجودیت حادثه را به نمای خواندنی همراه با شاخص‌های تجمیعی تبدیل می‌کند.</summary>
    /// <param name="query">پرس‌وجوی مبدأ حوادث.</param>
    /// <returns>پرس‌وجوی قابل‌ترجمه به نمای حوادث.</returns>
    private static IQueryable<AccidentResponse> ProjectToResponse(
        IQueryable<AccidentEntities> query)
    {
        return query
            .Select(a => new
            {
                Accident = a,
                OutsidePeopleCount = a.PeopleList.Count(),
                PassengersCount = a.CarList
                    .SelectMany(c => c.PassengerList)
                    .Count(),
                OutsidePeopleInjurySum = a.PeopleList
                    .Sum(p => (double?)p.InjuryPercentage) ?? 0,
                PassengersInjurySum = a.CarList
                    .SelectMany(c => c.PassengerList)
                    .Sum(p => (double?)p.InjuryPercentage) ?? 0
            })
            .Select(x => new AccidentResponse
            {
                Id = x.Accident.Id,
                AccidentTimeUnix = x.Accident.AccidentTimeUnix,
                AccidentType = x.Accident.AccidentType,
                AccidentCause = x.Accident.AccidentCause,
                FkIdRoad = x.Accident.FkIdRoad,
                Latitude = x.Accident.Latitude,
                Longitude = x.Accident.Longitude,
                LightingCondition = x.Accident.LightingCondition,
                Weather = x.Accident.Weather,
                RoadCondition = x.Accident.RoadCondition,
                TrafficSignCondition = x.Accident.TrafficSignCondition,
                DamagePercentageCar = (int)(
                    x.Accident.CarList
                        .Average(c => (double?)c.DamagePercentage) ?? 0),
                PeopleCount = x.OutsidePeopleCount + x.PassengersCount,
                InjuryPercentage = x.OutsidePeopleCount + x.PassengersCount == 0
                    ? 0
                    : (int)(
                        (x.OutsidePeopleInjurySum + x.PassengersInjurySum) /
                        (x.OutsidePeopleCount + x.PassengersCount))
            });
    }
    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(BaseAccident argo, Guid id, CancellationToken token = default)
    {
        var find = await _db.AccidentDs.FirstOrDefaultAsync(a => a.Id == id, token);
        if (find == null) return false;
        _db.Entry(find).CurrentValues.SetValues(argo);
        return await _db.SaveChangesAsync(token) >= 0;
    }

    /// <inheritdoc/>
    public async Task<bool> AnyAsync(Guid idAccident, CancellationToken token = default) =>
      await _db.AccidentDs.AsNoTracking().AnyAsync(i => i.Id == idAccident, token);
}
