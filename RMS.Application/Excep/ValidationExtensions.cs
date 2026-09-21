using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace RMS.Application.Excep;

/// <summary>
/// عملیات کمکی اعتبارسنجی مدل‌ها با Data Annotations و تبدیل خطاها به JSON را فراهم می‌کند.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// تمام ویژگی‌های اعتبارسنجی Data Annotations یک شیء را ارزیابی می‌کند.
    /// </summary>
    /// <param name="model">شیء موردنظر برای اعتبارسنجی.</param>
    /// <param name="results">فهرست خطاهای اعتبارسنجی یافت‌شده.</param>
    /// <returns><see langword="true"/> در صورت معتبر بودن شیء؛ در غیر این صورت <see langword="false"/>.</returns>
    public static bool TryValidate(this object model, out List<ValidationResult> results)
    {
        results = [];
        var context = new ValidationContext(model, serviceProvider: null, items: null);

        // آرگومان validateAllProperties: true یعنی تمام پروپرتی‌ها چک شوند نه فقط اولین خطا
        return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
    }

   
    /// <summary>فهرست خطاهای اعتبارسنجی را به متن JSON تبدیل می‌کند.</summary>
    /// <param name="argo">خطاهای اعتبارسنجی.</param>
    /// <returns>متن JSON شامل خطاهای اعتبارسنجی.</returns>
    public static string JsonErrors(this List<ValidationResult> argo)
    {
        return JsonSerializer.Serialize(argo);
    }
}
