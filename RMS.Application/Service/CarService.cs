
using Mapster;
using RMS.Application.Controlr;
using RMS.Application.Excep;
using RMS.Application.Interface;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using System.ComponentModel.DataAnnotations;


namespace RMS.Application.Service;

/// <inheritdoc cref="ICar"/>
internal sealed class CarService(
    IAccidentRepository _repAcc,
    ICarRepository _repCar,
    IPassengerRepository _repPass
    ) : ICar
{
    /// <inheritdoc/>
    public async Task<Guid> AddAsync(CreateCarCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        argo.PlateNumber = argo.PlateNumber.NormalizeUnicode();
        argo.DriverLicNumber = argo.DriverLicNumber.NormalizeUnicode();
        argo.DriverPhone = argo.DriverPhone.NormalizeUnicode();
        if (!await _repAcc.AnyAsync(argo.FkAccident, token)) throw new KeyNotFoundException(" کد ارسالی تصادفات موجود نیست ");
        if (await _repCar.DuplicatePlateAsync(argo.PlateNumber, argo.FkAccident, token)) throw new InvalidOperationException(" شماره پلاک تکراری است ");

        var insert = argo.Adapt<CarEntities>();
        insert.Id = Guid.NewGuid();

        if (await _repCar.AddAsync(insert, token)) return insert.Id;
        else return Guid.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(DeleteCarCommand command, CancellationToken token = default)
    {
        if (command.Id == Guid.Empty) throw new ValidationException("شناسهٔ خودرو الزامی است.");
        if (await _repPass.AnySubsetPassengerAsync(command.Id, token)) throw new InvalidOperationException("برای خودرو سرنشین ثبت شده و امکان حذف نیست");
        return await _repCar.DeleteAsync(command.Id, token);
    }

    /// <inheritdoc/>
    public Task<PagedResponse<CarListItemResponse>> GetAllAsync(CarQueries argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.CarClass.HasValue && !Enum.IsDefined(argo.CarClass.Value)) throw new ValidationException("رده خودرو نامعتبر است.");
        return _repCar.GetAllAsync(argo, token);
    }

    /// <inheritdoc/>
    public Task<CarResponse?> GetAsync(Guid argo, CancellationToken token = default) => _repCar.GetAsync(argo, token);

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdateCarCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ خودرو الزامی است.");
        var find = await _repCar.GetAsync(argo.Id, token);
        if (find == null) throw new KeyNotFoundException(" کد ارسالی خودرو  موجود نیست ");
        argo.PlateNumber = argo.PlateNumber.NormalizeUnicode();
        argo.DriverLicNumber = argo.DriverLicNumber.NormalizeUnicode();
        argo.DriverPhone = argo.DriverPhone.NormalizeUnicode();
        if (await _repCar.DuplicatePlateAsync(argo.PlateNumber, find.FkAccident, argo.Id, token)) throw new InvalidOperationException(" شماره پلاک تکراری است ");


        return await _repCar.UpdateAsync(argo, argo.Id, token);

    }
}
