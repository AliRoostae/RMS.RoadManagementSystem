using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IAnalyticsService service) : ControllerBase
{
    [HttpGet("overview")]
    [HasPermission(UserSection.Reports, UserAccessOperation.Report)]
    public async Task<ActionResult<DashboardOverviewResponse>> GetOverview(
        [FromQuery] DashboardQuery query,
        CancellationToken token) =>
        Ok(await service.GetDashboardAsync(query, token));
}