using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="ICarRepository"/>
public sealed class CarRepository
     (
      RmsDbContext _db


    )
    : ICarRepository
{
    /// <inheritdoc/>
    public async Task<bool> AddAsync(CarEntities argo, CancellationToken token = default)
    {
        await _db.CarDs.AddAsync(argo);
        return await _db.SaveChangesAsync(token) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> AnyCarInAccidenAsync(Guid idAcciden, CancellationToken token = default) => await _db.CarDs.AsNoTracking().AnyAsync(i => i.FkAccident == idAcciden, token);


    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid argo, CancellationToken token = default)
    {
        var del = await _db.CarDs.FirstOrDefaultAsync(i => i.Id == argo, token);
        if (del == null) return false;

        _db.CarDs.Remove(del);
        return await _db.SaveChangesAsync(token) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DuplicatePlateAsync(string plateNumber, Guid fkAccident, CancellationToken token = default) => await _db.CarDs.AsNoTracking().Where(i => i.FkAccident == fkAccident).AnyAsync(i => i.PlateNumber == plateNumber, token);

    /// <inheritdoc/>
    public async Task<bool> DuplicatePlateAsync(string plateNumber, Guid fkAccident, Guid id, CancellationToken token = default) => await _db.CarDs.AsNoTracking().Where(i => i.FkAccident == fkAccident && i.Id != id).AnyAsync(i => i.PlateNumber == plateNumber, token);

    /// <summary>پرس‌وجوی موجودیت خودرو را به نمای خواندنی همراه با میانگین آسیب سرنشینان تبدیل می‌کند.</summary>
    /// <param name="query">پرس‌وجوی مبدأ خودروها.</param>
    /// <returns>پرس‌وجوی قابل‌ترجمه به نمای خودروها.</returns>
    private static IQueryable<CarResponse> ProjectToResponse(
    IQueryable<CarEntities> query)
    {
        return query.
            Select(i => new
            {
                Car = i,
                InjuryPercentage = i.PassengerList.Average(p => (double?)p.InjuryPercentage) ?? 0
            })
            .Select(i => new CarResponse
            {
                CarClass = i.Car.CarClass,
                CarName = i.Car.CarName,
                Color = i.Car.Color,
                DamagePercentage = i.Car.DamagePercentage,
                DateLic = i.Car.DateLic,
                DateLicValidity = i.Car.DateLicValidity,
                DriverLicNumber = i.Car.DriverLicNumber,
                DriverPhone = i.Car.DriverPhone,
                FkAccident = i.Car.FkAccident,
                Id = i.Car.Id,
                PlateNumber = i.Car.PlateNumber,
                ProductionYear = i.Car.ProductionYear,
                InjuryPercentage = (int)i.InjuryPercentage
            });


    }

    /// <inheritdoc/>
    public async Task<PagedResponse<CarListItemResponse>> GetAllAsync(CarQueries argo, CancellationToken token = default)
    {
        var source = _db.CarDs.AsNoTracking();
        if (argo.AccidentId.HasValue)
            source = source.Where(car => car.FkAccident == argo.AccidentId.Value);
        if (!string.IsNullOrWhiteSpace(argo.Plate))
            source = source.Where(car => car.PlateNumber.Contains(argo.Plate.Trim()));
        if (argo.CarClass.HasValue)
            source = source.Where(car => car.CarClass == argo.CarClass.Value);
        if (!string.IsNullOrWhiteSpace(argo.SearchTerm))
        {
            var term = argo.SearchTerm.Trim();
            if (Guid.TryParse(term, out var accidentId))
                source = source.Where(car =>
                    car.FkAccident == accidentId ||
                    car.PlateNumber.Contains(term) ||
                    car.CarName.Contains(term) ||
                    car.DriverPhone.Contains(term));
            else
                source = source.Where(car =>
                    car.PlateNumber.Contains(term) ||
                    car.CarName.Contains(term) ||
                    car.DriverPhone.Contains(term));
        }

        var totalCount = await source.CountAsync(token);
        var rawItems = await source
            .OrderBy(car => car.PlateNumber)
            .ThenBy(car => car.Id)
            .Skip(argo.Skip)
            .Take(argo.Take)
            .Select(car => new
            {
                car.Id,
                AccidentId = car.FkAccident,
                car.PlateNumber,
                car.CarName,
                car.CarClass,
                car.ProductionYear,
                DamagePercentage = (int)car.DamagePercentage
            })
            .ToListAsync(token);
        var items = rawItems.Select(car => new CarListItemResponse(
                car.Id,
                car.AccidentId,
                AccidentCode(car.AccidentId),
                car.PlateNumber,
                car.CarName,
                car.CarClass,
                car.ProductionYear,
                car.DamagePercentage))
            .ToList();

        return new PagedResponse<CarListItemResponse>(items, totalCount, argo.Skip, argo.Take);
    }

    /// <inheritdoc/>
    public async Task<CarResponse?> GetAsync(Guid argo, CancellationToken token = default)
    {
        var sorc = _db.CarDs.AsNoTracking()
            .Where(i => i.Id == argo);


        var result = await ProjectToResponse(sorc)
                    .FirstOrDefaultAsync(token);


        return result;
    }



    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(BaseCarEdit argo, Guid id, CancellationToken token = default)
    {
        var find = await _db.CarDs.FirstOrDefaultAsync(i => i.Id == id, token);
        if (find == null) return false;
        _db.Entry(find).CurrentValues.SetValues(argo);

        return await _db.SaveChangesAsync(token) >= 0;

    }

    /// <inheritdoc/>
    public async Task<bool> AnyAsync(Guid idcar, CancellationToken token = default) =>
        await _db.CarDs.AsNoTracking().AnyAsync(_ => _.Id == idcar, token);

    public async Task<bool> CarIsAccidentAsync(Guid idCar, Guid idAccident, CancellationToken token) =>
        await _db.CarDs.AsNoTracking().AnyAsync(_ => _.Id == idCar && _.FkAccident == idAccident, token);

    public async Task<Guid> GetParentAsync(Guid idcar, CancellationToken token)
    {
        var find = await _db.CarDs.AsNoTracking().FirstOrDefaultAsync(i => i.Id == idcar, cancellationToken: token);
        return find?.FkAccident ?? Guid.Empty;
    }

    private static string AccidentCode(Guid id) => $"ACC-{id.ToString("N")[..8].ToUpperInvariant()}";
}
