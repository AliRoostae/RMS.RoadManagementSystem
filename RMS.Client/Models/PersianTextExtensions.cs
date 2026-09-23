namespace RMS.Client.Models;

/// <summary>تبدیل‌های کوچک مربوط به نمایش متن فارسی.</summary>
public static class PersianTextExtensions
{
    private static readonly char[] PersianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];

    /// <summary>رقم‌های لاتین و عربی را به رقم فارسی تبدیل می‌کند.</summary>
    public static string TranslateDigits(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var characters = value.ToCharArray();
        for (var index = 0; index < characters.Length; index++)
        {
            if (characters[index] is >= '0' and <= '9')
                characters[index] = PersianDigits[characters[index] - '0'];
        }

        return new string(characters);
    }
}