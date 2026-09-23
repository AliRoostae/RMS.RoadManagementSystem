using System.ComponentModel.DataAnnotations;
using Mapster;
using NetTopologySuite.Geometries;
using RMS.Application.Excep;
using RMS.Application.Interface;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Service;

/// <inheritdoc cref="IRoad"/>
public sealed class RoadService(
    IRoadRepository _repRoad,
    IAccidentRepository _repAcc
    ) : IRoad
{
    /// <inheritdoc/>
    public async Task<Guid> AddAsync(CreateRoadCommand argo, CancellationToken token = default)
    {
        ValidateAndNormalizeRoute(argo);
        if (await _repRoad.DuplicateNameAsync(argo.Name, token))
            throw new InsufficientExecutionStackException("نام ارسالی تکراری است ");


        var inser = argo.Adapt<RoadsEntities>();
        inser.Id = Guid.NewGuid();
        if (await _repRoad.AddAsync(inser, token))
            return inser.Id;
        else
            return Guid.Empty;

    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(DeleteRoadCommand command, CancellationToken token = default)
    {
        if (command.Id == Guid.Empty)
            throw new ValidationException("شناسهٔ راه الزامی است.");
        if (await _repAcc.AnySubsetRoadAsync(command.Id, token))
            throw new InvalidOperationException("برای مسیر تصادف ثبت شده قابل حذف نیست");
        return await _repRoad.DeleteAsync(command.Id, token);

    }

    /// <inheritdoc/>
    public Task<PagedResponse<RoadListItemResponse>> GetAllAsync(RoadQueries argo, CancellationToken token = default)
    {
        if (!argo.TryValidate(out var validationErrors))
            throw new ValidationException(validationErrors.JsonErrors());
        return _repRoad.GetAllAsync(argo, token);
    }

    /// <inheritdoc cref="IRoad.GetMapAsync"/>
    public Task<IReadOnlyList<RoadMapItemResponse>> GetMapAsync(
        RoadMapQuery query,
        CancellationToken token = default)
    {
        if (!query.TryValidate(out var validationErrors))
            throw new ValidationException(validationErrors.JsonErrors());
        var bounds = new[] { query.MinLatitude, query.MaxLatitude, query.MinLongitude, query.MaxLongitude };
        if (bounds.Any(value => value.HasValue) && bounds.Any(value => !value.HasValue))
            throw new ValidationException("برای فیلتر نقشه باید هر چهار مقدار محدوده ارسال شوند.");
        if (query.MinLatitude >= query.MaxLatitude || query.MinLongitude >= query.MaxLongitude)
            throw new ValidationException("محدوده نقشه نامعتبر است.");
        return _repRoad.GetMapAsync(query, token);
    }

    /// <inheritdoc/>
    public async Task<RoadResponse?> GetAsync(Guid argo, CancellationToken token = default) =>
        await _repRoad.GetAsync(argo, token);

    /// <inheritdoc cref="IRoad.GetGeometryAsync"/>
    public Task<RoadGeometryResponse?> GetGeometryAsync(Guid id, CancellationToken token = default)
    {
        if (id == Guid.Empty)
            throw new ValidationException("شناسهٔ راه الزامی است.");
        return _repRoad.GetGeometryAsync(id, token);
    }

    /// <inheritdoc cref="IRoad.GetNearestPointAsync"/>
    public Task<RoadLocationResponse?> GetNearestPointAsync(
        Guid id,
        RoadLocationQuery query,
        CancellationToken token = default)
    {
        if (id == Guid.Empty)
            throw new ValidationException("شناسهٔ راه الزامی است.");
        if (!query.TryValidate(out var validationErrors))
            throw new ValidationException(validationErrors.JsonErrors());
        return _repRoad.GetNearestPointAsync(id, query, token);
    }


    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdateRoadCommand argo, CancellationToken token = default)
    {
        ValidateAndNormalizeRoute(argo);
        if (argo.Id == Guid.Empty)
            throw new ValidationException("شناسهٔ راه الزامی است.");
        if (!await _repRoad.AnyAsync(argo.Id, token))
            throw new KeyNotFoundException(" کد ارسالی یافت نشد");
        if (await _repRoad.DuplicateEditNameAsync(argo.Name, argo.Id, token))
            throw new InsufficientExecutionStackException("نام ارسالی تکراری است ");


        return await _repRoad.UpdateAsync(argo, argo.Id, token);

    }

    private static void ValidateAndNormalizeRoute(BaseRoads road)
    {
        // Both the persisted representative point and route type are server-derived.
        road.Centroid = null;
        if (!road.TryValidate(out var validationErrors))
            throw new ValidationException(validationErrors.JsonErrors());

        if (road.Boundary is LineString lineString)
            road.Boundary = lineString.Factory.CreateMultiLineString([lineString]);

        var interiorPoint = road.Boundary.InteriorPoint;
        interiorPoint.SRID = road.Boundary.SRID;
        road.Centroid = interiorPoint;
    }
}