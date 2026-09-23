using Microsoft.EntityFrameworkCore;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Infrastructure.Persistence.EF.Repository;

public sealed class LookupRepository(RmsDbContext db) : ILookupRepository
{
    public async Task<IReadOnlyList<RoadLookupResponse>> GetRoadsAsync(
        LookupQuery query,
        CancellationToken token = default)
    {
        var roads = db.RoadsDs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            if (Guid.TryParse(term, out var roadId))
                roads = roads.Where(road => road.Id == roadId || road.Name.Contains(term));
            else
                roads = roads.Where(road => road.Name.Contains(term));
        }

        return await roads
            .OrderBy(road => road.Name)
            .ThenBy(road => road.Id)
            .Take(query.Take)
            .Select(road => new RoadLookupResponse(road.Id, road.Name))
            .ToListAsync(token);
    }

    public async Task<IReadOnlyList<AccidentLookupResponse>> GetAccidentsAsync(
        AccidentLookupQuery query,
        CancellationToken token = default)
    {
        var accidents = db.AccidentDs.AsNoTracking();
        if (query.RoadId.HasValue)
            accidents = accidents.Where(accident => accident.FkIdRoad == query.RoadId.Value);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            if (Guid.TryParse(term, out var accidentId))
                accidents = accidents.Where(accident => accident.Id == accidentId || accident.Road.Name.Contains(term));
            else
                accidents = accidents.Where(accident => accident.Road.Name.Contains(term));
        }

        return await accidents
            .OrderByDescending(accident => accident.AccidentTimeUnix)
            .ThenBy(accident => accident.Id)
            .Take(query.Take)
            .Select(accident => new AccidentLookupResponse(
                accident.Id,
                accident.AccidentTimeUnix,
                accident.FkIdRoad,
                accident.Road.Name))
            .ToListAsync(token);
    }

    public async Task<IReadOnlyList<CarLookupResponse>> GetCarsAsync(
        CarLookupQuery query,
        CancellationToken token = default)
    {
        var cars = db.CarDs.AsNoTracking();
        if (query.AccidentId.HasValue)
            cars = cars.Where(car => car.FkAccident == query.AccidentId.Value);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            if (Guid.TryParse(term, out var carId))
                cars = cars.Where(car =>
                    car.Id == carId ||
                    car.PlateNumber.Contains(term) ||
                    car.CarName.Contains(term));
            else
                cars = cars.Where(car =>
                    car.PlateNumber.Contains(term) ||
                    car.CarName.Contains(term));
        }

        return await cars
            .OrderBy(car => car.PlateNumber)
            .ThenBy(car => car.Id)
            .Take(query.Take)
            .Select(car => new CarLookupResponse(
                car.Id,
                car.FkAccident,
                car.PlateNumber,
                car.CarName))
            .ToListAsync(token);
    }
}