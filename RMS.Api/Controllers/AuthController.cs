using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RMS.Api.Authentication;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Authentication;
using System.Security.Claims;

namespace RMS.Api.Controllers;

/// <summary>endpointهای ورود و دریافت اطلاعات کاربر جاری.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IUserLoginService loginService,
    IUser userService) : ControllerBase
{
    /// <summary>کاربر را احراز هویت و توکن JWT صادر می‌کند.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken token)
    {
        var result = await loginService.LoginAsync(request, token);
        if (result is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "اطلاعات ورود نامعتبر است.",
                Detail = "شماره موبایل یا رمز عبور صحیح نیست."
            });
        }

        return Ok(result);
    }

    /// <summary>اطلاعات کاربری که با توکن فعلی وارد شده را برمی‌گرداند.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken token)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var user = await userService.GetAsync(userId, token);
        return user is null ? NotFound() : Ok(user);
    }
}