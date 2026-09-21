using ImageMagick;
using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Responses;
using RMS.Infrastructure.Persistence.EF.Core;

namespace RMS.Infrastructure.Persistence.EF.Repository;

public sealed class ImageRepository
     (
    RmsDbContext _db
    ) : IImageRepository
{

    private const int MaxBase64Length = 2_000_000;
    const string subFolder = "accidents";

    /// <summary>موجودیت تصویر را در دیتابیس ثبت می‌کند.</summary>
    /// <param name="argo">موجودیت تصویر.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    public async Task<bool> AddAsync(ImageEntities argo, CancellationToken token = default)
    {
        await _db.ImageDs.AddAsync(argo, token);
        return await _db.SaveChangesAsync(token) >0;

    }

    /// <summary>همهٔ تصاویر یک حادثه را حذف می‌کند و نشانی فایل‌ها را برمی‌گرداند.</summary>
    public async Task<IList<string>> DeleteAllAsync(Guid accidentId, CancellationToken token = default)
    {
       var find =  _db.ImageDs.Where(i=> i.FkAccident == accidentId);
        _db.ImageDs.RemoveRange(find);
        var result = await find.Select(i => i.ImageUrls).ToListAsync(token);
         await _db.SaveChangesAsync(token);
        return result;
    }

    /// <summary>یک تصویر را حذف می‌کند و نشانی فایل حذف‌شده را برمی‌گرداند.</summary>
    public async Task<string> DeleteAsync(Guid imageId, CancellationToken token = default)
    {
        var find =await _db.ImageDs.FirstOrDefaultAsync(i => i.Id == imageId,token);
        if (find == null) return string.Empty;
        _db.ImageDs.Remove(find);
         await _db.SaveChangesAsync(token) ;
        return find.ImageUrls;
    }



    private static IQueryable<ImageResponse> ProjectToResponse(
IQueryable<ImageEntities> query)
    {
        return query
            .Select(i => new ImageResponse
            {
                Id = i.Id,
                FkAccident = i.FkAccident,
                ImageUrls = i.ImageUrls
            });


    }
    /// <summary>تصاویر یک حادثه را بدون tracking می‌خواند.</summary>
    public async Task<IReadOnlyList<ImageResponse>> GetAllAsync(Guid accidentId, CancellationToken token = default)
    {
        var sor= _db.ImageDs.AsNoTracking().Where(I=> I.FkAccident == accidentId);

        return await ProjectToResponse(sor).ToListAsync(token);
    }

    /// <summary>جزئیات یک تصویر را می‌خواند.</summary>
    public async Task<ImageResponse?> GetAsync(Guid imageId, CancellationToken token = default)
    {
        var sor = _db.ImageDs.AsNoTracking().Where(I => I.Id == imageId); ;

        return await ProjectToResponse(sor).FirstOrDefaultAsync(I => I.Id == imageId,token);
    }


  


    /// <summary>فایل تصویر را با کنترل مسیر امن از web root حذف می‌کند.</summary>
    public async Task RemoveImg(string imageUrl ,string WebRootPath)
    {
        try
        {

            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException(
                    "نشانی تصویر الزامی است.",
                    nameof(imageUrl));

            const string expectedPrefix = "/uploads/accidents/";

            var normalizedUrl = imageUrl
                .Split('?', '#')[0]
                .Replace('\\', '/');

            if (!normalizedUrl.StartsWith(
                    expectedPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "نشانی تصویر خارج از مسیر مجاز است.");
            }

            var fileName = normalizedUrl[expectedPrefix.Length..];

            // مسیرهای تو‌در‌تو و Path Traversal مجاز نیستند.
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName.Contains('/') ||
                fileName.Contains('\\') ||
                Path.GetExtension(fileName) is not ".webp")
            {
                throw new InvalidOperationException(
                    "نام فایل تصویر نامعتبر است.");
            }

            // نام فایل‌های این پروژه به شکل Guid با فرمت N تولید می‌شود.
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (!Guid.TryParseExact(nameWithoutExtension, "N", out _))
                throw new InvalidOperationException(
                    "نام فایل تصویر معتبر نیست.");

            var webRoot = Path.Combine(WebRootPath);

            var allowedDirectory = Path.GetFullPath(
                Path.Combine(webRoot, "uploads", "accidents"));

            var physicalPath = Path.GetFullPath(
                fileName,
                allowedDirectory);

            var allowedPrefix =
                allowedDirectory.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (!physicalPath.StartsWith(
                    allowedPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "مسیر تصویر خارج از پوشهٔ مجاز است.");
            }

            File.Delete(physicalPath);
        }
        catch
        {
            //
        }
    }



    /// <summary>تصویر Base64 را به WebP تبدیل و در web root ذخیره می‌کند.</summary>
    public async Task<string> SaveBase64ImageAsync(string base64String, string WebRootPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            throw new ArgumentException("داده تصویر نمی‌تواند خالی باشد.", nameof(base64String));
        if (base64String.Length > MaxBase64Length)
            throw new ArgumentException("داده تصویر نمی‌تواند خالی باشد.", nameof(base64String));
        // ۱. پاک‌سازی هدر احتمالی Base64 (data:image/...)
        var commaIndex = base64String.IndexOf(',');
        var cleanBase64 = commaIndex >= 0 ? base64String[(commaIndex + 1)..] : base64String;
        var imageBytes = Convert.FromBase64String(cleanBase64.Trim());

        // ۲. تشخیص خودکار روت wwwroot از طریق Environment وب‌سرور
        var rootPath = Path.Combine(WebRootPath);
        var targetDirectory = Path.Combine(rootPath, "uploads", subFolder);

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // ۳. تولید نام فایل یکتا
        var fileName = $"{Guid.NewGuid():N}.webp";
        var physicalPath = Path.Combine(targetDirectory, fileName);

        // ۴. پردازش، فشرده‌سازی و ذخیره با Magick.NET
        using var image = new MagickImage(imageBytes);
        image.Strip(); // حذف EXIF و متادیتاها
        image.Format = MagickFormat.WebP;
        image.Quality = 80;

        await image.WriteAsync(physicalPath, cancellationToken);

        // ۵. برگرداندن URL نسبی آماده برای ذخیره در دیتابیس
        return $"/uploads/{subFolder}/{fileName}";
    }
}
