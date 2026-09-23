using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace RMS.Api.Authorization;

internal sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (AnyPermissionPolicy.TryParse(policyName, out var permissions))
        {
            var anyPermissionPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AnyUserPermissionRequirement(permissions))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(anyPermissionPolicy);
        }

        if (!PermissionPolicy.TryParse(policyName, out var section, out var operation))
            return base.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new UserPermissionRequirement(section, operation))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}