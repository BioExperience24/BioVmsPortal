using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web.Controller;


public class MediaController(IMediaService mediaService) : BaseController
{
    private readonly IMediaService _mediaService = mediaService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
        => Ok(await _mediaService.Get(id));

    [HttpGet]
    public async Task<IActionResult> Get(int pageIndex = 0, int pageSize = 10)
        => Ok(await _mediaService.Get(pageIndex, pageSize));

    [HttpPost]
    public async Task<IActionResult> Add(MediaDTO request, CancellationToken token)
    {
        await _mediaService.Add(request, token);
        return NoContent();
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string name)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var mediaDto = new MediaDTO
        {
            FilePath = await _mediaService.UploadFileAsync(file, name) // Menggunakan service untuk upload
        };

        return Ok(new { mediaDto.FilePath });
    }


    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update(Media request, CancellationToken token)
    {
        await _mediaService.Update(request, token);
        return NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        await _mediaService.Delete(id, token);
        return NoContent();
    }
}
