using RMS.Application.Interface;
using RMS.Domain.Entities;
using RMS.Application.Excep;
using RMS.Shared.Contracts.Commands;
using RMS.Domain.Interfaces;
using RMS.Shared.Contracts.Responses;
using System.ComponentModel.DataAnnotations;

namespace RMS.Application.Service;





/// <inheritdoc cref="IImageStorageService"/>
/// <remarks>تصاویر با استفاده از Magick.NET به WebP تبدیل و در فضای فایل محلی برنامه ذخیره می‌شوند.</remarks>
public sealed class ImageStorageService
    (string WebRootPath,
    IImageRepository _repImag
    ) : IImageStorageService
{
    

    public async Task<Guid> AddImageUrlAsync(CreateAccidentImageCommand command, CancellationToken token = default)
    {
        if (!command.TryValidate(out var validationErrors)) throw new ValidationException(validationErrors.JsonErrors());
        if (command.AccidentId == Guid.Empty) throw new ValidationException("شناسهٔ حادثه الزامی است.");
        var pht = await _repImag.SaveBase64ImageAsync(command.Base64Image, WebRootPath, token);
        var insert = new ImageEntities
        {
            FkAccident =command.AccidentId,
            Id = Guid.NewGuid(),
            ImageUrls =pht
        };
        if (await _repImag.AddAsync(insert,token))
            return insert.Id;
        else  
        {
            await _repImag.RemoveImg(pht,WebRootPath);
            return Guid.Empty;
        }
    }

    public async Task DeleteAllAsync(DeleteAccidentImagesCommand command, CancellationToken token = default)
    {
        if (command.AccidentId == Guid.Empty) throw new ValidationException("شناسهٔ حادثه الزامی است.");
        var rem = await _repImag.DeleteAllAsync(command.AccidentId, token);
        foreach (var item in rem) await _repImag.RemoveImg(item, WebRootPath);
      

    }  

    public async Task DeleteAsync(DeleteAccidentImageCommand command, CancellationToken token = default)
    {  
        if (command.ImageId == Guid.Empty) throw new ValidationException("شناسهٔ تصویر الزامی است.");
        var rem =await _repImag.DeleteAsync(command.ImageId, token);
        await _repImag.RemoveImg(rem, WebRootPath);
      
    }
      

    public async Task<IReadOnlyList<ImageResponse>> GetAllAsync(Guid accidentId, CancellationToken token = default)=>
        await _repImag.GetAllAsync(accidentId, token);
   

    public async Task<ImageResponse?> GetAsync(Guid imageId, CancellationToken token = default)=>
        await _repImag.GetAsync(imageId, token);
  

}

