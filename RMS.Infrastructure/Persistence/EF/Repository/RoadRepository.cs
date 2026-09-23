using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Simplify;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Infrastructure.Persistence.EF.Repository;

/// <inheritdoc cref="IRoadRepository"/>
public sealed class RoadRepository
    (
     RmsDbContext _db
    ) : IRoadRepository
{
    private const double RoadCoordinateToleranceMeters = 100d;

    /// <inheritdoc/>
    public async Task<bool> AddAsync(RoadsEntities argo, CancellationToken token = default)
    {
        await _db.RoadsDs.AddAsync(argo, token);
        return await _db.SaveChangesAsync(token) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid argo, CancellationToken token = default)
    {
        var find = await _db.RoadsDs.FirstOrDefaultAsync(i => i.Id == argo, token);
        if (find == null) return false;
        _db.RoadsDs.Remove(find);
        return await _db.SaveChangesAsync(token) > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DuplicateEditNameAsync(string name, Guid id, CancellationToken token = default) =>
        await _db.RoadsDs.AsNoTracking().AnyAsync(i => i.Id != id && i.Name == name, token);

    /// <inheritdoc/>
    public async Task<bool> DuplicateNameAsync(string name, CancellationToken token = default) =>
        await _db.RoadsDs.AsNoTracking().AnyAsync(i => i.Name == name, token);

    /// <summary>پرس‌وجوی موجودیت راه را به نمای خواندنی تبدیل می‌کند.</summary>
    /// <param name="query">پرس‌وجوی مبدأ راه‌ها و محدوده‌ها.</param>
    /// <returns>پرس‌وجوی قابل‌ترجمه به نمای راه‌ها.</returns>
    private static IQueryable<RoadResponse> ProjectToResponse(
IQueryable<RoadsEntities> query)
    {
        return query
            .Select(i => new RoadResponse
            {
                Boundary = i.Boundary,
                Centroid = i.Centroid,
                Id = i.Id,
                Name = i.Name,



            });


    }
    /// <inheritdoc/>
    public async Task<PagedResponse<RoadListItemResponse>> GetAllAsync(RoadQueries argo, CancellationToken token = default)
    {
        var source = _db.RoadsDs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(argo.SearchTerm))
        {
            var term = argo.SearchTerm.Trim();
            source = source.Where(road => road.Name.Contains(term));
        }

        var totalCount = await source.CountAsync(token);
        var items = await source
            .OrderBy(road => road.Name)
            .ThenBy(road => road.Id)
            .Skip(argo.Skip)
            .Take(argo.Take)
            .Select(road => new RoadListItemResponse(
                road.Id,
                road.Name,
                road.AccountList.Count(),
                road.Centroid != null))
            .ToListAsync(token);

        return new PagedResponse<RoadListItemResponse>(items, totalCount, argo.Skip, argo.Take);
    }

    public async Task<IReadOnlyList<RoadMapItemResponse>> GetMapAsync(
        RoadMapQuery query,
        CancellationToken token = default)
    {
        var source = _db.RoadsDs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            source = source.Where(road => road.Name.Contains(term));
        }

        if (query.MinLatitude.HasValue)
        {
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var viewport = factory.CreatePolygon(
            [
                new Coordinate(query.MinLongitude!.Value, query.MinLatitude.Value),
                new Coordinate(query.MaxLongitude!.Value, query.MinLatitude.Value),
                new Coordinate(query.MaxLongitude.Value, query.MaxLatitude!.Value),
                new Coordinate(query.MinLongitude.Value, query.MaxLatitude.Value),
                new Coordinate(query.MinLongitude.Value, query.MinLatitude.Value)
            ]);
            source = source.Where(road => road.Boundary.Intersects(viewport));
        }

        var roads = await source
            .OrderBy(road => road.Name)
            .ThenBy(road => road.Id)
            .Take(query.Take)
            .Select(road => new
            {
                road.Id,
                road.Name,
                road.Boundary,
                AccidentCount = road.AccountList.Count()
            })
            .ToListAsync(token);

        var tolerance = query.Zoom switch
        {
            >= 15 => 0d,
            >= 12 => 0.00005d,
            >= 10 => 0.0002d,
            >= 8 => 0.0008d,
            _ => 0.002d
        };

        return roads.Select(road => new RoadMapItemResponse(
                road.Id,
                road.Name,
                tolerance == 0 ? road.Boundary : DouglasPeuckerSimplifier.Simplify(road.Boundary, tolerance),
                road.AccidentCount))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<RoadResponse?> GetAsync(Guid argo, CancellationToken token = default)
    {
        var sor = _db.RoadsDs.AsNoTracking()
                    .Where(i => i.Id == argo);

        return await ProjectToResponse(sor).FirstOrDefaultAsync(token);
    }

    public Task<RoadGeometryResponse?> GetGeometryAsync(Guid id, CancellationToken token = default) =>
        _db.RoadsDs
            .AsNoTracking()
            .Where(road => road.Id == id)
            .Select(road => new RoadGeometryResponse(road.Id, road.Boundary, road.Centroid))
            .FirstOrDefaultAsync(token);

    public async Task<RoadLocationResponse?> GetNearestPointAsync(
        Guid id,
        RoadLocationQuery query,
        CancellationToken token = default)
    {
        var boundary = await _db.RoadsDs
            .AsNoTracking()
            .Where(road => road.Id == id)
            .Select(road => road.Boundary)
            .FirstOrDefaultAsync(token);
        if (boundary is null)
            return null;

        var point = boundary.Factory.CreatePoint(new Coordinate(query.Longitude, query.Latitude));
        point.SRID = boundary.SRID;
        var nearest = DistanceOp.NearestPoints(boundary, point)[0];
        var distanceMeters = HaversineMeters(
            query.Latitude,
            query.Longitude,
            nearest.Y,
            nearest.X);

        return new RoadLocationResponse(
            distanceMeters <= query.MaxDistanceMeters,
            distanceMeters,
            nearest.Y,
            nearest.X);
    }

    /// <inheritdoc/>
    public async Task<bool> IsValidCoordinate(double latitude, double longitude, Guid idRoad, CancellationToken token = default)
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var point = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        return await _db.RoadsDs
            .AsNoTracking()
            .Where(i => i.Id == idRoad)
            .AnyAsync(p => p.Boundary.Distance(point) <= RoadCoordinateToleranceMeters, token);

    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(BaseRoads argo, Guid id, CancellationToken token = default)
    {
        var find = await _db.RoadsDs.FirstOrDefaultAsync(i => i.Id == id, token);
        if (find == null) return false;
        _db.Entry(find).CurrentValues.SetValues(argo);
        return await _db.SaveChangesAsync(token) >= 0;
    }

    public async Task<bool> AnyAsync(Guid id, CancellationToken token = default) => await _db.RoadsDs.AsNoTracking().AnyAsync(i => i.Id == id, token);

    private static double HaversineMeters(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusMeters = 6_371_008.8;
        static double ToRadians(double value) => value * Math.PI / 180d;

        var latitudeDelta = ToRadians(latitude2 - latitude1);
        var longitudeDelta = ToRadians(longitude2 - longitude1);
        var startLatitude = ToRadians(latitude1);
        var endLatitude = ToRadians(latitude2);
        var a = Math.Pow(Math.Sin(latitudeDelta / 2d), 2d) +
                Math.Cos(startLatitude) * Math.Cos(endLatitude) *
                Math.Pow(Math.Sin(longitudeDelta / 2d), 2d);
        return earthRadiusMeters * 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
    }

}