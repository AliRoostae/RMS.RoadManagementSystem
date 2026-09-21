using RMS.Shared.Contracts.Authentication;
using RMS.Shared.Contracts.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RMS.Client.Services;

/// <summary>ورود، خروج، refresh کاربر و ارسال درخواست‌های احراز‌شده را مدیریت می‌کند.</summary>
public sealed class AuthenticationService(
    HttpClient httpClient,
    RmsAuthenticationStateProvider authenticationStateProvider)
{
    /// <summary>کاربر فعلی یا null اگر کسی وارد نشده باشد.</summary>
    public UserResponse? CurrentUser => authenticationStateProvider.CurrentUser;

    /// <summary>کاربر را وارد سامانه می‌کند.</summary>
    public async Task<UserResponse> LoginAsync(
        LoginRequest request,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await ApiClientException.FromResponseAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new ApiClientException("پاسخ ورود از سرویس معتبر نیست.", HttpStatusCode.BadGateway);

        await authenticationStateProvider.SetSessionAsync(new ClientAuthSession(
            result.AccessToken,
            result.ExpiresAtUtc,
            result.User,
            rememberMe));

        return result.User;
    }

    /// <summary>اطلاعات کاربر فعلی را از API تازه می‌کند.</summary>
    public async Task<UserResponse?> RefreshCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (await authenticationStateProvider.GetSessionAsync() is null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        using var response = await SendAuthorizedAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw await ApiClientException.FromResponseAsync(response, cancellationToken);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken)
            ?? throw new ApiClientException("اطلاعات کاربر جاری از سرویس دریافت نشد.", HttpStatusCode.BadGateway);
        await authenticationStateProvider.UpdateUserAsync(user);
        return user;
    }

    /// <summary>درخواست HTTP را با توکن فعلی ارسال می‌کند.</summary>
    public async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var session = await authenticationStateProvider.GetSessionAsync();
        if (session is null || session.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            await authenticationStateProvider.ClearAsync();
            throw new ApiClientException("نشست کاربری منقضی شده است؛ دوباره وارد شوید.", HttpStatusCode.Unauthorized);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            await authenticationStateProvider.ClearAsync();

        return response;
    }

    /// <summary>نشست فعلی را پاک و کاربر را خارج می‌کند.</summary>
    public Task LogoutAsync() => authenticationStateProvider.ClearAsync();
}
