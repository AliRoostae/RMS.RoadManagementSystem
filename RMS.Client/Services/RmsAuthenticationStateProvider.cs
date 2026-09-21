using Microsoft.AspNetCore.Components.Authorization;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;
using System.Security.Claims;

namespace RMS.Client.Services;

/// <summary>وضعیت احراز هویت Blazor را از روی نشست مرورگر تأمین می‌کند.</summary>
public sealed class RmsAuthenticationStateProvider(AuthSessionStore sessionStore)
    : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool initialized;
    private ClientAuthSession? session;

    /// <summary>کاربر فعلی.</summary>
    public UserResponse? CurrentUser => session?.User;
    /// <summary>توکن دسترسی فعلی.</summary>
    public string? AccessToken => session?.AccessToken;
    /// <summary>مشخص می‌کند نشست فعلی معتبر است یا نه.</summary>
    public bool IsAuthenticated => session is not null && session.ExpiresAtUtc > DateTimeOffset.UtcNow;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await EnsureInitializedAsync();
        return CreateState(session);
    }

    public async Task<ClientAuthSession?> GetSessionAsync()
    {
        await EnsureInitializedAsync();
        return session;
    }

    public async Task SetSessionAsync(ClientAuthSession value)
    {
        session = value;
        initialized = true;
        await sessionStore.SetAsync(value);
        NotifyAuthenticationStateChanged(Task.FromResult(CreateState(value)));
    }

    public async Task UpdateUserAsync(UserResponse user)
    {
        await EnsureInitializedAsync();
        if (session is null)
            return;

        session = session with { User = user };
        await sessionStore.SetAsync(session);
        NotifyAuthenticationStateChanged(Task.FromResult(CreateState(session)));
    }

    public async Task ClearAsync()
    {
        session = null;
        initialized = true;
        await sessionStore.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    /// <summary>بررسی می‌کند کاربر فعلی مجوز عملیات را دارد یا نه.</summary>
    public bool HasPermission(UserSection section, UserAccessOperation operation)
    {
        var user = CurrentUser;
        if (user is null)
            return false;
        if (user.Role == UserRole.Administrator)
            return true;

        var operations = user.Permissions
            .Where(permission => permission.Section == section)
            .Aggregate(UserAccessOperation.None, (current, permission) => current | permission.Operations);
        return (operations & operation) == operation;
    }

    private async Task EnsureInitializedAsync()
    {
        if (initialized)
            return;

        await initializationLock.WaitAsync();
        try
        {
            if (initialized)
                return;

            session = await sessionStore.GetAsync();
            if (session is not null && session.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                session = null;
                await sessionStore.ClearAsync();
            }

            initialized = true;
        }
        finally
        {
            initializationLock.Release();
        }
    }

    private static AuthenticationState CreateState(ClientAuthSession? value)
    {
        if (value is null || value.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            return AnonymousState;

        var user = value.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.MobilePhone, user.MobileNumber),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        claims.AddRange(user.Permissions.Select(permission =>
            new Claim("rms_permission", $"{(byte)permission.Section}:{(byte)permission.Operations}")));

        return new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity(claims, "RMS.Jwt", ClaimTypes.Name, ClaimTypes.Role)));
    }
}
