using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/lookups")]
public sealed class LookupsController(ILookupService service) : ControllerBase
{
    [HttpGet("roads")]
    [HasAnyPermission(
        UserSection.Roads, UserAccessOperation.Report,
        UserSection.Accidents, UserAccessOperation.Create,
        UserSection.Accidents, UserAccessOperation.Update)]
    public async Task<ActionResult<IReadOnlyList<RoadLookupResponse>>> GetRoads(
        [FromQuery] LookupQuery query,
        CancellationToken token) =>
        Ok(await service.GetRoadsAsync(query, token));

    [HttpGet("accidents")]
    [HasAnyPermission(
        UserSection.Accidents, UserAccessOperation.Report,
        UserSection.Cars, UserAccessOperation.Create,
        UserSection.People, UserAccessOperation.Create,
        UserSection.Images, UserAccessOperation.Create)]
    public async Task<ActionResult<IReadOnlyList<AccidentLookupResponse>>> GetAccidents(
        [FromQuery] AccidentLookupQuery query,
        CancellationToken token) =>
        Ok(await service.GetAccidentsAsync(query, token));

    [HttpGet("cars")]
    [HasAnyPermission(
        UserSection.Cars, UserAccessOperation.Report,
        UserSection.Passengers, UserAccessOperation.Create)]
    public async Task<ActionResult<IReadOnlyList<CarLookupResponse>>> GetCars(
        [FromQuery] CarLookupQuery query,
        CancellationToken token) =>
        Ok(await service.GetCarsAsync(query, token));
}
