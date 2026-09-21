using RMS.Domain.Entities;

namespace RMS.Api.Authentication;

internal interface IJwtTokenService
{
    JwtTokenResult Create(UserEntities user);
}
