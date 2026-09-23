using Microsoft.EntityFrameworkCore;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;

namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="INationalCode"/>
public sealed class NationalCodeRepository
      (
      RmsDbContext _db


    )
    : INationalCode
{
    /// <inheritdoc/>
    public async Task<bool> DuplicateNationalCodeAsync(string nationalCode, Guid fkAccident, CancellationToken token = default) =>
        await _db.AccidentDs.AsNoTracking().Where(i => i.Id == fkAccident).Select(i => new
        {
            people = i.PeopleList.Any(p => p.NationalCode == nationalCode),
            passenger = i.CarList.Select(o => o.PassengerList.Any(p => p.NationalCode == nationalCode)).Any(p => p)
        }).AnyAsync(p => p.people || p.passenger, token);


    /// <inheritdoc/>
    public async Task<bool> DuplicateNationalCodeEditAsync(string nationalCode, Guid fkId, Guid id, bool peoplOrPass, CancellationToken token = default)
    {
        var qur = _db.AccidentDs.AsNoTracking().Where(i => i.Id == fkId);

        if (peoplOrPass)
            return await qur.Select(i => new
            {
                people = i.PeopleList.Any(p => p.Id != id && p.NationalCode == nationalCode),
                passenger = i.CarList.Select(o => o.PassengerList.Any(p => p.NationalCode == nationalCode)).Any(p => p)
            }).AnyAsync(p => p.people || p.passenger, token);
        else
            return await qur.Select(i => new
            {
                people = i.PeopleList.Any(p => p.NationalCode == nationalCode),
                passenger = i.CarList.Select(o => o.PassengerList.Any(p => p.Id != id && p.NationalCode == nationalCode)).Any(p => p)
            }).AnyAsync(p => p.people || p.passenger, token);
    }



}