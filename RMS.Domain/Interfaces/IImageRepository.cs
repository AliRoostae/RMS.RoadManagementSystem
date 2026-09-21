using RMS.Domain.Entities;
using RMS.Shared.Contracts.Responses;

namespace RMS.Domain.Interfaces;

public interface IImageRepository
{


    Task<bool> AddAsync(ImageEntities argo, CancellationToken token = default);


    Task<string> DeleteAsync(Guid imageId, CancellationToken token = default);


    Task<IList<string>> DeleteAllAsync(Guid accidentId, CancellationToken token = default);


    Task<IReadOnlyList<ImageResponse>> GetAllAsync(Guid accidentId, CancellationToken token = default);

    Task<ImageResponse?> GetAsync(Guid imageId, CancellationToken token = default);

    Task<string> SaveBase64ImageAsync(string base64String, string WebRootPath, CancellationToken cancellationToken = default);
    Task RemoveImg(string imageUrl, string WebRootPath);
}
