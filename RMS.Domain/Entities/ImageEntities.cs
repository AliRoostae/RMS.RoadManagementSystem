using System.ComponentModel.DataAnnotations;

namespace RMS.Domain.Entities;

public class ImageEntities
{
    /// <summary>شناسهٔ یکتای تصویر در سامانه.</summary>
    [Key]
    public Guid Id
    {
        get; set;
    }

    /// <summary>شناسهٔ حادثه‌ای که تصویر در آن درگیر بوده است.</summary>
    public Guid FkAccident
    {
        get; set;
    }

    /// <summary>حادثه‌ای که تصویر به آن تعلق دارد.</summary>
    public AccidentEntities Accident { get; set; } = null!;

    /// <summary>نشانی ذخیره‌شدهٔ تصویر یا تصاویر حادثه.</summary>
    [Required]
    public string ImageUrls { get; set; } = string.Empty;
}