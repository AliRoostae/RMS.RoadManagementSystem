using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using RMS.Api.Authentication;
using RMS.Api.Authorization;
using System.Text;

namespace RMS.Api.Api.Configuration;

public static class SecurityConfiguration
{
    public static void ConfigureApiSecurity(this WebApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException("تنظیمات Jwt یافت نشد.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
            string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
            jwtOptions.Key.Length < 32 ||
            jwtOptions.ExpirationMinutes is < 5 or > 1440)
        {
            throw new InvalidOperationException("تنظیمات Jwt نامعتبر است.");
        }

        builder.Services.Configure<JwtOptions>(jwtSection);
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, UserPermissionAuthorizationHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, AnyUserPermissionAuthorizationHandler>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IUserLoginService, UserLoginService>();
    }
}