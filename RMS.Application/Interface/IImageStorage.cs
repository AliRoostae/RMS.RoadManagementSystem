using RMS.Shared.Contracts.Responses;

using RMS.Shared.Contracts.Commands;

namespace RMS.Application.Interface;



/// <summary>
/// عملیات ذخیره‌سازی، پردازش و حذف تصاویر حادثه را تعریف می‌کند.
/// </summary>
public interface IImageStorageService
{
    /// <summary>تصویر حادثه را ذخیره می‌کند و شناسهٔ آن را برمی‌گرداند.</summary>
    /// <param name="command">اطلاعات تصویر.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task<Guid> AddImageUrlAsync(CreateAccidentImageCommand command, CancellationToken token = default);

    /// <summary>یک تصویر را حذف می‌کند.</summary>
    /// <param name="command">شناسهٔ تصویر.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task DeleteAsync(DeleteAccidentImageCommand command, CancellationToken token = default);

    /// <summary>تمام تصاویر یک حادثه را حذف می‌کند.</summary>
    /// <param name="command">شناسهٔ حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task DeleteAllAsync(DeleteAccidentImagesCommand command, CancellationToken token = default);

    /// <summary>تمام تصاویر یک حادثه را برمی‌گرداند.</summary>
    /// <param name="accidentId">شناسهٔ حادثه.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task<IReadOnlyList<ImageResponse>> GetAllAsync(Guid accidentId, CancellationToken token = default);

    /// <summary>جزئیات یک تصویر را برمی‌گرداند.</summary>
    /// <param name="imageId">شناسهٔ تصویر.</param>
    /// <param name="token">توکن لغو عملیات.</param>
    Task<ImageResponse?> GetAsync(Guid imageId, CancellationToken token = default);
}
