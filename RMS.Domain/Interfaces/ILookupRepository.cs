using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;

public interface ILookupRepository
{
    Task<IReadOnlyList<RoadLookupResponse>> GetRoadsAsync(
        LookupQuery query,
        CancellationToken token = default);

    Task<IReadOnlyList<AccidentLookupResponse>> GetAccidentsAsync(
        AccidentLookupQuery query,
        CancellationToken token = default);

    Task<IReadOnlyList<CarLookupResponse>> GetCarsAsync(
        CarLookupQuery query,
        CancellationToken token = default);
}