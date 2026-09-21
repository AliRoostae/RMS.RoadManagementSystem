using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace RMS.Shared.Validation;

/// <summary>هندسه خطی راه را از نظر نوع، اعتبار ساختاری و SRID 4326 اعتبارسنجی می‌کند.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ValidBoundaryGeometryAttribute : ValidationAttribute
{
    private const int ExpectedSrid = 4326;

    public ValidBoundaryGeometryAttribute() =>
        ErrorMessage = "مسیر جغرافیایی نامعتبر است. هندسه باید LineString یا MultiLineString و دارای SRID 4326 باشد.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;
        if (value is not Geometry geometry)
            return new ValidationResult("نوع داده ارسالی باید از نوع هندسه باشد.");
        if (geometry.IsEmpty)
            return new ValidationResult("محدوده هندسی نمی‌تواند خالی باشد.");
        if (geometry is not (LineString or MultiLineString))
            return new ValidationResult("مسیر فقط می‌تواند LineString یا MultiLineString باشد.");
        if (geometry.SRID != ExpectedSrid)
            return new ValidationResult($"سیستم مختصات باید {ExpectedSrid} (WGS 84) باشد.");
        if (!geometry.IsValid)
            return new ValidationResult("مسیر خطی ارسالی از نظر هندسی معتبر نیست.");
        return ValidationResult.Success;
    }
}

/// <summary>نقطه مرکزی را از نظر بازه مختصات و SRID 4326 اعتبارسنجی می‌کند.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ValidCentroidPointAttribute : ValidationAttribute
{
    private const int ExpectedSrid = 4326;

    public ValidCentroidPointAttribute() =>
        ErrorMessage = "نقطه مرکزی نامعتبر است. مختصات جغرافیایی خارج از بازه مجاز یا فاقد SRID 4326 است.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;
        if (value is not Point point)
            return new ValidationResult("داده ارسالی باید از نوع Point باشد.");
        if (point.IsEmpty)
            return new ValidationResult("نقطه مرکزی نمی‌تواند خالی باشد.");
        if (point.SRID != ExpectedSrid)
            return new ValidationResult($"سیستم مختصات نقطه مرکزی باید {ExpectedSrid} باشد.");
        if (point.Y is < -90 or > 90)
            return new ValidationResult("عرض جغرافیایی باید بین 90- و 90 باشد.");
        if (point.X is < -180 or > 180)
            return new ValidationResult("طول جغرافیایی باید بین 180- و 180 باشد.");
        return ValidationResult.Success;
    }
}
