namespace RMS.Api.Authentication;

internal sealed record JwtTokenResult(string AccessToken, DateTimeOffset ExpiresAtUtc);
