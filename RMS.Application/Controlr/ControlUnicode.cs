using System;
using System.Collections.Generic;
using System.Text;

namespace RMS.Application.Controlr;

public static   class ControlUnicode
{

    /// <summary>
    /// یکسان‌سازی یونیکد  پیش از ذخیره‌سازی.
    /// بر اساس تعداد کاراکترهای فارسی/عربی در برابر لاتین، تشخیص می‌دهد پلاک
    /// فارسی است یا انگلیسی و همان یونیکد را برای کل رشته اعمال می‌کند.
    /// اعداد همیشه به‌صورت ارقام لاتین استاندارد ذخیره می‌شوند.
    /// </summary>
    /// <param name="inputText">متنی که باید یکدست‌سازی شود.</param>
    /// <returns>متن نرمال‌شده با اعداد لاتین و حروف استاندارد.</returns>
    public static string NormalizeUnicode(this string? inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
            return string.Empty;

        // یکسان‌سازی فرم یونیکد: کاراکترهای Presentation Form عربی (مثل ﻫ) به فرم پایه تبدیل می‌شوند
        var normalized = inputText.Normalize(NormalizationForm.FormKC);

        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            // تطویل (ـ) و فاصله‌های اضافی OCR حذف می‌شوند
            if (ch == '\u0640' || char.IsWhiteSpace(ch))
                continue;

            sb.Append(ch switch
            {
                '\u0643' => 'ک', // Arabic Kaf  -> Persian Kaf
                '\u064A' => 'ی', // Arabic Yeh  -> Persian Yeh
                '\u0649' => 'ی', // Arabic Alef Maksura -> Persian Yeh
                _ => ch
            });
        }

        var text = sb.ToString().ToEnglishNumber(); // اعداد همیشه لاتین استاندارد

        var persianCount = text.Count(IsPersianLetter);
        var latinCount = text.Count(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));

        // پلاک فارسی: همان‌طور که هست (با حروف فارسی استاندارد) نگه داشته می‌شود
        // پلاک انگلیسی/لاتین: حروف به فرم استاندارد بزرگ تبدیل می‌شوند
        return persianCount >= latinCount ? text : text.ToUpperInvariant();
    }

    private static bool IsPersianLetter(char c) =>
       (c >= '\u0600' && c <= '\u06FF') ||   // بلوک عربی/فارسی
       (c >= '\uFB50' && c <= '\uFDFF') ||   // Arabic Presentation Forms-A
       (c >= '\uFE70' && c <= '\uFEFF');     // Arabic Presentation Forms-B

    private static string ToEnglishNumber(this string input)
    {

        //۰ ۱ ۲ ۳ ۴ ۵ ۶ ۷ ۸ ۹
        input = input.Replace("۰", "0");
        input = input.Replace("۱", "1");
        input = input.Replace("۲", "2");
        input = input.Replace("۳", "3");
        input = input.Replace("۴", "4");
        input = input.Replace("۵", "5");
        input = input.Replace("۶", "6");
        input = input.Replace("۷", "7");
        input = input.Replace("۸", "8");
        input = input.Replace("۹", "9");
        input = input.Replace("؟", "?");
        return input;
    }

}
