using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace RMS.Application.Controlr;

internal static partial class UserInputNormalizer
{
    internal static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormKC)
            .Replace('ك', 'ک')
            .Replace('ي', 'ی')
            .Replace('ى', 'ی');

        return MultipleWhitespaceRegex().Replace(normalized.Trim(), " ");
    }

    internal static string NormalizeMobile(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("شماره موبایل الزامی است.");

        var normalized = value.Normalize(NormalizationForm.FormKC);
        var digits = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (character is '+' && digits.Length == 0)
            {
                digits.Append(character);
                continue;
            }

            if (TryConvertDigit(character, out var digit))
            {
                digits.Append(digit);
                continue;
            }

            if (char.IsWhiteSpace(character) || character is '-' or '(' or ')')
                continue;

            throw new ValidationException("شماره موبایل نامعتبر است.");
        }

        var mobile = digits.ToString();
        if (mobile.StartsWith("+98", StringComparison.Ordinal))
            mobile = $"0{mobile[3..]}";
        else if (mobile.StartsWith("0098", StringComparison.Ordinal))
            mobile = $"0{mobile[4..]}";
        else if (mobile.StartsWith("98", StringComparison.Ordinal) && mobile.Length == 12)
            mobile = $"0{mobile[2..]}";

        if (!IranMobileRegex().IsMatch(mobile))
            throw new ValidationException("شماره موبایل باید یک شماره معتبر ایران باشد.");

        return mobile;
    }

    private static bool TryConvertDigit(char value, out char digit)
    {
        digit = value switch
        {
            >= '0' and <= '9' => value,
            >= '۰' and <= '۹' => (char)('0' + value - '۰'),
            >= '٠' and <= '٩' => (char)('0' + value - '٠'),
            _ => '\0'
        };

        return digit != '\0';
    }

    [GeneratedRegex(@"^09\d{9}$", RegexOptions.CultureInvariant)]
    private static partial Regex IranMobileRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex MultipleWhitespaceRegex();
}
