using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Models.Media;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IMediaService
{
    Task<Pagination<Media>> Get(int pageIndex, int pageSize);
    Task<Media> Get(int id);
    Task Add(MediaDTO request, CancellationToken token);
    Task<string> UploadFileAsync(IFormFile file, string name);
    Task Update(Media request, CancellationToken token);
    Task Delete(int id, CancellationToken token);
}
