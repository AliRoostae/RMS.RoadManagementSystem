
using System.ComponentModel.DataAnnotations;
using RMS.Shared.Enums;

namespace RMS.Domain.Entities;

/// <summary>
/// موجودیت پایدار شخص حاضر در حادثه را نمایش می‌دهد که به خودرویی منتسب نیست؛ مانند عابر پیاده.
/// هر نمونه فقط به یک حادثه تعلق دارد و حضور همان شخص در حادثه‌ای دیگر باید به‌صورت نمونه‌ای مستقل ثبت شود.
/// </summary>
public class PeopleEntities
{
    /// <summary>شناسهٔ یکتای شخص در سامانه.</summary>
    [Key]
    public Guid Id
    {
        get; set;
    }

    /// <summary>شناسهٔ حادثه‌ای که شخص در آن حضور داشته است.</summary>
    public Guid FkAccident
    {
        get; set;
    }

    /// <summary>
    /// کد ملی شخص.
    /// </summary> 
    [Required]
    [MaxLength(20)]
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// نام و نام خانوادگی شخص.
    /// </summary>
    [MaxLength(100, ErrorMessage = "نهایت 100 کاراکتر.")]
    [Required]
    public string PassengerFullName { get; set; } = string.Empty;
    /// <summary>
    /// جنسیت شخص.
    /// </summary>
    public GenderEnums Gender { get; set; } = GenderEnums.Unknown;

    /// <summary>
    /// سن شخص برحسب سال.
    /// </summary>
    [Range(1, 150, ErrorMessage = "1 تا 150 معتبر است")]
    public byte Age { get; set; } = 1;


    /// <summary>درصد شدت آسیب بدنی شخص بر اساس اعتبارسنجی فعلی (0 تا ۱۰۰).</summary>
    [Range(0, 100, ErrorMessage = "بین 0 تا 100")]
    public byte InjuryPercentage { get; set; } = 1;
    /// <summary>
    /// توضیحات تکمیلی دربارهٔ آسیب واردشده به شخص.
    /// </summary>
    [MaxLength(250)]
    public string DescriptionDamage { get; set; } = string.Empty;


    /// <summary>
    /// سطح آسیب واردشده به شخص.
    /// </summary>
    public DamageTypePersonEnum TypePersonDamage { get; set; } = DamageTypePersonEnum.None;
    /// <summary>حادثه‌ای که شخص در آن حضور داشته است.</summary>
    public AccidentEntities Accident { get; set; } = null!;






}