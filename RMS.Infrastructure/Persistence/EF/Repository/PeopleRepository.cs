using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="IPeopleRepository"/>
public sealed class PeopleRepository
    (
    RmsDbContext _db
    )
    : IPeopleRepository
{
    /// <inheritdoc/>
    public async Task<bool> AddAsync(PeopleEntities argo, CancellationToken token = default)
    {
        await _db.PeopleDs.AddAsync(argo, token);
        return await _db.SaveChangesAsync(token) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> AnyPeopleInAccidenAsync(Guid idAcciden, CancellationToken token = default) => await _db.PeopleDs.AsNoTracking().AnyAsync(i => i.FkAccident == idAcciden, token);

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid argo, CancellationToken token = default)
    {
        var find = await _db.PeopleDs.FirstOrDefaultAsync(i => i.Id == argo, token);
        if (find == null)
            return false;
        _db.PeopleDs.Remove(find);
        return await _db.SaveChangesAsync(token) > 0;
    }




    /// <summary>تکراری‌بودن کد ملی را هنگام ویرایش، با صرف‌نظر از رکورد جاری، میان عابر بررسی می‌کند.</summary>
    /// <param name="nationalCode">کد ملی شخص.</param>
    /// <param name="fkAccident">شناسهٔ یکتای حادثه.</param>
    /// <param name="id">شناسهٔ شخص در حال ویرایش.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    /// <returns><see langword="true"/> در صورت وجود کد ملی در رکوردی دیگر؛ در غیر این صورت <see langword="false"/>.</returns>
    public async Task<bool> DuplicateNationalCodeAsync(string nationalCode, Guid fkAccident, Guid id, CancellationToken token = default) => await _db.PeopleDs.AsNoTracking().AnyAsync(i => i.FkAccident == fkAccident && i.Id != id && i.NationalCode == nationalCode);

    /// <summary>پرس‌وجوی موجودیت شخص خارج از خودرو را به نمای خواندنی تبدیل می‌کند.</summary>
    /// <param name="query">پرس‌وجوی مبدأ اشخاص.</param>
    /// <returns>پرس‌وجوی قابل‌ترجمه به نمای اشخاص.</returns>
    private static IQueryable<PeopleResponse> ProjectToResponse(
IQueryable<PeopleEntities> query)
    {
        return query
            .Select(i => new PeopleResponse
            {
                Age = i.Age,
                DescriptionDamage = i.DescriptionDamage,
                FkAccident = i.FkAccident,
                Gender = i.Gender,
                Id = i.Id,
                InjuryPercentage = i.InjuryPercentage,
                PassengerFullName = i.PassengerFullName,
                NationalCode = i.NationalCode,
                TypePersonDamage = i.TypePersonDamage

            });


    }


    /// <inheritdoc/>
    public async Task<PagedResponse<PeopleListItemResponse>> GetAllAsync(PeopleQueries argo, CancellationToken token = default)
    {
        var source = _db.PeopleDs.AsNoTracking();
        if (argo.AccidentId.HasValue)
            source = source.Where(person => person.FkAccident == argo.AccidentId.Value);
        if (!string.IsNullOrWhiteSpace(argo.NationalCode))
            source = source.Where(person => person.NationalCode.Contains(argo.NationalCode.Trim()));
        if (argo.DamageType.HasValue)
            source = source.Where(person => person.TypePersonDamage == argo.DamageType.Value);
        if (!string.IsNullOrWhiteSpace(argo.SearchTerm))
        {
            var term = argo.SearchTerm.Trim();
            if (Guid.TryParse(term, out var accidentId))
                source = source.Where(person =>
                    person.FkAccident == accidentId ||
                    person.NationalCode.Contains(term) ||
                    person.PassengerFullName.Contains(term));
            else
                source = source.Where(person =>
                    person.NationalCode.Contains(term) ||
                    person.PassengerFullName.Contains(term));
        }

        var totalCount = await source.CountAsync(token);
        var rawItems = await source
            .OrderBy(person => person.NationalCode)
            .ThenBy(person => person.Id)
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
        var items = rawItems.Select(person => new PeopleListItemResponse(
                person.Id,
                person.AccidentId,
                AccidentCode(person.AccidentId),
                person.NationalCode,
                person.FullName,
                person.Gender,
                person.Age,
                person.InjuryPercentage,
                person.DamageType))
            .ToList();

        return new PagedResponse<PeopleListItemResponse>(items, totalCount, argo.Skip, argo.Take);
    }

    /// <inheritdoc/>
    public async Task<PeopleResponse?> GetAsync(Guid argo, CancellationToken token = default)
    {
        var sor = _db.PeopleDs.AsNoTracking()
                .Where(i => i.Id == argo);



        return await ProjectToResponse(sor).FirstOrDefaultAsync(token);
    }



    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(BasePeople argo, Guid id, CancellationToken token = default)
    {
        var find = await _db.PeopleDs.FirstOrDefaultAsync(i => i.Id == id, token);
        if (find == null)
            return false;
        _db.Entry(find).CurrentValues.SetValues(argo);
        return await _db.SaveChangesAsync(token) >= 0;
    }

    public async Task<Guid> GetParentAsync(Guid idPeople, CancellationToken token)
    {
        var find = await _db.PeopleDs.FirstOrDefaultAsync(i => i.Id == idPeople, token);
        return find?.FkAccident ?? Guid.Empty;
    }

    private static string AccidentCode(Guid id) => $"ACC-{id.ToString("N")[..8].ToUpperInvariant()}";
}