using RMS.Shared.Contracts.Authentication;

namespace RMS.Api.Authentication;

public interface IUserLoginService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken token = default);
}
