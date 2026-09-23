using System.Net;
using System.Net.Http.Json;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Client.Services;

public sealed class UserApiService(AuthenticationService authenticationService)
{
    public async Task<PagedResponse<UserListItemResponse>> GetAllAsync(
        UserQuery query,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>
        {
            $"skip={query.Skip}",
            $"take={query.Take}"
        };
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            parameters.Add($"searchTerm={Uri.EscapeDataString(query.SearchTerm.Trim())}");
        if (query.Role.HasValue)
            parameters.Add($"role={(byte)query.Role.Value}");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/users?{string.Join('&', parameters)}");
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PagedResponse<UserListItemResponse>>(cancellationToken)
            ?? new PagedResponse<UserListItemResponse>([], 0, query.Skip, query.Take);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{id}");
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken)
            ?? throw new ApiClientException("اطلاعات کاربر از سرویس دریافت نشد.", HttpStatusCode.BadGateway);
    }

    public async Task<AccessMetadataResponse> GetAccessMetadataAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/access/metadata");
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AccessMetadataResponse>(cancellationToken)
            ?? throw new ApiClientException("اطلاعات نقش‌ها و دسترسی‌ها دریافت نشد.", HttpStatusCode.BadGateway);
    }

    public async Task CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/users")
        {
            Content = JsonContent.Create(command)
        };
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/users/{id}")
        {
            Content = JsonContent.Create(command)
        };
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/users/{id}");
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            throw await ApiClientException.FromResponseAsync(response, cancellationToken);
    }
}