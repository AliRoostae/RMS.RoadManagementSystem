
using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RMS.Domain.Entities;



/// <summary>
/// موجودیت پایدار راننده یا سرنشین یک خودرو را نمایش می‌دهد.
/// هر نمونه فقط به یک خودرو منتسب است و حضور همان شخص در خودرویی دیگر باید به‌صورت نمونه‌ای مستقل ثبت شود.
/// </summary>
public class PassengerEntities
{

    /// <summary>شناسهٔ یکتای سرنشین در سامانه.</summary>
    [Key]
    public Guid Id { get; set; }
    /// <summary>شناسهٔ خودرویی که شخص در آن حضور داشته است.</summary>
    public Guid FkCar { get; set; }
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
    /// <summary>
    /// مشخص می‌کند شخص رانندهٔ خودرو بوده است یا خیر.
    /// </summary>
    public bool IsDriver { get; set; }

    /// <summary>درصد شدت آسیب بدنی شخص بر اساس اعتبارسنجی فعلی (0 تا ۱۰۰).</summary>
    [Range(0, 100, ErrorMessage = "بین 0تا 100")]
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
    /// <summary>خودرویی که شخص به آن منتسب است.</summary>
    public CarEntities Car { get; set; } = null!;






}
