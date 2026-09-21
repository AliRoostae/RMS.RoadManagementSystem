using System.Net;
using System.Net.Http.Json;

namespace RMS.Client.Services;

public abstract class AuthorizedApiService(AuthenticationService authenticationService)
{
    protected async Task<T> GetAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken)
            ?? throw new ApiClientException("پاسخ دریافتی از سرویس معتبر نیست.", HttpStatusCode.BadGateway);
    }

    protected async Task SendAsync(
        HttpMethod method,
        string uri,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, uri);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: ApiJson.Options);

        using var response = await authenticationService.SendAuthorizedAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    protected static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            throw await ApiClientException.FromResponseAsync(response, cancellationToken);
    }
}
