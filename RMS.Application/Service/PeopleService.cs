using Mapster;
using RMS.Application.Controlr;
using RMS.Application.Excep;
using RMS.Application.Interface;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;


namespace RMS.Application.Service;

/// <inheritdoc cref="IPeople"/>
public sealed class PeopleService
    (
     IAccidentRepository _repAcc,
    IPeopleRepository _repPeople,
      INationalCode _repNativeCode

    ) : IPeople
{
    /// <inheritdoc/>
    public async Task<Guid> AddAsync(CreatePeopleCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (!await _repAcc.AnyAsync(argo.FkAccident, token)) throw new KeyNotFoundException("کد تصادف ارسالی در تصادف یافت نشد");

        argo.NationalCode = argo.NationalCode.NormalizeUnicode();
        if (await _repNativeCode.DuplicateNationalCodeAsync(argo.NationalCode, argo.FkAccident, token)) throw new InvalidOperationException("کد ملی یک بار در این تصادف ثبت شده");
        argo.InjuryPercentage = argo.TypePersonDamage == DamageTypePersonEnum.Fatal ? (byte)100 : argo.InjuryPercentage;
        var insert = argo.Adapt<PeopleEntities>();
        insert.Id = Guid.NewGuid();

        if (await _repPeople.AddAsync(insert, token))
            return insert.Id;
        else return Guid.Empty;

    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(DeletePeopleCommand argo, CancellationToken token = default)
    {
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ شخص الزامی است.");
        return await _repPeople.DeleteAsync(argo.Id, token);
    }

    /// <inheritdoc/>
    public Task<PagedResponse<PeopleListItemResponse>> GetAllAsync(PeopleQueries argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.DamageType.HasValue && !Enum.IsDefined(argo.DamageType.Value)) throw new ValidationException("نوع آسیب نامعتبر است.");
        return _repPeople.GetAllAsync(argo, token);
    }

    /// <inheritdoc/>
    public async Task<PeopleResponse?> GetAsync(Guid argo, CancellationToken token = default) => await _repPeople.GetAsync(argo, token);

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdatePeopleCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ شخص الزامی است.");
        var idAccident = await _repPeople.GetParentAsync(argo.Id, token);
        if (idAccident == Guid.Empty) throw new KeyNotFoundException("کد  ارسالی در عابر یافت نشد");
        argo.InjuryPercentage = argo.TypePersonDamage == DamageTypePersonEnum.Fatal ? (byte)100 : argo.InjuryPercentage;
        argo.NationalCode = argo.NationalCode.NormalizeUnicode();
        if (await _repNativeCode.DuplicateNationalCodeEditAsync(argo.NationalCode, idAccident, argo.Id, true, token)) throw new InvalidOperationException("کد ملی یک بار در این تصادف ثبت شده");

        var update = argo.Adapt<BasePeople>();
        update.FkAccident = idAccident;
        return await _repPeople.UpdateAsync(update, argo.Id, token);
    }
}