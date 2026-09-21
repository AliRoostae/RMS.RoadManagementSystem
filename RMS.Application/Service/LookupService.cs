using RMS.Application.Interface;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Application.Service;

public sealed class LookupService(ILookupRepository repository) : ILookupService
{
    public Task<IReadOnlyList<RoadLookupResponse>> GetRoadsAsync(
        LookupQuery query,
        CancellationToken token = default)
    {
        Normalize(query);
        return repository.GetRoadsAsync(query, token);
    }

    public Task<IReadOnlyList<AccidentLookupResponse>> GetAccidentsAsync(
        AccidentLookupQuery query,
        CancellationToken token = default)
    {
        Normalize(query);
        return repository.GetAccidentsAsync(query, token);
    }

    public Task<IReadOnlyList<CarLookupResponse>> GetCarsAsync(
        CarLookupQuery query,
        CancellationToken token = default)
    {
        Normalize(query);
        return repository.GetCarsAsync(query, token);
    }

    private static void Normalize(LookupQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        query.SearchTerm = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? null
            : query.SearchTerm.Trim();
    }
}
