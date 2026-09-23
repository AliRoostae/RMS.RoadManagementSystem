using Mapster;
using RMS.Application.Excep;
using RMS.Application.Interface;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using System.ComponentModel.DataAnnotations;


namespace RMS.Application.Service;


/// <inheritdoc cref="IAccident"/>
public sealed class AccidentService(
    IAccidentRepository _repAcc,
    IRoadRepository _repRoad,
    ICarRepository _repCar,
    IPeopleRepository _repPeople,
    IImageRepository _repImage
    ) : IAccident
{



    /// <inheritdoc/>
    public async Task<Guid> AddAsync(CreateAccidentCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (!await _repRoad.IsValidCoordinate(argo.Latitude, argo.Longitude, argo.FkIdRoad, token)) throw new InvalidOperationException("لوکشین ارسالی برای این جاده نیست ");
        var add = argo.Adapt<AccidentEntities>();
        add.Id = Guid.NewGuid();
        if (await _repAcc.AddAsync(add, token))
            return add.Id;
        else return Guid.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(DeleteAccidentCommand argo, CancellationToken token = default)
    {
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ حادثه الزامی است.");
        if (await _repCar.AnyCarInAccidenAsync(argo.Id, token)) throw new InvalidOperationException("برای این رویداد خودرو ثبت شده و امکان حذف آن وجود ندارد.");
        if (await _repPeople.AnyPeopleInAccidenAsync(argo.Id, token)) throw new InvalidOperationException("برای این رویداد عابر ثبت شده و امکان حذف آن وجود ندارد.");
        await _repImage.DeleteAllAsync(argo.Id, token);
        return await _repAcc.DeleteAsync(argo.Id, token);
    }

    /// <inheritdoc/>
    public async Task<AccidentResponse?> GetAsync(Guid argo, CancellationToken token = default) => await _repAcc.GetAsync(argo, token);


    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdateAccidentCommand argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.Id == Guid.Empty) throw new ValidationException("شناسهٔ حادثه الزامی است.");
        if (!await _repRoad.IsValidCoordinate(argo.Latitude, argo.Longitude, argo.FkIdRoad, token)) throw new InvalidOperationException("لوکشین ارسالی برای این جاده نیست ");

        return await _repAcc.UpdateAsync(argo, argo.Id, token);
    }


    /// <inheritdoc/>
    public async Task<PagedResponse<AccidentListItemResponse>> GetAllAsync(AccidentQuery argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (argo.EndTime <= argo.StartTime) throw new ValidationException("پایان بازه باید بعد از شروع بازه باشد.");
        if (argo.AccidentType.HasValue && !Enum.IsDefined(argo.AccidentType.Value)) throw new ValidationException("نوع حادثه نامعتبر است.");
        if (argo.Weather.HasValue && !Enum.IsDefined(argo.Weather.Value)) throw new ValidationException("وضعیت آب‌وهوا نامعتبر است.");
        if (argo.Cause.HasValue && !Enum.IsDefined(argo.Cause.Value)) throw new ValidationException("علت حادثه نامعتبر است.");
        if (!Enum.IsDefined(argo.OrderBy)) throw new ValidationException("ترتیب نمایش حوادث نامعتبر است.");
        return await _repAcc.GetAllAsync(argo, token);
    }

    public Task<IReadOnlyList<AccidentMapPointResponse>> GetMapAsync(
        AccidentMapQuery query,
        CancellationToken token = default)
    {
        if (!query.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (query.EndTime <= query.StartTime) throw new ValidationException("پایان بازه باید بعد از شروع بازه باشد.");
        var bounds = new[] { query.MinLatitude, query.MaxLatitude, query.MinLongitude, query.MaxLongitude };
        if (bounds.Any(value => value.HasValue) && bounds.Any(value => !value.HasValue))
            throw new ValidationException("برای فیلتر نقشه باید هر چهار مقدار محدوده ارسال شوند.");
        if (query.MinLatitude >= query.MaxLatitude || query.MinLongitude >= query.MaxLongitude)
            throw new ValidationException("محدوده نقشه نامعتبر است.");
        return _repAcc.GetMapAsync(query, token);
    }



}