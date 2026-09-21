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

/// <inheritdoc cref="IPassenger"/>
public sealed class PassengerService(
    ICarRepository _repCar,
    IPassengerRepository _repPass,
    INationalCode _repNativeCode


    ) : IPassenger
{
    /// <inheritdoc/>
    public async Task<Guid> AddAsync(CreatePassengerCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        argo.InjuryPercentage = argo.TypePersonDamage == DamageTypePersonEnum.Fatal ? (byte)100 : argo.InjuryPercentage;
        argo.NationalCode = argo.NationalCode.NormalizeUnicode();
        var accidentId = await _repCar.GetParentAsync(argo.FkCar, token);
        if (accidentId == Guid.Empty) throw new KeyNotFoundException("خودروی ارسالی یافت نشد");
        if (await _repNativeCode.DuplicateNationalCodeAsync(argo.NationalCode, accidentId, token)) throw new InvalidOperationException("کد ملی یک بار در این تصادف ثبت شده");
        if (await _repPass.OneDriverValidAsync(argo.FkCar, argo.IsDriver, token)) throw new InvalidOperationException("برای این خودرو قبلا  راننده ثبت شده است ");

        var insert = argo.Adapt<PassengerEntities>();
        insert.Id = Guid.NewGuid();
        if (await _repPass.AddAsync(insert, token))
            return insert.Id;
        else return Guid.Empty;

    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(DeletePassengerCommand argo, CancellationToken token = default)
    {
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ سرنشین الزامی است.");
        return await _repPass.DeleteAsync(argo.Id, token);
    }

    /// <inheritdoc/>
    public Task<PagedResponse<PassengerListItemResponse>> GetAllAsync(PassengerQueries argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.DamageType.HasValue && !Enum.IsDefined(argo.DamageType.Value)) throw new ValidationException("نوع آسیب نامعتبر است.");
        return _repPass.GetAllAsync(argo, token);
    }

    /// <inheritdoc/>
    public async Task<PassengerResponse?> GetAsync(Guid argo, CancellationToken token = default) => await _repPass.GetAsync(argo, token);

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdatePassengerCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ سرنشین الزامی است.");
        var idcar = await _repPass.GetParentAsync(argo.Id, token);
        if (idcar == Guid.Empty) throw new KeyNotFoundException("کد  ارسالی در سرنشین یافت نشد");
        if (await _repPass.OneDriverValidEditAsync(argo.Id, idcar, argo.IsDriver, token)) throw new InvalidOperationException("برای این خودرو قبلا  راننده ثبت شده است ");
        var idacc = await _repCar.GetParentAsync(idcar, token);
        if (idacc == Guid.Empty) throw new KeyNotFoundException("کد خودرو در حادثه یافت نشد");


        argo.InjuryPercentage = argo.TypePersonDamage == DamageTypePersonEnum.Fatal ? (byte)100 : argo.InjuryPercentage;
        argo.NationalCode = argo.NationalCode.NormalizeUnicode();
        if (await _repNativeCode.DuplicateNationalCodeEditAsync(argo.NationalCode, idacc, argo.Id, false, token)) throw new InvalidOperationException("کد ملی یک بار در این تصادف ثبت شده");

        var updat = argo.Adapt<BasePassenger>();
        updat.IsDriver = argo.IsDriver;
        updat.FkCar = idcar;
        return await _repPass.UpdateAsync(updat, argo.Id, token);
    }
}
