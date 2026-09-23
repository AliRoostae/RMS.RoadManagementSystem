using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="IPassengerRepository"/>
public sealed class PassengerRepository
    (
    RmsDbContext _db
    ) : IPassengerRepository
{
    /// <inheritdoc/>
    public async Task<bool> AddAsync(PassengerEntities argo, CancellationToken token = default)
    {
        await _db.PassengerDs.AddAsync(argo, token);
        return await _db.SaveChangesAsync(token) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid argo, CancellationToken token = default)
    {
        var find = await _db.PassengerDs.FirstOrDefaultAsync(i => i.Id == argo, token);
        if (find == null)
            return false;
        _db.PassengerDs.Remove(find);
        return await _db.SaveChangesAsync(token) > 0;
    }


    /// <summary>پرس‌وجوی موجودیت سرنشین را به نمای خواندنی تبدیل می‌کند.</summary>
    /// <param name="query">پرس‌وجوی مبدأ رانندگان و سرنشینان.</param>
    /// <returns>پرس‌وجوی قابل‌ترجمه به نمای سرنشینان.</returns>
    private static IQueryable<PassengerResponse> ProjectToResponse(
IQueryable<PassengerEntities> query)
    {
        return query
            .Select(i => new PassengerResponse
            {
                Age = i.Age,
                DescriptionDamage = i.DescriptionDamage,
                FkCar = i.FkCar,
                Gender = i.Gender,
                Id = i.Id,
                InjuryPercentage = i.InjuryPercentage,
                IsDriver = i.IsDriver,
                PassengerFullName = i.PassengerFullName,
                NationalCode = i.NationalCode,
                TypePersonDamage = i.TypePersonDamage


            });


    }
    /// <inheritdoc/>
    public async Task<PagedResponse<PassengerListItemResponse>> GetAllAsync(PassengerQueries argo, CancellationToken token = default)
    {
        var source = _db.PassengerDs.AsNoTracking();
        if (argo.CarId.HasValue)
            source = source.Where(passenger => passenger.FkCar == argo.CarId.Value);
        if (argo.AccidentId.HasValue)
            source = source.Where(passenger => passenger.Car.FkAccident == argo.AccidentId.Value);
        if (!string.IsNullOrWhiteSpace(argo.NationalCode))
            source = source.Where(passenger => passenger.NationalCode.Contains(argo.NationalCode.Trim()));
        if (argo.IsDriver.HasValue)
            source = source.Where(passenger => passenger.IsDriver == argo.IsDriver.Value);
        if (argo.DamageType.HasValue)
            source = source.Where(passenger => passenger.TypePersonDamage == argo.DamageType.Value);
        if (!string.IsNullOrWhiteSpace(argo.SearchTerm))
        {
            var term = argo.SearchTerm.Trim();
            if (Guid.TryParse(term, out var accidentId))
                source = source.Where(passenger =>
                    passenger.Car.FkAccident == accidentId ||
                    passenger.NationalCode.Contains(term) ||
                    passenger.PassengerFullName.Contains(term) ||
                    passenger.Car.PlateNumber.Contains(term));
            else if (term == "راننده")
                source = source.Where(passenger => passenger.IsDriver);
            else if (term == "سرنشین")
                source = source.Where(passenger => !passenger.IsDriver);
            else
                source = source.Where(passenger =>
                    passenger.NationalCode.Contains(term) ||
                    passenger.PassengerFullName.Contains(term) ||
                    passenger.Car.PlateNumber.Contains(term));
        }

        var totalCount = await source.CountAsync(token);
        var rawItems = await source
            .OrderBy(passenger => passenger.NationalCode)
            .ThenBy(passenger => passenger.Id)
            .Skip(argo.Skip)
            .Take(argo.Take)
            .Select(passenger => new
            {
                passenger.Id,
                CarId = passenger.FkCar,
                AccidentId = passenger.Car.FkAccident,
                CarPlate = passenger.Car.PlateNumber,
                passenger.NationalCode,
                FullName = passenger.PassengerFullName,
                passenger.IsDriver,
                passenger.Gender,
                passenger.InjuryPercentage,
                DamageType = passenger.TypePersonDamage
            })
            .ToListAsync(token);
        var items = rawItems.Select(passenger => new PassengerListItemResponse(
                passenger.Id,
                passenger.CarId,
                passenger.AccidentId,
                AccidentCode(passenger.AccidentId),
                passenger.CarPlate,
                passenger.NationalCode,
                passenger.FullName,
                passenger.IsDriver,
                passenger.Gender,
                passenger.InjuryPercentage,
                passenger.DamageType))
            .ToList();

        return new PagedResponse<PassengerListItemResponse>(items, totalCount, argo.Skip, argo.Take);
    }

    /// <inheritdoc/>
    public async Task<PassengerResponse?> GetAsync(Guid argo, CancellationToken token = default)
    {
        var query = _db.PassengerDs.AsNoTracking().Where(item => item.Id == argo);
        return await ProjectToResponse(query).FirstOrDefaultAsync(token);
    }



    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(BasePassenger argo, Guid id, CancellationToken token = default)
    {
        var find = await _db.PassengerDs.FirstOrDefaultAsync(i => i.Id == id, token);
        if (find == null)
            return false;
        _db.Entry(find).CurrentValues.SetValues(argo);
        return await _db.SaveChangesAsync(token) >= 0;

    }

    /// <inheritdoc/>
    public async Task<bool> OneDriverValidAsync(Guid fkCar, bool isDriver, CancellationToken token = default) =>
       isDriver ? await _db.PassengerDs.AsNoTracking().Where(i => i.FkCar == fkCar && i.IsDriver == isDriver).AnyAsync(token) : false;


    /// <inheritdoc/>
    public async Task<bool> OneDriverValidEditAsync(Guid id, Guid fkCar, bool isDriver, CancellationToken token = default) =>
       isDriver ? await _db.PassengerDs.AsNoTracking().Where(i => i.FkCar == fkCar && i.IsDriver == isDriver).AnyAsync(i => i.Id != id, token) : false;

    /// <inheritdoc/>
    public async Task<bool> AnyAsync(Guid idCar, CancellationToken token = default) => await _db.PassengerDs.AsNoTracking().Where(i => i.FkCar == idCar).AnyAsync(token);

    /// <inheritdoc/>
    public async Task<bool> AnySubsetPassengerAsync(Guid idCar, CancellationToken token = default) => await _db.PassengerDs.AsNoTracking().Where(i => i.FkCar == idCar).AnyAsync(token);

    public async Task<Guid> GetParentAsync(Guid idpassenger, CancellationToken token)
    {
        var find = await _db.PassengerDs.AsNoTracking().FirstOrDefaultAsync(i => i.Id == idpassenger);
        return find?.FkCar ?? Guid.Empty;
    }

    private static string AccidentCode(Guid id) => $"ACC-{id.ToString("N")[..8].ToUpperInvariant()}";
}