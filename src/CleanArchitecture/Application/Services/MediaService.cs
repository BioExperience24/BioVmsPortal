using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Models.Media;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Services
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkNotToken _unitOfWorkNotToken;
        private readonly IMapper _mapper;
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CleanArchitecture.Frontend", "UploadedFiles");

        public MediaService(IUnitOfWork unitOfWork, IMapper mapper, IUnitOfWorkNotToken unitOfWorkNotToken)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _unitOfWorkNotToken = unitOfWorkNotToken;

            if (!Directory.Exists(_filePath))
            {
                Directory.CreateDirectory(_filePath);
            }
        }

        public async Task<Pagination<Media>> Get(int pageIndex, int pageSize)
        {
            var media = await _unitOfWork.MediaRepository.ToPagination(
                pageIndex: pageIndex,
                pageSize: pageSize,
                orderBy: x => x.Name,
                ascending: true
            );

            return media;
        }

        public async Task<Media> Get(int id)
        {
            var media = await _unitOfWork.MediaRepository.FirstOrDefaultAsync(x => x.Id == id);
            return media;
        }

        public async Task Add(MediaDTO request, CancellationToken token)
        {
            var media = _mapper.Map<Media>(request);
            await _unitOfWork.ExecuteTransactionAsync(async () => await _unitOfWork.MediaRepository.AddAsync(media), token);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string name)
        {
            var fileName = Path.GetFileName(file.FileName);
            var fullPath = Path.Combine(_filePath, fileName);

            // debug fileName
            Console.WriteLine(fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var media = new Media
            {
                Name = name,
                FilePath = fullPath
            };

             await _unitOfWorkNotToken.ExecuteTransactionAsync(async () => await _unitOfWork.MediaRepository.AddAsync(media));

            return fileName; // Kembalikan nama file atau path
        }

        public async Task Update(Media request, CancellationToken token)
        {
            var media = await _unitOfWork.MediaRepository.FirstOrDefaultAsync(x => x.Id == request.Id);
            await _unitOfWork.ExecuteTransactionAsync(() => _unitOfWork.MediaRepository.Update(media), token);
        }

        public async Task Delete(int id, CancellationToken token)
        {
            var media = await _unitOfWork.MediaRepository.FirstOrDefaultAsync(x => x.Id == id);
            await _unitOfWork.ExecuteTransactionAsync(() => _unitOfWork.MediaRepository.Delete(media), token);
        }
    }
}
