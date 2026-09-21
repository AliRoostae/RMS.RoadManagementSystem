using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RMS.Domain.Entities;




/// <summary>
/// موجودیت پایدار یک خودروی درگیر در حادثه و سرنشینان منتسب به آن را نمایش می‌دهد.
/// </summary>
public class CarEntities
{
    /// <summary>شناسهٔ یکتای خودرو در سامانه.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>شناسهٔ حادثه‌ای که خودرو در آن درگیر بوده است.</summary>
    public Guid FkAccident { get; set; }

    /// <summary>نام یا مدل خودرو.</summary>
    [MaxLength(50, ErrorMessage = "نهایت 50 کاراکتر.")]
    [Required]
    public string CarName { get; set; } = string.Empty;

    /// <summary>ردهٔ بدنه یا کاربری خودرو.</summary>
    public CarClassEnums CarClass { get; set; } = CarClassEnums.Unknown;

    /// <summary>سال تولید خودرو.</summary>
    [Range(1300, 2200, ErrorMessage = "بین 1300 تا 2200")]
    [Required]
    public int ProductionYear { get; set; }

    /// <summary>شمارهٔ پلاک خودرو.</summary>
    [MaxLength(20, ErrorMessage = "نهایت 20 کاراکتر.")]
    [Required]
    public string PlateNumber { get; set; } = string.Empty;

    /// <summary>رنگ خودرو.</summary>
    [MaxLength(50, ErrorMessage = "نهایت 50 کاراکتر.")]
    public string Color { get; set; } = string.Empty;
    /// <summary>شمارهٔ تماس راننده.</summary>

    [MaxLength(20, ErrorMessage = "نهایت 20 کاراکتر.")]
    [Required]
    public string DriverPhone { get; set; } = string.Empty;
    /// <summary>
    /// شمارهٔ گواهینامهٔ راننده.
    /// </summary>
    [MaxLength(20, ErrorMessage = "نهایت 20 کاراکتر.")]
    [Required]
    public string DriverLicNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاریخ صدور گواهینامهٔ راننده.
    /// </summary>
    [Range(typeof(DateOnly), "2020-01-01", "2100-12-31",
       ErrorMessage = "تاریخ نامعتبر است.")]
    public DateOnly DateLic { get; set; }

    /// <summary>
    /// مدت اعتبار گواهینامه برحسب سال.
    /// </summary>
    [Range(0, 20, ErrorMessage = "بین 0 تا 20 معتبر است .")]
    public byte DateLicValidity { get; set; }

    /// <summary>درصد خسارت واردشده به بدنهٔ خودرو بر اساس اعتبارسنجی فعلی (۱ تا ۱۰۰).</summary>
    [Range(1, 100, ErrorMessage = "بین 1 تا 100")]
    public byte DamagePercentage { get; set; } = 1;

    /// <summary>حادثه‌ای که خودرو به آن تعلق دارد.</summary>
    public AccidentEntities Accident { get; set; } = null!;


    /// <summary>
    /// مجموعهٔ راننده و سرنشینان خودرو.
    /// </summary>
    public ICollection<PassengerEntities> PassengerList { get; set; } = new List<PassengerEntities>();
}
