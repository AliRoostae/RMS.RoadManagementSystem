using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Authentication;
using RMS.Shared.Contracts.Responses;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace RMS.Api.Authentication;

internal sealed partial class UserLoginService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IUserLoginService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken token = default)
    {
        var mobileNumber = NormalizeMobile(request.MobileNumber);
        var user = await userRepository.GetForAuthenticationAsync(mobileNumber, token);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var jwt = jwtTokenService.Create(user);
        return new LoginResponse(jwt.AccessToken, jwt.ExpiresAtUtc, new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            Role = user.Role,
            Permissions = user.Permissions
                .OrderBy(permission => permission.Section)
                .Select(permission => new UserPermissionResponse
                {
                    Section = permission.Section,
                    Operations = permission.Operations
                })
                .ToArray()
        });
    }

    private static string NormalizeMobile(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("شماره موبایل الزامی است.");

        var normalized = value.Normalize(NormalizationForm.FormKC);
        var result = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (character == '+' && result.Length == 0)
            {
                result.Append(character);
                continue;
            }

            var digit = character switch
            {
                >= '0' and <= '9' => character,
                >= '\u06F0' and <= '\u06F9' => (char)('0' + character - '\u06F0'),
                >= '\u0660' and <= '\u0669' => (char)('0' + character - '\u0660'),
                _ => '\0'
            };

            if (digit != '\0')
            {
                result.Append(digit);
                continue;
            }

            if (!char.IsWhiteSpace(character) && character is not ('-' or '(' or ')'))
                throw new ValidationException("شماره موبایل نامعتبر است.");
        }

        var mobileNumber = result.ToString();
        if (mobileNumber.StartsWith("+98", StringComparison.Ordinal))
            mobileNumber = $"0{mobileNumber[3..]}";
        else if (mobileNumber.StartsWith("0098", StringComparison.Ordinal))
            mobileNumber = $"0{mobileNumber[4..]}";
        else if (mobileNumber.StartsWith("98", StringComparison.Ordinal) && mobileNumber.Length == 12)
            mobileNumber = $"0{mobileNumber[2..]}";

        if (!IranMobileRegex().IsMatch(mobileNumber))
            throw new ValidationException("شماره موبایل نامعتبر است.");

        return mobileNumber;
    }

    [GeneratedRegex(@"^09\d{9}$", RegexOptions.CultureInvariant)]
    private static partial Regex IranMobileRegex();
}
