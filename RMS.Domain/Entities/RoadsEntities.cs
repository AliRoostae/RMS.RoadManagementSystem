using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using RMS.Shared.Validation;

namespace RMS.Domain.Entities;

/// <summary>
/// موجودیت پایدار یک راه یا محدودهٔ جغرافیایی و حوادث ثبت‌شده در آن را نمایش می‌دهد.
/// </summary>
public class RoadsEntities
{

    /// <summary>شناسهٔ یکتای راه یا محدودهٔ جغرافیایی.</summary>
    public Guid Id
    {
        get; set;
    }

    /// <summary>
    /// نام راه یا عنوان محدودهٔ جغرافیایی.
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// مسیر اصلی و شاخه‌های راه در قالب هندسه خطی WGS 84 با SRID 4326.
    /// </summary>
    [Required(ErrorMessage = "تعیین مسیر خطی راه الزامی است.")]
    [ValidBoundaryGeometry]
    public Geometry Boundary { get; set; } = null!; // داده‌های جدید همواره به MultiLineString نرمال می‌شوند.

    /// <summary>
    /// نقطه‌ای نماینده روی مسیر برای تنظیم مرکز نقشه، بزرگ‌نمایی و نمایش برچسب.
    /// </summary>
    [ValidCentroidPoint]
    public NetTopologySuite.Geometries.Point? Centroid
    {
        get; set;
    }
    /// <summary>مجموعهٔ حوادث ثبت‌شده برای این راه یا محدوده.</summary>
    public ICollection<AccidentEntities> AccountList { get; set; } = new List<AccidentEntities>();

}