using Microsoft.JSInterop;
using RMS.Shared.Contracts.Responses;

namespace RMS.Client.Services;

/// <summary>اطلاعات نشست احراز هویت ذخیره‌شده در مرورگر.</summary>
public sealed record ClientAuthSession(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    UserResponse User,
    bool RememberMe);

/// <summary>نشست احراز هویت را در storage مرورگر نگه می‌دارد.</summary>
public sealed class AuthSessionStore(IJSRuntime jsRuntime)
{
    /// <summary>نشست ذخیره‌شده را می‌خواند.</summary>
    public ValueTask<ClientAuthSession?> GetAsync() =>
        jsRuntime.InvokeAsync<ClientAuthSession?>("rmsAuthStorage.get");

    /// <summary>نشست را ذخیره می‌کند.</summary>
    public ValueTask SetAsync(ClientAuthSession session) =>
        jsRuntime.InvokeVoidAsync("rmsAuthStorage.set", session, session.RememberMe);

    /// <summary>نشست ذخیره‌شده را پاک می‌کند.</summary>
    public ValueTask ClearAsync() =>
        jsRuntime.InvokeVoidAsync("rmsAuthStorage.clear");
}