using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IAnalyticsService service) : ControllerBase
{
    [HttpGet("accidents")]
    [HasPermission(UserSection.Reports, UserAccessOperation.Report)]
    public async Task<ActionResult<AccidentReportResponse>> GetAccidents(
        [FromQuery] AccidentReportQuery query,
        CancellationToken token) =>
        Ok(await service.GetAccidentReportAsync(query, token));

    [HttpGet("humans")]
    [HasPermission(UserSection.Reports, UserAccessOperation.Report)]
    public async Task<ActionResult<PagedResponse<HumanItemResponse>>> GetAllHuman(
       [FromQuery] HumanQueries query,
       CancellationToken token) =>
       Ok(await service.GetAllHumanAsync(query, token));




}