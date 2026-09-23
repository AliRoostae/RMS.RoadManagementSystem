using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/access")]
public sealed class AccessController : ControllerBase
{
    [HttpGet("metadata")]
    [HasPermission(UserSection.Users, UserAccessOperation.View)]
    [ProducesResponseType<AccessMetadataResponse>(StatusCodes.Status200OK)]
    public ActionResult<AccessMetadataResponse> GetMetadata() => Ok(new AccessMetadataResponse(
        Enum.GetValues<UserRole>()
            .Select(value => new AccessMetadataItem((byte)value, value.ToString()))
            .ToArray(),
        Enum.GetValues<UserSection>()
            .Select(value => new AccessMetadataItem((byte)value, value.ToString()))
            .ToArray(),
        Enum.GetValues<UserAccessOperation>()
            .Where(value => value != UserAccessOperation.None)
            .Select(value => new AccessMetadataItem((byte)value, value.ToString()))
            .ToArray()));
}